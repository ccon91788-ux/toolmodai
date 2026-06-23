using System.Collections.Generic;

namespace Panel.Models;

public class DauThanSettings
{
    // Xin Đậu
    public bool AutoRequest { get; set; } = false;
    public bool RequestCondition { get; set; } = false;
    public int RequestIfUnder { get; set; } = 0;

    // Cho Đậu
    public bool AutoDonate { get; set; } = false;
    public bool DonateFilter { get; set; } = false;
    public string DonateNames { get; set; } = "";

    // Dùng Đậu (Búp đậu)
    public bool AutoBuffPet { get; set; } = false;
    public int PetHpUnder { get; set; } = 10;
    public int PetKiUnder { get; set; } = 10;

    public bool AutoBuffMaster { get; set; } = false;
    public int MasterHpUnder { get; set; } = 10;
    public int MasterKiUnder { get; set; } = 10;

    public DauThanSettings Clone()
    {
        return new DauThanSettings
        {
            AutoRequest = this.AutoRequest,
            RequestCondition = this.RequestCondition,
            RequestIfUnder = this.RequestIfUnder,
            AutoDonate = this.AutoDonate,
            DonateFilter = this.DonateFilter,
            DonateNames = this.DonateNames,
            AutoBuffPet = this.AutoBuffPet,
            PetHpUnder = this.PetHpUnder,
            PetKiUnder = this.PetKiUnder,
            AutoBuffMaster = this.AutoBuffMaster,
            MasterHpUnder = this.MasterHpUnder,
            MasterKiUnder = this.MasterKiUnder
        };
    }
}
