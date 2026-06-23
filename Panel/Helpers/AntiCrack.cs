using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace Panel.Helpers
{
    public static class AntiCrack
    {
        [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true)]
        private static extern bool IsDebuggerPresent();

        [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true)]
        private static extern bool CheckRemoteDebuggerPresent(IntPtr hProcess, ref bool isDebuggerPresent);

        [DllImport("ntdll.dll", SetLastError = true)]
        private static extern int NtQueryInformationProcess(
            IntPtr processHandle,
            int processInformationClass,
            ref IntPtr processInformation,
            int processInformationLength,
            ref int returnLength);

        private static string _cachedExecutableHash = null;

        // Danh sách tên process debug phổ biến (tất cả lowercase để so sánh)
        private static readonly string[] _debugProcessNames = {
            "x64dbg", "x32dbg", "ollydbg",
            "ida", "ida64", "idag", "idag64", "idaw", "idaw64",
            "dnspy", "dotpeek", "ilspy",
            "cheatengine", "cheatengine-x86_64",
            "processhacker", "procmon", "procexp",
            "httpdebuggerui", "httpdebugger",
            "fiddler", "wireshark",
            "megadumper", "de4dot",
            "windbg", "dbgview"
        };

        /// <summary>
        /// Khởi động vòng lặp kiểm tra Debugger nâng cao.
        /// Chạy ngầm vô thời hạn, tốn rất ít CPU.
        /// </summary>
        public static void Initialize()
        {
            Task.Run(() => AntiDebugLoop());
        }

        private static void AntiDebugLoop()
        {
            int cycleCount = 0;

            while (true)
            {
                // === CHECK 1: Windows API IsDebuggerPresent ===
                bool isDebuggerPresent = false;
                try
                {
                    CheckRemoteDebuggerPresent(Process.GetCurrentProcess().Handle, ref isDebuggerPresent);
                }
                catch { }

                if (IsDebuggerPresent() || isDebuggerPresent || Debugger.IsAttached)
                {
                    Environment.Exit(0);
                }

                // === CHECK 2: NtQueryInformationProcess - ProcessDebugPort (0x7) ===
                try
                {
                    IntPtr debugPort = IntPtr.Zero;
                    int returnLength = 0;
                    int status = NtQueryInformationProcess(
                        Process.GetCurrentProcess().Handle,
                        7, // ProcessDebugPort
                        ref debugPort,
                        IntPtr.Size,
                        ref returnLength);

                    if (status == 0 && debugPort != IntPtr.Zero)
                    {
                        // Có debug port → debugger đang attach
                        Environment.Exit(0);
                    }
                }
                catch { }

                // === CHECK 3: Timing-based anti-debug (mỗi 5 chu kỳ) ===
                if (cycleCount % 5 == 0)
                {
                    try
                    {
                        var sw = Stopwatch.StartNew();
                        Thread.Sleep(100);
                        sw.Stop();

                        // Nếu sleep 100ms mà thực tế > 800ms → có debugger step-through
                        if (sw.ElapsedMilliseconds > 800)
                        {
                            Environment.Exit(0);
                        }
                    }
                    catch { }
                }

                // === CHECK 4: Quét tên process debug (mỗi 3 chu kỳ, giảm CPU) ===
                if (cycleCount % 3 == 0)
                {
                    try
                    {
                        Process[] processes = Process.GetProcesses();
                        foreach (Process proc in processes)
                        {
                            try
                            {
                                string name = proc.ProcessName.ToLowerInvariant();
                                foreach (string debugName in _debugProcessNames)
                                {
                                    if (name.Contains(debugName))
                                    {
                                        Environment.Exit(0);
                                    }
                                }
                            }
                            catch { }
                        }
                    }
                    catch { }
                }

                cycleCount++;
                Thread.Sleep(3000); // 3 giây quét 1 lần
            }
        }

        /// <summary>
        /// Tự băm mã SHA-256 của chính file thực thi (.exe) đang chạy.
        /// NativeAOT: Mã này sẽ luôn thay đổi mỗi lần Build dự án trên Visual Studio.
        /// Khách hàng dù đổi 1 byte (vd: string, jump code) cũng sẽ ra mã băm hoàn toàn mới.
        /// </summary>
        public static string GetExecutableHash()
        {
            if (_cachedExecutableHash != null)
                return _cachedExecutableHash;

            try
            {
                // Environment.ProcessPath lấy chính xác vị trí file exe đang chạy
                string exePath = Environment.ProcessPath;
                if (string.IsNullOrEmpty(exePath) || !File.Exists(exePath))
                    return "UNKNOWN_HASH";

                using (var sha256 = SHA256.Create())
                {
                    using (var stream = File.OpenRead(exePath))
                    {
                        byte[] hash = sha256.ComputeHash(stream);
                        _cachedExecutableHash = Convert.ToHexString(hash).ToLowerInvariant();
                        return _cachedExecutableHash;
                    }
                }
            }
            catch
            {
                return "ERROR_HASH";
            }
        }
    }
}
