namespace NRO_v247.Mods
{
    public interface IAutoFeature
    {
        void Update();
        bool IsActive { get; }
        string CurrentState { get; }

        // --- HỆ THỐNG PHÂN LUỒNG MỚI ---

        /// <summary>
        /// Tác vụ Phụ trợ (Chạy song song) hoặc Tác vụ Hành động (Độc chiếm)
        /// Mặc định là false (Tác vụ Độc chiếm)
        /// Các tính năng cờ đen, vứt đồ cần override cái này thành true.
        /// </summary>
        bool IsUtilityTask => false;

        /// <summary>
        /// Độ ưu tiên của tính năng (Càng cao càng ưu tiên). 
        /// VD: Train = 0, Úp mảnh = 50, Săn Boss = 80, Hồi sinh = 100
        /// </summary>
        int Priority => 0;

        /// <summary>
        /// Tính năng đang yêu cầu được chạy không? (Vd đến hẹn giờ thì trả về true)
        /// Mặc định: Trả về IsActive (nếu bật tính năng là mặc định xin chạy).
        /// </summary>
        bool IsRequested => IsActive;
    }
}
