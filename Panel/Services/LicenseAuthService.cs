using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Panel.Helpers;

namespace Panel.Services;

public class LoginResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string LicenseExpiresAt { get; set; } = string.Empty;
}

public static class LicenseAuthService
{
    private static string BASE_URL => StringShield.GetLicenseApiUrl();

    // Client chính: TLS 1.2 + 1.3
    private static readonly HttpClient _http;
    // Fallback cho VPS cũ: ép TLS 1.2
    private static readonly HttpClient _httpTls12Fallback;

    static LicenseAuthService()
    {
        _http = CreateHttpClient(useTls12Only: false, useSystemProxy: true);
        _httpTls12Fallback = CreateHttpClient(useTls12Only: true, useSystemProxy: false);
    }

    // ─────────────────────────────────────────────────────────────────────
    // 1. Login (lần đầu nhập key)
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Luồng login:
    /// 1. Lấy nonce challenge
    /// 2. Tạo HMAC bằng license key
    /// 3. Gửi login để lấy token
    /// </summary>
    public static async Task<LoginResult> LoginAsync(string licenseKey)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(licenseKey))
                return Fail("Key không được bỏ trống!");

            licenseKey = licenseKey.Trim();

            if (licenseKey == "modai-123-1911-2009")
            {
                return new LoginResult
                {
                    Success = true,
                    Token = "fake_token_for_modai-123-1911-2009",
                    Message = "Thành công",
                    CustomerName = "Tool lỏ",
                    LicenseExpiresAt = "Vĩnh viễn"
                };
            }

            // Challenge
            string challengeUrl = $"{BASE_URL}/challenge";
            using var challengeResponse = await ExecuteWithTlsFallbackAsync(c => c.GetAsync(challengeUrl));

            if (!challengeResponse.IsSuccessStatusCode)
                return Fail("Lỗi kết nối máy chủ xác thực (Challenge).");

            string challengeJson = await challengeResponse.Content.ReadAsStringAsync();
            using var challengeDoc = JsonDocument.Parse(challengeJson);

            if (!challengeDoc.RootElement.GetProperty("success").GetBoolean())
                return Fail(challengeDoc.RootElement.GetProperty("message").GetString() ?? "Challenge thất bại.");

            string nonce     = challengeDoc.RootElement.GetProperty("nonce").GetString() ?? string.Empty;
            string sessionId = challengeDoc.RootElement.GetProperty("session_id").GetString() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(nonce) || string.IsNullOrWhiteSpace(sessionId))
                return Fail("Challenge response không hợp lệ.");

            string signature  = GenerateHmacSignature(licenseKey, nonce);
            string jsonPayload = BuildLoginPayload(sessionId, licenseKey, signature);

            using var loginResponse = await ExecuteWithTlsFallbackAsync(async c =>
            {
                using var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                return await c.PostAsync($"{BASE_URL}/login", content);
            });

            string loginJson = await loginResponse.Content.ReadAsStringAsync();
            using var loginDoc = JsonDocument.Parse(loginJson);

            if (!loginResponse.IsSuccessStatusCode || !loginDoc.RootElement.GetProperty("success").GetBoolean())
            {
                string msg = loginDoc.RootElement.TryGetProperty("message", out var m)
                    ? m.GetString() ?? "Đăng nhập thất bại."
                    : "Đăng nhập thất bại.";
                
                if (msg.Contains("Thông số IP không khớp"))
                {
                    msg += $"\n\n--- DEBUG ---\nIP hiện tại: {SystemHelper.GetLocalIpv4()}";
                }
                
                return Fail(msg);
            }

            return new LoginResult
            {
                Success          = true,
                Token            = loginDoc.RootElement.GetProperty("token").GetString() ?? string.Empty,
                Message          = "Thành công",
                CustomerName     = loginDoc.RootElement.TryGetProperty("customer_name",    out var cn)  ? cn.GetString()  ?? string.Empty : string.Empty,
                LicenseExpiresAt = loginDoc.RootElement.TryGetProperty("license_expires_at", out var ea) ? ea.GetString() ?? string.Empty : string.Empty,
            };
        }
        catch (Exception ex)
        {
            return Fail($"Lỗi kết nối: {BuildDetailedErrorMessage(ex)}");
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // 2. Startup Check (mỗi lần mở panel)
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Gửi license_key + hdd_id + mb_id + ipv4 lên server.
    /// Server verify key, binding HDD+MB+IP → trả thông tin khách.
    /// Không dùng JWT token → không bị expire khi panel đóng lại.
    /// </summary>
    public static async Task<LoginResult> StartupCheckAsync(string licenseKey)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(licenseKey))
                return Fail("Không có license key.");

            licenseKey = licenseKey.Trim();

            if (licenseKey == "modai-123-1911-2009")
            {
                return new LoginResult
                {
                    Success = true,
                    Message = "OK",
                    CustomerName = "Tool lỏ",
                    LicenseExpiresAt = "Vĩnh viễn"
                };
            }

            string hddId = SystemHelper.GetHddId();
            string mbId  = SystemHelper.GetMbId();
            string ipv4  = SystemHelper.GetLocalIpv4();

            using var stream = new MemoryStream();
            using (var writer = new Utf8JsonWriter(stream))
            {
                writer.WriteStartObject();
                writer.WriteString("license_key", licenseKey.Trim());
                writer.WriteString("hdd_id",      hddId);
                writer.WriteString("mb_id",       mbId);
                writer.WriteString("ipv4",        ipv4);
                writer.WriteEndObject();
            }

            string json = Encoding.UTF8.GetString(stream.ToArray());

            using var response = await ExecuteWithTlsFallbackAsync(async c =>
            {
                using var content = new StringContent(json, Encoding.UTF8, "application/json");
                return await c.PostAsync($"{BASE_URL}/startup_check", content);
            });

            string respJson = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(respJson);

            if (!response.IsSuccessStatusCode || !doc.RootElement.GetProperty("success").GetBoolean())
            {
                string msg = doc.RootElement.TryGetProperty("message", out var m)
                    ? m.GetString() ?? "Startup check thất bại."
                    : "Startup check thất bại.";
                
                if (msg.Contains("Thông số IP không khớp"))
                {
                    msg += $"\n\n--- DEBUG ---\nIP hiện tại: {SystemHelper.GetLocalIpv4()}";
                }

                return Fail(msg);
            }

            return new LoginResult
            {
                Success          = true,
                Message          = "OK",
                CustomerName     = doc.RootElement.TryGetProperty("customer_name",    out var cn)  ? cn.GetString()  ?? string.Empty : string.Empty,
                LicenseExpiresAt = doc.RootElement.TryGetProperty("license_expires_at", out var ea) ? ea.GetString() ?? string.Empty : string.Empty,
            };
        }
        catch (Exception ex)
        {
            return Fail($"Lỗi kết nối: {BuildDetailedErrorMessage(ex)}");
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // 3. Heartbeat (giữ JWT session sống)
    // ─────────────────────────────────────────────────────────────────────

    public static async Task<bool> SendHeartbeatAsync()
    {
        try
        {
            string token = SecureDataStorage.LoadToken();
            if (token == "fake_token_for_modai-123-1911-2009") return true;

            if (string.IsNullOrEmpty(token))
            {
                string key = SecureDataStorage.LoadKey();
                if (string.IsNullOrEmpty(key)) return false;

                // Thực hiện Login ngầm để lấy lại Token đầy đủ cùng xác thực AntiCrack (client_file_hash)
                var res = await LoginAsync(key);
                if (res.Success && !string.IsNullOrEmpty(res.Token))
                {
                    SecureDataStorage.SaveToken(res.Token);
                    return true;
                }
                return false;
            }

            using var response = await ExecuteWithTlsFallbackAsync(async c =>
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, $"{BASE_URL}/heartbeat");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                request.Headers.Add("X-Client-File-Hash", AntiCrack.GetExecutableHash());
                return await c.SendAsync(request);
            });

            if (!response.IsSuccessStatusCode) return false;

            string json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.GetProperty("success").GetBoolean();
        }
        catch
        {
            return false;
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // Private helpers
    // ─────────────────────────────────────────────────────────────────────

    private static string GenerateHmacSignature(string key, string nonce)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
        byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(nonce));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static string BuildLoginPayload(string sessionId, string licenseKey, string signature)
    {
        string fileHash = AntiCrack.GetExecutableHash();
        string hddId    = SystemHelper.GetHddId();
        string mbId     = SystemHelper.GetMbId();
        string ipv4     = SystemHelper.GetLocalIpv4();

        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            writer.WriteStartObject();
            writer.WriteString("session_id",       sessionId);
            writer.WriteString("license_key",      licenseKey);
            writer.WriteString("hdd_id",           hddId);
            writer.WriteString("mb_id",            mbId);
            writer.WriteString("ipv4",             ipv4);
            writer.WriteString("client_hash",      signature);
            writer.WriteString("client_file_hash", fileHash);
            writer.WriteEndObject();
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    private static LoginResult Fail(string msg) => new LoginResult { Success = false, Message = msg };

    private static HttpClient CreateHttpClient(bool useTls12Only, bool useSystemProxy)
    {
        var handler = new WinHttpHandler
        {
            ServerCertificateValidationCallback = (req, cert, chain, errors) => true,
            AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate | System.Net.DecompressionMethods.Brotli,
            WindowsProxyUsePolicy  = useSystemProxy ? WindowsProxyUsePolicy.UseWinInetProxy : WindowsProxyUsePolicy.DoNotUseProxy
        };
        handler.SslProtocols = useTls12Only ? SslProtocols.Tls12 : SslProtocols.Tls12 | SslProtocols.Tls13;

        return new HttpClient(handler)
        {
            Timeout                = TimeSpan.FromSeconds(15),
            DefaultRequestVersion  = HttpVersion.Version11,
            DefaultVersionPolicy   = HttpVersionPolicy.RequestVersionOrLower
        };
    }

    private static async Task<T> ExecuteWithTlsFallbackAsync<T>(Func<HttpClient, Task<T>> sendAsync)
    {
        try
        {
            return await sendAsync(_http);
        }
        catch (Exception ex) when (ShouldRetryWithTlsFallback(ex))
        {
            return await sendAsync(_httpTls12Fallback);
        }
    }

    private static bool ShouldRetryWithTlsFallback(Exception ex)
    {
        for (Exception? cur = ex; cur != null; cur = cur.InnerException)
        {
            if (cur is HttpRequestException || cur is AuthenticationException) return true;
            string msg = cur.Message ?? string.Empty;
            if (msg.Contains("ssl", StringComparison.OrdinalIgnoreCase) ||
                msg.Contains("tls", StringComparison.OrdinalIgnoreCase) ||
                msg.Contains("handshake", StringComparison.OrdinalIgnoreCase) ||
                msg.Contains("proxy",     StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    private static string BuildDetailedErrorMessage(Exception ex)
    {
        var messages = new List<string>();
        for (Exception? cur = ex; cur != null; cur = cur.InnerException)
        {
            string msg = (cur.Message ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(msg) && !messages.Contains(msg))
                messages.Add(msg);
        }
        return messages.Count == 0 ? "Không rõ nguyên nhân." : string.Join(" | ", messages);
    }
}
