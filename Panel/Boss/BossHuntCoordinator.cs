using Panel.Models;
using Panel.Sockets;

namespace Panel.Boss;

/// <summary>
/// Bộ não điều phối Săn Boss phía Panel (Hub).
///
/// Luồng:
///   1. Nhận BOSS_FOUND từ AC dò → broadcast BOSS_HUNT_START tới TẤT CẢ AC đã đăng ký
///   2. Nhận BOSS_KILLED từ AC đánh → ghi nhận, chờ BOSS_DEAD
///   3. Nhận BOSS_DEAD (hết hồi sinh) → broadcast BOSS_ANTIADMIN_START (nếu bật)
///   4. Nhận ANTI_ADMIN_DONE từ TẤT CẢ AC → broadcast BOSS_RESET → vòng mới
/// </summary>
public class BossHuntCoordinator
{
    private readonly PanelSocketServer _server;

    // ──────────────────────────────────────────────────────────────────
    // Events để Form1 cập nhật UI log
    // ──────────────────────────────────────────────────────────────────
    public event Action<string>? OnLog;
    public event Action<BossHuntState>? OnStateChanged;

    // ──────────────────────────────────────────────────────────────────
    // State nội bộ
    // ──────────────────────────────────────────────────────────────────
    public enum BossHuntState { Idle, Hunting, AntiAdmin }

    private class ServerHuntContext
    {
        public BossHuntState State = BossHuntState.Idle;
        public int BossMapId = -1;
        public int BossZoneId = -1;
        public string BossName = "";

        // Theo dõi ANTI_ADMIN_DONE
        public readonly HashSet<int> AntiAdminDoneIds = new();
        public int ExpectedAntiAdminCount = 0;

        // Theo dõi SCOUT_DONE
        public readonly HashSet<int> ScoutDoneIds = new();
    }

    // Danh sách context theo từng máy chủ (Server ID)
    private readonly Dictionary<int, ServerHuntContext> _serverContexts = new();

    // Lấy state của Server hiện hành (chỉ dùng cho mục đích logging hoặc gỡ rối trên UI nếu cần)
    // Hiện tại UI có thể hiển thị trạng thái của 1 server hoặc chung chung. Ta tạm lấy server đầu tiên đang hunt.
    public BossHuntState State 
    {
        get
        {
            lock (_serverContexts)
            {
                var huntingCtx = _serverContexts.Values.FirstOrDefault(c => c.State != BossHuntState.Idle);
                return huntingCtx?.State ?? BossHuntState.Idle;
            }
        }
    }

    // Danh sách accountId tham gia hunt (gửi BOSS_SETTING với Enabled=true)
    private readonly HashSet<int> _participantIds = new();

    // Cài đặt hiện tại
    private BossFeatureSettings? _settings;
    private bool _isEnabled = false;

    public BossHuntCoordinator(PanelSocketServer server)
    {
        _server = server;
    }

    private ServerHuntContext GetContext(int serverId)
    {
        lock (_serverContexts)
        {
            if (!_serverContexts.TryGetValue(serverId, out var ctx))
            {
                ctx = new ServerHuntContext();
                _serverContexts[serverId] = ctx;
            }
            return ctx;
        }
    }

    // ──────────────────────────────────────────────────────────────────
    // Public API: Panel gọi khi người dùng bật/tắt hoặc lưu cài đặt
    // ──────────────────────────────────────────────────────────────────

