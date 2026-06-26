using System;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DemoUpdater
{
    public class DemoResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }
        [JsonPropertyName("filename")]
        public string? Filename { get; set; }
        [JsonPropertyName("url")]
        public string? Url { get; set; }
        [JsonPropertyName("time")]
        public string? Time { get; set; }
        [JsonPropertyName("message")]
        public string? Message { get; set; }
    }

    [JsonSerializable(typeof(DemoResponse))]
    public partial class DemoJsonContext : JsonSerializerContext { }

    public partial class MainForm : Form
    {
        private Label lblStatus;
        private ProgressBar progressBar;
        private Label lblUpdateDate;
        
        public MainForm()
        {
            InitializeComponent();
            this.Load += MainForm_Load;
        }

        private void InitializeComponent()
        {
            this.Text = "Cập Nhật Bản Demo";
            this.Size = new Size(400, 180);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            lblStatus = new Label()
            {
                Text = "Đang kiểm tra cập nhật...",
                Location = new Point(20, 20),
                AutoSize = true,
                Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point)
            };

            lblUpdateDate = new Label()
            {
                Text = "Bản update gần nhất: Đang tải...",
                Location = new Point(20, 50),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Italic, GraphicsUnit.Point),
                ForeColor = Color.SaddleBrown
            };

            progressBar = new ProgressBar()
            {
                Location = new Point(20, 80),
                Size = new Size(340, 25),
                Style = ProgressBarStyle.Marquee
            };

            this.Controls.Add(lblStatus);
            this.Controls.Add(lblUpdateDate);
            this.Controls.Add(progressBar);
        }

        private async void MainForm_Load(object? sender, EventArgs e)
        {
            await ProcessUpdateAsync();
        }

        private async Task ProcessUpdateAsync()
        {
            try
            {
                using var client = new HttpClient();
                
                // Fetch JSON
                lblStatus.Text = "Đang kết nối tới máy chủ...";
                var jsonStr = await client.GetStringAsync("https://raw.githubusercontent.com/ccon91788-ux/toolmodai/master/update.json");                
                var data = JsonSerializer.Deserialize(jsonStr, DemoJsonContext.Default.DemoResponse);
                
                if (data == null || !data.Success || string.IsNullOrEmpty(data.Filename))
                {
                    lblStatus.Text = "Không có bản cập nhật hoặc lỗi server.";
                    lblUpdateDate.Text = "Lý do: " + (data?.Message ?? "Phản hồi rỗng");
                    progressBar.Style = ProgressBarStyle.Blocks;
                    return;
                }

                // Show date
                lblUpdateDate.Text = "Bản update gần nhất: " + data.Time;

                // Download File
                lblStatus.Text = "Đang tải xuống tệp: " + data.Filename;
                progressBar.Style = ProgressBarStyle.Continuous;
                progressBar.Value = 0;

                string localZipPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, data.Filename);
                // Chỉ lấy duy nhất đường link URL khai báo trên GitHub của ông
string downloadUrl = data.Url;

if (string.IsNullOrEmpty(downloadUrl))
{
    lblStatus.Text = "Lỗi: File cấu hình trên GitHub chưa có link tải!";
    return;
}

                using (var response = await client.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead))
                {
                    response.EnsureSuccessStatusCode();
                    var totalBytes = response.Content.Headers.ContentLength ?? -1L;
                    using var stream = await response.Content.ReadAsStreamAsync();
                    using var fileStream = new FileStream(localZipPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

                    var buffer = new byte[8192];
                    long totalRead = 0;
                    int bytesRead;

                    while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) != 0)
                    {
                        await fileStream.WriteAsync(buffer, 0, bytesRead);
                        totalRead += bytesRead;
                        if (totalBytes != -1)
                        {
                            int percent = (int)(totalRead * 100 / totalBytes);
                            if (percent >= 0 && percent <= 100) {
                                progressBar.Value = percent;
                            }
                        }
                    }
                }

                // Check file name safety as user requested (đuôi ngày giờ)
                string downloadedFileCheck = Path.GetFileName(localZipPath);
                if (downloadedFileCheck != data.Filename)
                {
                    lblStatus.Text = "Tên tệp không khớp với thông tin server. Dừng cập nhật!";
                    return;
                }

                if (!downloadedFileCheck.StartsWith("demo_") || !downloadedFileCheck.EndsWith(".zip"))
                {
                    lblStatus.Text = "Tên file không đúng định dạng chứa ngày giờ bảo mật.";
                    return;
                }

                // Extract handling
                lblStatus.Text = "Đang giải nén tập tin. Vui lòng đợi...";
                progressBar.Style = ProgressBarStyle.Marquee;

                await Task.Run(() =>
                {
                    System.Threading.Thread.Sleep(500); // brief pause to let UI render
                    
                    string currentDir = AppDomain.CurrentDomain.BaseDirectory;
                    using var archive = ZipFile.OpenRead(localZipPath);
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        if (string.IsNullOrEmpty(entry.Name)) continue; // ignore directory nodes
                        
                        string destPath = Path.Combine(currentDir, entry.FullName);
                        string destDir = Path.GetDirectoryName(destPath) ?? "";
                        if (!Directory.Exists(destDir))
                            Directory.CreateDirectory(destDir);
                            
                        entry.ExtractToFile(destPath, true);
                    }
                });

                // Remove zip file after success
                try { File.Delete(localZipPath); } catch { }

                lblStatus.Text = "Hoàn tất giải nén!";
                progressBar.Style = ProgressBarStyle.Blocks;
                progressBar.Value = 100;
                
                await Task.Delay(1500); // give user time to see success
                
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Lỗi trong quá trình cập nhật.";
                MessageBox.Show("Chi tiết lỗi: " + ex.Message, "Lỗi Cập Nhật", MessageBoxButtons.OK, MessageBoxIcon.Error);
                try { this.Close(); } catch { }
            }
        }
    }
}
