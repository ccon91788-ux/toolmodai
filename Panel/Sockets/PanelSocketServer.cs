using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Panel.Sockets;

/// <summary>
/// TCP Socket Server nháº­n káº¿t ná»‘i tá»« nhiá»u game client (NRO_v247.exe).
/// Má»—i client Ä‘Äƒng kÃ½ báº±ng chuá»—i "HELLO|<accountId>".
/// </summary>
public class PanelSocketServer
{
    public const int Port = 9999;

    private TcpListener? _listener;
    private readonly Dictionary<int, TcpClient> _clients = new();
    private readonly object _lock = new();
    private bool _running;

    /// <summary>
    /// Callback khi client bÃ¡o tráº¡ng thÃ¡i:
    /// STATUS|<accountId>|<charName>|<mapId>|<zoneId>
    /// </summary>
    public event Action<int, string, string>? OnStatusReceived;

    /// <summary>
    /// Callback khi client gá»­i thÃ´ng tin nhÃ¢n váº­t:
    /// CHAR_INFO|<accountId>|<infoText>
    /// </summary>
    public event Action<int, string>? OnCharInfoReceived;

    /// <summary>
    /// Callback khi client gá»­i thÃ´ng tin sá»‘ lÆ°á»£ng item Ä‘áº·c biá»‡t:
    /// CHAR_ITEMS|<accountId>|<kilis>|<mvbt>|<mhbt>
    /// </summary>
    public event Action<int, int, int, int>? OnCharItemsReceived;

    /// <summary>
    /// Callback khi client gá»­i stats raw má»—i 5s:
    /// CHAR_STATS|<accountId>|<gold>|<power>
    /// </summary>
    public event Action<int, long, long, bool>? OnCharStatsReceived;

    /// <summary>
    /// Callback khi client gá»­i thÃ´ng tin Ä‘á»‡ tá»­:
    /// PET_INFO|<accountId>|<infoText>
    /// </summary>
    public event Action<int, string>? OnPetInfoReceived;

    /// <summary>
    /// Callback khi client gá»­i stats Ä‘á»‡ tá»­ má»—i 5s:
    /// PET_STATS|<accountId>|<power>|<potential>|<isAutoOn>
    /// </summary>
    public event Action<int, long, long, bool>? OnPetStatsReceived;

    /// <summary>
    /// SYS_POS|<accountId>|<mapId>|<zoneId>|<x>|<y>
    /// </summary>
    public event Action<int, int, int, int, int>? OnSysPosReceived;

    /// <summary>
    /// SKH_TIME|<accountId>|<timeString>
    /// </summary>
    public event Action<int, string>? OnSkhTimeReceived;

    /// <summary>
    /// SKH_DATA|<accountId>|<total>|<n1>|<v1>|<n2>|<v2>|<n3>|<v3>|<n4>|<v4>|<n5>|<v5>
    /// </summary>
    public event Action<int, int, string[], string[]>? OnSkhDataReceived;

    /// <summary>
    /// GAME_LOG|<accountId>|<type>|<msg>
    /// </summary>
    public event Action<int, string, string>? OnGameLogReceived;

    /// <summary>
    /// INVENTORY_DATA|<accountId>|<type>|...
    /// </summary>
    public event Action<int, int>? OnInventoryDataReceived;
    public event Action<int, int, int, int, int, int, long, long, long, string, string>? OnBuffNamekStateReceived;
    public event Action<int, bool, string, string, int, int, bool>? OnDailyQuestStatusReceived;
    public event Action<int, bool, string, string, string, string, int, int, bool, string>? OnAttendanceStatusReceived;

    // --- BUFF NAMEK SYNC EVENTS ---
    public event Action<int, long, long, bool, bool, int, int, int, int, long>? OnSyncStateReceived;
    public event Action<int, string, bool, string>? OnSyncAckReceived;
    public event Action<int, string, bool, long, long, string>? OnSyncResultReceived;
    public event Action<int, string, string, int, long>? OnSyncTargetEventReceived;

    // --- BOSS HUNT EVENTS ---
    public event Action<int, int, int, int, string>? OnBossFoundReceived;
    public event Action<int, int, int, int, string>? OnBossKilledReceived;
    public event Action<int, int, int>? OnBossDeadReceived;
    public event Action<int, int, int>? OnBossScoutDoneReceived;
    public event Action<int, int>? OnAntiAdminDoneReceived;

