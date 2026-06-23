using System;
using System.Collections.Generic;

namespace NRO_v247.Mods.Support
{
    public class AutoDauThanFeature : NRO_v247.Mods.HotReloadFeatureBase<AutoDauThanFeature.DauThanSettings>, IAutoFeature
    {
        public class DauThanSettings
        {
            public bool AutoRequest;
            public bool RequestCond;
            public int RequestUnder;
            public bool AutoDonate;
            public bool DonateFilter;
            public string DonateNamesRaw = string.Empty;
            public bool AutoBuffMaster;
            public int MHpUnder;
            public int MKiUnder;
            public bool AutoBuffPet;
            public int PHpUnder;
            public int PKiUnder;
        }

        public bool Enabled => _autoRequest || _autoDonate || _autoBuffMaster || _autoBuffPet;
        public string FeatureName => "AutoDauThan_Utility";
        public bool IsActive => _autoRequest || _autoBuffMaster || _autoBuffPet;
        public string CurrentState { get; private set; } = "";
        public bool IsUtilityTask => true;

        // Settings từ Panel: Nhóm Xin Đậu
        internal bool _autoRequest;
        internal bool _requestCond;
        internal int _requestUnder;

        // Settings từ Panel: Nhóm Cho Đậu
        internal bool _autoDonate;
        internal bool _donateFilter;
        internal readonly HashSet<string> _donateNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Settings từ Panel: Nhóm Buff Đậu
        internal bool _autoBuffMaster;
        internal int _mHpUnder;
        internal int _mKiUnder;
        internal bool _autoBuffPet;
        internal int _pHpUnder;
        internal int _pKiUnder;

        // Nội bộ hệ thống
        internal long _lastTimeRequested;
        internal long _lastTimeDonated;

        // Cơ chế State Machine cho Auto Donate
        internal enum DonateState { Idle, GoHome, Harvest, Donate }
        internal DonateState _currentState = DonateState.Idle;
        internal long _lastStateChangeMs;

        private AutoDonateAction _donateAction;

        public AutoDauThanFeature()
        {
            _donateAction = new AutoDonateAction(this);
            // Đăng ký Action Task này vào ModManager
            // Cần delay register hoặc nhờ ModBootstrap gọi, nhưng ta có thể register luân ở constructor
            // Tuy ModManager có thể chưa Init danh sách, ta sẽ register qua ModBootstrap ở bên ngoài.
        }

        public AutoDonateAction GetDonateAction() => _donateAction;

        public void ApplyRequestSettingsFromPanel(bool autoReq, bool reqCond, int reqUnder)
        {
            DauThanSettings next = CloneCurrentSettings();
            next.AutoRequest = autoReq;
            next.RequestCond = reqCond;
            next.RequestUnder = reqUnder;
            UpdateSettings(next);
            ApplyPendingSettingsImmediately();
        }

        public void ApplyDonateSettingsFromPanel(bool autoDon, bool donFilter, string donNamesRaw)
        {
            DauThanSettings next = CloneCurrentSettings();
            next.AutoDonate = autoDon;
            next.DonateFilter = donFilter;
            next.DonateNamesRaw = donNamesRaw ?? string.Empty;
            UpdateSettings(next);
            ApplyPendingSettingsImmediately();
        }

        public void ApplyBuffSettingsFromPanel(bool bMaster, int mHp, int mKi, bool bPet, int pHp, int pKi)
        {
            DauThanSettings next = CloneCurrentSettings();
            next.AutoBuffMaster = bMaster;
            next.MHpUnder = mHp;
            next.MKiUnder = mKi;
            next.AutoBuffPet = bPet;
            next.PHpUnder = pHp;
            next.PKiUnder = pKi;
            UpdateSettings(next);
            ApplyPendingSettingsImmediately();
        }

