using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using NRO_v247.Mods;
using NRO_v247.Mods.Attendance;

namespace NRO_v247;

public static class SocketGame
{
    private static TcpClient _client;
    private static NetworkStream _stream;
    private static Thread _receiveThread;
    private static Thread _reconnectThread;
    private static readonly object _connectLock = new();
    private static bool _isConnected;
    private static bool _isReconnecting;
    private static bool _allowReconnect = true;
    private const int ReconnectIntervalMs = 5000;
    public static bool HasReceivedProxySetting { get; private set; }
    public static bool IsConnectedToPanel => _isConnected;

    public static void Connect()
    {
        if (string.IsNullOrEmpty(AutoLogin.idClientSocket)) return;
        _allowReconnect = true;
        HasReceivedProxySetting = false;

        if (!TryConnectOnce())
            StartReconnectLoop();
    }

    private static long _lastCharInfoMs = 0L;
    private static long _lastPetInfoRequestMs = 0L;
    private const long CharInfoIntervalMs = 1000L;
    private const long PetInfoRequestIntervalMs = 5000L;
    public static bool isAutoRequestingPetInfo = false;

    public static void ReportStatus()
    {
        string cName = Char.myCharz()?.cName ?? "Chua ro";
        string autoState = AutoMod.ActivityState;
        SendMessage($"STATUS|{AutoLogin.idClientSocket}|{cName}|{autoState}");

        long now = mSystem.currentTimeMillis();
        if (now - _lastCharInfoMs >= CharInfoIntervalMs)
        {
            _lastCharInfoMs = now;
            SendCharInfo();
            SendCharStats();
            SendDailyQuestStatus();
            SendAttendanceStatus();
            
            if (now - _lastPetInfoRequestMs >= PetInfoRequestIntervalMs)
            {
                _lastPetInfoRequestMs = now;
                if (Char.myCharz() != null && Char.myCharz().havePet)
                {
                    isAutoRequestingPetInfo = true;
                    Service.gI().petInfo();
                }
            }

            SendPetInfo();
            SendPetStats();
        }
    }

    private static void SendDailyQuestStatus()
    {
        try
        {
            var feature = ModBootstrap.DailyQuestFeature;
            bool isRunning = feature != null && feature.IsActive;
            string runMode = feature?.RunMode ?? string.Empty;
            string stateText = feature?.StateText ?? string.Empty;
            int completed = feature?.CompletedToday ?? 0;
            int canceled = feature?.CanceledToday ?? 0;
            bool finishedToday = feature?.FinishedToday ?? false;

            SendMessage(
                $"DAILY_QUEST_STATUS|{AutoLogin.idClientSocket}|{(isRunning ? 1 : 0)}|{EncodeText(runMode)}|{EncodeText(stateText)}|{completed}|{canceled}|{(finishedToday ? 1 : 0)}");
        }
        catch
        {
        }
    }

    private static void SendAttendanceStatus()
    {
        try
        {
            var feature = ModBootstrap.AttendanceFeature;
            if (feature == null) return;
            SendAttendanceStatus(feature, feature.NextOnlineSeconds);
        }
        catch
        {
        }
    }

    public static void SendAttendanceStatus(AutoAttendanceFeature feature, int nextOnlineSeconds)
    {
        try
        {
            if (feature == null) return;
            bool enabled = feature.IsActive;
            string lastCheck = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            SendMessage(
                $"ATTENDANCE_STATUS|{AutoLogin.idClientSocket}|{(enabled ? 1 : 0)}|{EncodeText(feature.StateText)}|{EncodeText(feature.MonthlyClaimedKey)}|{EncodeText(feature.ContinuousClaimDate)}|{EncodeText(feature.OnlineClaimDate)}|{feature.OnlineClaimedCount}|{nextOnlineSeconds}|{(feature.CanClaimOnline ? 1 : 0)}|{EncodeText(lastCheck)}");
        }
        catch
        {
        }
    }

    /// <summary>
    /// Format: CHAR_STATS|accountId|gold|power|isAutoOn
    /// </summary>
    private static void SendCharStats()
    {
        try
        {
            Char me = Char.myCharz();
            if (me == null) return;
            int isAutoOn = (ModManager.AutoMod != null && ModManager.AutoMod.IsGlobalAutoEnabled) ? 1 : 0;
            SendMessage($"CHAR_STATS|{AutoLogin.idClientSocket}|{me.xu}|{me.cPower}|{isAutoOn}");
        }
        catch { }
    }

    /// <summary>
    /// Format: PET_STATS|accountId|power|potential|isAutoOn
    /// </summary>
    private static void SendPetStats()
    {
        try
        {
            if (Char.myCharz()?.havePet != true) return;
            Char pet = Char.myPetz();
            if (pet == null) return;
            
            int isAutoOn = (ModManager.AutoMod != null && ModManager.AutoMod.IsGlobalAutoEnabled) ? 1 : 0;
            SendMessage($"PET_STATS|{AutoLogin.idClientSocket}|{pet.cPower}|{pet.cTiemNang}|{isAutoOn}");
        }
        catch { }
    }


    private static void SendCharInfo()
    {
        try
        {
            Char me = Char.myCharz();
            if (me == null) return;

  
            int bagUsed = 0, bagMax = 0;
            if (me.arrItemBag != null)
            {
                bagMax = me.arrItemBag.Length;
                foreach (var item in me.arrItemBag)
                    if (item != null) bagUsed++;
            }
            int boxUsed = 0, boxMax = 0;
            if (me.arrItemBox != null)
            {
                boxMax = me.arrItemBox.Length;
                foreach (var item in me.arrItemBox)
                    if (item != null) boxUsed++;
            }
            int manhVoBT = 0;
            int manhHonBT = 0;
            int tongKilis = 0;
            
            Item[][] itemArrays = new Item[][] { me.arrItemBag, me.arrItemBox };
            foreach (var arr in itemArrays)
            {
                if (arr == null) continue;
                foreach (var item in arr)
                {
                    if (item == null || item.template == null) continue;
                    int tid = item.template.id;
                    if (tid == 1852)
                    {
                        if (item.itemOption != null)
                        {
                            for (int i = 0; i < item.itemOption.Length; i++)
                            {
                                var opt = item.itemOption[i];
                                if (opt != null && opt.optionTemplate != null && opt.optionTemplate.id == 253)
                                {
                                    tongKilis += opt.param;
                                    break;
                                }
                            }
                        }
                    }
                    if (tid == 933 || tid == 1868)
                    {
                        manhVoBT += item.GetQuantity();
                    }
                    else if (tid == 934)
                    {
                        manhHonBT += item.GetQuantity();
                    }
                }
            }

            // % thể lực (cStamina max 10000)
            int staminaPct = me.cStamina * 100 / 10000;

            var sb = new System.Text.StringBuilder();
            sb.Append($"- Tên Nhân Vật: {me.cName}\\n");
            string mapDisplay = !string.IsNullOrEmpty(TileMap.mapName) ? TileMap.mapName : "Map " + TileMap.mapID;
            sb.Append($"- Map / Khu / Tọa Độ:\\n  {mapDisplay} / {TileMap.zoneID} / {me.cx},{me.cy}\\n");
            sb.Append($"- HP: {me.cHP:#,0} / {me.cHPFull:#,0}\\n");
            sb.Append($"- MP: {me.cMP:#,0} / {me.cMPFull:#,0}\\n");
            sb.Append($"- Sức mạnh: {me.cPower:#,0}\\n");
            sb.Append($"- Vàng / Ngọc / Hồng Ngọc:\\n  {me.xu:#,0} / {me.luong:#,0} / {me.luongKhoa:#,0}\\n");
            sb.Append($"- Hành Trang: {bagUsed}/{bagMax}\\n");
            sb.Append($"- Rương: {boxUsed}/{boxMax}\\n");
            sb.Append($"- Thể lực: {me.cStamina}/10000, {staminaPct}%\\n");
            sb.Append($"- Tổng Kilis: {tongKilis:#,0}\\n");
            sb.Append($"- Tổng mảnh vỡ BT: {manhVoBT:#,0}\\n");
            sb.Append($"- Tổng mảnh hồn BT: {manhHonBT:#,0}");

            SendMessage($"CHAR_INFO|{AutoLogin.idClientSocket}|{sb}");
            if (me.arrItemBag != null)
            {
                SendMessage($"CHAR_ITEMS|{AutoLogin.idClientSocket}|{tongKilis}|{manhVoBT}|{manhHonBT}");
            }
        }
        catch { }
    }

