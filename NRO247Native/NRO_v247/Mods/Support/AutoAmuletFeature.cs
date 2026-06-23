using System;
using System.Collections.Generic;
using Assets.src.g;
using NRO_v247.Mods;

namespace NRO_v247.Mods.Support
{
    public class AmuletEntry
    {
        public int ItemId { get; set; }
        public bool Enabled { get; set; }
        public float RemainMinutes { get; set; }
        public long ReadAtMs { get; set; }
        public bool BoughtThisCycle { get; set; }

        public AmuletEntry(int itemId, bool enabled)
        {
            ItemId = itemId;
            Enabled = enabled;
            RemainMinutes = -1f;
            ReadAtMs = 0;
            BoughtThisCycle = false;
        }

        public float GetCurrentRemain()
        {
            if (RemainMinutes <= 0)
                return 0f;
            float elapsed = (Environment.TickCount64 - ReadAtMs) / 60000f;
            float r = RemainMinutes - elapsed;
            return r > 0 ? r : 0f;
        }

        public void SetRemain(float minutes)
        {
            RemainMinutes = minutes;
            ReadAtMs = Environment.TickCount64;
        }

        public bool NeedBuy(float bufferMinutes)
        {
            return GetCurrentRemain() <= bufferMinutes;
        }

        public void ResetCycle()
        {
            BoughtThisCycle = false;
        }
    }

    public class AutoAmuletFeature : IAutoFeature
    {
        private const int NPC_ID = 21;
        private const string MENU_BUA = "Cửa hàng Bùa";
        private const int RETRY_MAX = 8;
        private const long TICK_MS = 800L;
        private const long CONFIRM_DELAY_MS = 350L;
        private const float BUY_BUFFER_MIN = 2f;

        private const string DUR_1H = "1 giờ";
        private const string DUR_8H = "8 giờ";
        private const string DUR_1M = "1 tháng";

        private const string MENU_DUR_1H = "Bùa dùng 1 giờ";
        private const string MENU_DUR_8H = "Bùa dùng 8 giờ";
        private const string MENU_DUR_1M = "Bùa dùng 1 tháng";

        private string _globalDuration = DUR_1H;
        private List<AmuletEntry> _amuletList = new List<AmuletEntry>();

        private int _step = 0;
        private int _entryIdx = -1;
        private int _retryCount = 0;
        private long _lastConfirmMs = 0;
        private bool _menuOpened = false;
        private bool _menuConfirmed = false;
        private bool _cacheReady = false;

        private bool _savedTrain = false;
        private bool _savedBuy = false;

        private bool _enabled = false;

        private static AutoAmuletFeature? _instance;

