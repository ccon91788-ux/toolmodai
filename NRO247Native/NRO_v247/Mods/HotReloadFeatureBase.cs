using System.Threading;

namespace NRO_v247.Mods
{
    /// <summary>
    /// Base class cho các feature có hỗ trợ hot-reload settings từ Panel.
    ///
    /// Thread-safety:
    ///   - <see cref="UpdateSettings"/> được gọi từ socket-receive thread (Panel gửi lệnh).
    ///   - <see cref="EnsureSettingsApplied"/> được gọi từ game-update thread (main loop).
    ///   - Dùng <c>volatile</c> cho version counters và <c>Interlocked</c> cho increment để
    ///     đảm bảo visibility đúng mà không cần lock nặng.
    /// </summary>
    public abstract class HotReloadFeatureBase<TSettings>
        where TSettings : new()
    {
        // Dùng Interlocked với _settingsVersion làm memory barrier thay thế cho volatile trên TSettings
        private TSettings _pendingSettings = new TSettings();
        protected TSettings _settings = new TSettings();

        private volatile int _settingsVersion = 0;
        private int _appliedSettingsVersion = -1;

        // ── Enabled tracking cho OnSettingsEnabled / OnSettingsDisabled ──
        private bool _lastKnownEnabled = false;

        // ─────────────────────────────────────────────────────────────────
        // Public API
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Ghi settings mới từ socket thread. Thread-safe.
        /// </summary>
        protected void UpdateSettings(TSettings newSettings)
        {
            _pendingSettings = newSettings;                     // volatile write
            Interlocked.Increment(ref _settingsVersion);        // atomic increment
        }

        /// <summary>
        /// Gọi từ Update() trên game thread để áp dụng settings nếu có pending.
        /// </summary>
        protected void EnsureSettingsApplied()
        {
            int pending = _settingsVersion;                     // volatile read
            if (_appliedSettingsVersion == pending) return;

            _settings = _pendingSettings;                       // lấy snapshot từ pending
            _appliedSettingsVersion = pending;

            bool newEnabled = GetEnabledState(_settings);
            OnSettingsHotReload();

            if (newEnabled && !_lastKnownEnabled)
                OnSettingsEnabled();
            else if (!newEnabled && _lastKnownEnabled)
                OnSettingsDisabled();

            _lastKnownEnabled = newEnabled;
        }

        /// <summary>
        /// Buộc apply lại settings vào lần Update() tiếp theo dù version không đổi.
        /// Hữu ích khi reconnect hoặc cần reset state nội bộ.
        /// </summary>
        public void ForceReapplySettings()
        {
            _appliedSettingsVersion = _settingsVersion - 1;
        }

        /// <summary>
        /// Gọi ngay lập tức (không đợi Update) — dùng sau khi <see cref="UpdateSettings"/>
        /// trong cùng thread để apply ngay mà không chờ frame sau.
        /// </summary>
        protected void ApplyPendingSettingsImmediately()
        {
            EnsureSettingsApplied();
        }

        // ─────────────────────────────────────────────────────────────────
        // Hooks để subclass override
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Gọi mỗi khi settings thay đổi (kể cả Enabled). Override để áp dụng settings vào state.
        /// </summary>
        protected abstract void OnSettingsHotReload();

        /// <summary>
        /// Gọi khi feature chuyển từ disabled → enabled (lần đầu bật).
        /// Mặc định: không làm gì.
        /// </summary>
        protected virtual void OnSettingsEnabled() { }

        /// <summary>
        /// Gọi khi feature chuyển từ enabled → disabled (tắt đi).
        /// Mặc định: không làm gì. Override để cleanup (tắt buff, clear focus...).
        /// </summary>
        protected virtual void OnSettingsDisabled() { }

        /// <summary>
        /// Override để chỉ ra field Enabled trong TSettings.
        /// Mặc định trả về true (luôn coi như enabled nếu không override).
        /// </summary>
        protected virtual bool GetEnabledState(TSettings settings) => true;
    }
}
