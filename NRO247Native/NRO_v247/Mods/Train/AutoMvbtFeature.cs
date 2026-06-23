using System;

namespace NRO_v247.Mods
{
    public class AutoMvbtFeature : AutoTrainFeature, IAutoFeature
    {
        private bool _enabled;
        private int _startHour, _startMin;
        private int _stopHour, _stopMin;
        private int _targetCount;

        // --- Explicit IAutoFeature override to guarantee Priority=50 beats Train=0 ---
        int IAutoFeature.Priority => 50;
        bool IAutoFeature.IsRequested => this.IsRequested;
        bool IAutoFeature.IsActive => this.IsActive;
        string IAutoFeature.CurrentState => this.CurrentState;
        void IAutoFeature.Update() => this.Update();

        public new bool IsActive => _enabled;

        public bool IsRequested => IsActive;

        public new string CurrentState => IsRequested ? base.GetState() : "Tắt Auto / Ngoài giờ / Xong mục tiêu MVBT";

        public void ApplyMvbtSettings(
            bool enabled,
            int startHour, int startMin,
            int stopHour, int stopMin,
            int mapId, bool requireZone, int zoneId,
            bool useTdlt, bool onlyUsePunch, bool[] skills, bool avoidSuperMob,
            int mobTargetType, bool changeLowZone, bool checkLag,
            int armorMode, bool freezePunchCd, int targetCount, string mobIdsRaw)
        {
            _enabled = enabled;
            _startHour = startHour;
            _startMin = startMin;
            _stopHour = stopHour;
            _stopMin = stopMin;
            _targetCount = targetCount;
            bool[] actualSkills = skills ?? ModBootstrap.TrainFeature?.Skills;
            bool onlyPunch = ModBootstrap.TrainFeature != null ? ModBootstrap.TrainFeature.OnlyUsePunch : onlyUsePunch;
            bool optimizeKs = false; // Luôn tắt KS vàng cho MVBT

            // Truyền cài đặt combat xuống Base Class (AutoTrainFeature)
            base.ApplySettingsFromPanel(mapId, requireZone, zoneId, useTdlt, onlyPunch, actualSkills, avoidSuperMob, mobTargetType, changeLowZone, checkLag, armorMode, freezePunchCd, mobIdsRaw);
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
                // Ngoài giờ / Tắt: tắt IsTrain để nhường quyền cho AutoTrain
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
