using System;
using System.Runtime.InteropServices;

namespace Panel.Helpers;

public static class SystemResourceHelper
{
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool GetSystemTimes(out FILETIME lpIdleTime, out FILETIME lpKernelTime, out FILETIME lpUserTime);

    [StructLayout(LayoutKind.Sequential)]
    private struct FILETIME
    {
        public uint dwLowDateTime;
        public uint dwHighDateTime;
    }

    [DllImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX lpBuffer);

    [StructLayout(LayoutKind.Sequential)]
    private struct MEMORYSTATUSEX
    {
        public uint dwLength;
        public uint dwMemoryLoad;
        public ulong ullTotalPhys;
        public ulong ullAvailPhys;
        public ulong ullTotalPageFile;
        public ulong ullAvailPageFile;
        public ulong ullTotalVirtual;
        public ulong ullAvailVirtual;
        public ulong ullAvailExtendedVirtual;
    }

    [DllImport("kernel32.dll")]
    static extern IntPtr GetCurrentProcess();

    [DllImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool SetProcessWorkingSetSize(IntPtr hProcess, UIntPtr dwMinimumWorkingSetSize, UIntPtr dwMaximumWorkingSetSize);

    private static ulong _lastIdleTime;
    private static ulong _lastSystemTime;
    private static double _lastCpuUsage;

    private static ulong ToUInt64(FILETIME ft) => ((ulong)ft.dwHighDateTime << 32) | ft.dwLowDateTime;

    public static double GetCpuUsage()
    {
        if (GetSystemTimes(out var idle, out var kernel, out var user))
        {
            ulong currentIdleTime = ToUInt64(idle);
            ulong currentSystemTime = ToUInt64(kernel) + ToUInt64(user);

            if (_lastSystemTime > 0)
            {
                ulong systemTimeDelta = currentSystemTime - _lastSystemTime;
                ulong idleTimeDelta = currentIdleTime - _lastIdleTime;
                if (systemTimeDelta > 0)
                {
                    _lastCpuUsage = 100.0 - (idleTimeDelta * 100.0 / systemTimeDelta);
                }
            }

            _lastIdleTime = currentIdleTime;
            _lastSystemTime = currentSystemTime;
        }

        return _lastCpuUsage;
    }

    public static (double totalMb, double usedMb, double loadPercentage) GetRamUsage()
    {
        var memStatus = new MEMORYSTATUSEX { dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX)) };
        if (GlobalMemoryStatusEx(ref memStatus))
        {
            double totalMb = memStatus.ullTotalPhys / (1024.0 * 1024.0);
            double availMb = memStatus.ullAvailPhys / (1024.0 * 1024.0);
            return (totalMb, totalMb - availMb, memStatus.dwMemoryLoad);
        }
        return (0, 0, 0);
    }

    [DllImport("psapi.dll")]
    static extern int EmptyWorkingSet(IntPtr hwProc);

    public static void CleanOsMemory()
    {
        // Duyệt qua toàn bộ các tiến trình đang chạy trên toàn cục (OS)
        foreach (var proc in System.Diagnostics.Process.GetProcesses())
        {
            try
            {
                // Ép xả bộ nhớ làm việc ngay lập tức xuống Paging File
                EmptyWorkingSet(proc.Handle);
            }
            catch 
            { 
                // Bỏ qua các process hệ thống hoặc các process không đủ quyền truy cập (Access Denied)
            }
            finally
            {
                // Dispose C# wrapper object
                proc.Dispose();
            }
        }
    }
}