    /// <summary>
    /// LOGIN|accountId|statusText → client báo trạng thái login chi tiết (hiển thị vào Data In Game)
    /// </summary>
    public event Action<int, string>? OnLoginStatusReceived;

    /// <summary>
    /// BACK_TO_LOGIN|accountId → client báo đã bị disconnect khỏi game server, quay về màn hình login.
    /// Khác với OnClientConnectionChanged: socket Panel vẫn sống, chỉ là game session bị mất.
    /// </summary>
    public event Action<int>? OnBackToLoginReceived;

    /// <summary>
    /// REDUCE_POWER_DEAD|accountId|mapId|zoneId|x|y
    /// </summary>
    public event Action<int, int, int, int, int>? OnReducePowerDeadReceived;
    /// <summary>
    /// REDUCE_POWER_ALIVE|accountId|mapId|zoneId|x|y
    /// </summary>
    public event Action<int, int, int, int, int>? OnReducePowerAliveReceived;
    /// <summary>
    /// REDUCE_POWER_HELP_ACK|helperAccountId|deadAccountId|jobId|ok|castAtMs|reason
    /// </summary>
    public event Action<int, int, string, bool, long, string>? OnReducePowerHelpAckReceived;

    /// <summary>
    /// Callback khi client káº¿t ná»‘i/ngáº¯t káº¿t ná»‘i.
    /// isConnected = true/false.
    /// </summary>
    public event Action<int, bool>? OnClientConnectionChanged;

    public void StartServer()
    {
        if (_running) return;

        _running = true;
        _listener = new TcpListener(IPAddress.Loopback, Port);
        _listener.Start();

        Thread acceptThread = new Thread(AcceptLoop)
        {
            IsBackground = true,
            Name = "PanelSocketAccept"
        };
        acceptThread.Start();
    }

    public void StopServer()
    {
        _running = false;
        _listener?.Stop();

        lock (_lock)
        {
            foreach (var kv in _clients)
            {
                try { kv.Value.Close(); } catch { }
            }
            _clients.Clear();
        }
    }

    /// <summary>Gá»­i lá»‡nh tá»›i má»™t client theo accountId.</summary>
    public bool SendCommand(int accountId, string command)
    {
        TcpClient? client;
        lock (_lock)
        {
            _clients.TryGetValue(accountId, out client);
        }

        if (client == null || !client.Connected)
            return false;

        try
        {
            byte[] data = Encoding.UTF8.GetBytes(command + "\n");
            client.GetStream().Write(data, 0, data.Length);
            return true;
        }
        catch
        {
            RemoveClient(accountId);
            return false;
        }
    }

    /// <summary>Gá»­i lá»‡nh broadcast tá»›i táº¥t cáº£ client Ä‘ang káº¿t ná»‘i.</summary>
    public void Broadcast(string command)
    {
        List<int> ids;
        lock (_lock)
        {
            ids = new List<int>(_clients.Keys);
        }

        foreach (var id in ids)
        {
            SendCommand(id, command);
        }
    }

    /// <summary>Láº¥y danh sÃ¡ch cÃ¡c accountId Ä‘ang káº¿t ná»‘i.</summary>
    public List<int> GetConnectedAccountIds()
    {
        lock (_lock)
        {
            return new List<int>(_clients.Keys);
        }
    }

    /// <summary>Sá»‘ client Ä‘ang káº¿t ná»‘i.</summary>
    public int ConnectedCount
    {
        get
        {
            lock (_lock)
            {
                return _clients.Count;
            }
        }
    }

    private void AcceptLoop()
    {
        while (_running)
        {
            try
            {
                var client = _listener!.AcceptTcpClient();
                Thread clientThread = new Thread(() => HandleClient(client))
                {
                    IsBackground = true,
                    Name = "PanelSocketClient"
                };
                clientThread.Start();
            }
            catch
            {
                // listener stopped
            }
        }
    }

