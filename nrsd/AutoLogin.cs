using System;
using System.Security.Cryptography;
using System.Text;

namespace NRO_v247;

public class AutoLogin
{
    private static AutoLogin _instance;

    // === Credentials & Config ===
    public static string idClientSocket = "";
    public static string Account = "";
    public static string Password = "";
    public static int server = -1;
    public static string IP = "";
    public static int Port = 0;
    public static int LasterLogin;

    public static int WindowWidth = 260;
    public static int WindowHeight = 320;
    public static string CustomTitle = "";

    // === State ===
    public static bool IsEnabled;
    public static bool dataLoaded;
    public static bool isLoadingData;
    public bool isFirstLogin = true;

    // === Timing constants ===
    private const long UPDATE_INTERVAL    = 1000L;
    private const long LOGIN_TIMEOUT      = 45000L;
    private const long POST_CONNECT_DELAY = 2000L;
    private const long PRE_SELECT_DELAY   = 1500L;
    private const long LOGIN_TIMEOUT_BUFFER = 10000L;

    // === Timing state ===
    private long nextUpdateTime = 0;
    private long loginStartTime = 0;
    private bool hasPerformedLogin = false;
    private long serverScreenSinceTime = 0;
    private long connectedSinceTime = 0;
    
    private bool _hasSelectedServer = false;
    private bool _wasOnline = false;
    
    private static string _lastServerMessage = "";
    private static string _lastDisplayedMessage = "";

    private static byte[] _md5KeyCache;

    // === Singleton ===
    private AutoLogin() { }

    public static AutoLogin gI()
        => _instance ??= new AutoLogin();

    // Tương thích gọi cũ
    public static AutoLogin getInstance() => gI();

    // =========================================================
    // Update – gọi mỗi frame từ AutoMod
    // =========================================================
    public void Update()
    {
        long now = mSystem.currentTimeMillis();
        if (now < nextUpdateTime) return;
        nextUpdateTime = now + UPDATE_INTERVAL;

        bool currentlyOnline = IsLoginSuccess();
        if (currentlyOnline)
        {
            if (isFirstLogin)
            {
                isFirstLogin = false;
                string title = string.IsNullOrEmpty(CustomTitle) ? $"[{idClientSocket}] - {Char.myCharz()?.cName ?? Account}" : CustomTitle;
                GameWindow.SetTitle(title);
                Console.WriteLine("[AutoLogin] Login successful!");
                SocketGame.SendMessage($"LOG|{idClientSocket}|Đăng nhập thành công!");
            }
            _lastServerMessage = "";
            serverScreenSinceTime = 0;
            _lastDisplayedMessage = "";
            LoginScr.timeLogin = 0;

            _wasOnline = true;
            if (Mods.AutoMod.GlobalOverrideState != "")
            {
                Mods.AutoMod.GlobalOverrideState = "";
            }

            SocketGame.ReportStatus();
        }
        else 
        {
            if (_wasOnline)
            {
                _wasOnline = false;
                Console.WriteLine("[AutoLogin] Character logged out, notifying Panel...");
                SocketGame.SendMessage($"BACK_TO_LOGIN|{idClientSocket}");
                SocketGame.SendMessage($"LOG|{idClientSocket}|Nhân vật mất kết nối, đang theo dõi tự động...");
                isFirstLogin = true; // reset để báo thành công khi vào lại
            }

            ShowLoginStatus();
        }

        if (now < connectedSinceTime)
            return;

        connectedSinceTime = now + 1000;

        if (!IsEnabled || !HasValidCredentials() || !ServerListScreen.bigOk)
            return;

        if (Controller.isConnectOK)
            return;

        if (!currentlyOnline && IsInMaintenanceWindow())
        {
            if (Session_ME.gI().isConnected())
                Session_ME.gI().close(); // Bảo đảm đóng kết nối
            
            _lastServerMessage = "Nghỉ bảo trì hằng ngày (03:30-04:15). Bot đang ngủ đông...";
            serverScreenSinceTime = mSystem.currentTimeMillis() + 60000;
            return;
        }

        try
        {
            if (Session_ME.gI().isConnected() && !currentlyOnline)
            {
                if (!hasPerformedLogin)
                {
                    int serverIndex = server - 1;
                    if (ServerListScreen.nameServer != null && serverIndex >= 0 && serverIndex < ServerListScreen.nameServer.Length && ServerListScreen.ipSelect != serverIndex)
                    {
                        try
                        {
                            Rms.saveRMSInt("svselect", serverIndex);
                            ServerListScreen.ipSelect = serverIndex;
                            GameCanvas.serverScreen.selectServer();
                        }
                        catch { }
                        return;
                    }

                    GameCanvas.serverScreen.perform(7, null);
                    try
                    {
                        Rms.saveRMSString("acc", Account);
                        Rms.saveRMSString("pass", Password);
                        Rms.saveRMSInt("svselect", serverIndex);
                    }
                    catch { }

                    GameCanvas.serverScreen.switchToMe();
                    GameCanvas.serverScreen.perform(3, null);

                    hasPerformedLogin = true;
                    loginStartTime = mSystem.currentTimeMillis();
                }
                else if (hasPerformedLogin && loginStartTime > 0)
                {
                    CheckLoginTimeout();
                }
            }
            else if (!currentlyOnline)
            {
                if (!Session_ME.gI().isConnected() && !Session_ME.connecting)
                {
                    if (serverScreenSinceTime == 0)
                    {
                        serverScreenSinceTime = mSystem.currentTimeMillis() + 15000;
                    }
                    else if (mSystem.currentTimeMillis() > serverScreenSinceTime)
                    {
                        Console.WriteLine("[AutoLogin] Reconnecting to game server...");
                        serverScreenSinceTime = mSystem.currentTimeMillis() + 15000;
                        Session_ME.gI().close();
                        GameCanvas.connect();
                    }
                }
                
                hasPerformedLogin = false;
                loginStartTime = 0;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"[AutoLogin] Error: {e.Message}");
        }
    }