    /// <summary>
    /// Gửi cài đặt săn boss tới TẤT CẢ client và đăng ký danh sách tham gia.
    /// Gọi khi người dùng nhấn nút Apply/Save trên UI Panel.
    /// </summary>
    public void ApplyAndBroadcastSettings(BossFeatureSettings settings, bool enabled)
    {
        _settings  = settings;
        _isEnabled = enabled;

        string cmd = BuildSettingsCommand(settings, enabled);

        _server.Broadcast(cmd);
        Log($"Broadcast BOSS_SETTING tới {_server.ConnectedCount} client (Enabled={enabled})");

        if (!enabled)
        {
            ResetAllStates();
        }
        else
        {
            // Cập nhật danh sách participant = tất cả client đang kết nối
            lock (_participantIds)
            {
                _participantIds.Clear();
                foreach (var id in _server.GetConnectedAccountIds())
                    _participantIds.Add(id);
            }

            // Sync lại trạng thái hiện hành cho toàn bộ server (để các client mới nhất nhận được)
            lock (_serverContexts)
            {
                foreach (var kvp in _serverContexts)
                {
                    int sId = kvp.Key;
                    var ctx = kvp.Value;
                    
                    if (ctx.State == BossHuntState.Hunting)
                    {
                        string subCmd = $"BOSS_HUNT_START|{sId}|{ctx.BossMapId}|{ctx.BossZoneId}|{ctx.BossName}";
                        _server.Broadcast(subCmd);
                        Log($"Broadcast lại BOSS_HUNT_START cho Server {sId}");
                    }
                    else if (ctx.State == BossHuntState.AntiAdmin)
                    {
                        int duration = _settings?.AntiBanAttackMobsSeconds ?? 60;
                        string subCmd = $"BOSS_ANTIADMIN_START|{sId}|{duration}";
                        _server.Broadcast(subCmd);
                        Log($"Broadcast lại BOSS_ANTIADMIN_START cho Server {sId}");
                    }
                }
            }
        }
    }

    public string BuildSettingsCommand(BossFeatureSettings settings, bool enabled)
    {
        string chatContents = settings.AntiBanChatContents.Replace("\r\n", "~").Replace("\n", "~");

        // Gom danh sách ID Skill cho phép thành 1 chuỗi
        List<int> validSkills = new List<int>();
        if (settings.SkillEarthDragon) validSkills.Add(0);
        if (settings.SkillEarthKame) validSkills.Add(1);
        if (settings.SkillEarthTdhs) validSkills.Add(6);
        if (settings.SkillEarthKaioken) validSkills.Add(9);
        if (settings.SkillEarthDctt) validSkills.Add(20);
        if (settings.SkillEarthThoiMien) validSkills.Add(22);
        if (settings.SkillEarthKhien) validSkills.Add(19);

        if (settings.SkillNamekLienHoan) validSkills.Add(17);
        if (settings.SkillNamekDemon) validSkills.Add(2);
        if (settings.SkillNamekMakan) validSkills.Add(3);
        if (settings.SkillNamekDeTrung) validSkills.Add(12);
        if (settings.SkillNamekKhien && !validSkills.Contains(19)) validSkills.Add(19);

        if (settings.SkillSaiyanGalick) validSkills.Add(4);
        if (settings.SkillSaiyanAntomic) validSkills.Add(5);
        if (settings.SkillSaiyanBienHinh) validSkills.Add(13);
        if (settings.SkillSaiyanTtNl) validSkills.Add(8);
        if (settings.SkillSaiyanKhien && !validSkills.Contains(19)) validSkills.Add(19);

        string allowedSkillsStr = string.Join(",", validSkills);
        if (string.IsNullOrEmpty(allowedSkillsStr)) allowedSkillsStr = "NONE";

        return "BOSS_SETTING|"
            + (enabled ? "1" : "0") + "|"
            + (settings.GoAttackBoss ? "1" : "0") + "|"
            + (settings.GoTieBoss ? "1" : "0") + "|"
            + (settings.AutoScoutContinuous ? "1" : "0") + "|"
            + (settings.ScoutOnVipChat ? "1" : "0") + "|"
            + (settings.LimitMap ? "1" : "0") + "|"
            + settings.MapRanges + "|"
            + (settings.LimitZone ? "1" : "0") + "|"
            + settings.ZoneRanges + "|"
            + settings.BossNames.Replace("\r\n", ",") + "|"
            + (settings.EnableAntiBan ? "1" : "0") + "|"
            + settings.AntiBanAttackMobsSeconds + "|"
            + chatContents + "|"
            // Index 14-20: Item settings
            + (settings.AutoTdlt    ? "1" : "0") + "|"
            + (settings.EatCuongNo  ? "1" : "0") + "|"
            + (settings.EatBoHuyet  ? "1" : "0") + "|"
            + (settings.EatGiapXen  ? "1" : "0") + "|"
            + (settings.EatAnDanh   ? "1" : "0") + "|"
            + (settings.EatCo4La    ? "1" : "0") + "|"
            + (settings.EatThucAn   ? "1" : "0") + "|"
            + (settings.AntiBanAttackMobs ? "1" : "0") + "|"
            + allowedSkillsStr + "|"
            + (settings.UseShieldUnderHp ? "1" : "0") + "|"
            + settings.ShieldHpPercent + "|"
            + (settings.LimitHpAbove ? "1" : "0") + "|"
            + settings.HpAboveValue + "|"
            + (settings.LimitHpBelow ? "1" : "0") + "|"
            + settings.HpBelowValue + "|"
            + (settings.EnableFinishingMove ? "1" : "0") + "|"
            + settings.FinishingMoveHpValue + "|"
            + (settings.EnableTimeSchedule ? "1" : "0") + "|"
            + settings.TimeSchedules.Replace("\r\n", ",").Replace("|", ",") + "|"
            + (settings.UnequipTrainingArmor ? "1" : "0") + "|"
            + settings.BossCtId + "|"
            + settings.BossVpdlId + "|"
            + settings.BossPetId;
    }