        public void Update()
        {
            EnsureSettingsApplied();

            if (!Enabled)
            {
                CurrentState = "";
                return;
            }

            Char me = Char.myCharz();
            if (me == null || me.meDead) return;

            long now = mSystem.currentTimeMillis();

            // 1. Logic Xin đậu (Chạy ngầm liên tục)
            if (_autoRequest && (now - _lastTimeRequested > 310000)) // 310s = 5m10s
            {
                bool canRequest = true;
                if (_requestCond)
                {
                    int beanCount = CountBeansInBag(me);
                    if (beanCount >= _requestUnder) canRequest = false;
                }

                if (canRequest)
                {
                    Service.gI().clanMessage(1, "", -1);
                    _lastTimeRequested = now;
                }
            }

            // 2. Logic Buff đậu (Chạy ngầm liên tục)
            if (now % 40 == 0) // Limit check rate
            {
                bool needBuff = false;
                if (_autoBuffMaster && me.cHPFull > 0 && me.cMPFull > 0)
                {
                    long hpPct = me.cHP * 100 / me.cHPFull;
                    long kiPct = me.cMP * 100 / me.cMPFull;
                    if (hpPct < _mHpUnder || kiPct < _mKiUnder) needBuff = true;
                }

                if (!needBuff && _autoBuffPet)
                {
                    Char pet = Char.myPetz();
                    if (pet != null && pet.cHPFull > 0 && pet.cMPFull > 0)
                    {
                        long pHpPct = pet.cHP * 100 / pet.cHPFull;
                        long pKiPct = pet.cMP * 100 / pet.cMPFull;
                        if (pHpPct < _pHpUnder || pKiPct < _pKiUnder) needBuff = true;
                    }
                }

                if (needBuff)
                {
                    if (GameScr.hpPotion > 0) // Có đậu
                    {
                        GameScr.gI().doUseHP();
                    }
                }
            }
        }

        internal int CountBeansInBag(Char me)
        {
            if (me.arrItemBag == null) return 0;
            int total = 0;
            for (int i = 0; i < me.arrItemBag.Length; i++)
            {
                Item item = me.arrItemBag[i];
                if (item != null && item.template != null && item.template.type == 6) // type 6 = Senzu Bean
                {
                    total += item.quantity <= 0 ? 1 : item.quantity;
                }
            }
            return total;
        }

        internal bool IsBagFull(Char myChar)
        {
            if (myChar.arrItemBag == null) return false;
            for (int i = 0; i < myChar.arrItemBag.Length; i++)
            {
                if (myChar.arrItemBag[i] == null) return false;
            }
            return true;
        }

        public void OnPaint(mGraphics g) { }

        protected override void OnSettingsHotReload()
        {
            _autoRequest = _settings.AutoRequest;
            _requestCond = _settings.RequestCond;
            _requestUnder = _settings.RequestUnder;
            _autoDonate = _settings.AutoDonate;
            _donateFilter = _settings.DonateFilter;
            _autoBuffMaster = _settings.AutoBuffMaster;
            _mHpUnder = _settings.MHpUnder;
            _mKiUnder = _settings.MKiUnder;
            _autoBuffPet = _settings.AutoBuffPet;
            _pHpUnder = _settings.PHpUnder;
            _pKiUnder = _settings.PKiUnder;

            _donateNames.Clear();
            if (!string.IsNullOrEmpty(_settings.DonateNamesRaw))
            {
                var names = _settings.DonateNamesRaw.Split(new[] { '\n', '\r', ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var name in names)
                {
                    _donateNames.Add(name.Trim());
                }
            }

            _currentState = DonateState.Idle;
            _lastStateChangeMs = 0L;
            _lastTimeDonated = 0L;
            CurrentState = string.Empty;
        }

        private DauThanSettings CloneCurrentSettings()
        {
            return new DauThanSettings
            {
                AutoRequest = _autoRequest,
                RequestCond = _requestCond,
                RequestUnder = _requestUnder,
                AutoDonate = _autoDonate,
                DonateFilter = _donateFilter,
                DonateNamesRaw = string.Join(",", _donateNames),
                AutoBuffMaster = _autoBuffMaster,
                MHpUnder = _mHpUnder,
                MKiUnder = _mKiUnder,
                AutoBuffPet = _autoBuffPet,
                PHpUnder = _pHpUnder,
                PKiUnder = _pKiUnder,
            };
        }
    }

    /// <summary>
    /// Action Task Độc chiếm, có Priority = 70.
    /// Priority 70 > 60 (Úp Đệ) > 50 (Train Quái).
    /// </summary>
    public class AutoDonateAction : IAutoFeature
    {
        private readonly AutoDauThanFeature _parent;

        public AutoDonateAction(AutoDauThanFeature parent)
        {
            _parent = parent;
        }

        public string FeatureName => "AutoDauThan_Donate";
        public bool IsUtilityTask => false; // ĐỘC CHIẾM
        public int Priority => 70; // Ưu tiên trên Train và Đệ