        public static AutoAmuletFeature Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new AutoAmuletFeature();
                return _instance;
            }
        }

        public AutoAmuletFeature()
        {
            InitAmuletList();
        }

        public void Update()
        {
            if (!_enabled)
                return;

            RunStep();
        }

        private void InitAmuletList()
        {
            _amuletList.Clear();
            _amuletList.Add(new AmuletEntry(213, false)); // Bùa Trí Tuệ
            _amuletList.Add(new AmuletEntry(214, false)); // Bùa Mạnh Mẽ
            _amuletList.Add(new AmuletEntry(215, false)); // Bùa Da Trâu
            _amuletList.Add(new AmuletEntry(216, false)); // Bùa Oai Hùng
            _amuletList.Add(new AmuletEntry(217, false)); // Bùa Bất Tử
            _amuletList.Add(new AmuletEntry(218, false)); // Bùa Dẻo Dai
            _amuletList.Add(new AmuletEntry(219, false)); // Bùa Thu Hút
            _amuletList.Add(new AmuletEntry(522, false)); // Bùa Đệ Tử
            _amuletList.Add(new AmuletEntry(671, false)); // Bùa Trí Tuệ x3
            _amuletList.Add(new AmuletEntry(672, false)); // Bùa Trí Tuệ x4
        }

        public void ApplySettingsFromPanel(bool enabled, int durationMode, 
            bool wisdom, bool strong, bool buffaloSkin, bool heroic, bool immortal,
            bool enduring, bool magnet, bool disciple, bool wisdomX3, bool wisdomX4)
        {
            _enabled = enabled;
            _globalDuration = durationMode switch
            {
                1 => DUR_8H,
                2 => DUR_1M,
                _ => DUR_1H
            };

            _amuletList[0].Enabled = wisdom;
            _amuletList[1].Enabled = strong;
            _amuletList[2].Enabled = buffaloSkin;
            _amuletList[3].Enabled = heroic;
            _amuletList[4].Enabled = immortal;
            _amuletList[5].Enabled = enduring;
            _amuletList[6].Enabled = magnet;
            _amuletList[7].Enabled = disciple;
            _amuletList[8].Enabled = wisdomX3;
            _amuletList[9].Enabled = wisdomX4;

            if (enabled)
            {
                // Khi bật, luôn reset cache để đi đọc thời gian
                _cacheReady = false;
            }
        }

        public bool IsActive => _enabled;

        public string CurrentState => _step switch
        {
            0 => "Idle",
            1 => "Đi map bùa",
            2 => "Tìm NPC",
            3 => "Mở menu",
            4 => "Đợi shop",
            5 => "Đọc thời gian",
            6 => "Chọn bùa",
            7 => "Đợi menu thời lượng",
            8 => "Xác nhận mua",
            _ => "Unknown"
        };

        public new int Priority => _step > 0 ? 100 : 0;

        public new bool IsRequested => _enabled && (_step > 0 || !_cacheReady);

        private void RunStep()
        {
            switch (_step)
            {
                case 0:
                    StepIdle();
                    break;
                case 1:
                    StepGoMap();
                    break;
                case 2:
                    StepTeleNpc();
                    break;
                case 3:
                    StepOpenMenu();
                    break;
                case 4:
                    StepWaitShop();
                    break;
                case 5:
                    StepReadTime();
                    break;
                case 6:
                    StepSelectAndBuy();
                    break;
                case 7:
                    StepWaitDurMenu();
                    break;
                case 8:
                    StepAfterBuy();
                    break;
            }
        }

        private void StepIdle()
        {
            if (Char.myCharz() == null)
                return;
            if (!(GameCanvas.currentScreen is GameScr))
                return;
            if (Char.isLoadingMap)
                return;

            // Check AutoBuy/AutoSell đang chạy không
            try
            {
                var buyFeature = ModBootstrap.AutoBuyFeature;
                if (buyFeature != null && buyFeature.IsActive)
                    return;
            }
            catch { }
            try
            {
                var sellFeature = ModBootstrap.AutoSell;
                if (sellFeature != null && sellFeature.IsRunning())
                    return;
            }
            catch { }

            bool needGo = false;
            if (!_cacheReady)
            {
                // Chưa đọc thời gian lần đầu → đi đọc
                needGo = true;
            }
            else
            {
                // Đã đọc thời gian, check xem có bùa nào cần mua không
                float minRemain = float.MaxValue;
                bool hasEnabled = false;
                foreach (var e in _amuletList)
                {
                    if (!e.Enabled)
                        continue;
                    hasEnabled = true;
                    float remain = e.GetCurrentRemain();
                    if (remain < minRemain)
                        minRemain = remain;
                }

                // Nếu không có bùa nào enabled, vẫn đi đọc lại thời gian
                if (!hasEnabled)
                {
                    needGo = true;
                    _cacheReady = false;
                }
                else if (minRemain <= BUY_BUFFER_MIN)
                {
                    needGo = true;
                }
            }

            if (needGo)
            {
                foreach (var e in _amuletList)
                    e.ResetCycle();
                PauseOthers();
                GoTo(1);
            }
        }

        private void StepGoMap()
        {
            if (Char.myCharz() == null)
                return;
            int target = Char.myCharz().cgender + 42;
            if (TileMap.mapID == target)
            {
                _retryCount = 0;
                GoTo(2);
                return;
            }

            // Gọi AutoXmapFeature để xmap
            var xmapFeature = ModBootstrap.XmapFeature;
            if (xmapFeature == null || xmapFeature.IsXmaping())
            {
                if (_retryCount++ >= RETRY_MAX)
                {
                    Abort("Không xmap được");
                    return;
                }
                return;
            }

            xmapFeature.StartGoToMapFromPanel(target);
            _retryCount = 0;
        }

        private void StepTeleNpc()
        {
            if (Char.isLoadingMap)
                return;
            Npc? npc = FindNpc(NPC_ID);
            if (npc == null)
            {
                if (_retryCount++ >= RETRY_MAX)
                {
                    Abort("Không thấy NPC " + NPC_ID);
                    return;
                }
                return;
            }

            Char.myCharz().npcFocus = npc;
            _menuOpened = false;
            _menuConfirmed = false;
            _retryCount = 0;
            GoTo(3);
        }

        private void StepOpenMenu()
        {
            if (IsShopOpen())
            {
                ClearStaleMenu();
                _menuOpened = false;
                _menuConfirmed = false;
                _retryCount = 0;
                GoTo(4);
                return;
            }

            long now = Environment.TickCount64;

            if (!_menuOpened)
            {
                Service.gI().openMenu(NPC_ID);
                _menuOpened = true;
                _menuConfirmed = false;
                _lastConfirmMs = now;
                return;
            }

            if (now - _lastConfirmMs < CONFIRM_DELAY_MS)
                return;

            if (now - _lastConfirmMs >= 2000L)
            {
                if (_retryCount++ >= RETRY_MAX)
                {
                    Abort("Không mở được shop");
                    return;
                }
                Service.gI().confirmMenu((short)NPC_ID, 0);
                if (GameCanvas.menu != null)
                    GameCanvas.menu.doCloseMenu();
                Char.chatPopup = null;
                _menuOpened = false;
                _menuConfirmed = false;
                return;
            }

            if (!_menuConfirmed)
            {
                if (ConfirmByName(MENU_BUA))
                {
                    _menuConfirmed = true;
                    _lastConfirmMs = now;
                }
            }
        }

        private void StepWaitShop()
        {
            if (ShopReady())
            {
                ClearStaleMenu();
                _retryCount = 0;
                GoTo(5);
                return;
            }
            if (_retryCount++ >= RETRY_MAX)
                Abort("Shop không load");
        }

        private void StepReadTime()
        {
            foreach (var e in _amuletList)
            {
                Item? it = FindInShop(e.ItemId);

                if (it == null)
                {
                    if (e.BoughtThisCycle)
                        e.SetRemain(DurationMinutes());
                    else
                        e.SetRemain(0f);
                    continue;
                }

                try
                {
                    if (it.itemOption != null && it.itemOption.Length > 0 && it.itemOption[0] != null)
                    {
                        string str = "";
                        try
                        {
                            str = it.itemOption[0].getOptionString();
                        }
                        catch { }
                        float parsed = ParseRemainStr(str);

                        if (e.BoughtThisCycle && parsed <= BUY_BUFFER_MIN)
                            e.SetRemain(DurationMinutes());
                        else
                            e.SetRemain(parsed);
                    }
                    else
                    {
                        if (e.BoughtThisCycle)
                            e.SetRemain(DurationMinutes());
                        else
                            e.SetRemain(0f);
                    }
                }
                catch
                {
                    e.SetRemain(e.BoughtThisCycle ? DurationMinutes() : 0f);
                }
            }

            _cacheReady = true;
            GoTo(6);
        }

        private float ParseRemainStr(string str)
        {
            if (string.IsNullOrEmpty(str))
                return 0f;
            string s = str.ToLower();
            if (s.Contains("chưa"))
                return 0f;

            int num = 0;
            bool found = false;
            foreach (char c in s)
            {
                if (c >= '0' && c <= '9')
                {
                    num = num * 10 + (c - '0');
                    found = true;
                }
                else if (found)
                    break;
            }
            if (!found || num == 0)
                return 0f;

            if (s.Contains("ngày"))
                return num * 1440f;
            if (s.Contains("giờ") || s.Contains("gio"))
                return num * 60f;
            return (float)num;
        }

        private void StepSelectAndBuy()
        {
            _entryIdx = -1;
            for (int i = 0; i < _amuletList.Count; i++)
            {
                var e = _amuletList[i];
                if (e.Enabled && e.RemainMinutes <= BUY_BUFFER_MIN && !e.BoughtThisCycle)
                {
                    _entryIdx = i;
                    break;
                }
            }

            if (_entryIdx < 0)
            {
                if (GameCanvas.panel != null && GameCanvas.panel.isShow)
                    GameCanvas.panel.hide();
                Done();
                return;
            }

            var entry = _amuletList[_entryIdx];
            ClearStaleMenu();

            int[]? ts = FindTabSlot(entry.ItemId);
            if (ts == null)
            {
                entry.SetRemain(9999f);
                return;
            }

            var panel = GameCanvas.panel;
            if (panel != null && panel.isShow)
            {
                panel.currentTabIndex = ts[0];
                panel.selected = ts[1];
                Service.gI().buyItem(1, entry.ItemId, 0);
                entry.BoughtThisCycle = true;
                _lastConfirmMs = Environment.TickCount64;
                _retryCount = 0;
                GoTo(7);
            }
        }

        private void StepWaitDurMenu()
        {
            long now = Environment.TickCount64;
            if (now - _lastConfirmMs < CONFIRM_DELAY_MS)
                return;

            if (GameCanvas.menu != null && GameCanvas.menu.menuItems != null
                && GameCanvas.menu.menuItems.size() > 0)
            {
                string durText = DurationMenuText();
                if (ConfirmByName(durText))
                {
                    _lastConfirmMs = now;
                    GoTo(8);
                }
                else
                {
                    if (_retryCount++ >= RETRY_MAX)
                        Abort("Không confirm '" + durText + "'");
                }
                return;
            }

            if (now - _lastConfirmMs >= 5000L)
            {
                if (_retryCount++ >= RETRY_MAX)
                {
                    Abort("Không có menu duration");
                    return;
                }
                GoTo(6);
            }
        }

        private void StepAfterBuy()
        {
            // TODO: Xử lý ClientInput nếu có
            // Tạm thời skip

            if (_entryIdx >= 0 && _entryIdx < _amuletList.Count)
            {
                var entry = _amuletList[_entryIdx];
                entry.SetRemain(DurationMinutes());
            }

            _retryCount = 0;
            Done();
        }

        private void ClearStaleMenu()
        {
            try
            {
                if (GameCanvas.menu != null && GameCanvas.menu.menuItems != null
                    && GameCanvas.menu.menuItems.size() > 0)
                {
                    GameCanvas.menu.doCloseMenu();
                    Char.chatPopup = null;
                }
            }
            catch { }
        }

        private bool ConfirmByName(string menuName)
        {
            if (string.IsNullOrEmpty(menuName) || GameCanvas.menu == null || GameCanvas.menu.menuItems == null)
                return false;
            string search = menuName.ToLower().Trim().Replace('\r', ' ').Replace('\n', ' ');
            for (int i = 0; i < GameCanvas.menu.menuItems.size(); i++)
            {
                try
                {
                    var cmd = (Command)GameCanvas.menu.menuItems.elementAt(i);
                    if (cmd == null || cmd.caption == null)
                        continue;
                    string text = cmd.caption.ToLower().Trim().Replace('\r', ' ').Replace('\n', ' ');
                    if (text == search || text.Contains(search))
                    {
                        Service.gI().confirmMenu((short)NPC_ID, (sbyte)i);
                        GameCanvas.menu.doCloseMenu();
                        Char.chatPopup = null;
                        return true;
                    }
                }
                catch { }
            }
            return false;
        }

        private Item? FindInShop(int itemId)
        {
            try
            {
                var shop = Char.myCharz().arrItemShop;
                if (shop == null)
                    return null;
                foreach (var tab in shop)
                {
                    if (tab == null)
                        continue;
                    foreach (var it in tab)
                    {
                        if (it != null && it.template != null && it.template.id == itemId)
                            return it;
                    }
                }
            }
            catch { }
            return null;
        }

        private int[]? FindTabSlot(int itemId)
        {
            try
            {
                var shop = Char.myCharz().arrItemShop;
                if (shop == null)
                    return null;
                for (int t = 0; t < shop.Length; t++)
                {
                    if (shop[t] == null)
                        continue;
                    for (int s = 0; s < shop[t].Length; s++)
                    {
                        var it = shop[t][s];
                        if (it != null && it.template != null && it.template.id == itemId)
                            return new int[] { t, s };
                    }
                }
            }
            catch { }
            return null;
        }

        private bool IsShopOpen()
        {
            return GameCanvas.panel != null && GameCanvas.panel.isShow
                && Char.myCharz() != null && Char.myCharz().arrItemShop != null;
        }

        private bool ShopReady()
        {
            try
            {
                var shop = Char.myCharz().arrItemShop;
                return shop != null && shop.Length > 0 && shop[0] != null && shop[0].Length > 0;
            }
            catch
            {
                return false;
            }
        }

        private Npc? FindNpc(int id)
        {
            try
            {
                if (GameScr.vNpc == null || GameScr.vNpc.size() == 0)
                    return null;

                for (int i = 0; i < GameScr.vNpc.size(); i++)
                {
                    var npc = (Npc)GameScr.vNpc.elementAt(i);
                    if (npc != null && npc.template != null && npc.template.npcTemplateId == id)
                        return npc;
                }
            }
            catch { }
            return null;
        }

        private void PauseOthers()
        {
            try
            {
                var trainFeature = ModBootstrap.TrainFeature;
                if (trainFeature != null && trainFeature.IsAutoTrainStarted())
                {
                    _savedTrain = true;
                    trainFeature.PauseTrainForAction();
                }
            }
            catch { }
            try
            {
                var buyFeature = ModBootstrap.AutoBuyFeature;
                if (buyFeature != null && buyFeature.IsActive)
                {
                    _savedBuy = true;
                    // AutoBuy không có method Stop, chỉ cần set IsActive = false
                    // buyFeature.Stop();
                }
            }
            catch { }
        }

        private void ResumeOthers()
        {
            try
            {
                if (_savedTrain)
                {
                    var trainFeature = ModBootstrap.TrainFeature;
                    if (trainFeature != null)
                        trainFeature.ResumeTrainAfterAction();
                }
            }
            catch { }
            try
            {
                if (_savedBuy)
                {
                    var buyFeature = ModBootstrap.AutoBuyFeature;
                    // AutoBuy không có method Start, chỉ cần set IsActive = true
                    // buyFeature.Start();
                }
            }
            catch { }
            _savedTrain = _savedBuy = false;
        }

        private float DurationMinutes()
        {
            return _globalDuration switch
            {
                DUR_8H => 480f,
                DUR_1M => 43200f,
                _ => 60f
            };
        }

        private string DurationMenuText()
        {
            return _globalDuration switch
            {
                DUR_8H => MENU_DUR_8H,
                DUR_1M => MENU_DUR_1M,
                _ => MENU_DUR_1H
            };
        }

        private void GoTo(int s)
        {
            _step = s;
            _retryCount = 0;
            _lastConfirmMs = 0;
        }

        private void Abort(string msg)
        {
            ResumeOthers();
            Reset();
        }

        private void Done()
        {
            ResumeOthers();
            Reset();
        }

        private void Reset()
        {
            _step = 0;
            _retryCount = 0;
            _entryIdx = -1;
            _lastConfirmMs = 0;
            _menuOpened = false;
            _menuConfirmed = false;
        }

        public bool IsRunning()
        {
            return _step > 0;
        }
    }
}