    public void SyncSettingsToClient(int accountId, BossFeatureSettings settings, bool enabled)
    {
        _settings = settings;
        _isEnabled = enabled;

        if (enabled)
        {
            lock (_participantIds)
            {
                _participantIds.Add(accountId);
            }
        }
        else
        {
            lock (_participantIds)
            {
                _participantIds.Remove(accountId);
            }
        }

        _server.SendCommand(accountId, BuildSettingsCommand(settings, enabled));
        Log($"Sync BOSS_SETTING tới AC {accountId} (Enabled={enabled})");

        // Sync lại trạng thái hiện hành nếu đang bật
        if (enabled)
        {
            lock (_serverContexts)
            {
                foreach (var kvp in _serverContexts)
                {
                    int sId = kvp.Key;
                    var ctx = kvp.Value;
                    
                    if (ctx.State == BossHuntState.Hunting)
                    {
                        string subCmd = $"BOSS_HUNT_START|{sId}|{ctx.BossMapId}|{ctx.BossZoneId}|{ctx.BossName}";
                        _server.SendCommand(accountId, subCmd);
                        Log($"Replay BOSS_HUNT_START cho AC {accountId} (Server {sId})");
                    }
                    else if (ctx.State == BossHuntState.AntiAdmin)
                    {
                        int duration = _settings?.AntiBanAttackMobsSeconds ?? 60;
                        string subCmd = $"BOSS_ANTIADMIN_START|{sId}|{duration}";
                        _server.SendCommand(accountId, subCmd);
                        Log($"Replay BOSS_ANTIADMIN_START cho AC {accountId} (Server {sId})");
                    }
                }
            }
        }
    }

    public void OnClientDisconnected(int accountId)
    {
        if (!_isEnabled) return;

        lock (_participantIds)
        {
            if (_participantIds.Remove(accountId))
            {
                Log($"[AC {accountId}] Disconnected, xoá khỏi danh sách Boss Participants.");
                
                // Kiểm tra xem việc thoát này có làm thỏa mãn điều kiện ScoutDone không
                int expected = _participantIds.Count;
                if (expected == 0) return; // Nếu không còn ai thì không cần reset vì ai cũng off

                lock (_serverContexts)
                {
                    foreach (var kvp in _serverContexts)
                    {
                        var ctx = kvp.Value;
                        if (ctx.State == BossHuntState.Hunting || ctx.State == BossHuntState.Idle || ctx.State == BossHuntState.AntiAdmin)
                        {
                            // Nếu đang chờ ScoutDone, việc a client thoát có thể làm những client còn lại chờ mãi
                            // Nên check
                            if (ctx.ScoutDoneIds.Count >= expected)
                            {
                                Log($"[SV {kvp.Key}] Tất cả AC còn lại đã dò xong sau khi 1 AC thoát → Phát lệnh Hủy.");
                                BroadcastReset(kvp.Key);
                            }
                        }
                    }
                }
            }
        }
    }


