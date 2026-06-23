using System;

namespace NRO_v247.Mods
{
    public class AutoKilisFeature : AutoTrainFeature, IAutoFeature
    {
        private bool _enabled;
        private int _startHour, _startMin;
        private int _stopHour, _stopMin;
        private bool _autoBuyAmulet;
        private int _amuletType;

        // --- Explicit IAutoFeature override to guarantee Priority=55 beats Mvbt=50 & Train=0 ---
        int IAutoFeature.Priority => 55;
        bool IAutoFeature.IsRequested => this.IsRequested;
        bool IAutoFeature.IsActive => this.IsActive;
        string IAutoFeature.CurrentState => this.CurrentState;
        void IAutoFeature.Update() => this.Update();

        public new bool IsActive => _enabled;

        public bool IsRequested => IsActive;

        public new string CurrentState => IsRequested ? base.GetState() : "Tắt Auto / Ngoài giờ úp Kilis / Xong mục tiêu Kilis";

        public void ApplyKilisSettings(
            bool enabled,
            int startHour, int startMin,
            int stopHour, int stopMin,
            int zoneId,
            bool autoBuyAmulet, int amuletType,
            bool useTdlt, bool autoZone, int armorMode)
        {
            _enabled = enabled;
            _startHour = startHour;
            _startMin = startMin;
            _stopHour = stopHour;
            _stopMin = stopMin;
            _autoBuyAmulet = autoBuyAmulet;
            _amuletType = amuletType;

            int mapId = 168; // Map Kilis
            bool requireZone = true;
            bool onlyUsePunch = false;
            bool[] skills = null;
            bool avoidSuperMob = false;
            int mobTargetType = 0; // Đánh mọi loại
            bool changeLowZone = autoZone;
            bool checkLag = true; // Luôn kiểm tra quái lag
            bool freezePunchCd = false;
            string mobIdsRaw = ""; // Đánh mọi mob
            bool[] actualSkills = ModBootstrap.TrainFeature?.Skills ?? skills;
            bool mainOnlyPunch = ModBootstrap.TrainFeature != null ? ModBootstrap.TrainFeature.OnlyUsePunch : onlyUsePunch;
            bool optimizeKs = false; // Luôn tắt KS vàng cho Kilis

            // Truyền cài đặt combat xuống Base Class (AutoTrainFeature)
            base.ApplySettingsFromPanel(
                mapId, requireZone, zoneId,
                useTdlt, mainOnlyPunch, actualSkills, avoidSuperMob,
                mobTargetType, changeLowZone, checkLag, armorMode,
                freezePunchCd, mobIdsRaw);
            base.ApplyAdvancedFromPanel(3, 30, false, 0, false, 0, false, string.Empty, false, optimizeKs);

            // Nếu disabled hoặc ngoài giờ, tắt trạng thái train nội bộ để nhường quyền
            if (!IsRequested)
            {
                base.IsTrain = false;
            }
        }

        public new void Update()
        {
            if (!IsRequested)
            {
                // Ngoài giờ / Tắt: tắt IsTrain để nhường quyền cho AutoTrain / Mod khác
                if (base.IsTrain)
                {
                    base.IsTrain = false;
                    base.ClearFocus();
                }
                return;
            }

            // Trong giờ: đảm bảo IsTrain=true trước khi gọi base.Update() 
            // (nếu IsTrain=false thì base.Update() sẽ return sớm mà không xmap)
            if (!base.IsTrain)
            {
                base.ApplySettingsFromPanel(
                    base.MapId, base.RequireZone, base.ZoneId,
                    base.UseTDLT, base.OnlyUsePunch, base.Skills,
                    base.IsAvoidSuperMob, base.MobTargetType,
                    base.ChangeLowPlayerZoneIfNoMob, base.IsCheckLagMob,
                    base.TrainingArmorMode, base.FreezePunchSkillCd,
                    string.Join(",", base.ListMobIds));
                // ApplySettingsFromPanel đã gọi TurnOnAutoTrain → IsTrain=true
            }

            base.Update();
        }
    }
}
