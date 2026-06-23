using System;
using System.Collections.Generic;
using NRO_v247.Mods.Utils;
using NRO_v247.Mods.Xmap;

namespace NRO_v247.Mods
{
    public partial class AutoTrainFeature : HotReloadFeatureBase<AutoTrainFeature.TrainPanelSettings>, IAutoFeature, ICleanupFeature
    {
        public class TrainPanelSettings
        {
            public bool Enabled;
            public int MapId = -1;
            public bool RequireZone;
            public int ZoneId = -1;
            public bool UseTdlt;
            public bool OnlyUsePunch;
            public bool[] Skills;
            public bool AvoidSuperMob = true;
            public int MobTargetType;
            public bool ChangeLowPlayerZoneIfNoMob;
            public bool CheckLagMob = true;
            public int TrainingArmorMode;
            public bool FreezePunchSkillCd;
            public string MobIdsRaw = string.Empty;
            public bool UseShieldUnderHp;
            public int ShieldHpPercent;
        }

        // ──────────────────────────────────────────────────────────────────
        // Hằng số
        // ──────────────────────────────────────────────────────────────────
        private const int TileSize = 24;
        private const int MoveDistanceThreshold = 50;

        private const long LagMobTimeoutMs = 15000L;
        private const double LagMobHpThreshold = 0.9;
        private const long LagMobClearMs = 30000L;
        private const long AttackCooldownMs = 100;
        private const long AttackCooldownTdltMs = 30;   // Cooldown riêng khi TDLT — KS vàng nhanh hơn
        private const long SkillSwitchCooldownMs = 250;
        private const long UseTdltCooldownMs = 1500;
        private const long FindAttackIntervalMs = 0;
        private const long UseSkillIntervalMs = 100;

        private const long AutoZoneCooldownNoTdltMs = 10500;   // auto zone khi hết quái
        private const long AutoZoneCooldownTdltMs = 5500;      // auto zone có TDLT
        private const long NoMobResetIntervalMs = 10000;       // Đổi khu random nếu không thấy quái hợp lệ trong 10s
        private const long TrainingArmorCooldownMs = 1500;     // cooldown mặc/tháo giáp luyện tập
        private const sbyte BagBodyType = 4;                   // PanelG.BAG_BODY (mặc đồ từ bag lên body)
        private const sbyte BodyBagType = 5;                   // PanelG.BODY_BAG (tháo đồ từ body về bag)
        
        private static readonly int[] PrioritySkillIds = { 17, 9, 0, 2, 4, 1, 5, 3, 6, 8, 12, 13, 19, 21 };
        private static readonly HashSet<int> NoFocusSkillIds = new HashSet<int> { 6, 8, 12, 13, 19, 21 };
        private static readonly HashSet<int> PacketAttackSkillIds = new HashSet<int> { 0, 1, 2, 3, 4, 5, 9, 17 };
        private static readonly HashSet<int> PunchSkillIds = new HashSet<int> { 0, 2, 4 };
        private static readonly Random _zoneRandom = new Random();

        // ──────────────────────────────────────────────────────────────────
        // State cơ bản
        // ──────────────────────────────────────────────────────────────────
        public bool IsTrain = false;
        public bool IsTrainAll = false;
        
        // ── Override Mode dành cho UpZin ─────────────────────────────────
        public bool IsUpZinOverride = false;
        public int UpZinMinHp = 0;
        public int UpZinMaxHp = int.MaxValue;
        
        private Mob _targetMob;
        public List<int> ListMobIds = new List<int>();
        public bool IsAvoidSuperMob = true;
        public bool IsCheckLagMob = true;

        // Cài đặt skill
        public bool OnlyUsePunch = false;
        public bool FreezePunchSkillCd = false;
        public bool[] Skills;

        // Cài đặt map/zone
        public int MapId = -1;
        public bool RequireZone = false;
        public int ZoneId = -1;
        public bool UseTDLT = false;
        public bool ChangeLowPlayerZoneIfNoMob = false;
        public int TrainingArmorMode = 0; 