        // Action này CHỈ YÊU CẦU ĐƯỢC CHẠY KHI:
        // Đã bật _autoDonate VÀ cần phải về nhà / đang thu hoạch / đang donate.
        // Nếu ở trạng thái Idle và chưa cần về nhà, sẽ nhường quyền lại cho Train (IsRequested = false).
        public bool IsRequested
        {
            get
            {
                if (!_parent._autoDonate) return false;
                
                Char me = Char.myCharz();
                if (me == null || me.meDead) return false;

                long now = mSystem.currentTimeMillis();
                int homeMapId = 21 + me.cgender;

                // Nếu đang rảnh rỗi chờ mốc 60s
                if (_parent._currentState == AutoDauThanFeature.DonateState.Idle)
                {
                    // Nếu đang ở nhà thì chiếm luôn để thu hoạch
                    if (TileMap.mapID == homeMapId) return true;

                    // Nếu chưa đến 60s kể từ lần cuối check, nhường quyền (trả về false)
                    if (now - _parent._lastStateChangeMs <= 60000) return false;
                }
                
                return true; // Các trạng thái GoHome, Harvest, Donate đều chiếm quyền!
            }
        }

        public bool IsActive => IsRequested;
        public string CurrentState { get; private set; } = "";

        public void Update()
        {
            if (!IsRequested) 
            {
                CurrentState = "";
                return;
            }

            Char me = Char.myCharz();
            if (me == null || me.meDead) return;

            long now = mSystem.currentTimeMillis();
            int homeMapId = 21 + me.cgender;

            switch (_parent._currentState)
            {
                case AutoDauThanFeature.DonateState.Idle:
                    if (TileMap.mapID == homeMapId)
                    {
                        _parent._currentState = AutoDauThanFeature.DonateState.Harvest;
                        _parent._lastStateChangeMs = now;
                    }
                    else
                    {
                        // Đã quá 60s ở Idle -> Tiến hành về nhà
                        _parent._currentState = AutoDauThanFeature.DonateState.GoHome;
                        _parent._lastStateChangeMs = now;
                    }
                    break;

                case AutoDauThanFeature.DonateState.GoHome:
                    CurrentState = "Đang về nhà cho đậu";
                    
                    if (TileMap.mapID == homeMapId)
                    {
                        _parent._currentState = AutoDauThanFeature.DonateState.Harvest;
                        _parent._lastStateChangeMs = now;
                        break;
                    }
                    
                    // Chưa ở nhà thì gọi Xmap
                    var xmap = ServiceLocator.Get<IXmapService>();
                    if (xmap != null && !xmap.IsXmaping())
                    {
                        xmap.StartGoToMapFromPanel(homeMapId);
                    }
                    else if (xmap == null && GameCanvas.gameTick % 60 == 0)
                    {
                        GameScr.info1.addInfo("Không tìm thấy tính năng XMap!", 0);
                    }
                    break;

                case AutoDauThanFeature.DonateState.Harvest:
                    CurrentState = "Thu hoạch dậu";
                    if (now - _parent._lastStateChangeMs > 2000)
                    {
                        var tree = GameScr.gI().magicTree;
                        if (tree != null && !tree.isUpdate && !tree.isPeasEffect && tree.currPeas > 0)
                        {
                            if (!_parent.IsBagFull(me))
                            {
                                Service.gI().openMenu(4);
                                Service.gI().confirmMenu(4, 0); // Menu Thu hoạch
                            }
                        }
                        
                        _parent._currentState = AutoDauThanFeature.DonateState.Donate;
                        _parent._lastStateChangeMs = now;
                    }
                    break;

                case AutoDauThanFeature.DonateState.Donate:
                    CurrentState = "Đứng check cho đậu";
                    if (now - _parent._lastTimeDonated > 3000) // 3s check donate queue 1 lần
                    {
                        if (ClanMessage.vMessage != null)
                        {
                            for (int i = 0; i < ClanMessage.vMessage.size(); i++)
                            {
                                ClanMessage msg = (ClanMessage)ClanMessage.vMessage.elementAt(i);
                                // type == 1: Xin đậu, recieve < maxCap
                                if (msg.type == 1 && msg.recieve < msg.maxCap && msg.playerId != me.charID)
                                {
                                    bool canDonate = true;
                                    if (_parent._donateFilter)
                                    {
                                        if (!_parent._donateNames.Contains(msg.playerNameClear)) canDonate = false;
                                    }

                                    if (canDonate)
                                    {
                                        Service.gI().clanDonate(msg.id);
                                        _parent._lastTimeDonated = now;
                                        break; // Donate 1 nháy rồi out đợi next vòng
                                    }
                                }
                            }
                        }

                        // Ở lại check một thời gian thôi, để còn đi train
                        if (now - _parent._lastStateChangeMs > 30000) // Đứng cho đậu 30 giây rồi trả quyền đi train
                        {
                            _parent._currentState = AutoDauThanFeature.DonateState.Idle;
                            _parent._lastStateChangeMs = now;
                        }
                    }
                    break;
            }
        }
    }
}
