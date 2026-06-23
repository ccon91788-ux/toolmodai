using System;
using System.IO;
using LoadAssets;

namespace NRO_v247;

internal static class Program
{
    // Win8.1+ per-monitor DPI aware V2 (Windows 10 1703+)
    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool SetProcessDpiAwarenessContext(IntPtr value);

    // Vista fallback
    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool SetProcessDPIAware();

    // DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2 = -4 (as IntPtr)
    private static readonly IntPtr DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2 = new IntPtr(-4);

    private static void SetDpiAware()
    {
        try
        {
            // Thử Per-Monitor V2 trước (Win10 1703+): chính xác nhất, không bị Windows scale
            if (SetProcessDpiAwarenessContext(DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2))
                return;
        }
        catch { }

        try
        {
            // Fallback: Vista API
            if (Environment.OSVersion.Version.Major >= 6)
                SetProcessDPIAware();
        }
        catch { }
    }

    private static void StartParentWatchdog(int parentPid)
    {
        if (parentPid <= 0) return;

        var thread = new System.Threading.Thread(() =>
        {
            while (true)
            {
                try
                {
                    var p = System.Diagnostics.Process.GetProcessById(parentPid);
                    if (p.HasExited)
                    {
                        Console.WriteLine("Parent process exited. Closing client.");
                        Environment.Exit(0);
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("Parent process not found. Closing client.");
                    Environment.Exit(0);
                }
                System.Threading.Thread.Sleep(1000);
            }
        });
        thread.IsBackground = true;
        thread.Start();
    }

    [STAThread]
    private static void Main()
    {
        try
        {
            SetDpiAware();
            System.Net.ServicePointManager.Expect100Continue = true;
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12 | System.Net.SecurityProtocolType.Tls13;

            if (!File.Exists("assets.dat"))
                throw new FileNotFoundException("Thieu file assets.dat - Folder LoadAssets co san");

            AssetBundle.LoadBundle("assets.dat");
            AutoLogin.InitLoginData();
            StartParentWatchdog(AutoLogin.ParentProcessId);

            string mutexName = string.IsNullOrEmpty(AutoLogin.Account)
                ? "NRO_Client_" + Guid.NewGuid().ToString()
                : "NRO_Client_" + AutoLogin.Account;

            bool createdNew;
            using (var mutex = new System.Threading.Mutex(true, mutexName, out createdNew))
            {
                if (!createdNew)
                {
                    Console.WriteLine("Client cho account nay da dang chay. Tu dong thoat!");
                    return;
                }

                string title = string.IsNullOrEmpty(AutoLogin.CustomTitle) ? "NRO_v247" : AutoLogin.CustomTitle;
                using var window = new GameWindow(AutoLogin.WindowWidth, AutoLogin.WindowHeight, title);
                window.Run();
            }
        }
        catch (Exception ex)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"[{DateTime.Now}] FATAL ERROR REPORT:");
            sb.AppendLine($"Message: {ex.Message}");
            sb.AppendLine($"Source: {ex.Source}");
            sb.AppendLine($"Stack Trace: {ex.StackTrace}");

            if (ex.InnerException != null)
            {
                sb.AppendLine("\n---------------- INNER EXCEPTION ----------------");
                sb.AppendLine($"Inner Message: {ex.InnerException.Message}");
                sb.AppendLine($"Inner Source: {ex.InnerException.Source}");
                sb.AppendLine($"Inner Stack Trace: {ex.InnerException.StackTrace}");

                if (ex.InnerException.InnerException != null)
                {
                    sb.AppendLine("\n--- INNER EXCEPTION (Level 2) ---");
                    sb.AppendLine($"Msg: {ex.InnerException.InnerException.Message}");
                }
            }

            File.WriteAllText("crash_debug.txt", sb.ToString());
        }
    }
}