    /// <summary>
    /// Gửi thông tin tĩnh của đệ tử lên Panel (PET_INFO)
    /// </summary>
    private static void SendPetInfo()
    {
        try
        {
            if (Char.myCharz()?.havePet != true)
            {
                SendMessage($"PET_INFO|{AutoLogin.idClientSocket}|Chưa có đệ tử");
                return;
            }
            
            Char pet = Char.myPetz();
            if (pet == null) return;

            string[] petStatuses = new string[] { "Theo sau", "Bảo vệ", "Tấn công", "Về nhà", "Hợp thể", "Hợp thể vĩnh viễn" };
            string currentStatus = (pet.petStatus >= 0 && pet.petStatus < petStatuses.Length) ? petStatuses[pet.petStatus] : "Không rõ";

            var sb = new System.Text.StringBuilder();
            sb.Append($"- Tên đệ tử: {(string.IsNullOrEmpty(pet.cName) ? "Đệ tử" : pet.cName)}\\n");
            sb.Append($"- Chế độ úp: {currentStatus}\\n");
            sb.Append($"---\\n");
            sb.Append($"- Sức mạnh: {pet.cPower:#,0}\\n");
            sb.Append($"- Tiềm năng: {pet.cTiemNang:#,0}\\n");
            sb.Append($"- Thể lực: {pet.cStamina}\\n");
            sb.Append($"---\\n");
            sb.Append($"- HP: {pet.cHP:#,0} / {pet.cHPFull:#,0}\\n");
            sb.Append($"- MP: {pet.cMP:#,0} / {pet.cMPFull:#,0}\\n");
            sb.Append($"- Sức đánh: {pet.cDamFull:#,0}\\n");
            sb.Append($"- Giáp: {pet.cDefull:#,0}\\n");
            sb.Append($"---\\n");

            if (pet.arrPetSkill != null && pet.arrPetSkill.Length > 0)
            {
                for (int i = 0; i < pet.arrPetSkill.Length; i++)
                {
                    Skill s = pet.arrPetSkill[i];
                    if (s != null)
                    {
                        if (s.template != null)
                        {
                            sb.Append($"- Skill {i + 1}. [Lv {s.point}] {s.template.name}\\n");
                        }
                        else
                        {
                            string unknownSkillName = string.IsNullOrEmpty(s.moreInfo) ? "Chưa mở" : s.moreInfo;
                            sb.Append($"- Skill {i + 1}. {unknownSkillName}\\n");
                        }
                    }
                }
            }
            else 
            {
               sb.Append($"- Đang lấy dữ liệu kỹ năng...\\n");
            }

            SendMessage($"PET_INFO|{AutoLogin.idClientSocket}|{sb.ToString()}");
        }
        catch { }
    }

    private static void SendInventory(int type)
    {
        try
        {
            Char me = Char.myCharz();
            if (me == null) return;

            Item[] items = type == 0 ? me.arrItemBag : me.arrItemBox;
            if (items == null) return;

            var sb = new StringBuilder();
            sb.Append("INVENTORY_DATA|");
            sb.Append(AutoLogin.idClientSocket).Append("|");
            
            // Format: type|gold|gem|ruby|maxBag|maxBox
            sb.Append(type).Append("|");
            sb.Append(me.xu).Append("|");
            sb.Append(me.luong).Append("|");
            sb.Append(me.luongKhoa).Append("|");
            sb.Append(me.arrItemBag?.Length ?? 0).Append("|");
            sb.Append(me.arrItemBox?.Length ?? 0).Append(";"); 

            // Format items: id,name,qty,flags
            for (int i = 0; i < items.Length; i++)
            {
                Item item = items[i];
                if (item == null || item.template == null) continue;

                int flags = GetItemVipFlags(item);
                string name = (item.template.name ?? "Unknown").Replace(",", ".").Replace("|", ""); // Safe encoding
                
                if ((flags & 4) != 0 && item.itemOption != null)
                {
                    int stars = 0;
                    foreach(var opt in item.itemOption)
                    {
                        if (opt?.optionTemplate == null) continue;
                        if (opt.optionTemplate.id == 107 || (opt.optionTemplate.name ?? "").StartsWith("#"))
                        {
                            stars = opt.param;
                        }
                    }
                    if (stars > 0) name += $" [{stars}â˜…]";
                }

                sb.Append(item.template.id).Append(",");
                sb.Append(name).Append(",");
                sb.Append(item.quantity).Append(",");
                sb.Append(flags).Append("|");
            }
            
            SendMessage(sb.ToString());
        }
        catch { }
    }

    private static int GetItemVipFlags(Item item)
    {
        if (item == null || item.template == null) return 1; // 1 = Váº­t pháº©m thÆ°á»ng
        
        int t = item.template.type;
        if (t >= 5 || t < 0) return 1; 

        int flags = 0;
        int level = item.template.level; 

        if (level == 13 || (item.template.id >= 555 && item.template.id <= 567)) flags |= 32; 
        if (level == 14 || (item.template.id >= 650 && item.template.id <= 662)) flags |= 8;  
        if (level >= 15) flags |= 64; 

        if (item.itemOption != null)
        {
            foreach (var opt in item.itemOption)
            {
                if (opt == null || opt.optionTemplate == null) continue;
                string optName = opt.optionTemplate.name ?? "";
                
                // SKH
                if (optName.StartsWith("$")) { flags |= 64; } 
                // SPL (ID 107) hoáº·c Äá»“ Sao (#)
                if ((opt.optionTemplate.id == 107 && opt.param > 0) || optName.StartsWith("#")) 
                { 
                    flags |= 4; 
                }
            }
        }

        if (flags == 0 && level < 12) 
        {
            flags = 2; 
        }
        else if (flags == 0)
        {
            flags = 2;
        }

        return flags;
    }

    public static void SendMessage(string msg)
    {
        if (!_isConnected || _stream == null) return;
        try
        {
            byte[] data = Encoding.UTF8.GetBytes(msg + "\n");
            _stream.Write(data, 0, data.Length);
            _stream.Flush();
        }
        catch
        {
            Disconnect();
        }
    }

    private static void ReceiveLoop()
    {
        NetworkStream localStream = _stream;
        if (localStream == null)
            return;

        try
        {
            using var reader = new System.IO.StreamReader(localStream, Encoding.UTF8, false, 1024, true);
            while (_isConnected)
            {
                string line = reader.ReadLine();
                if (line == null)
                    break;

                string msg = line.Trim();
                if (msg.Length == 0)
                    continue;

                HandleCommand(msg);
            }
        }
        catch
        {
        }
        finally
        {
            Disconnect();
        }
    }