    private void HandleClient(TcpClient client)
    {
        int accountId = -1;
        try
        {
            using var stream = client.GetStream();
            using var reader = new StreamReader(stream, Encoding.UTF8);

            string? line;
            while (_running && (line = reader.ReadLine()) != null)
            {
                line = line.Trim();
                if (string.IsNullOrEmpty(line))
                    continue;

                if (line.StartsWith("HELLO|"))
                {
                    if (int.TryParse(line.Split('|')[1], out int id))
                    {
                        accountId = id;
                        lock (_lock)
                        {
                            _clients[accountId] = client;
                        }
                        OnClientConnectionChanged?.Invoke(accountId, true);
                    }
                }
                else if (line.StartsWith("STATUS|"))
                {
                    var parts = line.Split('|');
                    if (parts.Length >= 3 && int.TryParse(parts[1], out int sid))
                    {
                        string charName = parts[2];
                        string extra = parts.Length > 3 ? parts[3] : "";
                        OnStatusReceived?.Invoke(sid, charName, extra);
                    }
                }
                else if (line.StartsWith("BACK_TO_LOGIN|"))
                {
                    var parts = line.Split('|');
                    if (parts.Length >= 2 && int.TryParse(parts[1], out int sid))
                    {
                        OnBackToLoginReceived?.Invoke(sid);
                    }
                }
                else if (line.StartsWith("LOGIN|"))
                {
                    // Format: LOGIN|accountId|statusText
                    var parts = line.Split('|', 3);
                    if (parts.Length >= 3 && int.TryParse(parts[1], out int sid))
                    {
                        OnLoginStatusReceived?.Invoke(sid, parts[2]);
                    }
                }
                else if (line.StartsWith("CHAR_INFO|"))
                {
                    var parts = line.Split('|', 3);
                    if (parts.Length >= 3 && int.TryParse(parts[1], out int cid))
                    {
                        string infoText = parts[2].Replace("\\n", "\n");
                        OnCharInfoReceived?.Invoke(cid, infoText);
                    }
                }
                else if (line.StartsWith("CHAR_ITEMS|"))
                {
                    var parts = line.Split('|');
                    if (parts.Length >= 5 
                        && int.TryParse(parts[1], out int cid)
                        && int.TryParse(parts[2], out int kilis)
                        && int.TryParse(parts[3], out int mvbt)
                        && int.TryParse(parts[4], out int mhbt))
                    {
                        OnCharItemsReceived?.Invoke(cid, kilis, mvbt, mhbt);
                    }
                }
                else if (line.StartsWith("CHAR_STATS|"))
                {
                    var parts = line.Split('|');
                    if (parts.Length >= 4
                        && int.TryParse(parts[1], out int sid)
                        && long.TryParse(parts[2], out long gold)
                        && long.TryParse(parts[3], out long power))
                    {
                        bool autoOn = parts.Length >= 5 && parts[4] == "1";
                        OnCharStatsReceived?.Invoke(sid, gold, power, autoOn);
                    }
                }
                else if (line.StartsWith("PET_INFO|"))
                {
                    var parts = line.Split('|', 3);
                    if (parts.Length >= 3 && int.TryParse(parts[1], out int cid))
                    {
                        string infoText = parts[2].Replace("\\n", "\n");
                        OnPetInfoReceived?.Invoke(cid, infoText);
                    }
                }
                else if (line.StartsWith("PET_STATS|"))
                {
                    var parts = line.Split('|');
                    if (parts.Length >= 4
                        && int.TryParse(parts[1], out int sid)
                        && long.TryParse(parts[2], out long power)
                        && long.TryParse(parts[3], out long potential))
                    {
                        bool autoOn = parts.Length >= 5 && parts[4] == "1";
                        OnPetStatsReceived?.Invoke(sid, power, potential, autoOn);
                    }
                }
                else if (line.StartsWith("SYS_POS|"))
                {
                    var parts = line.Split('|');
                    if (parts.Length >= 6
                        && int.TryParse(parts[1], out int sid)
                        && int.TryParse(parts[2], out int mapId)
                        && int.TryParse(parts[3], out int zoneId)
                        && int.TryParse(parts[4], out int x)
                        && int.TryParse(parts[5], out int y))
                    {
                        OnSysPosReceived?.Invoke(sid, mapId, zoneId, x, y);
                    }
                }
                else if (line.StartsWith("SKH_TIME|"))
                {
                    var parts = line.Split('|');
                    if (parts.Length >= 3 && int.TryParse(parts[1], out int sid))
                    {
                        OnSkhTimeReceived?.Invoke(sid, parts[2]);
                    }
                }
                else if (line.StartsWith("SKH_DATA|"))
                {
                    var parts = line.Split('|');
                    if (parts.Length >= 13 && int.TryParse(parts[1], out int sid) && int.TryParse(parts[2], out int total))
                    {
                        string[] names = new string[] { parts[3], parts[5], parts[7], parts[9], parts[11] };
                        string[] vals = new string[] { parts[4], parts[6], parts[8], parts[10], parts[12] };
                        OnSkhDataReceived?.Invoke(sid, total, names, vals);
                    }
                }
                else if (line.StartsWith("GAME_LOG|"))
                {
                    var parts = line.Split('|');
                    if (parts.Length >= 4 && int.TryParse(parts[1], out int sid))
                    {
                        string type = parts[2];
                        string msg = parts[3];
                        OnGameLogReceived?.Invoke(sid, type, msg);
                    }
                }
                else if (line.StartsWith("DAILY_QUEST_STATUS|"))
                {
                    var parts = line.Split('|');
                    if (parts.Length >= 8
                        && int.TryParse(parts[1], out int sid)
                        && int.TryParse(parts[5], out int completed)
                        && int.TryParse(parts[6], out int canceled))
                    {
                        bool isRunning = parts[2] == "1";
                        string runMode = NormalizeIncomingText(DecodeBase64OrText(parts[3]));
                        string stateText = NormalizeIncomingText(DecodeBase64OrText(parts[4]));
                        bool finishedToday = parts[7] == "1";
                        OnDailyQuestStatusReceived?.Invoke(sid, isRunning, runMode, stateText, completed, canceled, finishedToday);
                    }
                }
                else if (line.StartsWith("ATTENDANCE_STATUS|"))
                {
                    var parts = line.Split('|');
                    if (parts.Length >= 11
                        && int.TryParse(parts[1], out int sid)
                        && int.TryParse(parts[7], out int onlineCount)
                        && int.TryParse(parts[8], out int nextSeconds))
                    {
                        bool enabled = parts[2] == "1";
                        string stateText = NormalizeIncomingText(DecodeBase64OrText(parts[3]));
                        string monthlyKey = NormalizeIncomingText(DecodeBase64OrText(parts[4]));
                        string continuousDate = NormalizeIncomingText(DecodeBase64OrText(parts[5]));
                        string onlineDate = NormalizeIncomingText(DecodeBase64OrText(parts[6]));
                        bool canClaim = parts[9] == "1";
                        string lastCheck = NormalizeIncomingText(DecodeBase64OrText(parts[10]));
                        OnAttendanceStatusReceived?.Invoke(sid, enabled, stateText, monthlyKey, continuousDate, onlineDate, onlineCount, nextSeconds, canClaim, lastCheck);
                    }
                }
                else if (line.StartsWith("INVENTORY_DATA|"))
                {
                    var parts = line.Split('|');
                    if (parts.Length >= 8 && int.TryParse(parts[1], out int sid) && int.TryParse(parts[2], out int type))
                    {
                        var data = new Panel.Models.InventoryData();
                        long.TryParse(parts[3], out long gold); data.Gold = gold;
                        long.TryParse(parts[4], out long gem); data.Gem = gem;
                        long.TryParse(parts[5], out long ruby); data.Ruby = ruby;
                        int.TryParse(parts[6], out int bagMax); data.BagMax = bagMax;
                        
                        string[] boxParts = parts[7].Split(';');
                        int.TryParse(boxParts[0], out int boxMax); data.BoxMax = boxMax;

                        Action<string> parseItem = (s) => {
                            if (string.IsNullOrEmpty(s)) return;
                            string[] t = s.Split(',');
                            if (t.Length >= 4) {
                                int.TryParse(t[0], out int id);
                                int.TryParse(t[2], out int qty);
                                int.TryParse(t[3], out int flags);
                                data.Items.Add(new Panel.Models.InventoryItem { Id = id, Name = t[1], Quantity = qty, VipFlags = flags });
                            }
                        };

                        if (boxParts.Length > 1) parseItem(boxParts[1]);
                        for (int i = 8; i < parts.Length; i++) parseItem(parts[i]);

                        Panel.Models.InventoryCacheManager.UpdateCache(sid, type, data);
                        OnInventoryDataReceived?.Invoke(sid, type);
                    }
                }
                else if (line.StartsWith("SYNC_STATE|"))
                {
                    var p = line.Split('|');
                    if (p.Length >= 11 && int.TryParse(p[1], out int sid) && long.TryParse(p[2], out long cdRem) && long.TryParse(p[3], out long cdTtl) && int.TryParse(p[6], out int map) && int.TryParse(p[7], out int zone) && int.TryParse(p[8], out int x) && int.TryParse(p[9], out int y) && long.TryParse(p[10], out long lastCast))
                    {
                        bool alive = p[4] == "1";
                        bool busy = p[5] == "1";
                        OnSyncStateReceived?.Invoke(sid, cdRem, cdTtl, alive, busy, map, zone, x, y, lastCast);
                    }
                }
                else if (line.StartsWith("SYNC_ACK|"))
                {
                    var p = line.Split('|');
                    if (p.Length >= 5 && int.TryParse(p[1], out int sid))
                    {
                        OnSyncAckReceived?.Invoke(sid, p[2], p[3] == "1", p[4]);
                    }
                }
                else if (line.StartsWith("SYNC_RESULT|"))
                {
                    var p = line.Split('|');
                    if (p.Length >= 7 && int.TryParse(p[1], out int sid) && long.TryParse(p[4], out long castAt) && long.TryParse(p[5], out long cd))
                    {
                        OnSyncResultReceived?.Invoke(sid, p[2], p[3] == "1", castAt, cd, p[6]);
                    }
                }
                else if (line.StartsWith("SYNC_TARGET_EVENT|"))
                {
                    var p = line.Split('|');
                    if (p.Length >= 6 && int.TryParse(p[1], out int sid) && int.TryParse(p[4], out int hpPct) && long.TryParse(p[5], out long seenAt))
                    {
                        OnSyncTargetEventReceived?.Invoke(sid, p[2], p[3], hpPct, seenAt);
                    }
                }
                else if (line.StartsWith("BUFF_NAMEK_STATE|"))
                {
                    var p = line.Split('|');
                    if (p.Length >= 12
                        && int.TryParse(p[1], out int sid)
                        && int.TryParse(p[2], out int mapId)
                        && int.TryParse(p[3], out int zoneId)
                        && int.TryParse(p[4], out int x)
                        && int.TryParse(p[5], out int y)
                        && int.TryParse(p[6], out int skillId)
                        && long.TryParse(p[7], out long cdTotalMs)
                        && long.TryParse(p[8], out long cdRemainMs)
                        && long.TryParse(p[9], out long lastCastMs))
                    {
                        string state = p[10];
                        string targetName = p[11];
                        OnBuffNamekStateReceived?.Invoke(
                            sid, mapId, zoneId, x, y, skillId, cdTotalMs, cdRemainMs, lastCastMs, state, targetName);
                    }
                }
                else if (line.StartsWith("BOSS_FOUND|"))
                {
                    var p = line.Split('|');
                    if (p.Length >= 6
                        && int.TryParse(p[1], out int sid)
                        && int.TryParse(p[2], out int serverId)
                        && int.TryParse(p[3], out int mapId)
                        && int.TryParse(p[4], out int zoneId))
                    {
                        OnBossFoundReceived?.Invoke(sid, serverId, mapId, zoneId, p[5]);
                    }
                }
                else if (line.StartsWith("BOSS_KILLED|"))
                {
                    var p = line.Split('|');
                    if (p.Length >= 6
                        && int.TryParse(p[1], out int sid)
                        && int.TryParse(p[2], out int serverId)
                        && int.TryParse(p[3], out int mapId)
                        && int.TryParse(p[4], out int zoneId))
                    {
                        OnBossKilledReceived?.Invoke(sid, serverId, mapId, zoneId, p[5]);
                    }
                }
                else if (line.StartsWith("BOSS_DEAD|"))
                {
                    var p = line.Split('|');
                    if (p.Length >= 4
                        && int.TryParse(p[1], out int sid)
                        && int.TryParse(p[2], out int serverId)
                        && int.TryParse(p[3], out int mapId))
                    {
                        OnBossDeadReceived?.Invoke(sid, serverId, mapId);
                    }
                }
                else if (line.StartsWith("BOSS_SCOUT_DONE|"))
                {
                    var p = line.Split('|');
                    if (p.Length >= 4
                        && int.TryParse(p[1], out int sid)
                        && int.TryParse(p[2], out int serverId)
                        && int.TryParse(p[3], out int mapId))
                    {
                        OnBossScoutDoneReceived?.Invoke(sid, serverId, mapId);
                    }
                }
                else if (line.StartsWith("ANTI_ADMIN_DONE|"))
                {
                    var p = line.Split('|');
                    if (p.Length >= 3 && int.TryParse(p[1], out int sid) && int.TryParse(p[2], out int serverId))
                    {
                        OnAntiAdminDoneReceived?.Invoke(sid, serverId);
                    }
                }
                else if (line.StartsWith("REDUCE_POWER_DEAD|"))
                {
                    var p = line.Split('|');
                    if (p.Length >= 6 
                        && int.TryParse(p[1], out int sid)
                        && int.TryParse(p[2], out int mapId)
                        && int.TryParse(p[3], out int zoneId)
                        && int.TryParse(p[4], out int x)
                        && int.TryParse(p[5], out int y))
                    {
                        OnReducePowerDeadReceived?.Invoke(sid, mapId, zoneId, x, y);
                    }
                }
                else if (line.StartsWith("REDUCE_POWER_ALIVE|"))
                {
                    var p = line.Split('|');
                    if (p.Length >= 6
                        && int.TryParse(p[1], out int sid)
                        && int.TryParse(p[2], out int mapId)
                        && int.TryParse(p[3], out int zoneId)
                        && int.TryParse(p[4], out int x)
                        && int.TryParse(p[5], out int y))
                    {
                        OnReducePowerAliveReceived?.Invoke(sid, mapId, zoneId, x, y);
                    }
                }
                else if (line.StartsWith("REDUCE_POWER_HELP_ACK|"))
                {
                    var p = line.Split('|');
                    bool ok = p.Length > 4 && (p[4] == "1" || p[4].Equals("true", StringComparison.OrdinalIgnoreCase));
                    if (p.Length >= 7
                        && int.TryParse(p[1], out int helperSid)
                        && int.TryParse(p[2], out int deadSid)
                        && long.TryParse(p[5], out long castAtMs))
                    {
                        string jobId = p[3];
                        string reason = p[6];
                        OnReducePowerHelpAckReceived?.Invoke(helperSid, deadSid, jobId, ok, castAtMs, reason);
                    }
                }
            }
        }
        catch
        {
            // client disconnected
        }
        finally
        {
            if (accountId >= 0)
            {
                RemoveClient(accountId);
                OnClientConnectionChanged?.Invoke(accountId, false);
            }
            try { client.Close(); } catch { }
        }
    }

