using System.Linq;

namespace NRO_v247.Mods.Utils
{
    public enum SkhSetType
    {
        None = -1,
        // Trái Đất
        Earth_Songoku = 0,
        Earth_ThienXinHang = 1,
        Earth_Kirin = 2,
        Earth_ThanVuTruKaio = 3, // Set Mới
        // Namek
        Namek_Picolo = 4,
        Namek_OcTieu = 5,
        Namek_Daimao = 6,
        Namek_Nail = 7,         // Set Mới
        // Xayda
        Saiyan_Kakarot = 8,
        Saiyan_CaDic = 9,
        Saiyan_Nappa = 10,
        Saiyan_CadicM = 11,      // Set Mới
        // Chung (Gohan)
        Gohan = 12
    }

    public static class ItemHelper
    {
        // Danh sách ID Option của Set Kích Hoạt
        // Gồm các ID cơ bản (127-135) và các ID mới (233, 237, 241, 245)
        public static readonly int[] SkhOptionIds = { 
            127, 128, 129, 130, 131, 132, 133, 134, 135,
            233, 237, 241, 245,254,255,256,257
        };

        public static bool IsSkhItem(Item item)
        {
            if (item == null || item.itemOption == null) return false;
            foreach (var opt in item.itemOption)
            {
                if (opt?.optionTemplate == null) continue;
                int oid = opt.optionTemplate.id;
                string oname = opt.optionTemplate.name;

                if (oname != null && oname.StartsWith("$")) return true;
                if (SkhOptionIds.Contains(oid)) return true;
            }
            return false;
        }

        public static SkhSetType GetSkhSetType(Item item, int gender)
        {
            if (item == null || item.template.type > 4 || item.itemOption == null) return SkhSetType.None;
            
            foreach (var opt in item.itemOption)
            {
                if (opt?.optionTemplate == null) continue;
                int id = opt.optionTemplate.id;

                if (gender == 0) // Trái Đất
                {
                    if (id == 129) return SkhSetType.Earth_Songoku;
                    if (id == 245) return SkhSetType.Earth_ThanVuTruKaio;
                    if (id == 127) return SkhSetType.Earth_ThienXinHang;
                    if (id == 128) return SkhSetType.Earth_Kirin;
                    if (id == 233) return SkhSetType.Gohan;
                }
                else if (gender == 1) // Namek
                {
                    if (id == 130) return SkhSetType.Namek_Picolo;
                    if (id == 132) return SkhSetType.Namek_Daimao;
                    if (id == 131) return SkhSetType.Namek_OcTieu;
                    if (id == 237) return SkhSetType.Namek_Nail;
                    if (id == 233) return SkhSetType.Gohan;
                }
                else // Xayda
                {
                    if (id == 133) return SkhSetType.Saiyan_Kakarot;
                    if (id == 134) return SkhSetType.Saiyan_CaDic;
                    if (id == 241) return SkhSetType.Saiyan_CadicM;
                    if (id == 135) return SkhSetType.Saiyan_Nappa;
                    if (id == 233) return SkhSetType.Gohan;
                }
            }
            return SkhSetType.None;
        }

        public static bool IsGodItem(Item item)
        {
            if (item?.template?.name == null) return false;
            string name = item.template.name.ToLower();
            return name.Contains("thần") || name.Contains("hủy diệt") || name.Contains("thiên sứ");
        }

        public static bool IsStarItem(Item item)
        {
            if (item?.itemOption == null) return false;
            foreach (var opt in item.itemOption)
            {
                if (opt?.optionTemplate?.name != null &&
                    opt.optionTemplate.name.StartsWith("#") && opt.param > 0)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Đếm số ô trống trong hành trang.
        /// </summary>
        public static int GetEmptyBagSlotsCount()
        {
            var bag = Char.myCharz()?.arrItemBag;
            if (bag == null) return 999;
            int count = 0;
            for (int i = 0; i < bag.Length; i++)
            {
                if (bag[i] == null) count++;
            }
            return count;
        }

        /// <summary>
        /// Kiểm tra xem hành trang có bị đầy kín ô không.
        /// </summary>
        public static bool IsBagFull()
        {
            var bag = Char.myCharz()?.arrItemBag;
            if (bag == null) return false;
            for (int i = 0; i < bag.Length; i++)
            {
                if (bag[i] == null) return false;
            }
            return true;
        }

        /// <summary>
        /// Kiểm tra hành trang có chứa loại trang bị/vật phẩm này không.
        /// </summary>
        public static bool HasItemInBag(int templateId)
        {
            return GetItemIndexInBag(templateId) != -1;
        }

        /// <summary>
        /// Tìm slot index của vật phẩm trong hành trang (Để dùng Service.useItem(..., index)). Trả về -1 nếu không có.
        /// </summary>
        public static int GetItemIndexInBag(int templateId)
        {
            var bag = Char.myCharz()?.arrItemBag;
            if (bag == null) return -1;
            for (int i = 0; i < bag.Length; i++)
            {
                if (bag[i] != null && bag[i].template != null && bag[i].template.id == templateId)
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Tìm tổng số lượng vật phẩm (tính gộp) có id chỉ định trong hành trang.
        /// </summary>
        public static int GetItemQuantityInBag(int templateId)
        {
            var bag = Char.myCharz()?.arrItemBag;
            if (bag == null) return 0;
            int total = 0;
            for (int i = 0; i < bag.Length; i++)
            {
                if (bag[i] != null && bag[i].template != null && bag[i].template.id == templateId)
                {
                    total += bag[i].quantity;
                }
            }
            return total;
        }
    }
}
