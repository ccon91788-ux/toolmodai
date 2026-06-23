namespace Panel.Models;

public class ItemSettings
{
    // ── Dùng item ───────────────────────────────────────────────────────────
    public bool UseCuongNo { get; set; }
    public bool UseBoHuyet { get; set; }
    public bool UseBoKhi { get; set; }
    public bool UseGiapXen { get; set; }
    public bool UseMask { get; set; }
    public bool Use4LeafClover { get; set; }
    public bool UseFood { get; set; }
    public bool UseDetector { get; set; }
    public bool UseItemById { get; set; }
    public string ItemByIds { get; set; } = "";

    // ── Vứt đồ ──────────────────────────────────────────────────────────────
    public bool AutoDrop { get; set; } = true;
    public bool DropByHsd { get; set; }
    public string DropIds { get; set; } = "225\r\n226\r\n19\r\n20\r\n212";

    // ── Cất đồ ──────────────────────────────────────────────────────────────
    public bool AutoStoreWhenFull { get; set; }
    public bool StoreKichHoat { get; set; }
    public bool StoreThanLinh { get; set; }
    public bool StorePhaLe { get; set; }
    public int StoreStarCount { get; set; } = 1;
    public bool StoreCustom { get; set; }
    public string StoreCustomList { get; set; } = "";

    // ── Bán đồ rác ──────────────────────────────────────────────────────────
    /// <summary>Bật/tắt tính năng tự bán đồ rác.</summary>
    public bool AutoSellTrash { get; set; } = false;

    /// <summary>Bán khi túi còn ≤ N ô trống (0 = bán khi đầy túi).</summary>
    public int SellWhenEmptySlots { get; set; } = 0;

    /// <summary>Vứt đồ tại chỗ thay vì đi về trạm bán.</summary>
    public bool DropInsteadOfSell { get; set; }

    // Bộ lọc đồ VIP — giữ lại, KHÔNG bán
    /// <summary>Giữ đồ có sao (* +1 trở lên). Nếu false = được phép bán.</summary>
    public bool KeepStarItems { get; set; } = true;

    /// <summary>Giữ đồ Thần / Hủy diệt / Thiên sứ. Nếu false = được phép bán.</summary>
    public bool KeepGodItems { get; set; } = true;

    /// <summary>Giữ đồ có thuộc tính SKH ($-option hoặc option 127-135/34-36/107). Nếu false = được phép bán.</summary>
    public bool KeepSkhItems { get; set; } = true;

    /// <summary>Level tối đa của đồ thường cho phép bán (bán đồ có level ≤ giá trị này).</summary>
    public int SellMaxLevel { get; set; } = 10;

    /// <summary>Danh sách ID không bán (whitelist), mỗi dòng 1 ID.</summary>
    public string SellKeepIds { get; set; } = "521;1523;1524";

    // Bán theo ID tùy chọn (ưu tiên cao hơn bộ lọc)
    /// <summary>Bỏ qua thuộc tính VIP khi bán theo ID tùy chỉnh.</summary>
    public bool SellCustomNoStarCheck { get; set; }

    /// <summary>Danh sách ID muốn bán kèm số lượng (VD: 380|99), mỗi dòng 1 entry.</summary>
    public string SellCustomIdsList { get; set; } = "";

    // ── Mua đồ ──────────────────────────────────────────────────────────────
    public bool AutoBuyTdlt { get; set; }
    public bool AutoBuyKhauTrang { get; set; }
    public int BuyKhauTrangQty { get; set; }
    public bool AutoBuyCoBonLa { get; set; }
    public int BuyCoBonLaQty { get; set; }
    public bool AutoBuyBuaDe { get; set; }
    public int BuyBuaDeQty { get; set; }
    public bool AutoBuyPrivateTicket { get; set; }
    public bool AutoBuyCustom { get; set; }
    public string BuyCustomList { get; set; } = "";

    // ── Nhặt đồ ─────────────────────────────────────────────────────────────
    public bool AutoPick { get; set; } = true;
    /// <summary>0 = nhặt tất cả, 1 = nhặt theo whitelist (PickIdsList).</summary>
    public int PickMode { get; set; } = 0;
    /// <summary>Chỉ nhặt đồ của mình + đồ không chủ.</summary>
    public bool OnlyMyItems { get; set; }
    public int PickDistance { get; set; } = 75;
    /// <summary>Danh sách ID muốn nhặt (whitelist), dùng khi PickMode=1.</summary>
    public string PickIdsList { get; set; } = "";
    /// <summary>Danh sách ID KHÔNG nhặt (blacklist).</summary>
    public string PickBlackList { get; set; } = "";
}
