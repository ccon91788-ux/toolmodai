using System.Collections.Generic;
using System.Linq;

namespace NRO_v247.Mods
{
    public interface ICleanupFeature
    {
        void Cleanup();
    }

    public class AutoMod
    {
        private List<IAutoFeature> _features = new List<IAutoFeature>();

        private bool _globalAutoEnabled = true;
        private bool _isShuttingDown = false;
        private int _shutdownFrames = 0;
        public static string GlobalOverrideState { get; set; } = "";
        public static string ActivityState { get; private set; } = "";

        public void SetGlobalAutoEnabled(bool enabled)
        {
            if (enabled)
            {
                _globalAutoEnabled = true;
                _isShuttingDown = false;
                GlobalOverrideState = "";
            }
            else
            {
                if (_globalAutoEnabled)
                {
                    _isShuttingDown = true;
                    _shutdownFrames = 10;
                    GlobalOverrideState = "Đang dọn dẹp tắt Auto...";
                }
            }
        }

        public bool IsGlobalAutoEnabled => _globalAutoEnabled;

        public void RegisterFeature(IAutoFeature feature)
        {
            if (!_features.Contains(feature))
            {
                _features.Add(feature);
            }
        }

        public void Update()
        {
            if (!_globalAutoEnabled) return;

            if (_isShuttingDown)
            {
                if (_shutdownFrames > 0)
                {
                    _shutdownFrames--;

                    // FIX 2: Chỉ gọi Cleanup() MỘT LẦN duy nhất (frame đầu tiên shutdown)
                    // _shutdownFrames bắt đầu = 10 → gọi khi == 9 (tức frame đầu tiên giảm xuống)
                    if (_shutdownFrames == 9)
                    {
                        foreach (var feature in _features)
                        {
                            if (feature is ICleanupFeature cleanupFeature)
                            {
                                cleanupFeature.Cleanup();
                            }
                        }
                    }
                }
                else
                {
                    _globalAutoEnabled = false;
                    _isShuttingDown = false;
                    GlobalOverrideState = "";
                    ActivityState = "";
                }
                return;
            }

            List<string> activeStates = new List<string>();

            // 0. Xử lý khi nhân vật chết – áp dụng TOÀN BỘ cho mọi tính năng
            // Chỉ xử lý khi đang ở GameScr (đang in-game), không phải màn hình loading/login
            if (GameCanvas.currentScreen is GameScr)
            {
                Char me = Char.myCharz();
                if (me != null && me.meDead && GameCanvas.gameTick % 30 == 0)
                {
                    if (ModBootstrap.AutoReducePowerFeature?.ShouldOverrideOnDeath() == true)
                    {
                        // Acc chết giảm sức mạnh tự đứng chờ cứu, không dùng action chết mặc định.
                    }
                    else if (ModBootstrap.BossVegetaCityFeature?.HandleOwnDeath(me) != true)
                    {
                        AutoTrainFeature.HandleOnDeath(me);
                    }
                }
            }

            // 1. Chạy tất cả các luồng Song Song (Utility Tasks)
            foreach (var feature in _features.Where(f => f.IsUtilityTask))
            {
                feature.Update();
                if (feature.IsActive && !string.IsNullOrEmpty(feature.CurrentState))
                {
                    activeStates.Add(feature.CurrentState);
                }
            }

            // 2. Chạy DUY NHẤT 1 luồng Độc Chiếm (Action Task) có Priority cao ngất ngưởng
            var activeActionTask = _features
                .Where(f => !f.IsUtilityTask && f.IsRequested)
                .OrderByDescending(f => f.Priority)
                .FirstOrDefault();

            if (activeActionTask != null)
            {
                activeActionTask.Update();
                if (activeActionTask.IsActive && !string.IsNullOrEmpty(activeActionTask.CurrentState))
                {
                    activeStates.Add(activeActionTask.CurrentState);
                }
            }

            if (!string.IsNullOrEmpty(GlobalOverrideState))
            {
                ActivityState = GlobalOverrideState;
            }
            else
            {
                ActivityState = activeStates.Count > 0 ? string.Join(" | ", activeStates) : "";
            }
        }
    }
}
