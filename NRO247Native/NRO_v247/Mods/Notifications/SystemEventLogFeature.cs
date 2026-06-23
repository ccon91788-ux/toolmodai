using System;
using System.Text.RegularExpressions;

namespace NRO_v247.Mods.Notifications
{
    public class SystemEventLogFeature : IAutoFeature
    {
        public bool IsUtilityTask => true;
        public bool IsActive => true;
        public string CurrentState => ""; // Ẩn state đi để không choán cột status
        public int Priority => 15;
        public bool IsRequested => false;

        private DateTime _lastMaintenanceLogTime = DateTime.MinValue;

        // Pattern 1: Bắt sự kiện rớt đồ SKH. Group 1: Tên người chơi, Group 2: Tên Set (có thể rỗng)
        private static readonly Regex _regexSkhDrop = new Regex(@"^(.+?) vừa đánh quái may mắn nhận(.*)được 1 trang bị Set kích hoạt(?:\s+(.+))?$", RegexOptions.Compiled);
        
        // Pattern 2: Bắt bảo trì. Group 1: Số phút
        private static readonly Regex _regexMaintenance = new Regex(@"Hệ thống sẽ bảo trì sau (\d+) phút", RegexOptions.Compiled);

        // Pattern 3: Bắt thông báo tiêu diệt Boss. Group 1: Tên người chơi, Group 2: Tên Boss
        private static readonly Regex _regexBossKill = new Regex(@"^(.+?):\s*Đã tiêu diệt được\s+(.+?)\s+mọi người đều ngưỡng mộ.*$", RegexOptions.Compiled);

        public SystemEventLogFeature()
        {
            NotifyCatcher.OnNotifyReceived += HandleNotification;
        }

        private void HandleNotification(NotifyCatcher.NotifyEvent ev)
        {
            string msg = ev.Message;
            if (Char.myCharz() == null || Char.myCharz().cName == null) return;

            // 1. Phân tích Rớt đồ SKH
            if (msg.Contains("vừa đánh quái may mắn nhận được 1 trang bị Set kích hoạt") || msg.Contains("vừa đánh quái may mắn nhận"))
            {
                var match = _regexSkhDrop.Match(msg);
                if (match.Success)
                {
                    string playerName = match.Groups[1].Value.Trim();
                    string setName = match.Groups[3].Value.Trim();

                    // Yêu cầu: "Nếu player rơi đồ kích hoạt giống tên mình sẽ ghi nhật kí"
                    if (playerName.Equals(Char.myCharz().cName, StringComparison.OrdinalIgnoreCase))
                    {
                        string dropMsg = string.IsNullOrEmpty(setName) 
                            ? $"{playerName} đánh rơi trang bị Set kích hoạt" 
                            : $"{playerName} đánh rơi {setName}";

                        // Thay vì tự ghép chuỗi, dùng thư viện chuyên biệt GameLogger
                        GameLogger.SendLog("SKH_ME", dropMsg);
                    }
                }
                return;
            }

            // 2. Phân tích Thông báo Bảo trì
            if (msg.Contains("Hệ thống sẽ bảo trì sau"))
            {
                var match = _regexMaintenance.Match(msg);
                if (match.Success && int.TryParse(match.Groups[1].Value, out int minutes))
                {
                    // Block ghi log bảo trì 1 tiếng (60 phút)
                    if (DateTime.Now - _lastMaintenanceLogTime >= TimeSpan.FromMinutes(60))
                    {
                        _lastMaintenanceLogTime = DateTime.Now;
                        DateTime downtime = DateTime.Now.AddMinutes(minutes);
                        
                        string maintMsg = $"Máy chủ sẽ bảo trì vào lúc {downtime:HH:mm}";
                        GameLogger.SendLog("SYSTEM", maintMsg);
                    }
                }
                return;
            }

            // 3. Phân tích Thông báo Tiêu diệt Boss
            if (msg.Contains("Đã tiêu diệt được") && msg.Contains("mọi người đều ngưỡng mộ"))
            {
                var match = _regexBossKill.Match(msg);
                if (match.Success)
                {
                    string killerName = match.Groups[1].Value.Trim();
                    string bossName = match.Groups[2].Value.Trim();
                    
                    if (killerName.Equals(Char.myCharz().cName, StringComparison.OrdinalIgnoreCase))
                    {
                        GameLogger.SendLog("BOSS_KILL", $"Bạn vừa tiêu diệt {bossName}! Thật ngưỡng mộ!");
                    }
                }
                return;
            }
        }

        public void Update()
        {
            // Background task
        }

        public void Reset()
        {
            _lastMaintenanceLogTime = DateTime.MinValue;
        }
    }
}
