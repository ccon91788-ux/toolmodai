using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Panel.Services;

public class UpdateManifest
{
    public bool Success { get; set; }
    public string? PublishedAt { get; set; }
    public string? Url { get; set; }
    public string? Description { get; set; }
}

public class LocalUpdateState
{
    public string? LastUpdateDate { get; set; }
}

[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true, WriteIndented = true)]
[JsonSerializable(typeof(UpdateManifest))]
[JsonSerializable(typeof(LocalUpdateState))]
internal partial class UpdateJsonContext : JsonSerializerContext
{
}

public static class UpdateManager
{
    private static readonly string UpdateStatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "local_update_state.json");
    private static bool _isUpdating;

    public static async Task CheckForUpdatesAsync(Form? mainForm = null, bool showPopup = true)
    {
        if (_isUpdating) return;
        _isUpdating = true;

        try
        {
            // Lấy URL từ StringShield
            string updateUrl = Panel.Helpers.StringShield.GetUpdateUrl();

            // Thêm tham số ngẫu nhiên để chống trình duyệt/server cache
            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            string noCacheUrl = $"{updateUrl}?t={timestamp}&rnd={Guid.NewGuid():N}";

            using var client = new HttpClient();
            client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true, NoStore = true };
            client.DefaultRequestHeaders.Pragma.ParseAdd("no-cache");

            string json = await client.GetStringAsync(noCacheUrl);
            var manifest = JsonSerializer.Deserialize(json, UpdateJsonContext.Default.UpdateManifest);

            if (manifest == null || !manifest.Success || string.IsNullOrWhiteSpace(manifest.PublishedAt) || string.IsNullOrWhiteSpace(manifest.Url))
            {
                if (showPopup) MessageBox.Show("Không thể đọc thông tin cập nhật từ máy chủ hoặc không có bản cập nhật.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var localState = LoadLocalState();
            string localDate = localState?.LastUpdateDate ?? "Chưa rõ";

            // Có thể dùng string format so sánh đơn giản
            if (localDate.Trim().Equals(manifest.PublishedAt.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                if (showPopup)
                {
                    MessageBox.Show(
                        $"Bạn đang ở bản mới nhất.\nNgày cập nhật bản hiện tại: {localDate}",
                        "Cập nhật", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                return;
            }

            // Gặp bản khác ngày
            string message = $"Có bản cập nhật mới trên hệ thống!\n\n" +
                             $"- Cập nhật mới nhất trên web: {manifest.PublishedAt}\n" +
                             $"- Ngày cập nhật của bạn: {localDate}\n\n" +
                             (!string.IsNullOrWhiteSpace(manifest.Description) ? $"Nội dung:\n{manifest.Description}\n\n" : "") +
                             $"Bạn có muốn tải bản cập nhật từ file mới này không?";

            var result = MessageBox.Show(message, "Kiểm tra cập nhật", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                await DownloadAndUpdateAsync(manifest.Url, manifest.PublishedAt);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Lỗi khi lấy update: {ex.Message}");
            if (showPopup) MessageBox.Show($"Gặp lỗi khi kiểm tra cập nhật:\n{ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _isUpdating = false;
        }
    }

    private static LocalUpdateState? LoadLocalState()
    {
        try
        {
            if (File.Exists(UpdateStatePath))
            {
                return JsonSerializer.Deserialize(File.ReadAllText(UpdateStatePath), UpdateJsonContext.Default.LocalUpdateState);
            }
        }
        catch { }
        return null;
    }

    private static void SaveLocalState(string updatedDate)
    {
        try
        {
            var dir = Path.GetDirectoryName(UpdateStatePath);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

            var state = new LocalUpdateState { LastUpdateDate = updatedDate };
            File.WriteAllText(UpdateStatePath, JsonSerializer.Serialize(state, UpdateJsonContext.Default.LocalUpdateState));
        }
        catch { }
    }

    private static async Task DownloadAndUpdateAsync(string downloadUrl, string newDate)
    {
        try
        {
            string tempExtractDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "update_extracted");

            // Bỏ cache cho URL download zip bằng custom parameter nếu URL hỗ trợ
            string separator = downloadUrl.Contains('?') ? "&" : "?";
            string dlUrl = $"{downloadUrl}{separator}t={DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";

            using var client = new HttpClient();
            client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true, NoStore = true };

            var response = await client.GetAsync(dlUrl, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            // Cố gắng đọc tên file gốc từ Header hoặc URL thay vì đặt tên bừa
            string zipFileName = "";
            if (response.Content.Headers.ContentDisposition != null)
            {
                zipFileName = response.Content.Headers.ContentDisposition.FileNameStar ?? response.Content.Headers.ContentDisposition.FileName ?? "";
                zipFileName = zipFileName.Trim('\"');
            }
            
            if (string.IsNullOrWhiteSpace(zipFileName))
            {
                try
                {
                    Uri uri = new Uri(downloadUrl);
                    zipFileName = Path.GetFileName(uri.LocalPath);
                }
                catch { }
            }

            // Loại bỏ các ký tự không hợp lệ cho filename (đề phòng an toàn)
            if (!string.IsNullOrWhiteSpace(zipFileName))
            {
                foreach (char c in Path.GetInvalidFileNameChars())
                {
                    zipFileName = zipFileName.Replace(c.ToString(), "");
                }
            }

            // Nếu vẫn không có tên, fallback tên dự phòng
            if (string.IsNullOrWhiteSpace(zipFileName))
            {
                zipFileName = $"update_{DateTime.Now:yyyyMMdd_HHmmss}.zip";
            }

            string tempZipPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, zipFileName);

            using (var stream = await response.Content.ReadAsStreamAsync())
            using (var fs = new FileStream(tempZipPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                // Giả lập tiến trình tải / có thể chặn UI ở đây để an toàn (Tuy nhiên dùng await thì ko bị đơ form)
                await stream.CopyToAsync(fs);
            }

            // Giải nén ra thư mục update_extracted bằng C# (tránh lỗi PowerShell Expand-Archive ở các bản Win cũ)
            if (Directory.Exists(tempExtractDir))
            {
                Directory.Delete(tempExtractDir, true);
            }
            ZipFile.ExtractToDirectory(tempZipPath, tempExtractDir);

            // Lưu metadata cập nhật cục bộ
            SaveLocalState(newDate);

            // Sinh lệnh BAT cập nhật tự động
            string batPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "updater.bat");
            string exeName = Path.GetFileName(Application.ExecutablePath);

            string batContent = $@"@echo off
title ZFox Updater
echo Dang cho tien trinh '{exeName}' dong hoan toan...
timeout /t 3 /nobreak >nul

echo Dang cap nhat tep, vui long tro trong giay lat...
xcopy /y /e /h /q ""%~dp0update_extracted\*"" ""%~dp0"" >nul

echo Dang don dep...
rd /s /q ""%~dp0update_extracted""
del ""%~dp0{zipFileName}""

echo Dang mo lai ung dung...
start """" ""{exeName}""

echo Hoan tat! Chuc ban online vui ve.
(goto) 2>nul & del ""%~f0""
";
            File.WriteAllText(batPath, batContent);

            var psi = new ProcessStartInfo
            {
                FileName = batPath,
                UseShellExecute = true,
                CreateNoWindow = false,
                WindowStyle = ProcessWindowStyle.Normal
            };
            Process.Start(psi);

            Environment.Exit(0);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi trong quá trình tải và cài đặt bản cập nhật:\n{ex.Message}", "Lỗi update", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
