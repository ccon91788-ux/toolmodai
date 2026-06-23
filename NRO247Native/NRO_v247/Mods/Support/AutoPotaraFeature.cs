using System;

namespace NRO_v247.Mods.Support
{
    /// <summary>
    /// Tự động dùng bông tai (hợp thể / tách hợp thể).
    /// State: 0 = Không chạy, 1 = Hợp thể, 2 = Tách hợp thể
    /// </summary>
    public class AutoPotaraFeature : IAutoFeature
    {
        // Delay giữa mỗi lần thử (ms) để tránh spam lệnh (10s chuẩn với server)
        private const long ActionCooldownMs = 10000;

        private int _bongTaiState = 0;  // 0=off, 1=hợp thể, 2=tách
        private int _petAction = 3;     // 0=follow,1=guard,2=attack,3=return
        private bool _forceFusionFromDailyQuest = false;

        private long _lastActTime = 0;
        private bool _petActionDone = false;

        // New fields for IsActive and CurrentState
        private bool _enabled = false;
        private string _statusMsg = "";

        public bool IsActive => _enabled;
        public string CurrentState => string.IsNullOrEmpty(_statusMsg) ? "" : _statusMsg;
        public bool IsUtilityTask => true;
        // ID bông tai hợp thể (potara)
        private static readonly int[] PotaraIds = { 454, 921, 1884 };

        public void ApplySettingsFromPanel(int bongTaiState, int petAction)
        {
            _bongTaiState = bongTaiState;
            _petAction    = petAction;
            _petActionDone = false; // reset khi đổi setting
        }

        public void SetDailyQuestFusionOverride(bool enabled)
        {
            _forceFusionFromDailyQuest = enabled;
            if (enabled)
            {
                _petActionDone = false;
            }
        }

        public void Update()
        {
            int effectiveState = _forceFusionFromDailyQuest ? 1 : _bongTaiState;
            if (effectiveState == 0) return;

            var myChar = Char.myCharz();
            if (myChar == null) return;

            long now = Environment.TickCount64;
            if (now - _lastActTime < ActionCooldownMs) return;

            try
            {
                if (effectiveState == 1)
                {
                    // Hợp thể: chỉ dùng khi chưa nhập thể
                    if (!myChar.isNhapThe)
                    {
                        UsePotara(myChar);
                    }
                }
                else if (effectiveState == 2)
                {
                    if (myChar.isNhapThe)
                    {
                        // Đang nhập thể → dùng lại bông tai để tách ra (server xử lý)
                        UsePotara(myChar);
                        _petActionDone = false;
                    }
                    else if (!_petActionDone)
                    {
                        // Vừa tách xong → thực hiện hành động đệ tử
                        ApplyPetAction();
                        _petActionDone = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[AutoPotaraFeature] Error: " + ex.Message);
            }
        }

        private void UsePotara(Char myChar)
        {
            if (myChar.arrItemBag == null) return;

            foreach (var item in myChar.arrItemBag)
            {
                if (item == null) continue;

                bool isPotara = false;
                foreach (int id in PotaraIds)
                {
                    if (item.template.id == id) { isPotara = true; break; }
                }

                if (isPotara)
                {
                    // useItem(sbyte type, sbyte where, sbyte index, short template)
                    // type=0: dùng từ túi đồ, where=1: slot túi, index=vị trí slot UI
                    Service.gI().useItem((sbyte)0, (sbyte)1, (sbyte)item.indexUI, (short)-1);
                    _lastActTime = Environment.TickCount64;
                    break;
                }
            }
        }

        private void ApplyPetAction()
        {
            // petStatus map: 0=đi theo, 1=bảo vệ, 2=tấn công, 3=về nhà
            // _petAction UI: 0=đi theo, 1=bảo vệ, 2=tấn công, 3=về nhà
            sbyte status = (sbyte)_petAction;
            Service.gI().petStatus(status);
            _lastActTime = Environment.TickCount64;
        }
    }
}
