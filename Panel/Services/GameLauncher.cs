using Panel.Helpers;
using Panel.Models;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Panel.Services;

/// <summary>
/// Khởi động / tắt tiến trình game cho từng Account.
/// </summary>
public static class GameLauncher
{
    /// <summary>
    /// Tên file game exe (tìm ở cùng thư mục với Panel.exe).
    /// </summary>
    private const string GameExeName = "Soulmate.exe";

    /// <summary>
    /// Launch game với args chứa id|user|server|encryptedPass.
    /// Trả về ProcessId nếu thành công, -1 nếu thất bại.
    /// </summary>
    public static int LaunchGame(Account acc, bool hideWindow = false, string customTitle = "")
    {
        try
        {
            string gamePath = FindGameExe();
            if (!File.Exists(gamePath))
                throw new FileNotFoundException($"Không tìm thấy game exe: {gamePath}");

            var serverObj = Models.ServerInfo.All.FirstOrDefault(s => s.DisplayName == acc.Server) ?? Models.ServerInfo.All[0];
            int serverId = serverObj.Id;

            var config = ConfigManager.Load();
            string encryptedPassword = CryptoHelper.EncryptPassword(acc.Password);
            // Format args: idClientSocket|account|server|ip|port|encryptedPass|width|height|customTitle|targetFPS|parentPID
            string args = $"{acc.Id}|{acc.Username}|{serverId}|{serverObj.IP}|{serverObj.Port}|{encryptedPassword}|{config.WindowWidth}|{config.WindowHeight}|{customTitle}|{config.TargetFPS}|{Environment.ProcessId}";

            var psi = new ProcessStartInfo
            {
                FileName = gamePath,
                Arguments = $"\"{args}\"",
                UseShellExecute = false,
                WorkingDirectory = Path.GetDirectoryName(gamePath) ?? AppDomain.CurrentDomain.BaseDirectory
            };

            if (hideWindow)
            {
                psi.WindowStyle = ProcessWindowStyle.Hidden;
                psi.CreateNoWindow = true;
            }

            var process = Process.Start(psi);
            if (process != null)
            {
                acc.ProcessId = process.Id;
                return process.Id;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khởi động game:\n{ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        return -1;
    }

    /// <summary>Kill tiến trình game theo ProcessId.</summary>
    public static void KillGame(int processId)
    {
        if (processId <= 0) return;
        try
        {
            var proc = Process.GetProcessById(processId);
            proc.Kill();
        }
        catch { /* Process đã tắt – bỏ qua */ }
    }

    /// <summary>Kiểm tra process vẫn còn đang chạy.</summary>
    public static bool IsRunning(int processId)
    {
        if (processId <= 0) return false;
        try
        {
            var proc = Process.GetProcessById(processId);
            return !proc.HasExited;
        }
        catch { return false; }
    }

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern bool SetWindowText(IntPtr hwnd, string lpString);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT { public int Left, Top, Right, Bottom; }

    /// <summary>Tính kích thước chrome (title bar + border) của cửa sổ.</summary>
    private static (int chromeW, int chromeH) GetWindowChrome(IntPtr hWnd)
    {
        if (GetWindowRect(hWnd, out RECT wndRect) && GetClientRect(hWnd, out RECT cliRect))
        {
            int totalW = wndRect.Right - wndRect.Left;
            int totalH = wndRect.Bottom - wndRect.Top;
            int chromeW = totalW - (cliRect.Right - cliRect.Left);
            int chromeH = totalH - (cliRect.Bottom - cliRect.Top);
            return (chromeW, chromeH);
        }
        return (0, 0);
    }

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool IsWindowVisible(IntPtr hWnd);

    private const int SW_HIDE = 0;
    private const int SW_SHOW = 5;
    private const int SW_RESTORE = 9;

    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern int GetWindowTextLength(IntPtr hWnd);

    public static IntPtr GetAnyWindowHandle(int processId)
    {
        IntPtr found = IntPtr.Zero;
        EnumWindows((hWnd, lParam) =>
        {
            GetWindowThreadProcessId(hWnd, out int pid);
            if (pid == processId)
            {
                // Get top-level window that is most likely the main game window
                // Dòng này bắt thủ công cửa sổ của UID process, kể cả Hidden
                // Tránh bắt các cửa sổ rác (0 chiều dài text hoặc quá linh tinh)
                // Tuy nhiên unity game đôi khi không có Text lúc init, nên cứ lấy luôn
                found = hWnd;
                return false;
            }
            return true;
        }, IntPtr.Zero);
        return found;
    }

    private static readonly Dictionary<int, IntPtr> _processWindows = new();
    private static int _windowCounter = 0;

    /// <summary>Reset bộ đếm cửa sổ để sắp xếp lại từ đầu.</summary>
    public static void ResetWindowCounter()
    {
        _windowCounter = 0;
    }

    /// <summary>Sắp xếp cửa sổ tuần tự theo chiều ngang và dọc.</summary>
    public static void ArrangeGameWindow(int processId, int windowWidth, int windowHeight, bool onlyVisible = false)
    {
        if (processId <= 0) return;
        try
        {
            var proc = Process.GetProcessById(processId);
            IntPtr hWnd = GetAnyWindowHandle(processId);
            if (hWnd == IntPtr.Zero && _processWindows.ContainsKey(processId))
            {
                hWnd = _processWindows[processId];
            }

            if (hWnd != IntPtr.Zero)
            {
                if (onlyVisible && !IsWindowVisible(hWnd))
                {
                    return; // Bỏ qua nếu đánh dấu chỉ xếp tab đang hiện
                }

                var workArea = System.Windows.Forms.Screen.PrimaryScreen?.WorkingArea ?? new System.Drawing.Rectangle(0, 0, 1920, 1080);
                int cols = Math.Max(1, workArea.Width / windowWidth);
                
                int x = workArea.Left + (_windowCounter % cols) * windowWidth;
                int y = workArea.Top + (_windowCounter / cols) * windowHeight;
                
                // Tràn khỏi màn hình thì reset vị trí về đầu
                if (y + windowHeight > workArea.Bottom)
                {
                    _windowCounter = 0;
                    x = workArea.Left;
                    y = workArea.Top;
                }

                // Tính kích thước toàn bộ cửa sổ = clientSize + chrome (title bar + border)
                // Nếu không lấy được chrome thì dùng clientSize (fallback an toàn)
                var (chromeW, chromeH) = GetWindowChrome(hWnd);
                int totalW = windowWidth  + chromeW;
                int totalH = windowHeight + chromeH;

                MoveWindow(hWnd, x, y, totalW, totalH, true);
                _windowCounter++;
            }
        }
        catch { /* Bỏ qua nếu có lỗi process ID */ }
    }

    /// <summary>Ẩn cửa sổ game.</summary>
    public static void HideGame(int processId)
    {
        if (processId <= 0) return;
        try
        {
            var proc = Process.GetProcessById(processId);
            IntPtr hWnd = GetAnyWindowHandle(processId);
            
            if (hWnd != IntPtr.Zero)
            {
                _processWindows[processId] = hWnd;
            }
            else if (_processWindows.ContainsKey(processId))
            {
                hWnd = _processWindows[processId];
            }

            if (hWnd != IntPtr.Zero)
            {
                ShowWindow(hWnd, SW_HIDE);
            }
        }
        catch { /* Bỏ qua nếu lỗi */ }
    }

    /// <summary>Hiện cửa sổ game và đưa lên foreground.</summary>
    public static void ShowGame(int processId)
    {
        if (processId <= 0) return;
        try
        {
            IntPtr hWnd = IntPtr.Zero;
            if (_processWindows.ContainsKey(processId))
            {
                hWnd = _processWindows[processId];
            }
            else
            {
                var proc = Process.GetProcessById(processId);
                hWnd = GetAnyWindowHandle(processId);
                if (hWnd != IntPtr.Zero) _processWindows[processId] = hWnd;
            }

            if (hWnd != IntPtr.Zero)
            {
                ShowWindow(hWnd, SW_RESTORE);   // restore nếu bị minimize / ẩn
                SetForegroundWindow(hWnd);       // đưa lên trên foreground
            }
        }
        catch { /* Bỏ qua nếu lỗi */ }
    }

    /// <summary>Cập nhật lại tiêu đề (Window Title) cho game đang chạy.</summary>
    public static void UpdateWindowTitle(int processId, string newTitle)
    {
        if (processId <= 0) return;
        try
        {
            IntPtr hWnd = IntPtr.Zero;
            if (_processWindows.ContainsKey(processId))
            {
                hWnd = _processWindows[processId];
            }
            else
            {
                var proc = Process.GetProcessById(processId);
                hWnd = GetAnyWindowHandle(processId);
                if (hWnd != IntPtr.Zero) _processWindows[processId] = hWnd;
            }

            if (hWnd != IntPtr.Zero)
            {
                SetWindowText(hWnd, newTitle);
            }
        }
        catch { /* Bỏ qua nếu lỗi (ví dụ game đã thoát) */ }
    }


    private static string FindGameExe()
    {
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        return Path.Combine(baseDir, GameExeName);
    }
}

