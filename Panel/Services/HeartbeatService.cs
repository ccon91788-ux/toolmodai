using System;
using System.Threading;
using System.Threading.Tasks;

namespace Panel.Services;

/// <summary>
/// Chạy nền để gửi tín hiệu nhịp tim lên máy chủ (Heartbeat) ngẫu nhiên mỗi 3-7 phút.
/// Nếu fail liên tiếp 3 lần (mỗi lần cách 30s) mới trigger lockout.
/// </summary>
public static class HeartbeatService
{
    private static CancellationTokenSource? _cts;

    /// <summary>
    /// Event khi bị lockout. Tham số string là lý do để hiển thị popup sau khi tắt Panel.
    /// </summary>
    public static event Action<string>? OnLockoutTriggered;

    /// <summary>
    /// Lý do lockout cuối cùng — Program.cs đọc sau khi Application.Run() kết thúc.
    /// </summary>
    public static string? LastLockoutReason { get; private set; }

    private const int MAX_RETRIES = 3;
    private const int RETRY_DELAY_MS = 30_000; // 30 giây giữa mỗi lần retry

    public static void Start()
    {
        if (_cts != null && !_cts.IsCancellationRequested) return;

        _cts = new CancellationTokenSource();
        Task.Run(() => HeartbeatLoopAsync(_cts.Token));
    }

    public static void Stop()
    {
        _cts?.Cancel();
    }

    private static async Task HeartbeatLoopAsync(CancellationToken ct)
    {
        Random rnd = new Random();
        // Delay ngay lúc mới chạy 1 phút đầu
        await Task.Delay(60000, ct);

        while (!ct.IsCancellationRequested)
        {
            bool success = await LicenseAuthService.SendHeartbeatAsync();

            if (!success)
            {
                // ── Retry trước khi từ bỏ ──────────────────────────────
                bool recovered = false;
                for (int retry = 1; retry <= MAX_RETRIES; retry++)
                {
                    try
                    {
                        await Task.Delay(RETRY_DELAY_MS, ct);
                    }
                    catch (TaskCanceledException) { return; }

                    bool retryOk = await LicenseAuthService.SendHeartbeatAsync();
                    if (retryOk)
                    {
                        recovered = true;
                        break;
                    }
                }

                if (!recovered)
                {
                    // Fail liên tiếp (1 lần đầu + 3 retry = 4 lần) → lockout
                    string reason = "Mất kết nối Server xác thực!\n\n"
                        + $"Heartbeat thất bại liên tiếp {MAX_RETRIES + 1} lần.\n"
                        + "Nguyên nhân có thể:\n"
                        + "  • Mạng internet bị gián đoạn\n"
                        + "  • Server xác thực đang bảo trì\n"
                        + "  • Key đã bị sử dụng trên máy khác\n\n"
                        + "Panel sẽ tự đóng. Mở lại Panel để đăng nhập lại tự động.";

                    LastLockoutReason = reason;
                    SecureDataStorage.DeleteToken(); // Xóa JWT in-memory
                    // ⚠️ KHÔNG xóa license key file → mở lại Panel vẫn tự đăng nhập
                    OnLockoutTriggered?.Invoke(reason);
                    break;
                }
            }
            
            // Jitter ngẫu nhiên 3 phút đến 7 phút (180,000ms đến 420,000ms)
            int delayMs = rnd.Next(180_000, 420_000);
            try
            {
                await Task.Delay(delayMs, ct);
            }
            catch (TaskCanceledException)
            {
                break;
            }
        }
    }
}