    // =========================================================
    // Kiểm tra timeout login
    // =========================================================
    private void CheckLoginTimeout()
    {
        try
        {
            if (ServerListScreen.isGetData && ServerListScreen.percent < 100)
            {
                loginStartTime = mSystem.currentTimeMillis(); // Reset timeout khi đang tải Data
                return;
            }

            long elapsed = mSystem.currentTimeMillis() - loginStartTime;

            // Chờ tối đa LOGIN_TIMEOUT (45s), bất kể màn hình đang đếm ngược bao lâu
            if (elapsed > LOGIN_TIMEOUT)
            {
                OnLoginTimeout();
            }
        }
        catch { }
    }

    private void OnLoginTimeout()
    {
        Console.WriteLine("[AutoLogin] Login timeout, resetting socket...");
        hasPerformedLogin = false;
        loginStartTime = 0;
        Session_ME.gI().close(); // Ép đóng socket để mở lại luồng kết nối mới như mod_cong_dong
    }

    private static void ShowLoginStatus()
    {
        long now = mSystem.currentTimeMillis();
        long elapsed = now - gI().loginStartTime;
        long remainMs = 0;

        string message = string.IsNullOrEmpty(_lastServerMessage) ? "" : _lastServerMessage;

        if (gI().loginStartTime > 0)
        {
            if (LoginScr.timeLogin > 0)
            {
                string suffix = $"Xin chờ: {LoginScr.timeLogin}s";
                message = string.IsNullOrEmpty(message) ? suffix : message + " - " + suffix;
            }
            else
            {
                message = string.IsNullOrEmpty(message) ? "Đang xử lý đăng nhập..." : message;
            }
        }
        else if (gI().serverScreenSinceTime > 0 && !Session_ME.gI().isConnected())
        {
            remainMs = gI().serverScreenSinceTime - now;
            int remainSec = (int)System.Math.Ceiling(remainMs / 1000.0);
            if (remainSec < 0) remainSec = 0;

            if (remainSec > 0)
            {
                string suffix = "Đợi kết nối: " + remainSec + "s";
                message = string.IsNullOrEmpty(message) ? suffix : message + " - " + suffix;
            }
            else if (string.IsNullOrEmpty(message))
            {
                message = "Đang kết nối...";
            }
        }

        Mods.AutoMod.GlobalOverrideState = message;

        if (!message.Equals(_lastDisplayedMessage))
        {
            _lastDisplayedMessage = message;
            // Xóa việc đè Dialog gốc của game để cho GameCanvas tự hiển thị chờ đẹp hơn.
            // Báo log chi tiết lên Panel để user tiện theo dõi tình trạng bots.
            SocketGame.SendMessage($"LOG|{idClientSocket}|{message}");
        }
    }

