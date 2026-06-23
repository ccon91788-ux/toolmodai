using System.Linq;
using NRO_v247.Mods.Utils;

namespace NRO_v247.Mods.Items
{
    public class SkhReportFeature : IAutoFeature
    {
        public bool IsUtilityTask => true;
        public int Priority => 0;
        public bool IsRequested => true;
        public bool IsActive => true;
        public string CurrentState => "";
        public bool IsBackgroundAllowed => true;

        private long _lastReportTime = 0;
        private string _lastPayload = "";

        public SkhReportFeature()
        {
            Notifications.NotifyCatcher.OnNotifyReceived += OnNotify;
        }

        private void OnNotify(Notifications.NotifyCatcher.NotifyEvent ev)
        {
            TryReportSkhTimeFromText(ev.Message);
        }

        public static void TryReportSkhTimeFromText(string message)
        {
            if (string.IsNullOrEmpty(message)) return;

            var titleMatch = System.Text.RegularExpressions.Regex.IsMatch(
                message,
                @"Thời\s+gian\s+tìm\s+set\s+kích\s+hoạt",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            if (!titleMatch) return;

            var match = System.Text.RegularExpressions.Regex.Match(message, @"(\d{4}/\d{2}/\d{2}\s+\d{2}:\d{2}:\d{2})");
            if (match.Success)
            {
                NRO_v247.SocketGame.SendMessage($"SKH_TIME|{NRO_v247.AutoLogin.idClientSocket}|{match.Groups[1].Value}");
            }
        }

        public void Update()
        {
            if (global::Char.myCharz() == null) return;
            
            long now = mSystem.currentTimeMillis();
            if (now - _lastReportTime < 5000) return;
            _lastReportTime = now;

            int gender = global::Char.myCharz().cgender;
            // Arrays: each holds the count of [Áo, Quần, Găng, Giày, Radar]
            int[] set1 = new int[5];
            int[] set2 = new int[5];
            int[] set3 = new int[5];
            int[] set4 = new int[5];
            int[] set5 = new int[5];

            // Scan bags & body & box
            CountSkh(global::Char.myCharz().arrItemBag, gender, set1, set2, set3, set4, set5);
            CountSkh(global::Char.myCharz().arrItemBody, gender, set1, set2, set3, set4, set5);
            if (global::Char.myCharz().arrItemBox != null)
                CountSkh(global::Char.myCharz().arrItemBox, gender, set1, set2, set3, set4, set5);

            string name1, name2, name3, name4, name5;
            if (gender == 0) // Trái Đất
            {
                name1 = "Sôngôku"; name2 = "TVT Kaio"; name3 = "TXH"; name4 = "Gohan"; name5 = "Kirin";
            }
            else if (gender == 1) // Namek
            {
                name1 = "Picolo"; name2 = "Daimao"; name3 = "Ốc tiêu"; name4 = "Gohan"; name5 = "Nail";
            }
            else // Xayda
            {
                name1 = "Kakarot"; name2 = "Ca Đíc"; name3 = "Cađic M"; name4 = "Gohan"; name5 = "Nappa";
            }

            string EncodeSet(int[] set) => $"{set[0]}-{set[1]}-{set[2]}-{set[3]}-{set[4]}";
            string v1 = EncodeSet(set1), v2 = EncodeSet(set2), v3 = EncodeSet(set3), v4 = EncodeSet(set4), v5 = EncodeSet(set5);
            
            // Lấy tổng tất cả các món đồ SKH đếm được (mỗi món là 1 cái đồ)
            int total = set1.Sum() + set2.Sum() + set3.Sum() + set4.Sum() + set5.Sum();

            string payload = $"{total}|{name1}|{v1}|{name2}|{v2}|{name3}|{v3}|{name4}|{v4}|{name5}|{v5}";
            
            if (payload != _lastPayload)
            {
                _lastPayload = payload;
                NRO_v247.SocketGame.SendMessage($"SKH_DATA|{NRO_v247.AutoLogin.idClientSocket}|{payload}");
            }
        }

        private void CountSkh(Item[] items, int gender, int[] set1, int[] set2, int[] set3, int[] set4, int[] set5)
        {
            if (items == null) return;
            foreach (var item in items)
            {
                var type = ItemHelper.GetSkhSetType(item, gender);
                if (type == SkhSetType.None) continue;
                
                int itemType = item.template.type; // 0=Áo, 1=Quần, 2=Găng, 3=Giày, 4=Radar
                
                // Mảng tương ứng: set1(songoku/picolo/kakarot), set2, set3, set4(gohan), set5
                if (type == SkhSetType.Earth_Songoku || type == SkhSetType.Namek_Picolo || type == SkhSetType.Saiyan_Kakarot) set1[itemType]++;
                else if (type == SkhSetType.Earth_ThanVuTruKaio || type == SkhSetType.Namek_Daimao || type == SkhSetType.Saiyan_CaDic) set2[itemType]++;
                else if (type == SkhSetType.Earth_ThienXinHang || type == SkhSetType.Namek_OcTieu || type == SkhSetType.Saiyan_CadicM) set3[itemType]++;
                else if (type == SkhSetType.Gohan) set4[itemType]++;
                else if (type == SkhSetType.Earth_Kirin || type == SkhSetType.Namek_Nail || type == SkhSetType.Saiyan_Nappa) set5[itemType]++;
            }
        }
    }
}
