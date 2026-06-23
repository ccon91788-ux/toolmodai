using System;

namespace NRO_v247.Mods
{
    public class AutoBossVegetaCityFeature : AutoTrainFeature, IAutoFeature
    {
        private const int BossMapId = 126;
        private const int TargetBossId = 70;
        private const long DeathActionCooldownMs = 150L;

        private enum SessionStage
        {
            Idle,
            Farming
        }

        private bool _enabled;
        private bool _auto15h;
        private bool _auto2230;
        private bool _reviveByGem;
        private bool _useTdlt;
        private int _trainingArmorMode;
        private bool _freezePunchSkillCd;
        private bool _useShieldUnderHp;
        private int _shieldHpPercent = 30;
        private bool[] _skills;

        private SessionStage _stage = SessionStage.Idle;
        private DateTime _activeSpawnTime;
        private int _activeSlotId;
        private int _lastRun15DateKey;
        private int _lastRun2230DateKey;
        private long _lastDeathHandleMs;

        int IAutoFeature.Priority => 56;
        bool IAutoFeature.IsRequested => IsRequested;
        bool IAutoFeature.IsActive => IsActive;
        string IAutoFeature.CurrentState => CurrentState;
        void IAutoFeature.Update() => Update();

        public new bool IsActive => _enabled && _stage != SessionStage.Idle;
        public bool IsRequested => _enabled && (_stage != SessionStage.Idle || IsScheduleWindowOpen(DateTime.Now));

        public new string CurrentState
        {
            get
            {
                if (!_enabled) return "Tắt Boss VegetaCity";
                return _stage switch
                {
                    SessionStage.Farming => $"Boss VegetaCity: map {BossMapId} / boss {TargetBossId}",
                    _ => "Boss VegetaCity: chờ khung giờ"
                };
            }
        }

        public void ApplySettingsFromPanel(
            bool enabled,
            bool auto15h,
            bool auto2230,
            bool reviveByGem,
            bool useTdlt,
            int trainingArmorMode,
            bool freezePunchSkillCd,
            bool useShieldUnderHp,
            int shieldHpPercent,
            bool[] skills)
        {
            _enabled = enabled;
            _auto15h = auto15h;
            _auto2230 = auto2230;
            _reviveByGem = reviveByGem;
            _useTdlt = useTdlt;
            _trainingArmorMode = trainingArmorMode;
            _freezePunchSkillCd = freezePunchSkillCd;
            _useShieldUnderHp = useShieldUnderHp;
            _shieldHpPercent = shieldHpPercent;
            _skills = skills;

            if (!_enabled)
            {
                EndSession();
            }
            else if (_stage != SessionStage.Idle)
            {
                ApplyStageSettings();
            }
        }

        public bool HandleOwnDeath(Char me)
        {
            if (_stage == SessionStage.Idle)
            {
                return false;
            }

            long now = mSystem.currentTimeMillis();
            if (now - _lastDeathHandleMs < DeathActionCooldownMs)
            {
                return true;
            }

            _lastDeathHandleMs = now;

            if (_reviveByGem && me != null && (me.luong + me.luongKhoa) > 0)
            {
                Service.gI().wakeUpFromDead();
            }
            else
            {
                EndSession();
            }

            return true;
        }

        public new void Update()
        {
            if (!_enabled)
            {
                if (base.IsTrain)
                {
                    base.IsTrain = false;
                    base.ClearFocus();
                }
                return;
            }

            TryStartScheduledSession();
            if (_stage == SessionStage.Idle)
            {
                if (base.IsTrain)
                {
                    base.IsTrain = false;
                    base.ClearFocus();
                }
                return;
            }

            if (IsActiveSessionExpired(DateTime.Now))
            {
                EndSession();
                return;
            }

            base.Update();
        }

        private void TryStartScheduledSession()
        {
            if (_stage != SessionStage.Idle)
            {
                return;
            }

            DateTime now = DateTime.Now;
            int todayKey = ToDateKey(now);

            if (_auto15h)
            {
                DateTime standby15 = new DateTime(now.Year, now.Month, now.Day, 14, 58, 0);
                DateTime expire15 = new DateTime(now.Year, now.Month, now.Day, 15, 15, 0);
                if (_lastRun15DateKey != todayKey && now >= standby15 && now < expire15)
                {
                    StartSession(1, new DateTime(now.Year, now.Month, now.Day, 15, 0, 0), todayKey);
                    return;
                }
            }

            if (_auto2230)
            {
                DateTime standby2230 = new DateTime(now.Year, now.Month, now.Day, 22, 28, 0);
                DateTime expire2230 = new DateTime(now.Year, now.Month, now.Day, 22, 45, 0);
                if (_lastRun2230DateKey != todayKey && now >= standby2230 && now < expire2230)
                {
                    StartSession(2, new DateTime(now.Year, now.Month, now.Day, 22, 30, 0), todayKey);
                }
            }
        }

        private bool IsScheduleWindowOpen(DateTime now)
        {
            int todayKey = ToDateKey(now);

            if (_auto15h && _lastRun15DateKey != todayKey)
            {
                DateTime standby15 = new DateTime(now.Year, now.Month, now.Day, 14, 58, 0);
                DateTime expire15 = new DateTime(now.Year, now.Month, now.Day, 15, 15, 0);
                if (now >= standby15 && now < expire15)
                {
                    return true;
                }
            }

            if (_auto2230 && _lastRun2230DateKey != todayKey)
            {
                DateTime standby2230 = new DateTime(now.Year, now.Month, now.Day, 22, 28, 0);
                DateTime expire2230 = new DateTime(now.Year, now.Month, now.Day, 22, 45, 0);
                if (now >= standby2230 && now < expire2230)
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsActiveSessionExpired(DateTime now)
        {
            if (_stage == SessionStage.Idle || _activeSlotId == 0)
            {
                return false;
            }

            DateTime sessionExpireTime = _activeSlotId == 1
                ? new DateTime(now.Year, now.Month, now.Day, 15, 15, 0)
                : new DateTime(now.Year, now.Month, now.Day, 22, 45, 0);

            return now >= sessionExpireTime;
        }

        private void StartSession(int slotId, DateTime spawnTime, int todayKey)
        {
            _activeSlotId = slotId;
            _activeSpawnTime = spawnTime;
            _stage = SessionStage.Farming;

            if (slotId == 1) _lastRun15DateKey = todayKey;
            if (slotId == 2) _lastRun2230DateKey = todayKey;

            ApplyStageSettings();
        }

        private void ApplyStageSettings()
        {
            int mapId = BossMapId;
            string mobIdsRaw = TargetBossId.ToString();
            int mobTargetType = 1;

            base.ApplySettingsFromPanel(
                mapId,
                false,
                -1,
                _useTdlt,
                false,
                _skills,
                false,
                mobTargetType,
                false,
                true,
                _trainingArmorMode,
                _freezePunchSkillCd,
                mobIdsRaw,
                _useShieldUnderHp,
                _shieldHpPercent);

            base.ApplyAdvancedFromPanel(3, 30, false, 0, false, 0, false, string.Empty, false);
        }

        private void EndSession()
        {
            _stage = SessionStage.Idle;
            _activeSlotId = 0;
            _activeSpawnTime = DateTime.MinValue;
            base.Cleanup();
            base.DisableFromPanel();
            base.ClearFocus();
        }

        private static int ToDateKey(DateTime dateTime)
        {
            return dateTime.Year * 10000 + dateTime.Month * 100 + dateTime.Day;
        }
    }
}