    public static void CaptureServerMessage(string fullText)
    {
        if (string.IsNullOrEmpty(fullText) || fullText == mResources.PLEASEWAIT || fullText == mResources.maychutathoacmatsong)
            return;

        string lowerText = fullText.ToLower();
        if (lowerText.Contains("sai tài khoản") || lowerText.Contains("sai mật khẩu") || lowerText.Contains("sai thông tin") || lowerText.Contains("khóa")) 
        {
            _lastServerMessage = fullText.Replace("\n", " ");
            Mods.AutoMod.GlobalOverrideState = _lastServerMessage;
            SocketGame.SendMessage($"LOG|{idClientSocket}|Lỗi Đăng Nhập: {_lastServerMessage}");
            if (IsEnabled) Toggle();
            return;
        }

        if (!fullText.Contains("Kết nối lại sau:") && !fullText.Contains("Đang kết nối và đăng nhập"))
        {
            _lastServerMessage = fullText.Replace("\n", " ");
            Mods.AutoMod.GlobalOverrideState = _lastServerMessage;
            if (IsEnabled && (lowerText.Contains("bảo trì") || lowerText.Contains("quá tải"))) 
            {
                // Nhận thông điệp bảo trì ngẫu nhiên, cho sleep kết nối lại (ServerScreen) 1 nhịp 15-30s
                gI().serverScreenSinceTime = mSystem.currentTimeMillis() + 30000;
                SocketGame.SendMessage($"LOG|{idClientSocket}|Sever ngắt: {_lastServerMessage}");
                if (Session_ME.gI().isConnected()) Session_ME.gI().close(); // Bắt buộc đóng TCP socket
            }
        }
    }

    // =========================================================
    // Helpers
    // =========================================================
    private static bool HasValidCredentials()
        => !string.IsNullOrEmpty(idClientSocket)
        && !string.IsNullOrEmpty(Account)
        && !string.IsNullOrEmpty(Password)
        && server > 0;

    private static bool IsValidServerIndex(int index)
        => ServerListScreen.nameServer != null
        && index >= 0
        && index < ServerListScreen.nameServer.Length;

    private static bool IsLoginSuccess()
    {
        try
        {
            if (GameCanvas.currentScreen == null
                || GameCanvas.currentScreen is ServerListScreen
                || GameCanvas.currentScreen is ServerScr
                || GameCanvas.currentScreen is LoginScr
                || !Session_ME.gI().isConnected())
                return false;

            return true; // Khẳng định đang trong game (GameScr, TransportScr, CreateCharScr, vv)
        }
        catch { return false; }
    }

    private static bool IsInMaintenanceWindow()
    {
        var timeOfDay = DateTime.Now.TimeOfDay;
        var start = new TimeSpan(3, 30, 0); // 3:30 AM
        var end = new TimeSpan(4, 15, 0);   // 4:15 AM
        return timeOfDay >= start && timeOfDay <= end;
    }