    // ──────────────────────────────────────────────────────────────────
    // Nhận lệnh ngược từ Client → Panel
    // Được gọi từ PanelSocketServer.HandleClient (sau khi parse)
    // ──────────────────────────────────────────────────────────────────

    /// <summary>
    /// BOSS_FOUND|{accountId}|{serverId}|{mapId}|{zoneId}|{bossName}
    /// AC dò báo tìm thấy boss → Panel broadcast BOSS_HUNT_START tới tất cả.
    /// </summary>
    public void OnBossFound(int accountId, int serverId, int mapId, int zoneId, string bossName)
    {
        if (!_isEnabled) return;
        
        var ctx = GetContext(serverId);
        lock (ctx)
        {
            if (ctx.State == BossHuntState.Hunting) return; // Đã đang săn ở máy chủ này, bỏ qua

            ctx.BossMapId  = mapId;
            ctx.BossZoneId = zoneId;
            ctx.BossName   = bossName;
            
            SetState(ctx, BossHuntState.Hunting);
        }

        string cmd = $"BOSS_HUNT_START|{serverId}|{mapId}|{zoneId}|{bossName}";
        _server.Broadcast(cmd);
        Log($"[AC {accountId} / SV {serverId}] Tìm thấy [{bossName}] Map {mapId} Khu {zoneId} → Broadcast BOSS_HUNT_START");
    }


    /// <summary>
    /// BOSS_KILLED|{accountId}|{serverId}|{mapId}|{zoneId}|{bossName}
    /// AC báo boss vừa chết (đang chờ hồi sinh).
    /// </summary>
    public void OnBossKilled(int accountId, int serverId, int mapId, int zoneId, string bossName)
    {
        Log($"[AC {accountId} / SV {serverId}] Boss [{bossName}] Map {mapId} Khu {zoneId} đã chết - Chờ hồi sinh...");
    }

    /// <summary>
    /// BOSS_SCOUT_DONE|{accountId}|{serverId}|{mapId}
    /// AC báo đã dò xong các khu được phân công nhưng chưa thấy boss.
    /// Nếu tất cả các AC dò đều đã báo xong → Kết thúc vòng săn.
    /// </summary>
    public void OnBossScoutDone(int accountId, int serverId, int mapId)
    {
        if (!_isEnabled) return;
        
        var ctx = GetContext(serverId);
        lock (ctx)
        {
            if (ctx.State == BossHuntState.Hunting || ctx.State == BossHuntState.AntiAdmin) return;

            ctx.ScoutDoneIds.Add(accountId);
            int expected = 0;
            lock (_participantIds)
            {
                expected = _participantIds.Count;
            }

            Log($"[AC {accountId} / SV {serverId}] Vừa báo dò xong ({ctx.ScoutDoneIds.Count}/{expected}).");

            if (ctx.ScoutDoneIds.Count >= expected && expected > 0)
            {
                Log($"[SV {serverId}] Tất cả AC đã dò xong mà không thấy boss → Phát lệnh Hủy.");
                // Tất cả đều báo dò xong -> reset về Idle để vòng lặp mới tiếp tục
                BroadcastReset(serverId);
            }
        }
    }

