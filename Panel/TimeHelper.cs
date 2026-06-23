using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;

namespace Panel
{
    public static class TimeHelper
    {
        private static TimeSpan _offset = TimeSpan.Zero;
        private static bool _isSynced = false;

        public static async Task SyncWithInternetTimeAsync()
        {
            if (_isSynced) return;

            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(5);
                    string jsonStr = await client.GetStringAsync("http://worldtimeapi.org/api/timezone/Asia/Ho_Chi_Minh");
                    
                    using (JsonDocument doc = JsonDocument.Parse(jsonStr))
                    {
                        if (doc.RootElement.TryGetProperty("datetime", out JsonElement dtElement))
                        {
                            if (DateTimeOffset.TryParse(dtElement.GetString(), out DateTimeOffset dto))
                            {
                                // Extract the literal wall-clock time in Vietnam regardless of VPS timezone
                                DateTime vnWallTime = new DateTime(dto.Year, dto.Month, dto.Day, dto.Hour, dto.Minute, dto.Second);
                                _offset = vnWallTime - DateTime.Now;
                                _isSynced = true;
                            }
                        }
                    }
                }
            }
            catch
            {
                // Fallback to UTC+7 (Vietnam time) if network request fails
                DateTime vnWallTime = DateTime.UtcNow.AddHours(7);
                _offset = vnWallTime - DateTime.Now;
                _isSynced = false;
            }
        }

        public static DateTime GetRealTime()
        {
            return DateTime.Now.Add(_offset);
        }
    }
}
