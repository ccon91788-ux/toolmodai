using System;

namespace NRO_v247.Mods.Support
{
    public class AutoPointFeature : NRO_v247.Mods.HotReloadFeatureBase<AutoPointFeature.PointSettings>, IAutoFeature
    {
        public class PointSettings
        {
            public bool AddHP;
            public int TargetHP;
            public bool AddMP;
            public int TargetMP;
            public bool AddDamage;
            public int TargetDamage;
        }

        public bool Enabled => _addHP || _addMP || _addDamage;
        public string FeatureName => "AutoPoint_Utility";
        public bool IsActive => _addHP || _addMP || _addDamage;
        public string CurrentState => "";
        public bool IsUtilityTask => true;
        public int Priority => 100;

        // Settings từ Panel
        internal bool _addHP;
        internal int _targetHP;
        internal bool _addMP;
        internal int _targetMP;
        internal bool _addDamage;
        internal int _targetDamage;

        // Nội bộ hệ thống
        private long _lastAutoPointMs;

        public void ApplySettingsFromPanel(bool addHP, int targetHP, bool addMP, int targetMP, bool addDamage, int targetDamage)
        {
            PointSettings next = new PointSettings
            {
                AddHP = addHP,
                TargetHP = targetHP,
                AddMP = addMP,
                TargetMP = targetMP,
                AddDamage = addDamage,
                TargetDamage = targetDamage
            };
            UpdateSettings(next);
            ApplyPendingSettingsImmediately();
        }

        protected override void OnSettingsHotReload()
        {
            _addHP = _settings.AddHP;
            _targetHP = _settings.TargetHP;
            _addMP = _settings.AddMP;
            _targetMP = _settings.TargetMP;
            _addDamage = _settings.AddDamage;
            _targetDamage = _settings.TargetDamage;
        }

        public void Update()
        {
            if (!Enabled) return;
            var myChar = Char.myCharz();
            if (myChar == null) return;

            if (myChar.cTiemNang <= 0) return;

            // Chỉ gửi lệnh khi thực sự cần cộng điểm
            bool needAddHP = _addHP && myChar.cHPGoc < _targetHP;
            bool needAddMP = _addMP && myChar.cMPGoc < _targetMP;
            bool needAddDamage = _addDamage && myChar.cDamGoc < _targetDamage;

            if (!needAddHP && !needAddMP && !needAddDamage) return;

            long now = mSystem.currentTimeMillis();
            if (now - _lastAutoPointMs < 300) return;
            _lastAutoPointMs = now;

            AutoPoint(myChar);
        }

        private void AutoPoint(Char myChar)
        {
            // Ưu tiên cộng HP trước
            if (_addHP && myChar.cHPGoc < _targetHP)
            {
                if (myChar.cTiemNang > myChar.cHPGoc + 1000)
                {
                    Service.gI().upPotential(0, 1);
                    return;
                }
            }

            // Tiếp theo cộng MP
            if (_addMP && myChar.cMPGoc < _targetMP)
            {
                if (myChar.cTiemNang > myChar.cMPGoc + 1000)
                {
                    Service.gI().upPotential(1, 1);
                    return;
                }
            }

            // Cuối cùng cộng Sức đánh
            if (_addDamage && myChar.cDamGoc < _targetDamage)
            {
                if (myChar.cTiemNang > myChar.cDamGoc * 100)
                {
                    Service.gI().upPotential(2, 1);
                    return;
                }
            }
        }
    }
}
