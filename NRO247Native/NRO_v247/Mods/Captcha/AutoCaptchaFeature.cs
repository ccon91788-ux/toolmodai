using System;
using System.Collections.Specialized;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using SkiaSharp;
using NRO_v247.Mods;

namespace NRO_v247.Mods.Captcha
{
    public class AutoCaptchaFeature : IAutoFeature
    {
        private bool _isSolving;
        private string _apiServer = "";
        private string _apiKey = "";

        // Status for UI/Logs
        public string StateMessage { get; private set; } = "";

        public void ApplySettingsFromPanel(string apiServer, string apiKey)
        {
            _apiServer = apiServer;
            _apiKey = apiKey;
        }

        public void Update()
        {
            if (_isSolving) return;

            // Check if captcha is present
            if (GameScr.gI().mobCapcha != null && (!MobCapcha.isAttack || !MobCapcha.explode) && GameCanvas.gameTick % 100 == 0)
            {
                if (string.IsNullOrEmpty(_apiServer) || string.IsNullOrEmpty(_apiKey))
                {
                    StateMessage = "Thiếu API Key/Server";
                    return;
                }

                _isSolving = true;
                StateMessage = "Đang giải Captcha...";
                new Thread(SolveCaptchaLoop) { IsBackground = true }.Start();
            }
            else if (GameScr.gI().mobCapcha == null)
            {
                // Reset state when map clear
                StateMessage = "";
            }
        }

        private void SolveCaptchaLoop()
        {
            try
            {
                Thread.Sleep(1000);

                if (GameScr.imgCapcha == null || GameScr.imgCapcha.bitmap == null)
                {
                    StateMessage = "Lỗi ảnh Captcha rỗng!";
                    return;
                }

                string base64Image = EncodeCaptchaImage(GameScr.imgCapcha.bitmap);
                if (string.IsNullOrEmpty(base64Image))
                {
                    StateMessage = "Lỗi mã hoá ảnh Captcha!";
                    return;
                }

                string response = SendToAPI(base64Image);
                if (string.IsNullOrEmpty(response))
                {
                    StateMessage = "Lỗi mạng hoặc Server API không phản hồi";
                    return;
                }

                ProcessAPIResponse(response);
            }
            catch (Exception ex)
            {
                StateMessage = $"Giải Captcha lỗi: {ex.Message}";
            }
            finally
            {
                _isSolving = false;
            }
        }

        private string EncodeCaptchaImage(SKBitmap bitmap)
        {
            try
            {
                using (var image = SKImage.FromBitmap(bitmap))
                using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
                {
                    return Convert.ToBase64String(data.ToArray());
                }
            }
            catch
            {
                return null;
            }
        }

        private string SendToAPI(string imageBase64)
        {
            try
            {
                string address = _apiServer + _apiKey;
                using (WebClient webClient = new WebClient())
                {
                    webClient.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
                    webClient.Headers.Add("Accept", "application/json");
                    webClient.Encoding = Encoding.UTF8;

                    NameValueCollection data = new NameValueCollection
                    {
                        ["image"] = imageBase64
                    };

                    byte[] responseBytes = webClient.UploadValues(address, "POST", data);
                    return Encoding.UTF8.GetString(responseBytes);
                }
            }
            catch
            {
                return null;
            }
        }

        private void ProcessAPIResponse(string response)
        {
            Match matchCaptcha = Regex.Match(response, "\"captcha\"\\s*:\\s*\"(\\d+)\"");
            Match matchStatus = Regex.Match(response, "\"status\"\\s*:\\s*(\\d+)");

            if (matchCaptcha.Success && matchStatus.Success && matchStatus.Groups[1].Value == "0")
            {
                string captcha = matchCaptcha.Groups[1].Value;
                if (captcha.Length >= 4 && captcha.Length <= 7)
                {
                    InputCaptcha(captcha);
                }
                else
                {
                    StateMessage = $"Mã giải sai định dạng: {captcha}";
                }
            }
            else
            {
                StateMessage = "Server API trả về lỗi";
            }
        }

        private void InputCaptcha(string captcha)
        {
            StateMessage = $"Nhập mã: {captcha}";
            Thread.Sleep(500);

            foreach (char c in captcha)
            {
                if (Service.gI() == null) return;
                Service.gI().mobCapcha(c);
                Thread.Sleep(Res.random(200, 400));
            }

            Thread.Sleep(500);
            if (Service.gI() != null)
            {
                Service.gI().mobCapcha((char)13); // Gửi phím Enter (13)
            }
            
            StateMessage = "Giải mã xong, chờ biến mất...";
        }

        public bool IsActive => _isSolving && !string.IsNullOrEmpty(StateMessage);
        public string CurrentState => StateMessage;
        public bool IsUtilityTask => true;
    }
}