    // =========================================================
    // Public API
    // =========================================================
    public void Reset()
    {
        hasPerformedLogin = false;
        loginStartTime = 0;
        serverScreenSinceTime = 0;
        connectedSinceTime = 0;
        isFirstLogin = true;
        dataLoaded = false;
        _hasSelectedServer = false;
    }

    public static void Toggle()
    {
        IsEnabled = !IsEnabled;

        if (!IsEnabled)
        {
            var inst = gI();
            inst.hasPerformedLogin = false;
            inst.loginStartTime = 0;
            inst.serverScreenSinceTime = 0;
            inst.connectedSinceTime = 0;
            inst._hasSelectedServer = false;
        }

        GameScr.info1?.addInfo($"Auto Login: {(IsEnabled ? "Bật" : "Tắt")}", 0);
    }

    public string GetStatus()
    {
        if (!IsEnabled) return "Tắt";
        if (IsLoginSuccess()) return "Đã đăng nhập";
        if (hasPerformedLogin) return "Đang đăng nhập...";
        if (!Session_ME.gI().isConnected()) return "Đang kết nối...";
        return "Chờ đăng nhập";
    }

    // =========================================================
    // Khởi tạo credentials từ command-line args (Panel gọi lúc launch)
    // =========================================================
    public static void InitLoginData()
    {
        try
        {
            string[] args = Environment.GetCommandLineArgs();
            // Format: NRO247Native.exe "idClientSocket|username|serverId|passwordBase64[|width|height]"
            if (args.Length < 2) return;

            string[] data = args[1].Split('|');
            if (data.Length < 6) return;

            idClientSocket = data[0];
            Account        = data[1];
            server         = int.Parse(data[2]);
            IP             = data[3];
            Port           = int.Parse(data[4]);
            Password       = DecryptString(data[5], "ud");

            if (data.Length >= 8)
            {
                if (int.TryParse(data[6], out int w)) WindowWidth  = w;
                if (int.TryParse(data[7], out int h)) WindowHeight = h;
            }

            if (data.Length >= 9)
            {
                CustomTitle = data[8];
            }

            if (IP == "server.ngocrongsaoden.com")
            {
                ServerListScreen.javaVN = "NRSD:server.ngocrongsaoden.com:14445:0:0:0,0,0";
                ServerListScreen.smartPhoneVN = "NRSD:server.ngocrongsaoden.com:14445:0:0:0,0,0";
                ServerListScreen.linkDefault = ServerListScreen.javaVN;
                ServerListScreen.getServerList(ServerListScreen.linkDefault);
            }

            if (HasValidCredentials())
            {
                IsEnabled = true;
                SocketGame.Connect();
                gI().connectedSinceTime = mSystem.currentTimeMillis() + 3000L;
                Console.WriteLine($"[AutoLogin] Initialized: {Account} @ server {server}");
            }
            else
            {
                IsEnabled = false;
                Console.WriteLine("[AutoLogin] Invalid credentials from args.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("InitLoginData Error: " + ex);
            IsEnabled = false;
        }
    }

    // =========================================================
    // Giải mã mật khẩu (3DES-ECB, key = MD5(key))
    // =========================================================
    public static string DecryptString(string str, string key)
    {
        try
        {
            byte[] encrypted = Convert.FromBase64String(str);
            byte[] keyHash   = _md5KeyCache ??= MD5.Create()
                .ComputeHash(Encoding.UTF8.GetBytes(key));

            using var des = new TripleDESCryptoServiceProvider
            {
                Key     = keyHash,
                Mode    = CipherMode.ECB,
                Padding = PaddingMode.PKCS7
            };

            return Encoding.UTF8.GetString(
                des.CreateDecryptor().TransformFinalBlock(encrypted, 0, encrypted.Length)
            );
        }
        catch { return string.Empty; }
    }
}