    /// <summary>
    /// BOSS_DEAD|{accountId}|{serverId}|{mapId}
    /// AC báo boss không hồi sinh sau 10s → Panel kích hoạt Anti-Admin (hoặc Reset).
    /// </summary>
    public void OnBossDead(int accountId, int serverId, int mapId)
    {
        if (!_isEnabled) return;
        
        var ctx = GetContext(serverId);
        lock (ctx)
        {
            if (ctx.State != BossHuntState.Hunting) return;

            Log($"[AC {accountId} / SV {serverId}] Boss không hồi sinh tại Map {mapId} → Kết thúc vòng săn");

            if (_settings?.EnableAntiBan == true)
            {
                SetState(ctx, BossHuntState.AntiAdmin);

                // Xác định số AC cần chờ xác nhận Anti-Admin xong
                ctx.AntiAdminDoneIds.Clear();
                lock (_participantIds)
                    ctx.ExpectedAntiAdminCount = _participantIds.Count;

                // Lấy duration từ settings thay vì hardcode
                int durationSeconds = _settings?.AntiBanAttackMobsSeconds ?? 60;
                if (durationSeconds <= 0) durationSeconds = 60;
                string cmd = $"BOSS_ANTIADMIN_START|{serverId}|{durationSeconds}";
                _server.Broadcast(cmd);
                Log($"Broadcast BOSS_ANTIADMIN_START|{durationSeconds}s tới {_server.ConnectedCount} client (Server {serverId})");
            }
            else
            {
                // Không bật Anti-Admin → reset ngay → về Scouting
                BroadcastReset(serverId);
            }
        }
    }

    /// <summary>
    /// ANTI_ADMIN_DONE|{accountId}|{serverId}
    /// AC báo xong Anti-Admin → chờ đủ số lượng rồi broadcast BOSS_RESET.
    /// </summary>
    public void OnAntiAdminDone(int accountId, int serverId)
    {
        var ctx = GetContext(serverId);
        lock (ctx)
        {
            if (ctx.State != BossHuntState.AntiAdmin) return;

            ctx.AntiAdminDoneIds.Add(accountId);
            bool allDone = ctx.AntiAdminDoneIds.Count >= ctx.ExpectedAntiAdminCount;
            Log($"[AC {accountId} / SV {serverId}] Anti-Admin xong. ({ctx.AntiAdminDoneIds.Count}/{ctx.ExpectedAntiAdminCount})");

            if (allDone)
                BroadcastReset(serverId);
        }
    }

    // ──────────────────────────────────────────────────────────────────
    // Gửi lệnh BOSS_RESET → tất cả AC về Idle → AutoMod tự khôi phục
    // ──────────────────────────────────────────────────────────────────
    private void BroadcastReset(int serverId)
    {
        _server.Broadcast($"BOSS_RESET|{serverId}");
        Log($"Broadcast BOSS_RESET|{serverId} → Tất cả AC Server {serverId} về quy trình cũ");
        ResetStateFor(serverId);
    }

    // ──────────────────────────────────────────────────────────────────
    // Internal helpers
    // ──────────────────────────────────────────────────────────────────

    private void ResetAllStates()
    {
        lock (_serverContexts)
        {
            foreach (var kvp in _serverContexts)
            {
                var ctx = kvp.Value;
                ctx.BossMapId  = -1;
                ctx.BossZoneId = -1;
                ctx.BossName   = "";
                ctx.AntiAdminDoneIds.Clear();
                ctx.ScoutDoneIds.Clear();
                SetState(ctx, BossHuntState.Idle);
            }
        }
    }

    private void ResetStateFor(int serverId)
    {
        var ctx = GetContext(serverId);
        lock (ctx)
        {
            ctx.BossMapId  = -1;
            ctx.BossZoneId = -1;
            ctx.BossName   = "";
            ctx.AntiAdminDoneIds.Clear();
            ctx.ScoutDoneIds.Clear();
            SetState(ctx, BossHuntState.Idle);
        }
    }

    private void SetState(ServerHuntContext ctx, BossHuntState newState)
    {
        ctx.State = newState;
        // Có thể update tổng state UI nếu cần
        OnStateChanged?.Invoke(this.State);
    }

    private void Log(string msg)
    {
        OnLog?.Invoke($"[BossCoordinator] {msg}");
    }
}