        public bool AttackHpAbove = false;
        public int AttackHpAboveValue = 0;
        public bool AttackHpBelow = false;
        public int AttackHpBelowValue = 0;
        public bool RotateZone = false;
        public bool UseShieldUnderHp = false;
        public int ShieldHpPercent = 30;
        private readonly List<int> _rotateZoneIds = new List<int>();

        // Vé riêng tư
        public bool UsePrivateTicket = false;
        private long _lastUsePrivateTicketTime = 0L;
        public bool OptimizeKsVang = true;

        public int KsVangAutoZoneMode = 0;
        public int KsVangAutoZoneTrigger = 0;
        public int KsVangAutoZoneTimeMin = 5;
        public bool KsVangFilterPlayer = false;
        public int KsVangPlayerMin = 3;
        public int KsVangPlayerMax = 5;
        public bool KsVangAvoidChars = false;
        public string KsVangAvoidCharsList = "";

        // ──────────────────────────────────────────────────────────────────
        // Lag mob detection
        // ──────────────────────────────────────────────────────────────────
        private long _timeStartAttackMob = 0L;
        private int _currentAttackingMobId = -1;
        private long _lastMobHP = -1L;
        private readonly Dictionary<int, long> _ignoredLagMobs = new Dictionary<int, long>();
        private int _lastMapIdForClearLag = -1;

        // ──────────────────────────────────────────────────────────────────
        // Stats
        // ──────────────────────────────────────────────────────────────────
        private long _goldStart = 0L;
        public long TrainStartTime = 0L;

        private long _lastFindAttackMs = 0L;
        private long _lastUseSkillMs = 0L;

        // ──────────────────────────────────────────────────────────────────
        // Combat state
        // ──────────────────────────────────────────────────────────────────
        private long _lastAttackAtMs = 0L;
        private long _lastSkillSwitchAtMs = 0L;
        private long _lastTrainingArmorActionAtMs = 0L;
        private int _lastFocusMobId = -1;
        private long _lastChangeZoneAtMs = 0L;
        private long _mobDeadWaitUntil = 0L;
        internal bool _isTdltActiveThisFrame = false;
        private bool _lastFreezePunchState = false;
        private long _lastTimeMobSeenMs = 0L;
        // ──────────────────────────────────────────────────────────────────
        // Mob filter
        // ──────────────────────────────────────────────────────────────────
        private readonly HashSet<int> _mobIdSet = new HashSet<int>();
        public int MobTargetType = 0; 

        // ──────────────────────────────────────────────────────────────────
        // Pause
        // ──────────────────────────────────────────────────────────────────
        private bool _isActionPaused = false;
        public static bool IsRequestingZoneList;

        /// <summary>
        /// Check if the base train is running purely for itself, NOT borrowed by Boss, MVBT, NVHN, or UpZin.
        /// </summary>
        public bool IsPureTrainActive()
        {
            if (!IsActive || IsUpZinOverride) return false;
            
            if (ModBootstrap.DailyQuestFeature?.IsControllingTrainRuntime == true) return false;
            
            if (ModBootstrap.MvbtFeature?.IsRequested == true) return false;
            if (ModBootstrap.MhbtFeature?.IsRequested == true) return false;
            if (ModBootstrap.KilisFeature?.IsRequested == true) return false;
            
            return true;
        }

        // ──────────────────────────────────────────────────────────────────
        // Public API
        // ──────────────────────────────────────────────────────────────────
        public bool IsAutoTrainStarted() => (IsTrain || IsUpZinOverride) && !_isActionPaused;

        public bool IsActive => (IsTrain || IsUpZinOverride) && !_isActionPaused;
        public string CurrentState => GetState();

        public void PauseTrainForAction()
        {
            _isActionPaused = true;
            ClearFocus();
            GameScr.isAutoPlay = false;
        }

