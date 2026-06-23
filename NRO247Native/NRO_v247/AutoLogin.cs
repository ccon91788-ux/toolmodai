using System;
using System.Security.Cryptography;
using System.Text;
using Assets.src.g;

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
    public static int TargetFPS = 60;
    public static int ParentProcessId = -1;

    // === State ===
    public static bool IsEnabled;
    public static bool dataLoaded;
    public static bool isLoadingData;
    public bool isFirstLogin = true;

    // === Timing constants ===
    private const long UPDATE_INTERVAL     = 1000L;
    private const long LOGIN_TIMEOUT       = 45000L;
    private const long RECONNECT_DELAY     = 15000L;  // Chờ 15s giữa mỗi lần reconnect
    private const long POST_MAINTENANCE_DELAY = 30000L; // Chờ 30s sau bảo trì

    // === Timing state ===
    private long nextUpdateTime = 0;
    private long loginStartTime = 0;
    private bool hasPerformedLogin = false;
    private long serverScreenSinceTime = 0;
    
    private bool _wasOnline = false;
    private bool _hasNotifiedOffline = false;
    private bool _serverSelected = false; // Đã chọn đúng server chưa
    
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
    // Update – gọi mỗi frame từ GameCanvas.update()
    // =========================================================
    public void Update()
    {
        long now = mSystem.currentTimeMillis();
        if (now < nextUpdateTime) return;
        nextUpdateTime = now + UPDATE_INTERVAL;

        // Guard: Chờ game khởi tạo xong — không làm gì khi đang ở splash screen
        // (isLoading = true hoặc serverScreen chưa tồn tại)
        if (GameCanvas.isLoading || GameCanvas.serverScreen == null)
            return;

        bool currentlyOnline = IsLoginSuccess();

        // Màn hình tạo nhân vật: chỉ coi là "online đặc biệt" khi đang ở đúng màn hình CreateCharScr
        // FIX: Không dùng UpZinFeature.IsRequested vì nó = true ngay khi bật tính năng,
        // khiến AutoLogin bị block và game kẹt ở logo/splash khi chưa login.
        bool isCreatingChar = !currentlyOnline && (GameCanvas.currentScreen is CreateCharScr);

        // ===== PHẦN 1: XỬ LÝ KHI ONLINE HOẶC TẠO NHÂN VẬT =====
        if (currentlyOnline || isCreatingChar)
        {
            if (currentlyOnline && isFirstLogin)
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
            _hasNotifiedOffline = false; // Reset để khi mất kết nối sẽ gửi lại BACK_TO_LOGIN

            _wasOnline = true;
            if (Mods.AutoMod.GlobalOverrideState != "")
            {
                Mods.AutoMod.GlobalOverrideState = "";
            }

            SocketGame.ReportStatus();
            return; // Online rồi, không cần làm gì thêm
        }

        // ===== PHẦN 2: KHÔNG ONLINE — BÁO TRẠNG THÁI VỀ PANEL =====
        if (_wasOnline)
        {
            _wasOnline = false;
            _hasNotifiedOffline = false; // Đặt về false để luồng bên dưới gửi BACK_TO_LOGIN
            _serverSelected = false; // Reset lại khi bị disconnect
            hasPerformedLogin = false; // Reset để PerformAutoLogin gọi DoLogin() mới
            loginStartTime = 0;
            serverScreenSinceTime = mSystem.currentTimeMillis() + RECONNECT_DELAY; // Chờ 15s trước khi login lại
            Console.WriteLine("[AutoLogin] Character logged out, notifying Panel...");
            isFirstLogin = true;
        }

        if (!_hasNotifiedOffline)
        {
            _hasNotifiedOffline = true;
            SocketGame.SendMessage($"BACK_TO_LOGIN|{idClientSocket}");
            SocketGame.SendMessage($"LOG|{idClientSocket}|Đang theo dõi tự động...");
        }

        ShowLoginStatus();

        // ===== PHẦN 3: AUTO LOGIN LOGIC =====
        if (!IsEnabled || !HasValidCredentials())
            return;

        // Chặn race: chưa nhận PROXY_SETTING từ Panel thì chưa cho login.
        if (!SocketGame.HasReceivedProxySetting)
        {
            _lastServerMessage = "Chờ Panel đồng bộ PROXY_SETTING...";
            return;
        }

        // Nghỉ bảo trì
        if (IsInMaintenanceWindow())
        {
            if (Session_ME.gI().isConnected())
                Session_ME.gI().close();
            
            _lastServerMessage = "Nghỉ bảo trì hằng ngày (03:30-04:15). Bot đang ngủ đông...";
            serverScreenSinceTime = now + 60000;
            return;
        }

        // Nếu server list chưa load xong (bigOk = false)
        if (!ServerListScreen.bigOk)
        {
            // Trạng thái 1: Vẫn đang ở SplashScr — chờ Splash xong rồi game tự xử lý
            if (!(GameCanvas.currentScreen is ServerListScreen))
            {
                _lastServerMessage = "Đang khởi động game...";
                serverScreenSinceTime = 0;
                return;
            }

            // Trạng thái 2: Đang tải data (isGetData=true) — chờ download xong
            if (ServerListScreen.isGetData)
            {
                _lastServerMessage = ServerListScreen.percent > 0
                    ? $"Đang tải dữ liệu game: {ServerListScreen.percent}%"
                    : "Đang tải dữ liệu game...";
                serverScreenSinceTime = 0;
                return;
            }

            // Trạng thái 3: ServerListScreen đang hiển thị nhưng chưa bắt đầu tải — trigger
            try
            {
                GameCanvas.serverScreen.perform(2, null);
                Console.WriteLine("[AutoLogin] Triggered data download via serverScreen.perform(2)");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AutoLogin] perform(2) error: {ex.Message}");
            }
            _lastServerMessage = "Đang tải dữ liệu game lần đầu...";
            serverScreenSinceTime = 0;
            return;
        }

        try
        {
            PerformAutoLogin(now);
        }
        catch (Exception e)
        {
            Console.WriteLine($"[AutoLogin] Error: {e.Message}");
        }
    }

    // =========================================================
    // Logic auto login chính — chạy khi biết chắc bigOk=true
    // =========================================================
    private void PerformAutoLogin(long now)
    {
        // Dismiss dialog cũ block UI (vd: thông báo máy chủ mất kết nối)
        if (GameCanvas.currentDialog != null)
        {
            GameCanvas.currentDialog = null;
        }
        // Dismiss msg dialog nếu đang hiển thị
        try
        {
            if (GameCanvas.msgdlg != null && GameCanvas.currentDialog == GameCanvas.msgdlg)
                GameCanvas.endDlg();
        }
        catch { }

        if (!hasPerformedLogin)
        {
            // Chưa login lần nào (hoặc đã reset) → gọi Login_New()
            // Đợi timer nếu vừa bị disconnect
            if (serverScreenSinceTime > 0 && now < serverScreenSinceTime)
                return; // Chờ đủ delay rồi mới gọi

            DoLogin(now);
        }
        else if (loginStartTime > 0)
        {
            // Đã gọi Login_New(), đang chờ server response → check timeout
            CheckLoginTimeout();
        }
    }

    // =========================================================
    // Thực hiện login — gọi Service.gI().login() trực tiếp
    // Không dùng perform(7)/perform(3) nữa (gây double connect)
    // =========================================================
    private void DoLogin(long now)
    {
        // Bước 1: Fallback nếu ServerListScreen chưa load data (lần đầu tải game)
        int serverIndex = server - 1;
        bool hasData = ServerListScreen.nameServer != null;

        if (hasData && !IsValidServerIndex(serverIndex))
        {
            Console.WriteLine($"[AutoLogin] Invalid server index: {serverIndex}");
            return;
        }

        // Bước 2: Khởi tạo IP/Port và lưu RMS
        try
        {
            Rms.saveRMSString("acc", Account);
            Rms.saveRMSString("pass", Password);
            Rms.saveRMSInt("svselect", serverIndex);
            
            if (hasData)
            {
                ServerListScreen.ipSelect = serverIndex;
                GameMidlet.IP = ServerListScreen.address[serverIndex];
                GameMidlet.PORT = ServerListScreen.port[serverIndex];
                if (ServerListScreen.language != null && serverIndex < ServerListScreen.language.Length)
                    GameMidlet.LANGUAGE = ServerListScreen.language[serverIndex];
                LoginScr.serverName = ServerListScreen.nameServer[serverIndex];
                Console.WriteLine($"[AutoLogin] Calling Login_New() for server {serverIndex + 1} ({LoginScr.serverName})");
            }
            else
            {
                ServerListScreen.ipSelect = serverIndex >= 0 ? serverIndex : 0;
                GameMidlet.IP = IP;
                GameMidlet.PORT = Port;
                LoginScr.serverName = "Tải dữ liệu...";
                Console.WriteLine($"[AutoLogin] Data not loaded. Calling Login_New() with raw IP/Port: {IP}:{Port}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AutoLogin] Error setting server: {ex.Message}");
            return;
        }

        // Bước 3: Gọi Login_New() — đây là hàm chuẩn của game
        try
        {
            GameCanvas.serverScreen.Login_New();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AutoLogin] Login_New error: {ex.Message}");
            return;
        }

        hasPerformedLogin = true;
        loginStartTime = now;
        _serverSelected = true;
    }

    // =========================================================
    // Đảm bảo IP/PORT đúng trước khi connect
    // =========================================================
    private static void EnsureCorrectServer()
    {
        try
        {
            int serverIndex = server - 1;
            if (!IsValidServerIndex(serverIndex)) return;

            ServerListScreen.ipSelect = serverIndex;
            GameMidlet.IP = ServerListScreen.address[serverIndex];
            GameMidlet.PORT = ServerListScreen.port[serverIndex];
            if (ServerListScreen.language != null && serverIndex < ServerListScreen.language.Length)
            {
                GameMidlet.LANGUAGE = ServerListScreen.language[serverIndex];
            }
            LoginScr.serverName = ServerListScreen.nameServer[serverIndex];
            Rms.saveRMSInt("svselect", serverIndex);
        }
        catch { }
    }

    // =========================================================
    // Kiểm tra timeout login
    // =========================================================
    private void CheckLoginTimeout()
    {
        try
        {
            // Đang tải data → reset timeout
            if (ServerListScreen.isGetData && ServerListScreen.percent < 100)
            {
                loginStartTime = mSystem.currentTimeMillis();
                return;
            }

            // Server đang bắt chờ (countdown) → reset timeout
            if (LoginScr.timeLogin > 0)
            {
                loginStartTime = mSystem.currentTimeMillis();
                return;
            }

            long elapsed = mSystem.currentTimeMillis() - loginStartTime;

            if (elapsed > LOGIN_TIMEOUT)
            {
                OnLoginTimeout();
            }
        }
        catch { }
    }

    private void OnLoginTimeout()
    {
        long now = mSystem.currentTimeMillis();
        Console.WriteLine("[AutoLogin] Login timeout, will retry via Login_New() after delay...");
        hasPerformedLogin = false;
        loginStartTime = 0;
        _serverSelected = false;
        serverScreenSinceTime = now + RECONNECT_DELAY; // Đợi delay rồi gọi Login_New() lại
        try { Session_ME.gI().close(); } catch { }
    }

    // =========================================================
    // Báo trạng thái login lên Panel
    // FIX BUG 6: Gửi cả STATUS (không chỉ LOG)
    // =========================================================
    private static void ShowLoginStatus()
    {
        long now = mSystem.currentTimeMillis();

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
            long remainMs = gI().serverScreenSinceTime - now;
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

        // KHÔNG gửi STATUS ở đây vì zFox Panel mặc định coi STATUS là Online (Mã 1)
        // Dùng lệnh LOGIN và LOG để Panel đếm vào mục "Log" và hiện dữ liệu mà không bị "Online"
        string statusText = gI().GetStatus();
        SocketGame.SendMessage($"LOGIN|{idClientSocket}|3. LOGIN: {statusText}");

        if (!message.Equals(_lastDisplayedMessage))
        {
            _lastDisplayedMessage = message;
            SocketGame.SendMessage($"LOG|{idClientSocket}|{message}");
        }
    }

    public static void CaptureServerMessage(string fullText)
    {
        Mods.ModBootstrap.UpZinFeature?.OnServerMessage(fullText);

        if (string.IsNullOrEmpty(fullText) || fullText == mResources.PLEASEWAIT || fullText == mResources.maychutathoacmatsong)
            return;

        string lowerText = fullText.ToLower();

        if (!fullText.Contains("Kết nối lại sau:") && !fullText.Contains("Đang kết nối và đăng nhập"))
        {
            _lastServerMessage = fullText.Replace("\n", " ");
            Mods.AutoMod.GlobalOverrideState = _lastServerMessage;
            if (IsEnabled && (lowerText.Contains("bảo trì") || lowerText.Contains("quá tải"))) 
            {
                gI().serverScreenSinceTime = mSystem.currentTimeMillis() + POST_MAINTENANCE_DELAY;
                SocketGame.SendMessage($"LOG|{idClientSocket}|Sever ngắt: {_lastServerMessage}");
                if (Session_ME.gI().isConnected()) Session_ME.gI().close();
                gI().hasPerformedLogin = false;
                gI().loginStartTime = 0;
                gI()._serverSelected = false;
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
            bool isOnlineScreen = GameCanvas.currentScreen is GameScr 
                                || GameCanvas.currentScreen is TransportScr 
                                || GameCanvas.currentScreen is CrackBallScr 
                                || GameCanvas.currentScreen is RadarScr
                                || GameCanvas.currentScreen is ClientInput;

            if (!isOnlineScreen)
                return false;
            if (!Session_ME.gI().isConnected())
                return false;
            if (Char.myCharz() == null)
                return false;

            return true;
        }
        catch { return false; }
    }

    private static bool IsInMaintenanceWindow()
    {
        var timeOfDay = DateTime.Now.TimeOfDay;
        var start = new TimeSpan(3, 30, 0);
        var end = new TimeSpan(4, 15, 0);
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
        isFirstLogin = true;
        dataLoaded = false;
        _serverSelected = false;
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
            inst._serverSelected = false;
        }

        GameScr.info1?.addInfo($"Auto Login: {(IsEnabled ? "Bật" : "Tắt")}", 0);
    }

    public string GetStatus()
    {
        if (!IsEnabled) return "Tắt";
        if (IsLoginSuccess()) return "Đã đăng nhập";
        if (hasPerformedLogin) return "Đang đăng nhập...";
        if (Session_ME.connecting) return "Đang kết nối TCP...";
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

            if (data.Length >= 10)
            {
                if (int.TryParse(data[9], out int fps)) TargetFPS = fps > 0 ? fps : 60;
            }

            if (data.Length >= 11)
            {
                if (int.TryParse(data[10], out int pid)) ParentProcessId = pid;
            }

            if (HasValidCredentials())
            {
                IsEnabled = true;
                SocketGame.Connect();
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