    private static void HandleCommand(string message)
    {
        if (string.IsNullOrEmpty(message)) return;
        string[] parts = message.Split('|');
        if (parts.Length < 1) return;

        string command = parts[0];
        // Token bootstrap cu khong con dung.
        if (command == "TOKEN")
            return;

        switch (command)
        {
            case "ATTENDANCE":
            {
                if (parts.Length < 2) return;
                var settings = new AutoAttendanceFeature.AttendancePanelSettings
                {
                    Enabled = ParseBool(parts[1]),
                    AutoStart = parts.Length > 2 ? ParseBool(parts[2]) : true,
                    ClaimMonthly = parts.Length > 3 ? ParseBool(parts[3]) : true,
                    ClaimContinuous = parts.Length > 4 ? ParseBool(parts[4]) : true,
                    ClaimOnline = parts.Length > 5 ? ParseBool(parts[5]) : true,
                    ScheduleEnabled = parts.Length > 6 && ParseBool(parts[6]),
                    ScheduleHour = parts.Length > 7 && int.TryParse(parts[7], out int scheduleHour) ? scheduleHour : 7,
                    ScheduleMinute = parts.Length > 8 && int.TryParse(parts[8], out int scheduleMinute) ? scheduleMinute : 0,
                    SavedOnlineClaimedCount = parts.Length > 9 && int.TryParse(parts[9], out int savedOnlineCount) ? savedOnlineCount : 0,
                    SavedOnlineClaimDate = parts.Length > 10 ? DecodeText(parts[10]) : string.Empty
                };
                ModBootstrap.AttendanceFeature?.ApplySettingsFromPanel(settings);
                break;
            }
            case "ATTENDANCE_START":
            {
                ModBootstrap.AttendanceFeature?.StartFromPanel();
                break;
            }
            case "ATTENDANCE_OFF":
            {
                ModBootstrap.AttendanceFeature?.StopFromPanel();
                break;
            }
            case "DAILY_QUEST":
            {
                // Format:
                // DAILY_QUEST|enabled|scheduleEnabled|startHour|startMinute|difficulty|cancelKill|cancelGold|cancelMonster|trainMonsterEnabled|trainMonsterMapId|trainMonsterZoneId|trainMonsterMobNames|trainGoldMapId|trainGoldRequireZone|trainGoldZoneId|useGoldSuicideMode|trainGoldSuicideMapId|trainGoldSuicideZoneId|killPlayerMapId|killPlayerZoneId|killPlayerOnlyListedTargets|killPlayerTargetNames|autoFusion|trainingArmorMode|useTdlt|tdltForTrainMonster|tdltForKillPlayer|tdltForTrainGold
                if (parts.Length < 2) return;

                var settings = new NRO_v247.Mods.DailyQuest.AutoDailyQuestFeature.DailyQuestPanelSettings
                {
                    Enabled = ParseBool(parts[1]),
                    ScheduleEnabled = parts.Length > 2 && ParseBool(parts[2]),
                    StartHour = parts.Length > 3 && int.TryParse(parts[3], out int startHour) ? startHour : 4,
                    StartMinute = parts.Length > 4 && int.TryParse(parts[4], out int startMinute) ? startMinute : 0,
                    Difficulty = parts.Length > 5 ? DecodeText(parts[5]) : "siÃªu khÃ³",
                    CancelKillPlayerQuest = parts.Length > 6 && ParseBool(parts[6]),
                    CancelTrainGoldQuest = parts.Length > 7 && ParseBool(parts[7]),
                    CancelTrainMonsterQuest = parts.Length > 8 && ParseBool(parts[8]),
                    TrainMonsterEnabled = parts.Length > 9 ? ParseBool(parts[9]) : true,
                    TrainMonsterMapId = parts.Length > 10 && int.TryParse(parts[10], out int trainMonsterMapId) ? trainMonsterMapId : -1,
                    TrainMonsterZoneId = parts.Length > 11 && int.TryParse(parts[11], out int trainMonsterZoneId) ? trainMonsterZoneId : -1,
                    TrainMonsterMobNames = parts.Length > 12 ? DecodeText(parts[12]) : string.Empty,
                    TrainGoldMapId = parts.Length > 13 && int.TryParse(parts[13], out int trainGoldMapId) ? trainGoldMapId : -1,
                    TrainGoldRequireZone = parts.Length > 14 && ParseBool(parts[14]),
                    TrainGoldZoneId = parts.Length > 15 && int.TryParse(parts[15], out int trainGoldZoneId) ? trainGoldZoneId : -1,
                    UseGoldSuicideMode = parts.Length > 16 && ParseBool(parts[16]),
                    TrainGoldSuicideMapId = parts.Length > 17 && int.TryParse(parts[17], out int trainGoldSuicideMapId) ? trainGoldSuicideMapId : 0,
                    TrainGoldSuicideZoneId = parts.Length > 18 && int.TryParse(parts[18], out int trainGoldSuicideZoneId) ? trainGoldSuicideZoneId : -1,
                    KillPlayerMapId = parts.Length > 19 && int.TryParse(parts[19], out int killPlayerMapId) ? killPlayerMapId : -1,
                    KillPlayerZoneId = parts.Length > 20 && int.TryParse(parts[20], out int killPlayerZoneId) ? killPlayerZoneId : -1,
                    KillPlayerOnlyListedTargets = parts.Length > 21 && ParseBool(parts[21]),
                    KillPlayerTargetNames = parts.Length > 22 ? DecodeText(parts[22]) : string.Empty,
                    AutoFusion = parts.Length > 23 && ParseBool(parts[23]),
                    TrainingArmorMode = parts.Length > 24 && int.TryParse(parts[24], out int trainingArmorMode) ? trainingArmorMode : 0,
                    UseTdltWhenDoingDailyQuest = parts.Length > 25 ? ParseBool(parts[25]) : true,
                    TdltForTrainMonster = parts.Length > 26 && ParseBool(parts[26]),
                    TdltForKillPlayer = parts.Length > 27 && ParseBool(parts[27]),
                    TdltForTrainGold = parts.Length > 28 && ParseBool(parts[28])
                };

                ModBootstrap.DailyQuestFeature?.ApplySettingsFromPanel(settings);
                break;
            }
            case "DAILY_QUEST_START":
            {
                ModBootstrap.DailyQuestFeature?.StartFromPanel();
                break;
            }
            case "DAILY_QUEST_OFF":
            {
                ModBootstrap.DailyQuestFeature?.StopFromPanel();
                break;
            }
            case "XMAP_SETTING":
            {
                if (parts.Length < 2) return;

                bool eatChicken = ParseBool(parts[1]);
                int postMapLoadMs = parts.Length > 2 && int.TryParse(parts[2], out int pml) ? pml : -1;
                bool useTdlt = parts.Length > 3 ? ParseBool(parts[3]) : false;

                (ServiceLocator.Get<IXmapService>() as AutoXmapFeature)?.ApplySettingsFromPanel(eatChicken, postMapLoadMs, useTdlt);
                break;
            }
            case "CAPTCHA_SETTING":
            {
                if (parts.Length < 3) return;
                string apiServer = parts[1];
                string apiKey = parts[2];
                ModBootstrap.CaptchaFeature?.ApplySettingsFromPanel(apiServer, apiKey);
                break;
            }
            case "SET_RENDER":
            {
                if (parts.Length < 2) return;
                bool isEnabled = ParseBool(parts[1]);
                GameWindow.IsRenderingEnabled = isEnabled;
                break;
            }
            case "TRAIN":
            {
                if (parts.Length < 9) return;

                if (!int.TryParse(parts[1], out int mapId)) return;
                bool requireZone = ParseBool(parts[2]);
                if (!int.TryParse(parts[3], out int zoneId)) return;
                bool useTdlt = ParseBool(parts[4]);
                bool onlyUsePunch = ParseBool(parts[5]);

                bool[] skills = null;
                bool avoidSuperMob;
                int mobTargetType;
                bool changeLowPlayerZoneIfNoMob = false;
                bool checkLagMob = true;
                int trainingArmorMode = 0;
                bool freezePunchSkillCd = false;
                string mobIdsRaw;

                if (parts.Length >= 31)
                {
                    avoidSuperMob = ParseBool(parts[7]);
                    if (!int.TryParse(parts[8], out mobTargetType)) mobTargetType = 0;
                    changeLowPlayerZoneIfNoMob = ParseBool(parts[9]);
                    checkLagMob = ParseBool(parts[10]);
                    if (!int.TryParse(parts[11], out trainingArmorMode)) trainingArmorMode = 0;
                    freezePunchSkillCd = ParseBool(parts[12]);
                    mobIdsRaw = parts[13];
                    skills = new bool[17];
                    for (int i = 0; i < 17; i++)
                    {
                        skills[i] = ParseBool(parts[14 + i]);
                    }
                    bool useShield = parts.Length > 31 && ParseBool(parts[31]);
                    int shieldHp = parts.Length > 32 && int.TryParse(parts[32], out int sp) ? sp : 30;

                    ModBootstrap.TrainFeature?.ApplySettingsFromPanel(
                        mapId, requireZone, zoneId, useTdlt, onlyUsePunch,
                        skills, avoidSuperMob, mobTargetType,
                        changeLowPlayerZoneIfNoMob, checkLagMob, trainingArmorMode, freezePunchSkillCd, mobIdsRaw,
                        useShield, shieldHp);
                    break;
                }
                else if (parts.Length >= 14)
                {
                    avoidSuperMob = ParseBool(parts[7]);
                    if (!int.TryParse(parts[8], out mobTargetType)) mobTargetType = 0;
                    changeLowPlayerZoneIfNoMob = ParseBool(parts[9]);
                    checkLagMob = ParseBool(parts[10]);
                    if (!int.TryParse(parts[11], out trainingArmorMode)) trainingArmorMode = 0;
                    freezePunchSkillCd = ParseBool(parts[12]);
                    mobIdsRaw = parts.Length > 13 ? string.Join("|", parts, 13, parts.Length - 13) : string.Empty;
                }
                else if (parts.Length >= 13)
                {
                    avoidSuperMob = ParseBool(parts[7]);
                    if (!int.TryParse(parts[8], out mobTargetType)) mobTargetType = 0;
                    changeLowPlayerZoneIfNoMob = ParseBool(parts[9]);
                    checkLagMob = ParseBool(parts[10]);
                    if (!int.TryParse(parts[11], out trainingArmorMode)) trainingArmorMode = 0;
                    mobIdsRaw = parts.Length > 12 ? string.Join("|", parts, 12, parts.Length - 12) : string.Empty;
                }
                else if (parts.Length >= 12)
                {
                    avoidSuperMob = ParseBool(parts[7]);
                    if (!int.TryParse(parts[8], out mobTargetType)) mobTargetType = 0;
                    changeLowPlayerZoneIfNoMob = ParseBool(parts[9]);
                    checkLagMob = ParseBool(parts[10]);
                    mobIdsRaw = parts.Length > 11 ? string.Join("|", parts, 11, parts.Length - 11) : string.Empty;
                }
                else if (parts.Length >= 11)
                {
                    avoidSuperMob = ParseBool(parts[7]);
                    if (!int.TryParse(parts[8], out mobTargetType)) mobTargetType = 0;
                    changeLowPlayerZoneIfNoMob = ParseBool(parts[9]);
                    mobIdsRaw = parts.Length > 10 ? string.Join("|", parts, 10, parts.Length - 10) : string.Empty;
                }
                else if (parts.Length >= 10)
                {
                    avoidSuperMob = ParseBool(parts[7]);
                    if (!int.TryParse(parts[8], out mobTargetType)) mobTargetType = 0;
                    mobIdsRaw = parts.Length > 9 ? string.Join("|", parts, 9, parts.Length - 9) : string.Empty;
                }
                else
                {
                    avoidSuperMob = ParseBool(parts[6]);
                    if (!int.TryParse(parts[7], out mobTargetType)) mobTargetType = 0;
                    mobIdsRaw = parts.Length > 8 ? string.Join("|", parts, 8, parts.Length - 8) : string.Empty;
                }

                ModBootstrap.TrainFeature?.ApplySettingsFromPanel(
                    mapId, requireZone, zoneId, useTdlt, onlyUsePunch,
                    skills, avoidSuperMob, mobTargetType,
                    changeLowPlayerZoneIfNoMob, checkLagMob, trainingArmorMode, freezePunchSkillCd, mobIdsRaw);
                break;
            }
            case "TRAIN_OFF":
            {
                ModBootstrap.TrainFeature?.DisableFromPanel();
                break;
            }
            case "TRAIN_ADVANCED":
            {
                // Format: TRAIN_ADVANCED|stepSize|delay|hpAboveEnabled|hpAboveValue|hpBelowEnabled|hpBelowValue|rotateZoneEnabled|rotateZoneList|autoBuyThoiVang|minGold
                if (parts.Length < 9) return;
                if (!int.TryParse(parts[1], out int stepSize)) stepSize = 3;
                if (!int.TryParse(parts[2], out int delay)) delay = 30;
                bool hpAbove = ParseBool(parts[3]);
                if (!int.TryParse(parts[4], out int hpAboveValue)) hpAboveValue = 0;
                bool hpBelow = ParseBool(parts[5]);
                if (!int.TryParse(parts[6], out int hpBelowValue)) hpBelowValue = 0;
                bool rotateZone = ParseBool(parts[7]);
                string rotateZoneList = parts[8];

                bool autoBuyThoiVang = parts.Length > 9 && ParseBool(parts[9]);
                long minGold = parts.Length > 10 && long.TryParse(parts[10], out long mg) ? mg : 1_000_000_000L;
                bool usePrivateTicket = parts.Length > 11 && ParseBool(parts[11]);
                bool optimizeKsVang = parts.Length > 12 ? ParseBool(parts[12]) : true;

                int ksVangMode = parts.Length > 13 && int.TryParse(parts[13], out int km) ? km : 0;
                int ksVangTrigger = parts.Length > 14 && int.TryParse(parts[14], out int kt) ? kt : 0;
                int ksVangTimeMin = parts.Length > 15 && int.TryParse(parts[15], out int ktm) ? ktm : 5;
                bool ksVangFilterPlayer = parts.Length > 16 && ParseBool(parts[16]);
                int ksVangPlayerMin = parts.Length > 17 && int.TryParse(parts[17], out int kpm) ? kpm : 3;
                int ksVangPlayerMax = parts.Length > 18 && int.TryParse(parts[18], out int kpx) ? kpx : 5;
                bool ksVangAvoidChars = parts.Length > 19 && ParseBool(parts[19]);
                string ksVangAvoidCharsList = parts.Length > 20 ? parts[20] : "";

                ModBootstrap.TrainFeature?.ApplyAdvancedFromPanel(
                    stepSize, delay, hpAbove, hpAboveValue, hpBelow, hpBelowValue, rotateZone, rotateZoneList, usePrivateTicket, optimizeKsVang, ksVangMode, ksVangTrigger, ksVangTimeMin, ksVangFilterPlayer, ksVangPlayerMin, ksVangPlayerMax, ksVangAvoidChars, ksVangAvoidCharsList);
                
                ModBootstrap.AutoBuyFeature?.ApplyThoiVangSettings(autoBuyThoiVang, minGold);
                break;
            }
            case "MVBT_SETTING":
            case "MHBT_SETTING":
            {
                if (parts.Length < 19) return;

                bool enabled = ParseBool(parts[1]);
                if (!int.TryParse(parts[2], out int startH)) return;
                if (!int.TryParse(parts[3], out int startM)) return;
                if (!int.TryParse(parts[4], out int stopH)) return;
                if (!int.TryParse(parts[5], out int stopM)) return;
                
                if (!int.TryParse(parts[6], out int mapId)) return;
                bool requireZone = ParseBool(parts[7]);
                if (!int.TryParse(parts[8], out int zoneId)) return;
                bool useTdlt = ParseBool(parts[9]);
                bool onlyUsePunch = ParseBool(parts[10]);
                // bool useKaiokenLienHoan = ParseBool(parts[11]); bá»
                bool avoidSuperMob = ParseBool(parts[12]);
                if (!int.TryParse(parts[13], out int mobTargetType)) mobTargetType = 0;
                bool changeLowPlayerZoneIfNoMob = ParseBool(parts[14]);
                bool checkLagMob = ParseBool(parts[15]);
                if (!int.TryParse(parts[16], out int trainingArmorMode)) trainingArmorMode = 0;
                bool freezePunchSkillCd = ParseBool(parts[17]);
                if (!int.TryParse(parts[18], out int targetCount)) targetCount = 99;
                string mobIdsRaw = parts.Length > 19 ? string.Join("|", parts, 19, parts.Length - 19) : string.Empty;

                if (command == "MVBT_SETTING")
                {
                    ModBootstrap.MvbtFeature?.ApplyMvbtSettings(
                        enabled, startH, startM, stopH, stopM,
                        mapId, requireZone, zoneId, useTdlt, onlyUsePunch,
                        null, avoidSuperMob, mobTargetType,
                        changeLowPlayerZoneIfNoMob, checkLagMob, trainingArmorMode, freezePunchSkillCd, targetCount, mobIdsRaw);
                }
                else
                {
                    ModBootstrap.MhbtFeature?.ApplyMhbtSettings(
                        enabled, startH, startM, stopH, stopM,
                        mapId, requireZone, zoneId, useTdlt, onlyUsePunch,
                        null, avoidSuperMob, mobTargetType,
                        changeLowPlayerZoneIfNoMob, checkLagMob, trainingArmorMode, freezePunchSkillCd, targetCount, mobIdsRaw);
                }
                break;
            }
            case "KILIS_SETTING":
            {
                if (parts.Length < 12) return;
                bool enabled = ParseBool(parts[1]);
                if (!int.TryParse(parts[2], out int startH)) return;
                if (!int.TryParse(parts[3], out int startM)) return;
                if (!int.TryParse(parts[4], out int stopH)) return;
                if (!int.TryParse(parts[5], out int stopM)) return;
                if (!int.TryParse(parts[6], out int zoneId)) return;
                bool autoBuyAmulet = ParseBool(parts[7]);
                if (!int.TryParse(parts[8], out int amuletType)) amuletType = 0;
                bool useTdlt = ParseBool(parts[9]);
                bool autoZone = ParseBool(parts[10]);
                if (!int.TryParse(parts[11], out int trainingArmorMode)) trainingArmorMode = 0;

                ModBootstrap.KilisFeature?.ApplyKilisSettings(
                    enabled, startH, startM, stopH, stopM, zoneId,
                    autoBuyAmulet, amuletType, useTdlt, autoZone, trainingArmorMode);
                break;
            }
            case "BOSS_VEGETA_CITY_SETTING":
            {
                if (parts.Length < 27) return;

                bool enabled = ParseBool(parts[1]);
                bool auto15h = ParseBool(parts[2]);
                bool auto2230 = ParseBool(parts[3]);
                bool reviveByGem = ParseBool(parts[4]);
                bool useTdlt = ParseBool(parts[5]);
                if (!int.TryParse(parts[6], out int trainingArmorMode)) trainingArmorMode = 0;
                bool freezePunchSkillCd = ParseBool(parts[7]);
                bool useShieldUnderHp = ParseBool(parts[8]);
                if (!int.TryParse(parts[9], out int shieldHpPercent)) shieldHpPercent = 30;

                bool[] skills = new bool[17];
                for (int i = 0; i < 17; i++)
                {
                    skills[i] = ParseBool(parts[10 + i]);
                }

                ModBootstrap.BossVegetaCityFeature?.ApplySettingsFromPanel(
                    enabled,
                    auto15h,
                    auto2230,
                    reviveByGem,
                    useTdlt,
                    trainingArmorMode,
                    freezePunchSkillCd,
                    useShieldUnderHp,
                    shieldHpPercent,
                    skills);
                break;
            }
            case "STORE":
            {
                bool autoStore = ParseBool(parts[1]);
                bool kh = ParseBool(parts[2]);
                bool tl = ParseBool(parts[3]);
                bool pl = ParseBool(parts[4]);
                int star = int.Parse(parts[5]);
                
                bool storeCustom = false;
                string customList = string.Empty;
                if (parts.Length > 6)
                {
                    storeCustom = ParseBool(parts[6]);
                    customList = parts.Length > 7 ? parts[7] : "";
                }
                
                ModBootstrap.AutoStore?.ApplySettingsFromPanel(autoStore, kh, tl, pl, star, storeCustom, customList);
                break;
            }
            case "SELL_SETTING":
            {
                // Format: SELL_SETTING|enabled|emptySlots|keepStar|keepGod|keepSkh|sellMaxLevel|keepIds|forceSellIds|dropInsteadOfSell
                if (parts.Length < 2) return;

                bool enabled         = ParseBool(parts[1]);
                int  emptySlots      = parts.Length > 2 && int.TryParse(parts[2], out int es) ? es : 0;
                bool keepStar        = parts.Length > 3 ? ParseBool(parts[3]) : true;
                bool keepGod         = parts.Length > 4 ? ParseBool(parts[4]) : true;
                bool keepSkh         = parts.Length > 5 ? ParseBool(parts[5]) : true;
                int  sellMaxLevel    = parts.Length > 6 && int.TryParse(parts[6], out int ml) ? ml : 10;
                string keepIds       = parts.Length > 7 ? parts[7] : "";
                string forceSellIds  = parts.Length > 8 ? parts[8] : "";
                bool dropInsteadOfSell = parts.Length > 9 ? ParseBool(parts[9]) : false;

                ModBootstrap.AutoSell?.ApplySettingsFromPanel(
                    enabled, emptySlots, keepStar, keepGod, keepSkh,
                    sellMaxLevel, keepIds, forceSellIds, dropInsteadOfSell);
                break;
            }
            case "USE_ITEM_SETTING":
            {
                // Format: USE_ITEM_SETTING|cuongNo|boHuyet|boKhi|giapXen|mask|clover|food|detector|itemByIds
                if (parts.Length < 9) return;
                bool cuongNo  = ParseBool(parts[1]);
                bool boHuyet  = ParseBool(parts[2]);
                bool boKhi    = ParseBool(parts[3]);
                bool giapXen  = ParseBool(parts[4]);
                bool mask     = ParseBool(parts[5]);
                bool clover   = ParseBool(parts[6]);
                bool food     = ParseBool(parts[7]);
                bool detector = ParseBool(parts[8]);
                string itemByIds = parts.Length > 9 ? parts[9] : "";
                ModBootstrap.ItemFeature?.ApplyUseSettingsFromPanel(cuongNo, boHuyet, boKhi, giapXen, mask, clover, food, detector, itemByIds);
                break;
            }
            case "PICK_SETTING":
            {
                // Format: PICK_SETTING|autoPick|pickMode|onlyMyItems|pickDistance|whiteList|blackList
                if (parts.Length < 5) return;
                bool autoPick = ParseBool(parts[1]);
                int pickMode = 0;
                if (parts.Length > 2 && int.TryParse(parts[2], out int pm)) pickMode = pm;
                bool onlyMyItems = parts.Length > 3 && ParseBool(parts[3]);
                int pickDistance = 50;
                if (parts.Length > 4 && int.TryParse(parts[4], out int pd)) pickDistance = pd;
                string whiteList = parts.Length > 5 ? parts[5] : "";
                string blackList = parts.Length > 6 ? parts[6] : "";

                ModBootstrap.AutoPickFeature?.ApplySettingsFromPanel(
                    autoPick, pickMode, onlyMyItems, pickDistance, whiteList, blackList);
                break;
            }
            case "DAUTHAN_REQUEST":
            {
                if (parts.Length < 4) return;
                bool autoReq = ParseBool(parts[1]);
                bool reqCond = ParseBool(parts[2]);
                if (!int.TryParse(parts[3], out int reqUnder)) reqUnder = 0;
                ModBootstrap.AutoDauThanFeature?.ApplyRequestSettingsFromPanel(autoReq, reqCond, reqUnder);
                break;
            }
            case "DAUTHAN_DONATE":
            {
                if (parts.Length < 4) return;
                bool autoDon = ParseBool(parts[1]);
                bool donFilter = ParseBool(parts[2]);
                string donNames = parts[3].Replace(",", "\n");
                ModBootstrap.AutoDauThanFeature?.ApplyDonateSettingsFromPanel(autoDon, donFilter, donNames);
                break;
            }
            case "DAUTHAN_BUFF":
            {
                if (parts.Length < 7) return;
                bool bMaster = ParseBool(parts[1]);
                if (!int.TryParse(parts[2], out int mHp)) mHp = 10;
                if (!int.TryParse(parts[3], out int mKi)) mKi = 10;
                bool bPet = ParseBool(parts[4]);
                if (!int.TryParse(parts[5], out int pHp)) pHp = 10;
                if (!int.TryParse(parts[6], out int pKi)) pKi = 10;
                ModBootstrap.AutoDauThanFeature?.ApplyBuffSettingsFromPanel(bMaster, mHp, mKi, bPet, pHp, pKi);
                break;
            }
            case "DROP_SETTING":
            {
                // Format: DROP_SETTING|dropTrash|dropCustom|idsRaw
                if (parts.Length < 3) return;
                bool dropTrash  = ParseBool(parts[1]);
                bool dropCustom = ParseBool(parts[2]);
                // idsRaw dÃ¹ng \n encode thÃ nh \\n Ä‘á»ƒ trÃ¡nh xung Ä‘á»™t vá»›i line separator cá»§a socket
                string idsRaw = parts.Length > 3 ? parts[3].Replace("\\n", "\n") : "";
                ModBootstrap.ItemFeature?.ApplyDropSettingsFromPanel(dropTrash, dropCustom, idsRaw);
                break;
            }
            // Support feature removed
            case "SUPPORT_SETTING":
            {
                // Format: SUPPORT_SETTING|bongTaiState|petAction|autoCoDen|disableCoDenIfOthers|flagType
                if (parts.Length < 5) return;
                if (!int.TryParse(parts[1], out int bongTaiState)) bongTaiState = 0;
                if (!int.TryParse(parts[2], out int petAction))    petAction    = 3;
                bool autoCoDen       = ParseBool(parts[3]);
                bool disableIfOthers = ParseBool(parts[4]);
                int flagType = 8;
                if (parts.Length > 5 && int.TryParse(parts[5], out int fType)) flagType = fType;
                ModBootstrap.PotaraFeature?.ApplySettingsFromPanel(bongTaiState, petAction);
                ModBootstrap.CoDenFeature?.ApplySettingsFromPanel(autoCoDen, disableIfOthers, flagType);
                break;
            }
            case "AUTO_POINT_SETTING":
            {
                // Format: AUTO_POINT_SETTING|addHP|targetHP|addMP|targetMP|addDamage|targetDamage
                if (parts.Length < 7) return;
                bool addHP = ParseBool(parts[1]);
                if (!int.TryParse(parts[2], out int targetHP)) targetHP = 1000;
                bool addMP = ParseBool(parts[3]);
                if (!int.TryParse(parts[4], out int targetMP)) targetMP = 500;
                bool addDamage = ParseBool(parts[5]);
                if (!int.TryParse(parts[6], out int targetDamage)) targetDamage = 500;
                ModBootstrap.AutoPointFeature?.ApplySettingsFromPanel(addHP, targetHP, addMP, targetMP, addDamage, targetDamage);
                break;
            }
            case "AUTO_AMULET_SETTING":
            {
                // Format: AUTO_AMULET_SETTING|enabled|durationMode|wisdom|strong|buffaloSkin|heroic|immortal|enduring|magnet|disciple|wisdomX3|wisdomX4
                if (parts.Length < 13) return;
                bool enabled = ParseBool(parts[1]);
                if (!int.TryParse(parts[2], out int durationMode)) durationMode = 0;
                bool wisdom = ParseBool(parts[3]);
                bool strong = ParseBool(parts[4]);
                bool buffaloSkin = ParseBool(parts[5]);
                bool heroic = ParseBool(parts[6]);
                bool immortal = ParseBool(parts[7]);
                bool enduring = ParseBool(parts[8]);
                bool magnet = ParseBool(parts[9]);
                bool disciple = ParseBool(parts[10]);
                bool wisdomX3 = ParseBool(parts[11]);
                bool wisdomX4 = ParseBool(parts[12]);
                ModBootstrap.AutoAmuletFeature?.ApplySettingsFromPanel(enabled, durationMode, wisdom, strong, buffaloSkin, heroic, immortal, enduring, magnet, disciple, wisdomX3, wisdomX4);
                break;
            }
            case "BUY_SETTING":
            {
                // Format: BUY_SETTING|autoBuyTdlt|autoBuyKhauTrang|buyKhauTrangQty|autoBuyCoBonLa|buyCoBonLaQty|autoBuyBuaDe|buyBuaDeQty|autoBuyPrivateTicket
                if (parts.Length < 9) return;
                bool autoBuyTdlt = ParseBool(parts[1]);
                bool autoBuyKhauTrang = ParseBool(parts[2]);
                if (!int.TryParse(parts[3], out int buyKhauTrangQty)) buyKhauTrangQty = 0;
                bool autoBuyCoBonLa = ParseBool(parts[4]);
                if (!int.TryParse(parts[5], out int buyCoBonLaQty)) buyCoBonLaQty = 0;
                bool autoBuyBuaDe = ParseBool(parts[6]);
                if (!int.TryParse(parts[7], out int buyBuaDeQty)) buyBuaDeQty = 0;
                bool autoBuyPrivateTicket = ParseBool(parts[8]);

                ModBootstrap.AutoBuyFeature?.ApplySettingsFromPanel(
                    autoBuyTdlt, autoBuyKhauTrang, buyKhauTrangQty,
                    autoBuyCoBonLa, buyCoBonLaQty,
                    autoBuyBuaDe, buyBuaDeQty,
                    autoBuyPrivateTicket);
                break;
            }
            case "BOSS_SETTING":
            {
                // Format: BOSS_SETTING|enabled|goAttack|goTie|autoScout|scoutVip|limitMap|mapRanges|limitZone|zoneRanges|bossNames|enableAntiBan|antiAdminSec|chatContents|tdlt|cuongNo|boHuyet|giapXen|anDanh|co4La|thucAn|antiBanAttackMobs
                if (parts.Length < 2) return;
                bool enabled = ParseBool(parts[1]);
                bool goAttack          = parts.Length > 2  ? ParseBool(parts[2]) : false;
                bool goTie             = parts.Length > 3  ? ParseBool(parts[3]) : false;
                bool autoScout         = parts.Length > 4  ? ParseBool(parts[4]) : false;
                bool scoutVip          = parts.Length > 5  ? ParseBool(parts[5]) : false;
                bool limitMap          = parts.Length > 6  ? ParseBool(parts[6]) : false;
                string mapRanges       = parts.Length > 7  ? parts[7] : "";
                bool limitZone         = parts.Length > 8  ? ParseBool(parts[8]) : false;
                string zoneRanges      = parts.Length > 9  ? parts[9] : "";
                string bossNames       = parts.Length > 10 ? parts[10] : "";
                bool enableAntiBan     = parts.Length > 11 ? ParseBool(parts[11]) : false;
                int antiAdminSec       = parts.Length > 12 && int.TryParse(parts[12], out int s) ? s : 30;
                string chatContents    = parts.Length > 13 ? parts[13].Replace("~", "\n") : "";
                // Item settings (index 14-20)
                bool itemTdlt          = parts.Length > 14 ? ParseBool(parts[14]) : false;
                bool itemCuongNo       = parts.Length > 15 ? ParseBool(parts[15]) : false;
                bool itemBoHuyet       = parts.Length > 16 ? ParseBool(parts[16]) : false;
                bool itemGiapXen       = parts.Length > 17 ? ParseBool(parts[17]) : false;
                bool itemAnDanh        = parts.Length > 18 ? ParseBool(parts[18]) : false;
                bool itemCo4La         = parts.Length > 19 ? ParseBool(parts[19]) : false;
                bool itemThucAn        = parts.Length > 20 ? ParseBool(parts[20]) : false;
                bool antiBanAttackMobs = parts.Length > 21 ? ParseBool(parts[21]) : false;
                string allowedSkillsStr = parts.Length > 22 ? parts[22] : "NONE";
                bool useShieldUnderHp  = parts.Length > 23 ? ParseBool(parts[23]) : false;
                int shieldHpPercent    = parts.Length > 24 && int.TryParse(parts[24], out int sp) ? sp : 30;
                bool limitHpAbove      = parts.Length > 25 ? ParseBool(parts[25]) : false;
                long hpAboveValue      = parts.Length > 26 && long.TryParse(parts[26], out long v1) ? v1 : 0;
                bool limitHpBelow      = parts.Length > 27 ? ParseBool(parts[27]) : false;
                long hpBelowValue      = parts.Length > 28 && long.TryParse(parts[28], out long v2) ? v2 : 0;
                bool enableFinishing   = parts.Length > 29 ? ParseBool(parts[29]) : false;
                long finishingHp       = parts.Length > 30 && long.TryParse(parts[30], out long v3) ? v3 : 0;
                bool enableTimeSchedule = parts.Length > 31 ? ParseBool(parts[31]) : false;
                string timeSchedules    = parts.Length > 32 ? parts[32] : "";
                bool unequipTrainingArmor = parts.Length > 33 ? ParseBool(parts[33]) : false;
                int bossCtId = parts.Length > 34 ? (int.TryParse(parts[34], out int ct) ? ct : -1) : -1;
                int bossVpdlId = parts.Length > 35 ? (int.TryParse(parts[35], out int vpdl) ? vpdl : -1) : -1;
                int bossPetId = parts.Length > 36 ? (int.TryParse(parts[36], out int pet) ? pet : -1) : -1;

                ModBootstrap.AutoBossFeature?.ApplySettingsFromPanel(
                    enabled, goAttack, goTie, autoScout, scoutVip,
                    limitMap, mapRanges, limitZone, zoneRanges,
                    bossNames, enableAntiBan, antiAdminSec, chatContents,
                    itemTdlt, itemCuongNo, itemBoHuyet, itemGiapXen, itemAnDanh, itemCo4La, itemThucAn, antiBanAttackMobs,
                    allowedSkillsStr, useShieldUnderHp, shieldHpPercent,
                    limitHpAbove, hpAboveValue, limitHpBelow, hpBelowValue,
                    enableFinishing, finishingHp, enableTimeSchedule, timeSchedules, unequipTrainingArmor,
                    bossCtId, bossVpdlId, bossPetId);
                break;
            }
            case "BOSS_HUNT_START":
            {
                // Format: BOSS_HUNT_START|serverId|mapId|zoneId|bossName
                if (parts.Length < 5) return;
                if (!int.TryParse(parts[1], out int serverId) || serverId != AutoLogin.server) return; // Chá»‰ nháº­n lá»‡nh Ä‘Ãºng server
                if (!int.TryParse(parts[2], out int mapId)) return;
                if (!int.TryParse(parts[3], out int zoneId)) return;
                string bossName = parts[4];
                ModBootstrap.AutoBossFeature?.StartHuntingAt(mapId, zoneId, bossName);
                break;
            }
            case "BOSS_ANTIADMIN_START":
            {
                // Format: BOSS_ANTIADMIN_START|serverId|durationSeconds
                if (parts.Length < 3) return;
                if (!int.TryParse(parts[1], out int serverId) || serverId != AutoLogin.server) return;
                if (!int.TryParse(parts[2], out int durationSeconds)) durationSeconds = 60;
                ModBootstrap.AutoBossFeature?.StartAntiAdmin(durationSeconds);
                break;
            }
            case "BOSS_RESET":
            {
                // Format: BOSS_RESET|serverId
                if (parts.Length < 2) return;
                if (!int.TryParse(parts[1], out int serverId) || serverId != AutoLogin.server) return;
                ModBootstrap.AutoBossFeature?.ResetStateForNewHunt();
                break;
            }
            // (ToÃ n bá»™ logic xá»­ lÃ½ lá»‡nh BOSS_SETTING, BOSS_HUNT_START, v.v... Ä‘Ã£ Ä‘Æ°á»£c cáº­p nháº­t láº¡i)
            case "BUY_CUSTOM_SETTING":
            {
                if (parts.Length < 3) return;
                bool autoBuyCustom = ParseBool(parts[1]);
                string customList = parts[2];
                ModBootstrap.AutoBuyFeature?.ApplyCustomSettings(autoBuyCustom, customList);
                break;
            }
            case "PET":
            {
                if (parts.Length < 15) return;
                bool enableAutoPet = ParseBool(parts[1]);
                bool autoPemWhenPetCall = ParseBool(parts[2]);
                bool autoKOK = ParseBool(parts[3]);
                bool autoTTNL = ParseBool(parts[4]);
                if (!int.TryParse(parts[5], out int ttnlPercent)) ttnlPercent = 15;
                bool autoHealing = ParseBool(parts[6]);
                bool autoFocusPet = ParseBool(parts[7]);
                bool autoGobackMap = ParseBool(parts[8]);
                if (!int.TryParse(parts[9], out int targetMapId)) targetMapId = -1;
                bool autoGobackZone = ParseBool(parts[10]);
                if (!int.TryParse(parts[11], out int targetZoneId)) targetZoneId = -1;
                bool autoGobackPosition = ParseBool(parts[12]);
                if (!int.TryParse(parts[13], out int targetX)) targetX = -1;
                if (!int.TryParse(parts[14], out int targetY)) targetY = -1;
                bool autoStopAtPower = false;
                long targetPower = 150000000;
                if (parts.Length >= 17) {
                    autoStopAtPower = ParseBool(parts[15]);
                    if (!long.TryParse(parts[16], out targetPower)) targetPower = 150000000;
                }
                bool autoJump = parts.Length >= 19 && ParseBool(parts[17]);
                bool autoUsePetBuff = parts.Length >= 19 && ParseBool(parts[18]);
                ModBootstrap.AutoPetFeature?.ApplySettingsFromPanel(enableAutoPet, autoPemWhenPetCall, autoKOK, autoTTNL, ttnlPercent, autoHealing, autoFocusPet, autoGobackMap, targetMapId, autoGobackZone, targetZoneId, autoGobackPosition, targetX, targetY, autoStopAtPower, targetPower, autoJump, autoUsePetBuff);
                break;
            }
            case "BUFF_NAMEK":
            {
                if (parts.Length < 9) return;
                bool bEnabled = ParseBool(parts[1]);
                if (!int.TryParse(parts[2], out int bMapId)) bMapId = -1;
                bool bRequireZone = ParseBool(parts[3]);
                if (!int.TryParse(parts[4], out int bZoneId)) bZoneId = 0;
                bool bRequirePosition = ParseBool(parts[5]);
                if (!int.TryParse(parts[6], out int bPosX)) bPosX = 0;
                if (!int.TryParse(parts[7], out int bPosY)) bPosY = 0;
                if (!int.TryParse(parts[8], out int bSkillId)) bSkillId = 7;
                int bTargetMode = parts.Length > 9 && int.TryParse(parts[9], out int btm) ? btm : 0;
                int bCondition = parts.Length > 10 && int.TryParse(parts[10], out int bc) ? bc : 0;
                int bHpThreshold = parts.Length > 11 && int.TryParse(parts[11], out int bhp) ? bhp : 50;
                int bRangeMode = parts.Length > 12 && int.TryParse(parts[12], out int brm) ? brm : 0;
                string bNames = parts.Length > 13 ? string.Join("|", parts, 13, parts.Length - 13) : "";
                ModBootstrap.AutoBuffNamekFeature?.ApplySettingsFromPanel(bEnabled, bMapId, bRequireZone, bZoneId, bRequirePosition, bPosX, bPosY, bSkillId, bTargetMode, bCondition, bHpThreshold, bRangeMode, bNames.Replace("\\n", "\n"));
                break;
            }
            case "REDUCE_POWER":
            {
                if (parts.Length < 6) return;
                bool rEnabled = ParseBool(parts[1]);
                if (!int.TryParse(parts[2], out int rMapId)) rMapId = -1;
                if (!int.TryParse(parts[3], out int rZoneId)) rZoneId = -1;
                if (!int.TryParse(parts[4], out int rPosX)) rPosX = -1;
                if (!int.TryParse(parts[5], out int rPosY)) rPosY = -1;
                int rProvokeMobCount = parts.Length > 6 && int.TryParse(parts[6], out int pc) ? pc : 1;
                int rDeadReportDelayMs = parts.Length > 7 && int.TryParse(parts[7], out int drdm) ? drdm : 1000;
                bool rAutoPunchBlackFlag = parts.Length > 8 && ParseBool(parts[8]);
                bool rUseHpPunch = parts.Length > 9 && ParseBool(parts[9]);
                int rPunchHpPercent = parts.Length > 10 && int.TryParse(parts[10], out int php) ? php : 10;
                bool rUseTdlt = parts.Length > 11 && ParseBool(parts[11]);
                ModBootstrap.AutoReducePowerFeature?.ApplySettingsFromPanel(rEnabled, rMapId, rZoneId, rPosX, rPosY, rProvokeMobCount, rDeadReportDelayMs, rAutoPunchBlackFlag, rUseHpPunch, rPunchHpPercent, rUseTdlt);
                break;
            }
            case "REDUCE_POWER_HELP":
            {
                if (parts.Length >= 8) {
                    if (int.TryParse(parts[2], out int hDeadAccountId) && int.TryParse(parts[3], out int hMapId) && int.TryParse(parts[4], out int hZoneId) && int.TryParse(parts[5], out int hX) && int.TryParse(parts[6], out int hY)) {
                        ModBootstrap.AutoBuffNamekFeature?.HandleReducePowerHelpRequest(parts[1], hDeadAccountId, hMapId, hZoneId, hX, hY, parts[7]);
                    }
                    break;
                }
                if (parts.Length >= 6) {
                    if (int.TryParse(parts[1], out int hMapId) && int.TryParse(parts[2], out int hZoneId) && int.TryParse(parts[3], out int hX) && int.TryParse(parts[4], out int hY)) {
                        ModBootstrap.AutoBuffNamekFeature?.HandleReducePowerHelpRequest("legacy", -1, hMapId, hZoneId, hX, hY, parts[5]);
                    }
                }
                break;
            }
            case "GET_POS":
            {
                if (Char.myCharz() != null) SendMessage($"SYS_POS|{AutoLogin.idClientSocket}|{TileMap.mapID}|{TileMap.zoneID}|{Char.myCharz().cx}|{Char.myCharz().cy}");
                break;
            }
            case "GET_INVENTORY":
            {
                if (parts.Length < 2) return;
                if (!int.TryParse(parts[1], out int invType)) invType = 0;
                SendInventory(invType);
                break;
            }
            case "PROXY_SETTING":
            {
                HasReceivedProxySetting = true;

                if (parts.Length < 3 || string.IsNullOrWhiteSpace(parts[2]))
                {
                    // Táº¯t proxy
                    Session_ME.HasProxyConfigured = false;
                    Session_ME.ProxyHost = "";
                    Session_ME.ProxyPort = 0;
                    Session_ME.ProxyUsername = "";
                    Session_ME.ProxyPassword = "";
                    Session_ME2.HasProxyConfigured = false;
                    Session_ME2.ProxyHost = "";
                    Session_ME2.ProxyPort = 0;
                    Session_ME2.ProxyUsername = "";
                    Session_ME2.ProxyPassword = "";
                    break;
                }
                if (!int.TryParse(parts[1], out int pType)) pType = 0;
                
                string proxyAddrRaw = parts[2].Trim();
                string pHost = "";
                int pPort = 0;
                string pUser = "";
                string pPass = "";

                // Parse user:pass@host:port
                if (proxyAddrRaw.Contains("@"))
                {
                    string[] authAddr = proxyAddrRaw.Split('@');
                    if (authAddr.Length == 2)
                    {
                        string[] auth = authAddr[0].Split(':');
                        if (auth.Length == 2)
                        {
                            pUser = auth[0];
                            pPass = auth[1];
                        }
                        string[] addr = authAddr[1].Split(':');
                        if (addr.Length == 2 && int.TryParse(addr[1], out pPort))
                        {
                            pHost = addr[0];
                        }
                    }
                }
                else
                {
                    string[] partsAddr = proxyAddrRaw.Split(':');
                    if (partsAddr.Length == 4)
                    {
                        // Parse host:port:user:pass
                        pHost = partsAddr[0].Trim();
                        if (int.TryParse(partsAddr[1], out pPort))
                        {
                            pUser = partsAddr[2].Trim();
                            pPass = partsAddr[3].Trim();
                        }
                    }
                    else if (partsAddr.Length == 2 && int.TryParse(partsAddr[1], out pPort))
                    {
                        // Parse host:port
                        pHost = partsAddr[0].Trim();
                    }
                }

                Session_ME.HasProxyConfigured = true;
                Session_ME.ProxyType = pType;
                Session_ME.ProxyHost = pHost;
                Session_ME.ProxyPort = pPort;
                Session_ME.ProxyUsername = pUser;
                Session_ME.ProxyPassword = pPass;

                Session_ME2.HasProxyConfigured = true;
                Session_ME2.ProxyType = pType;
                Session_ME2.ProxyHost = pHost;
                Session_ME2.ProxyPort = pPort;
                Session_ME2.ProxyUsername = pUser;
                Session_ME2.ProxyPassword = pPass;
                break;
            }
            case "GLOBAL_AUTO_ON":
            {
                ModManager.AutoMod.SetGlobalAutoEnabled(true);
                break;
            }
            case "GLOBAL_AUTO_OFF":
            {
                ModManager.AutoMod.SetGlobalAutoEnabled(false);
                break;
            }
            case "ACTION_ON_DEATH":
            {
                // Format: ACTION_ON_DEATH|value
                // 0=Vá» nhÃ , 1=Há»“i sinh Ngá»c (Vá» nhÃ  náº¿u háº¿t ngá»c), 2=Chá»
                if (parts.Length >= 2 && int.TryParse(parts[1], out int aod))
                    ModBootstrap.ActionOnDeath = aod;
                break;
            }
            case "UPZIN":
            {
                bool enabled = parts.Length > 1 && ParseBool(parts[1]);
                string prefix = parts.Length > 2 ? parts[2] : string.Empty;
                int targetClass = parts.Length > 3 && int.TryParse(parts[3], out int tc) ? tc : -1;
                ModBootstrap.UpZinFeature?.ApplySettingsFromPanel(enabled, prefix, targetClass);
                ModBootstrap.NewbieTaskFeature?.ApplySettingsFromPanel(enabled);
                break;
            }
            case "UPZIN_OFF":
            {
                ModBootstrap.UpZinFeature?.DisableFromPanel();
                ModBootstrap.NewbieTaskFeature?.DisableFromPanel();
                break;
            }
            case "UPZIN700K":
            {
                bool enabled = parts.Length > 1 && ParseBool(parts[1]);
                string prefix = parts.Length > 2 ? parts[2] : string.Empty;
                int targetClass = parts.Length > 3 && int.TryParse(parts[3], out int tc) ? tc : -1;
                ModBootstrap.UpZinTo700kFeature?.ApplySettingsFromPanel(enabled, prefix, targetClass);
                break;
            }
            case "UPZIN700K_OFF":
            {
                ModBootstrap.UpZinTo700kFeature?.DisableFromPanel();
                break;
            }
        }
    }