        public void ResumeTrainAfterAction()
        {
            _isActionPaused = false;
        }

        /// <summary>Được gọi mỗi frame từ AutoMod.</summary>
        public void Update()
        {
            EnsureSettingsApplied();

            if (_isActionPaused)
            {
                GameScr.isAutoPlay = false;
                return;
            }

            AstarTele.gI().Update();

            if (!IsTrain && !IsUpZinOverride)
            {
                GameScr.isAutoPlay = false;
                return;
            }

            KsVangController.OnUpdate(this);

            Char me = Char.myCharz();
            if (me == null)
            {
                GameScr.isAutoPlay = false;
                return;
            }

            if (me.meDead)
            {
                GameScr.isAutoPlay = false;
                AstarTele.gI().Reset();
                return;
            }

            TryHandleTrainingArmor(me);

            GameScr.isAutoPlay = false;

            TdltController.Update(UseTDLT);
            _isTdltActiveThisFrame = TdltController.HasBuff();

            if (!EnsureMapAndZone())
                return;

            // Fix #4: Gộp check xmap + picking vào 1 block duy nhất
            var xmap = ServiceLocator.Get<IXmapService>();
            bool isXmaping = xmap != null && xmap.IsXmaping();
            if (isXmaping)
            {
                ClearFocus();
                AstarTele.gI().Reset();
                return;
            }

            bool isPickingOutOfRange = ModBootstrap.AutoPickFeature != null && ModBootstrap.AutoPickFeature.IsPickingNow;
            if (isPickingOutOfRange)
                return;

            long now = mSystem.currentTimeMillis();

            if (now - _lastUseSkillMs >= UseSkillIntervalMs)
            {
                _lastUseSkillMs = now;
                TryUseNoFocusSkill(me);
            }

            if (now - _lastFindAttackMs >= FindAttackIntervalMs)
            {
                _lastFindAttackMs = now;
                FindAndAttack();
            }

            // Fix #3: Chỉ gọi ApplyFreezePunchSkill khi trạng thái thay đổi (ON→OFF hoặc OFF→ON)
            if (FreezePunchSkillCd != _lastFreezePunchState)
            {
                ApplyFreezePunchSkill(me, FreezePunchSkillCd);
                _lastFreezePunchState = FreezePunchSkillCd;
            }

            if (me.cStamina <= 5 && GameCanvas.gameTick % 140 == 0)
                UseGrape(me);

            ClearIgnoredLagMobs();
        }

        // ──────────────────────────────────────────────────────────────────
        // Settings / Panel
        // ──────────────────────────────────────────────────────────────────
        public void ApplySettingsFromPanel(
            int mapId,
            bool requireZone,
            int zoneId,
            bool useTdlt,
            bool onlyUsePunch,
            bool[] skills,
            bool avoidSuperMob,
            int mobTargetType,
            bool changeLowPlayerZoneIfNoMob,
            bool checkLagMob,
            int trainingArmorMode,
            bool freezePunchSkillCd,
            string mobIdsRaw,
            bool useShield = false,
            int shieldHp = 30)
        {
            UpdateSettings(new TrainPanelSettings
            {
                Enabled = true,
                MapId = mapId,
                RequireZone = requireZone,
                ZoneId = zoneId,
                UseTdlt = useTdlt,
                OnlyUsePunch = onlyUsePunch,
                Skills = skills,
                AvoidSuperMob = avoidSuperMob,
                MobTargetType = mobTargetType,
                ChangeLowPlayerZoneIfNoMob = changeLowPlayerZoneIfNoMob,
                CheckLagMob = checkLagMob,
                TrainingArmorMode = trainingArmorMode,
                FreezePunchSkillCd = freezePunchSkillCd,
                MobIdsRaw = mobIdsRaw ?? string.Empty,
                UseShieldUnderHp = useShield,
                ShieldHpPercent = shieldHp
            });

            // Nếu DailyQuest đang mượn Train để đánh quái, KO override setting vào runtime hiện tại.
            // Chỉ lưu thiết lập từ Panel vào _panelSettings, để khi DailyQuest nhả Train ra thì sẽ tự xài setting này.
            var dq = ModBootstrap.DailyQuestFeature;
            if (dq != null && dq.IsActive && dq.IsControllingTrainRuntime)
            {
                return;
            }

            ApplyPendingSettingsImmediately();
        }

