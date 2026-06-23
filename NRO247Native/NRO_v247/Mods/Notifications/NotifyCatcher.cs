using System;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace NRO_v247.Mods.Notifications
{
    public class NotifyCatcher : IAutoFeature
    {
        public bool IsUtilityTask => true;
        public bool IsActive => true;
        public string CurrentState => "";
        public int Priority => 10;
        // Luôn true nếu có dữ liệu cần phát đi
        public bool IsRequested => !_queue.IsEmpty;

        public enum NotifyType
        {
            GlobalServer, // Cmd -25, 94: Thông báo góc màn hình
            ChatVip,      // Cmd 93: Chữ chạy ngang màn hình (Boss, Event)
            SystemNpc,    // Cmd 92: Bò mộng, Thần mèo, Rớt đồ, Thông báo user.
        }

        public struct NotifyEvent
        {
            public NotifyType Type;
            public string SourceName;  // Ai gửi (NPC name, hoặc rỗng nếu hệ thống)
            public string Message;     // Nội dung sau khi đã loại bỏ nhiễu/mã màu
            public Char SourceChar;    // Đối tượng Char (thường dùng cho rơi đồ SKH)
        }

        private static ConcurrentQueue<NotifyEvent> _queue = new ConcurrentQueue<NotifyEvent>();

        // Event công khai cho các module tự động săn boss, theo dõi đồ SKH đăng ký vào
        public static event Action<NotifyEvent> OnNotifyReceived;

        // Biểu thức chính quy dọn rác System/Server Custom
        private static readonly Regex _colorRegex = new Regex(@"\|[0-9]+(?:\|[0-9]+)?\|?", RegexOptions.Compiled);
        private static readonly Regex _prefixRegex = new Regex(@"^\[.*?\]\s*", RegexOptions.Compiled);
        private static readonly Regex _suffixRegex = new Regex(@"\s*\[.*?\]$", RegexOptions.Compiled);

        public void Update()
        {
            // Bắn dữ liệu đã được tổng hợp ở Background Thread vào Main Thread (Update của Unity)
            while (_queue.TryDequeue(out var ev))
            {
                OnNotifyReceived?.Invoke(ev);
            }
        }

        /// <summary>
        /// Hook bắt từ Controller.cs (Cmd -25 và 94)
        /// </summary>
        public static void CatchServerMessage(string rawText)
        {
            _queue.Enqueue(new NotifyEvent
            {
                Type = NotifyType.GlobalServer,
                SourceName = "Server",
                Message = CleanText(rawText)
            });
        }

        /// <summary>
        /// Hook bắt từ GameScr.cs (Hàm chatVip - Cmd 93)
        /// </summary>
        public static void CatchVipChat(string rawText)
        {
            _queue.Enqueue(new NotifyEvent
            {
                Type = NotifyType.ChatVip,
                SourceName = "System",
                Message = CleanText(rawText)
            });
        }

        /// <summary>
        /// Hook bắt từ Controller.cs (Cmd 92 - Hội thoại NPC/Thần mèo)
        /// </summary>
        public static void CatchNpcMessage(string charName, string rawText, Char charObj)
        {
            _queue.Enqueue(new NotifyEvent
            {
                Type = NotifyType.SystemNpc,
                SourceName = CleanName(charName),
                Message = CleanText(rawText),
                SourceChar = charObj
            });
        }

        private static string CleanText(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            
            // 1. Xoá mọi thẻ mã màu game (Vd: |0|, |5|1|)
            var result = _colorRegex.Replace(input, "");
            
            // 2. Xóa các tag tiền tố của server lậu (Vd: [HT] Boss mabu vừa chết)
            result = _prefixRegex.Replace(result, "");
            
            return result.Trim();
        }

        private static string CleanName(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            
            // Xoá tag hậu tố ở tên nhân vật (Vd: ajiruro01 [Indo])
            var result = _suffixRegex.Replace(input, "");
            return result.Trim();
        }
    }
}