    private static bool ParseBool(string value)
    {
        if (string.IsNullOrEmpty(value)) return false;
        if (value == "1") return true;
        if (value == "0") return false;
        return bool.TryParse(value, out bool parsed) && parsed;
    }

    private static string EncodeText(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
    }

    private static string DecodeText(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;
        try
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(value));
        }
        catch
        {
            return value;
        }
    }

    public static void Disconnect(bool stopReconnect = false)
    {
        if (stopReconnect)
            _allowReconnect = false;

        lock (_connectLock)
        {
            _isConnected = false;
            try { _stream?.Close(); } catch { }
            try { _client?.Close(); } catch { }
            _stream = null;
            _client = null;
        }

        if (_allowReconnect)
            StartReconnectLoop();
    }

    private static bool TryConnectOnce()
    {
        lock (_connectLock)
        {
            if (_isConnected)
                return true;

            try
            {
                _client = new TcpClient();
                _client.Connect("127.0.0.1", 9999);
                _stream = _client.GetStream();
                _isConnected = true;

                byte[] hello = Encoding.UTF8.GetBytes($"HELLO|{AutoLogin.idClientSocket}\n");
                _stream.Write(hello, 0, hello.Length);
                _stream.Flush();

                _receiveThread = new Thread(ReceiveLoop)
                {
                    IsBackground = true,
                    Name = "SocketGameReceive"
                };
                _receiveThread.Start();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("SocketGame Connect Error: " + ex.Message);
                _isConnected = false;
                try { _stream?.Close(); } catch { }
                try { _client?.Close(); } catch { }
                _stream = null;
                _client = null;
                return false;
            }
        }
    }

    private static void StartReconnectLoop()
    {
        lock (_connectLock)
        {
            if (_isReconnecting || !_allowReconnect || _isConnected)
                return;

            _isReconnecting = true;
            _reconnectThread = new Thread(() =>
            {
                try
                {
                    while (_allowReconnect && !_isConnected)
                    {
                        if (AutoLogin.ParentProcessId > 0)
                        {
                            try
                            {
                                var p = System.Diagnostics.Process.GetProcessById(AutoLogin.ParentProcessId);
                                if (p.HasExited) Environment.Exit(0);
                            }
                            catch { Environment.Exit(0); }
                        }

                        if (TryConnectOnce())
                            break;

                        Thread.Sleep(ReconnectIntervalMs);
                    }
                }
                finally
                {
                    lock (_connectLock)
                    {
                        _isReconnecting = false;
                    }
                }
            })
            {
                IsBackground = true,
                Name = "SocketGameReconnect"
            };
            _reconnectThread.Start();
        }
    }
}