        public void ApplyAdvancedFromPanel(int stepSize, int delay, bool hpAbove, int hpAboveValue, bool hpBelow, int hpBelowValue, bool rotateZone, string rotateZoneListRaw, bool usePrivateTicket = false, bool optimizeKsVang = true, int ksVangMode = 0, int ksVangTrigger = 0, int ksVangTimeMin = 5, bool ksVangFilterPlayer = false, int ksVangPlayerMin = 3, int ksVangPlayerMax = 5, bool ksVangAvoidChars = false, string ksVangAvoidCharsList = "")
        {
            stepSize = Math.Max(1, Math.Min(3, stepSize));
            delay = Math.Max(25, Math.Min(100, delay));
            AstarTele.gI().SetStepSize(stepSize);
            AstarTele.gI().SetDelay(delay);

            AttackHpAbove = hpAbove;
            AttackHpAboveValue = hpAboveValue;
            AttackHpBelow = hpBelow;
            AttackHpBelowValue = hpBelowValue;
            RotateZone = rotateZone;
            UsePrivateTicket = usePrivateTicket;
            OptimizeKsVang = optimizeKsVang;

            KsVangAutoZoneMode = ksVangMode;
            KsVangAutoZoneTrigger = ksVangTrigger;
            KsVangAutoZoneTimeMin = ksVangTimeMin;
            KsVangFilterPlayer = ksVangFilterPlayer;
            KsVangPlayerMin = ksVangPlayerMin;
            KsVangPlayerMax = ksVangPlayerMax;
            KsVangAvoidChars = ksVangAvoidChars;
            KsVangAvoidCharsList = ksVangAvoidCharsList;
            
            _rotateZoneIds.Clear();
            if (!string.IsNullOrWhiteSpace(rotateZoneListRaw))
            {
                string[] parts = rotateZoneListRaw.Split(new char[] { ' ', ';', ',', '|' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string part in parts)
                {
                    if (int.TryParse(part.Trim(), out int zid))
                        _rotateZoneIds.Add(zid);
                }
            }

            ResetRuntimeForHotReload();
        }

        public void DisableFromPanel()
        {
            UpdateSettings(new TrainPanelSettings());

            var dq = ModBootstrap.DailyQuestFeature;
            if (dq != null && dq.IsActive && dq.IsControllingTrainRuntime)
            {
                return;
            }

            ApplyPendingSettingsImmediately();
        }

        public void Cleanup()
        {
            if (UseTDLT) TdltController.Update(false);
        }

        // ──────────────────────────────────────────────────────────────────
        // Trạng thái và Stats
        // ──────────────────────────────────────────────────────────────────
        public string GetState()
        {
            if (!IsTrain) return null;
            if (!(GameCanvas.currentScreen is GameScr))
            {
                if (GameCanvas.currentScreen == null || GameCanvas.currentScreen is ServerListScreen || GameCanvas.currentScreen is LoginScr || GameCanvas.currentScreen is CreateCharScr || !Session_ME.gI().isConnected())
                    return null;
                return "Đang chuyển map bằng tàu vũ trụ...";
            }

            var xmap = ServiceLocator.Get<IXmapService>();
            if (xmap != null && xmap.IsXmaping())
            {
                string xmapState = xmap.GetState();
                return !string.IsNullOrEmpty(xmapState) ? xmapState : "Đang xmap";
            }

            if (MapId >= 0 && TileMap.mapID != MapId) return $"Đang xmap {TileMap.mapID} -> {MapId}";
            if (RequireZone && ZoneId >= 0 && TileMap.zoneID != ZoneId) return $"Đang đổi khu {TileMap.zoneID} -> {ZoneId}";

            Char me = Char.myCharz();
            Mob focus = me?.mobFocus;
            if (focus != null && focus.hp > 0 && focus.status != 0 && focus.status != 1)
                return $"{ResolveMobName(focus)} - {focus.hp} HP";

            // Hiển thị đếm ngược giây khi đang tìm quái
            long elapsed = mSystem.currentTimeMillis() - _lastTimeMobSeenMs;
            int elapsedSec = (int)(elapsed / 1000L);
            if (elapsedSec > 0)
                return $"Train: Đang tìm quái {elapsedSec}s";
            return "Train: Đang tìm quái";
        }

        public long GetGoldEarned()
        {
            try
            {
                Char player = Char.myCharz();
                if (player != null && IsTrain && _goldStart > 0L)
                    return player.xu - _goldStart;
            }
            catch { }
            return 0L;
        }

        public long GetTrainDuration()
        {
            return IsTrain && TrainStartTime > 0L ? mSystem.currentTimeMillis() - TrainStartTime : 0L;
        }

        private void TurnOnAutoTrain()
        {
            // Gỡ bỏ thông báo theo yêu cầu: GameScr.info1?.addInfo(ListMobIds.Count == 0 ? "Tàn sát tất cả: Bật!" : "Tàn sát danh sách: Bật!", 0);
            Char me = Char.myCharz();
            if (me != null) _goldStart = me.xu;
            TrainStartTime = mSystem.currentTimeMillis();
            IsTrain = true;
        }

        protected override bool GetEnabledState(TrainPanelSettings settings) => settings.Enabled;

        protected override void OnSettingsDisabled()
        {
            bool wasUseTdlt = UseTDLT;

            IsTrain = false;
            IsTrainAll = false;
            ListMobIds.Clear();
            _targetMob = null;
            UseTDLT = false;
            ClearFocus();
            AstarTele.gI().Reset();

            if (wasUseTdlt)
                TdltController.Update(false);

            GameScr.isAutoPlay = false;
            ServiceLocator.Get<IXmapService>()?.StopFromPanel();
        }

        protected override void OnSettingsHotReload()
        {
            if (!_settings.Enabled) return; // OnSettingsDisabled() đã xử lý

            bool wasUseTdlt = UseTDLT;

            MapId = _settings.MapId;
            RequireZone = _settings.RequireZone;
            ZoneId = _settings.ZoneId;
            UseTDLT = _settings.UseTdlt;
            OnlyUsePunch = _settings.OnlyUsePunch;
            FreezePunchSkillCd = _settings.FreezePunchSkillCd;
            Skills = _settings.Skills;
            IsAvoidSuperMob = _settings.AvoidSuperMob;
            MobTargetType = _settings.MobTargetType;
            ChangeLowPlayerZoneIfNoMob = _settings.ChangeLowPlayerZoneIfNoMob;
            IsCheckLagMob = _settings.CheckLagMob;
            TrainingArmorMode = (_settings.TrainingArmorMode == 1 || _settings.TrainingArmorMode == 2)
                ? _settings.TrainingArmorMode
                : 0;
            UseShieldUnderHp = _settings.UseShieldUnderHp;
            ShieldHpPercent = _settings.ShieldHpPercent;

            ParseMobIds(_settings.MobIdsRaw);
            IsTrainAll = MobTargetType == 0;

            if (wasUseTdlt && !UseTDLT)
                TdltController.Update(false);

            ResetRuntimeForHotReload();

            if (!IsTrain)
                TurnOnAutoTrain();

            // Gỡ bỏ thông báo theo yêu cầu: GameScr.info1?.addInfo($"Train ON: map {MapId}, zone {(RequireZone ? ZoneId.ToString() : "*")}", 0);
        }

        private void ResetRuntimeForHotReload()
        {
            _targetMob = null;
            _mobDeadWaitUntil = 0L;
            _lastChangeZoneAtMs = 0L;
            _lastFindAttackMs = 0L;
            _lastTimeMobSeenMs = mSystem.currentTimeMillis();
            _lastUseSkillMs = 0L;
            _lastAttackAtMs = 0L;
            _lastSkillSwitchAtMs = 0L;
            _lastTrainingArmorActionAtMs = 0L;
            _lastFocusMobId = -1;
            _timeStartAttackMob = 0L;
            _currentAttackingMobId = -1;
            _lastMobHP = -1L;
            _ignoredLagMobs.Clear();
            _lastMapIdForClearLag = -1;
            ClearFocus();
            AstarTele.gI().Reset();
            GameScr.isAutoPlay = false;
        }

        private bool EnsureMapAndZone()
        {
            // Nếu đang dùng vé riêng tư
            if (UsePrivateTicket)
            {
                // check xem đã ở trong map riêng tư chưa bằng TileMap.vGo (size == 0 nghĩa là ko có trạm)
                bool inPrivateMap = TileMap.vGo != null && TileMap.vGo.size() == 0;
                if (inPrivateMap)
                {
                    return true; // đang trong private map -> luôn cho train tiếp, bỏ qua check MapId/ZoneId hệ thống
                }

                // Chưa vào được map riêng tư (đang ở map thường)
                // Tìm kiếm xem trong hành trang CÓ vé (id 1825) không
                bool hasTicket = false;
                Char me = Char.myCharz();
                if (me != null && me.arrItemBag != null)
                {
                    for (int i = 0; i < me.arrItemBag.Length; i++)
                    {
                        Item item = me.arrItemBag[i];
                        if (item != null && item.template != null && item.template.id == 1825 && item.quantity > 0)
                        {
                            hasTicket = true;
                            long now = mSystem.currentTimeMillis();
                            if (now - _lastUsePrivateTicketTime > 3000L) // Cooldown 3 giây
                            {
                                Service.gI().useItem(0, 1, (sbyte)i, -1);
                                _lastUsePrivateTicketTime = now;
                                GameScr.info1?.addInfo("Auto Train: Đang dùng Vé riêng tư...", 0);
                            }
                            break;
                        }
                    }
                }

                if (hasTicket)
                {
                    ClearFocus();
                    GameScr.isAutoPlay = false;
                    return false; // Đợi vé hiệu lực bay vào trong, không chạy logic đi bộ
                }

                // Cảnh báo hết vé
                GameScr.info1?.addInfo("Auto Train: Hết vé riêng tư (id 1825), chuyển về map thường!", 0);
                // Nếu KHÔNG CÓ vé, rớt xuống logic dưới tự đi MapId/ZoneId như bình thường
            }

            if (MapId >= 0 && TileMap.mapID != MapId)
            {
                if (IsUpZinOverride) return true; // Ép chỉ đánh map hiện tại
                
                ClearFocus();
                GameScr.isAutoPlay = false;
                ServiceLocator.Get<IXmapService>()?.StartGoToMapFromPanel(MapId);
                return false;
            }

            if (RequireZone && ZoneId >= 0 && TileMap.zoneID != ZoneId)
            {
                if (IsUpZinOverride) return true; // Bỏ qua check khu
                
                ClearFocus();
                long now = mSystem.currentTimeMillis();
                long cooldown = _isTdltActiveThisFrame ? AutoZoneCooldownTdltMs : AutoZoneCooldownNoTdltMs;
                if (now - _lastChangeZoneAtMs >= cooldown && !Char.ischangingMap && !Controller.isStopReadMessage)
                {
                    Service.gI().requestChangeZone(ZoneId, -1);
                    _lastChangeZoneAtMs = now;
                }
                return false;
            }
            return true;
        }
    }
}