    private void RemoveClient(int accountId)
    {
        lock (_lock)
        {
            _clients.Remove(accountId);
        }
    }

    private static string DecodeBase64OrText(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return string.Empty;
        }

        try
        {
            byte[] data = Convert.FromBase64String(raw);
            return Encoding.UTF8.GetString(data);
        }
        catch
        {
            return raw.Replace("\\n", "\n");
        }
    }

    private static string NormalizeIncomingText(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return string.Empty;
        }

        string text = raw.Replace("\\n", "\n");
        return TryFixMojibake(text);
    }

    private static string TryFixMojibake(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        if (!LooksLikeMojibake(text))
        {
            return text;
        }

        try
        {
            byte[] latinBytes = Encoding.GetEncoding("ISO-8859-1").GetBytes(text);
            string recovered = Encoding.UTF8.GetString(latinBytes);
            return LooksReadableVietnamese(recovered) ? recovered : text;
        }
        catch
        {
            return text;
        }
    }

    private static bool LooksLikeMojibake(string text)
    {
        return text.Contains("\u00C3", StringComparison.Ordinal)          // Ã
            || text.Contains("\u00E1\u00BB", StringComparison.Ordinal)   // á»
            || text.Contains("\u00C6", StringComparison.Ordinal)         // Æ
            || text.Contains("\u00C4", StringComparison.Ordinal)         // Ä
            || text.Contains("\uFFFD", StringComparison.Ordinal);        // �
    }

    private static bool LooksReadableVietnamese(string text)
    {
        return text.Contains("\u0110", StringComparison.Ordinal)          // Đ
            || text.Contains("\u0111", StringComparison.Ordinal)         // đ
            || text.Contains("\u0103", StringComparison.Ordinal)         // ă
            || text.Contains("\u00E2", StringComparison.Ordinal)         // â
            || text.Contains("\u00EA", StringComparison.Ordinal)         // ê
            || text.Contains("\u00F4", StringComparison.Ordinal)         // ô
            || text.Contains("\u01A1", StringComparison.Ordinal)         // ơ
            || text.Contains("\u01B0", StringComparison.Ordinal);        // ư
    }
}

