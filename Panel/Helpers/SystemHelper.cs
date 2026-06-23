using System;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace Panel.Helpers;

public static class SystemHelper
{
    private static string? _cachedHwid;
    private static string? _cachedHddId;
    private static string? _cachedMbId;
    private static string? _cachedIpv4;


    /// <summary>
    /// HWID = MD5(ZFox- + HDD_Serial + - + MB_Serial).
    /// Bỏ CPU ProcessorId vì VPS hay fake.
    /// </summary>
    public static string GetHardwareId()
    {
        if (!string.IsNullOrEmpty(_cachedHwid)) return _cachedHwid;

        try
        {
            string hdd = GetRawHddSerial();
            string mb  = GetRawMbSerial();
            string combined = $"{StringShield.GetHwidPrefix()}{hdd}-{mb}";

            using MD5 md5 = MD5.Create();
            byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(combined));
            StringBuilder sb = new StringBuilder();
            foreach (byte b in hashBytes)
                sb.Append(b.ToString("X2"));

            _cachedHwid = sb.ToString();
            return _cachedHwid;
        }
        catch
        {
            return "UNKNOWN-HWID-" + Environment.MachineName;
        }
    }

    /// <summary>
    /// SHA256 của HDD serial — gửi riêng lên server để bind máy.
    /// </summary>
    public static string GetHddId()
    {
        if (!string.IsNullOrEmpty(_cachedHddId)) return _cachedHddId;
        _cachedHddId = Sha256Hash(GetRawHddSerial());
        return _cachedHddId;
    }

    /// <summary>
    /// SHA256 của Mainboard serial — gửi riêng lên server để bind máy.
    /// </summary>
    public static string GetMbId()
    {
        if (!string.IsNullOrEmpty(_cachedMbId)) return _cachedMbId;
        _cachedMbId = Sha256Hash(GetRawMbSerial());
        return _cachedMbId;
    }

    /// <summary>
    /// IPv4 nội bộ đầu tiên của máy (dùng để log, không bind).
    /// </summary>
    public static string GetLocalIpv4()
    {
        if (!string.IsNullOrEmpty(_cachedIpv4)) return _cachedIpv4;
        try
        {
            string hostName = Dns.GetHostName();
            IPAddress[] addresses = Dns.GetHostAddresses(hostName);
            _cachedIpv4 = addresses
                .FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork)
                ?.ToString() ?? "0.0.0.0";
        }
        catch
        {
            _cachedIpv4 = "0.0.0.0";
        }
        return _cachedIpv4;
    }

    // ───────────────────────────────────────────
    // Private helpers
    // ───────────────────────────────────────────

    public static string GetRawHddSerial()
    {
        // Tạm thời tắt check ổ cứng, trả về giá trị ảo
        return "NO-HDD-CHECK";
    }

    public static string GetRawMbSerial()
    {
        // Tạm thời tắt check Mainboard (OS), trả về giá trị ảo
        return "NO-MB-CHECK";
    }

    private static string Sha256Hash(string input)
    {
        using SHA256 sha = SHA256.Create();
        byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static string GetWmiValue(string className, string property)
    {
        try
        {
            using ManagementClass mc = new ManagementClass(className);
            foreach (ManagementObject mo in mc.GetInstances())
            {
                string val = mo.Properties[property].Value?.ToString();
                if (!string.IsNullOrEmpty(val))
                    return val.Trim();
            }
        }
        catch { }
        return "None";
    }
}
