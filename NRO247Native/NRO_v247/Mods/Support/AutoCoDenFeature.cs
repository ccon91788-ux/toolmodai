using System;

namespace NRO_v247.Mods.Support
{

    public class AutoCoDenFeature : IAutoFeature
    {
        private const long ActionCooldownMs = 1000;

        private bool _autoCoDen       = false;
        private bool _disableIfOthers = false;
        private bool _dailyQuestOverride = false;
        private int _flagType         = 8;
        private long _lastActTime     = 0;

        public bool IsActive => false;
        public string CurrentState => "";
        public bool IsUtilityTask => true;

        public void ApplySettingsFromPanel(bool autoCoDen, bool disableIfOthers, int flagType = 8)
        {
            _autoCoDen       = autoCoDen;
            _disableIfOthers = disableIfOthers;
            _flagType        = flagType;
        }

        public void SetDailyQuestOverride(bool enabled)
        {
            _dailyQuestOverride = enabled;
        }

        public void Update()
        {
            bool shouldEnableBlackFlag = _autoCoDen || _dailyQuestOverride;
            if (!shouldEnableBlackFlag) return;

            long now = Environment.TickCount64;
            if (now - _lastActTime < ActionCooldownMs) return;

            var myChar = Char.myCharz();
            if (myChar == null) return;

            try
            {
                // Logic Java: Nếu đang Xmap (isComeBack), tự động cất cờ
                var xmap = ServiceLocator.Get<IXmapService>();
                bool isXmapping = xmap != null && xmap.IsXmaping();
                if (isXmapping)
                {
                    if (myChar.cFlag != 0)
                    {
                        Service.gI().getFlag((sbyte)1, (sbyte)0);
                        _lastActTime = now;
                    }
                    return;
                }

                int targetFlag = _dailyQuestOverride ? 8 : _flagType;
                bool othersHaveFlag = !_dailyQuestOverride && _disableIfOthers && CheckOthersHaveFlag();

                // Logic Java (AutoFlag2): Có người trong map cũng có cờ -> Tắt cờ của mình
                if (othersHaveFlag && myChar.cFlag != 0)
                {
                    Service.gI().getFlag((sbyte)1, (sbyte)0);
                    _lastActTime = now;
                }
                // Nếu an toàn và chưa đúng cờ mục tiêu -> Bật cờ
                else if (!othersHaveFlag && myChar.cFlag != targetFlag)
                {
                    Service.gI().getFlag((sbyte)1, (sbyte)targetFlag);
                    _lastActTime = now;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[AutoCoDenFeature] Error: " + ex.Message);
            }
        }

        /// <summary>Kiểm tra cờ đen của bản thân có đang bật không (cFlag == 8).</summary>
        private static bool IsMyBlackFlagOn()
        {
            var myChar = Char.myCharz();
            return myChar != null && myChar.cFlag == 8;
        }

        /// <summary>Kiểm tra xem có người khác trong map đang bật BẤT KỲ cờ nào không (Giống Java).</summary>
        private static bool CheckOthersHaveFlag()
        {
            // Duyệt tất cả Char trong map, bỏ qua nhân vật của mình và phân thân (-myCharId)
            var vChars = GameScr.vCharInMap;
            if (vChars == null) return false;

            int myCharId = Char.myCharz()?.charID ?? -1;
            for (int i = 0; i < vChars.size(); i++)
            {
                var ch = vChars.elementAt(i) as Char;
                if (ch == null || ch.charID == myCharId || ch.charID == -myCharId) continue;
                if (ch.cFlag != 0)
                    return true;
            }
            return false;
        }
    }
}
