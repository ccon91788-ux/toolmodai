namespace NRO_v247.Mods
{
    public static class ModBootstrap
    {
        private static bool _initialized = false;
        public static AutoXmapFeature XmapFeature { get; private set; }
        public static AutoTrainFeature TrainFeature { get; private set; }
        public static NRO_v247.Mods.Captcha.AutoCaptchaFeature CaptchaFeature { get; private set; }
        public static NRO_v247.Mods.Items.AutoItemFeature ItemFeature { get; private set; }
        public static NRO_v247.Mods.Items.AutoStoreFeature AutoStore { get; private set; }
        public static NRO_v247.Mods.Items.AutoSellFeature AutoSell { get; private set; }
        public static NRO_v247.Mods.Items.AutoBuyFeature AutoBuyFeature { get; private set; }
        public static NRO_v247.Mods.Items.AutoPickFeature AutoPickFeature { get; private set; }
        public static NRO_v247.Mods.Items.SkhReportFeature SkhReportFeature { get; private set; }
        public static NRO_v247.Mods.Notifications.NotifyCatcher NotifyCatcher { get; private set; }
        // AutoBlackFlagFeature removed
        public static NRO_v247.Mods.Support.AutoPotaraFeature PotaraFeature { get; private set; }
        public static NRO_v247.Mods.Support.AutoCoDenFeature CoDenFeature { get; private set; }
        public static AutoMvbtFeature MvbtFeature { get; private set; }
        public static AutoMhbtFeature MhbtFeature { get; private set; }
        public static AutoKilisFeature KilisFeature { get; private set; }
        public static AutoBossVegetaCityFeature BossVegetaCityFeature { get; private set; }
        public static NRO_v247.Mods.Pet.AutoPetFeature AutoPetFeature { get; private set; }
        public static NRO_v247.Mods.Support.AutoDauThanFeature AutoDauThanFeature { get; private set; }
        public static NRO_v247.Mods.Support.AutoBuffNamekFeature AutoBuffNamekFeature { get; private set; }
        public static NRO_v247.Mods.Support.AutoReducePowerFeature AutoReducePowerFeature { get; private set; }
        public static NRO_v247.Mods.Support.AutoPointFeature AutoPointFeature { get; private set; }
        public static NRO_v247.Mods.Boss.AutoBossFeature AutoBossFeature { get; private set; }
        public static NRO_v247.Mods.UpZin.AutoUpZinFeature UpZinFeature { get; private set; }
        public static NRO_v247.Mods.UpZin.AutoNewbieTaskFeature NewbieTaskFeature { get; private set; }
        public static NRO_v247.Mods.UpZin.AutoUpZinTo700kFeature UpZinTo700kFeature { get; private set; }
        public static NRO_v247.Mods.DailyQuest.AutoDailyQuestFeature DailyQuestFeature { get; private set; }
        public static NRO_v247.Mods.Attendance.AutoAttendanceFeature AttendanceFeature { get; private set; }
        public static NRO_v247.Mods.Support.AutoAmuletFeature AutoAmuletFeature { get; private set; }


        public static NRO_v247.Mods.Notifications.SystemEventLogFeature SystemEventLogFeature { get; private set; }
        public static NRO_v247.Mods.UI.AutoUIFeature UIFeature { get; private set; }

        /// <summary>
        /// Hành động khi nhân vật bị chết:
        /// 0 = Về nhà, 1 = Hồi sinh Ngọc, 2 = HS Ngọc (Về nhà nếu hết ngọc), 3 = Chờ (đứng yên)
        /// </summary>
        public static int ActionOnDeath = 0;

        public static void Init()
        {
            if (_initialized) return;

            // Register all features here
            UIFeature = new NRO_v247.Mods.UI.AutoUIFeature();
            ModManager.AutoMod.RegisterFeature(UIFeature);
            NotifyCatcher = new NRO_v247.Mods.Notifications.NotifyCatcher();
            ModManager.AutoMod.RegisterFeature(NotifyCatcher);

            SystemEventLogFeature = new NRO_v247.Mods.Notifications.SystemEventLogFeature();
            ModManager.AutoMod.RegisterFeature(SystemEventLogFeature);

            XmapFeature = new AutoXmapFeature();
            ServiceLocator.Register<IXmapService>(XmapFeature);
            ModManager.AutoMod.RegisterFeature(XmapFeature);

            TrainFeature = new AutoTrainFeature();
            ModManager.AutoMod.RegisterFeature(TrainFeature);

            CaptchaFeature = new NRO_v247.Mods.Captcha.AutoCaptchaFeature();
            ModManager.AutoMod.RegisterFeature(CaptchaFeature);

            ItemFeature = new NRO_v247.Mods.Items.AutoItemFeature();
            ModManager.AutoMod.RegisterFeature(ItemFeature);

            AutoStore = new NRO_v247.Mods.Items.AutoStoreFeature();
            ModManager.AutoMod.RegisterFeature(AutoStore);

            AutoSell = new NRO_v247.Mods.Items.AutoSellFeature();
            ModManager.AutoMod.RegisterFeature(AutoSell);

            AutoBuyFeature = new NRO_v247.Mods.Items.AutoBuyFeature();
            ModManager.AutoMod.RegisterFeature(AutoBuyFeature);

            AutoPickFeature = new NRO_v247.Mods.Items.AutoPickFeature();
            ModManager.AutoMod.RegisterFeature(AutoPickFeature);

            SkhReportFeature = new NRO_v247.Mods.Items.SkhReportFeature();
            ModManager.AutoMod.RegisterFeature(SkhReportFeature);

            // AutoBlackFlagFeature initialization removed

            PotaraFeature = new NRO_v247.Mods.Support.AutoPotaraFeature();
            ModManager.AutoMod.RegisterFeature(PotaraFeature);

            CoDenFeature = new NRO_v247.Mods.Support.AutoCoDenFeature();
            ModManager.AutoMod.RegisterFeature(CoDenFeature);

            MvbtFeature = new AutoMvbtFeature();
            ModManager.AutoMod.RegisterFeature(MvbtFeature);

            MhbtFeature = new AutoMhbtFeature();
            ModManager.AutoMod.RegisterFeature(MhbtFeature);

            KilisFeature = new AutoKilisFeature();
            ModManager.AutoMod.RegisterFeature(KilisFeature);

            BossVegetaCityFeature = new AutoBossVegetaCityFeature();
            ModManager.AutoMod.RegisterFeature(BossVegetaCityFeature);

            AutoPetFeature = new NRO_v247.Mods.Pet.AutoPetFeature();
            ModManager.AutoMod.RegisterFeature(AutoPetFeature);

            AutoDauThanFeature = new NRO_v247.Mods.Support.AutoDauThanFeature();
            ModManager.AutoMod.RegisterFeature(AutoDauThanFeature);
            ModManager.AutoMod.RegisterFeature(AutoDauThanFeature.GetDonateAction());

            AutoBuffNamekFeature = new NRO_v247.Mods.Support.AutoBuffNamekFeature();
            ModManager.AutoMod.RegisterFeature(AutoBuffNamekFeature);

            AutoReducePowerFeature = new NRO_v247.Mods.Support.AutoReducePowerFeature();
            ModManager.AutoMod.RegisterFeature(AutoReducePowerFeature);

            AutoPointFeature = new NRO_v247.Mods.Support.AutoPointFeature();
            ModManager.AutoMod.RegisterFeature(AutoPointFeature);

            AutoBossFeature = new NRO_v247.Mods.Boss.AutoBossFeature();
            ModManager.AutoMod.RegisterFeature(AutoBossFeature);

            UpZinFeature = new NRO_v247.Mods.UpZin.AutoUpZinFeature();
            ModManager.AutoMod.RegisterFeature(UpZinFeature);

            NewbieTaskFeature = new NRO_v247.Mods.UpZin.AutoNewbieTaskFeature();
            ModManager.AutoMod.RegisterFeature(NewbieTaskFeature);

            UpZinTo700kFeature = new NRO_v247.Mods.UpZin.AutoUpZinTo700kFeature();
            ModManager.AutoMod.RegisterFeature(UpZinTo700kFeature);

            DailyQuestFeature = new NRO_v247.Mods.DailyQuest.AutoDailyQuestFeature();
            ModManager.AutoMod.RegisterFeature(DailyQuestFeature);

            AttendanceFeature = new NRO_v247.Mods.Attendance.AutoAttendanceFeature();
            ModManager.AutoMod.RegisterFeature(AttendanceFeature);

            AutoAmuletFeature = new NRO_v247.Mods.Support.AutoAmuletFeature();
            ModManager.AutoMod.RegisterFeature(AutoAmuletFeature);



            _initialized = true;
        }

        public static bool IsAnyAutoTrainActive()
        {
            if (TrainFeature != null && TrainFeature.IsAutoTrainStarted()) return true;
            if (MvbtFeature != null && MvbtFeature.IsAutoTrainStarted()) return true;
            if (MhbtFeature != null && MhbtFeature.IsAutoTrainStarted()) return true;
            if (KilisFeature != null && KilisFeature.IsAutoTrainStarted()) return true;
            if (BossVegetaCityFeature != null && BossVegetaCityFeature.IsAutoTrainStarted()) return true;
            return false;
        }

        public static bool IsAutoMoveActive()
        {
            if (XmapFeature != null && XmapFeature.IsXmaping()) return true;
            if (TrainFeature != null && TrainFeature.IsAutoTrainStarted()) return true;
            if (MvbtFeature != null && MvbtFeature.IsAutoTrainStarted()) return true;
            if (MhbtFeature != null && MhbtFeature.IsAutoTrainStarted()) return true;
            if (KilisFeature != null && KilisFeature.IsAutoTrainStarted()) return true;
            if (BossVegetaCityFeature != null && BossVegetaCityFeature.IsAutoTrainStarted()) return true;
            if (AutoSell != null && AutoSell.IsRunning()) return true;
            if (AutoBuyFeature != null && AutoBuyFeature.IsActive) return true;
            if (AutoPickFeature != null && AutoPickFeature.Enabled) return true;
            if (NewbieTaskFeature != null && NewbieTaskFeature.IsActive) return true;
            // Thêm check cho tính năng Mới như AutoBoss, Auto Nhiệm Vụ... ở đây trong tương lai
            return false;
        }
    }
}
