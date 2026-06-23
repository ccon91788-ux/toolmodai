using System;
using System.Drawing;
using System.Windows.Forms;
using Panel.Models;

namespace Panel;

public class AttendanceControl : UserControl
{
    public event EventHandler? SettingsChanged;
    public event EventHandler? ToggleAutoRequested;

    private readonly AttendanceFeatureSettings _currentSettings = new();
    private bool _isApplying;
    private bool _runtimeEnabled;

    private readonly Button _btnToggle;
    private readonly CheckBox _chkAutoStart;
    private readonly CheckBox _chkSchedule;
    private readonly NumericUpDown _nudScheduleHour;
    private readonly NumericUpDown _nudScheduleMinute;
    private readonly CheckBox _chkMonthly;
    private readonly CheckBox _chkContinuous;
    private readonly CheckBox _chkOnline;
    private readonly Label _lblState;
    private readonly Label _lblMonthly;
    private readonly Label _lblContinuous;
    private readonly Label _lblOnline;
    private readonly Label _lblNextOnline;
    private readonly Label _lblLastCheck;
    private readonly System.Windows.Forms.Timer _countdownTimer;

    private static readonly Font FontNormal = new Font("Segoe UI", 10F, FontStyle.Regular);
    private static readonly Font FontBold  = new Font("Segoe UI", 10F, FontStyle.Bold);
    private static readonly Font FontLarge = new Font("Segoe UI", 13F, FontStyle.Bold);
    private static readonly Font FontXLarge = new Font("Segoe UI", 16F, FontStyle.Bold);

    public AttendanceControl()
    {
        Dock = DockStyle.Fill;
        BackColor = Color.White;
        AutoScroll = true;
        AutoScrollMinSize = new Size(0, 520);
        Padding = new Padding(10);

        _btnToggle = new Button
        {
            Text = "Bật Auto Điểm Danh",
            Font = FontBold,
            Location = new Point(15, 15),
            Size = new Size(200, 34),
            BackColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            FlatAppearance = { BorderSize = 2, BorderColor = Color.FromArgb(0, 120, 215) }
        };
        _btnToggle.FlatAppearance.MouseOverBackColor = Color.FromArgb(230, 242, 255);
        _btnToggle.Click += (s, e) => ToggleAutoRequested?.Invoke(this, EventArgs.Empty);

        var grpSettings = new GroupBox
        {
            Text = "Cài đặt điểm danh",
            Font = FontBold,
            Location = new Point(15, 75),
            Size = new Size(560, 170),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(250, 250, 250)
        };

        _chkAutoStart = MakeCheckBox("Tự chạy khi vào game", 20, 28);
        _chkSchedule  = MakeCheckBox("Tự bật lúc", 290, 28);
        _chkSchedule.Size = new Size(115, 26);
        _nudScheduleHour = MakeNumberBox(410, 28, 0, 23);
        _nudScheduleMinute = MakeNumberBox(465, 28, 0, 59);
        _chkMonthly   = MakeCheckBox("Nhận điểm danh tháng", 20, 70);
        _chkContinuous = MakeCheckBox("Nhận liên tục mỗi ngày", 290, 70);
        _chkOnline    = MakeCheckBox("Nhận điểm danh online", 20, 112);
        var lblScheduleSeparator = new Label
        {
            Text = ":",
            Font = FontBold,
            Location = new Point(455, 30),
            Size = new Size(10, 22),
            TextAlign = ContentAlignment.MiddleCenter
        };
        grpSettings.Controls.AddRange(new Control[] { _chkAutoStart, _chkSchedule, _nudScheduleHour, lblScheduleSeparator, _nudScheduleMinute, _chkMonthly, _chkContinuous, _chkOnline });

        var grpStatus = new GroupBox
        {
            Text = "Trạng thái",
            Font = FontBold,
            Location = new Point(15, 260),
            Size = new Size(560, 220),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(250, 250, 250)
        };

        _lblState     = MakeStatusLabel("Trạng thái: Đang tắt",         20, 28);
        _lblMonthly   = MakeStatusLabel("Điểm danh tháng: Chưa biết",    20, 62);
        _lblContinuous = MakeStatusLabel("Liên tục: Chưa biết",          20, 96);
        _lblOnline    = MakeStatusLabel("Online: Chưa biết",             20, 130);
        _lblNextOnline = MakeStatusLabel("Mốc tiếp theo: Chưa biết",    20, 164);
        _lblLastCheck = MakeStatusLabel("Lần kiểm tra: Chưa có",       290, 28);
        grpStatus.Controls.AddRange(new Control[] { _lblState, _lblMonthly, _lblContinuous, _lblOnline, _lblNextOnline, _lblLastCheck });

        Controls.Add(_btnToggle);
        Controls.Add(grpSettings);
        Controls.Add(grpStatus);

        _countdownTimer = new System.Windows.Forms.Timer { Interval = 1000 };
        _countdownTimer.Tick += (s, e) => TickOnlineCountdown();
        _countdownTimer.Start();

        _chkAutoStart.CheckedChanged += RaiseSettingsChanged;
        _chkSchedule.CheckedChanged += RaiseSettingsChanged;
        _nudScheduleHour.ValueChanged += RaiseSettingsChanged;
        _nudScheduleMinute.ValueChanged += RaiseSettingsChanged;
        _chkMonthly.CheckedChanged   += RaiseSettingsChanged;
        _chkContinuous.CheckedChanged += RaiseSettingsChanged;
        _chkOnline.CheckedChanged   += RaiseSettingsChanged;
    }

    private static CheckBox MakeCheckBox(string text, int x, int y)
    {
        return new CheckBox
        {
            Text = text,
            Font = FontNormal,
            AutoSize = false,
            Location = new Point(x, y),
            Size = new Size(260, 26),
            UseVisualStyleBackColor = true
        };
    }

    private static NumericUpDown MakeNumberBox(int x, int y, int min, int max)
    {
        return new NumericUpDown
        {
            Font = FontNormal,
            Location = new Point(x, y),
            Size = new Size(48, 26),
            Minimum = min,
            Maximum = max,
            TextAlign = HorizontalAlignment.Center
        };
    }

    private static Label MakeStatusLabel(string text, int x, int y)
    {
        return new Label
        {
            Text = text,
            Font = FontNormal,
            AutoSize = false,
            Location = new Point(x, y),
            Size = new Size(520, 26),
            ForeColor = Color.FromArgb(30, 30, 30)
        };
    }

    public void ApplySettings(AttendanceFeatureSettings settings)
    {
        _isApplying = true;
        CopyFrom(settings);
        _runtimeEnabled = settings.Enabled;
        _chkAutoStart.Checked   = settings.AutoStart;
        _chkSchedule.Checked    = settings.ScheduleEnabled;
        _nudScheduleHour.Value  = Math.Max(_nudScheduleHour.Minimum, Math.Min(_nudScheduleHour.Maximum, settings.ScheduleHour));
        _nudScheduleMinute.Value = Math.Max(_nudScheduleMinute.Minimum, Math.Min(_nudScheduleMinute.Maximum, settings.ScheduleMinute));
        _chkMonthly.Checked    = settings.ClaimMonthly;
        _chkContinuous.Checked = settings.ClaimContinuous;
        _chkOnline.Checked     = settings.ClaimOnline;
        ApplyRuntime(settings);
        _isApplying = false;
    }

    public AttendanceFeatureSettings GetSettings()
    {
        _currentSettings.Enabled         = _runtimeEnabled;
        _currentSettings.AutoStart       = _chkAutoStart.Checked;
        _currentSettings.ScheduleEnabled = _chkSchedule.Checked;
        _currentSettings.ScheduleHour    = (int)_nudScheduleHour.Value;
        _currentSettings.ScheduleMinute  = (int)_nudScheduleMinute.Value;
        _currentSettings.ClaimMonthly     = _chkMonthly.Checked;
        _currentSettings.ClaimContinuous = _chkContinuous.Checked;
        _currentSettings.ClaimOnline      = _chkOnline.Checked;
        return _currentSettings;
    }

    public void ApplyRuntime(AttendanceFeatureSettings status)
    {
        if (status == null) return;
        _runtimeEnabled = status.Enabled;

        int adjustedNextSeconds = status.NextOnlineSeconds;
        bool isSameOnlineMilestone = status.OnlineClaimedCount == _currentSettings.OnlineClaimedCount
            && string.Equals(status.OnlineClaimDate ?? string.Empty, _currentSettings.OnlineClaimDate ?? string.Empty, StringComparison.Ordinal);
        if (isSameOnlineMilestone
            && _currentSettings.NextOnlineSeconds > 0
            && status.NextOnlineSeconds > _currentSettings.NextOnlineSeconds)
        {
            // Client có thể gửi lại số giây lấy từ popup cũ trong lúc panel đang tự đếm ngược.
            // Giữ số nhỏ hơn tại panel để tránh nhảy qua lại 3m19s -> 3m20s.
            adjustedNextSeconds = _currentSettings.NextOnlineSeconds;
        }

        _currentSettings.Enabled = status.Enabled;
        CopyStatus(status);
        _currentSettings.NextOnlineSeconds = adjustedNextSeconds;

        bool isOn = _runtimeEnabled;
        _btnToggle.Text = isOn ? "Tắt Auto Điểm Danh" : "Bật Auto Điểm Danh";
        _btnToggle.BackColor = isOn ? Color.FromArgb(220, 252, 231) : Color.White;
        _btnToggle.FlatAppearance.BorderColor = isOn ? Color.FromArgb(34, 139, 34) : Color.FromArgb(0, 120, 215);

        string today     = DateTime.Now.ToString("yyyy-MM-dd");
        string monthKey  = DateTime.Now.ToString("yyyyMM");
        string monthlyText    = status.MonthlyClaimedKey == monthKey  ? "Đã nhận tháng này" : "Chưa nhận / Chưa biết";
        string continuousText = status.ContinuousClaimDate == today   ? "Đã nhận hôm nay"    : "Chưa nhận / Chưa biết";

        int claimedOnlineCount = Math.Max(0, _currentSettings.OnlineClaimedCount);
        int nextMilestone = claimedOnlineCount + 1;
        string onlineDateText = claimedOnlineCount > 0
            ? $"Đã nhận {claimedOnlineCount} mốc"
            : "Chưa nhận / Chưa biết";
        string nextText = _currentSettings.NextOnlineSeconds > 0
            ? $"Mốc {nextMilestone}: {FormatNextOnline(_currentSettings.NextOnlineSeconds)}"
            : _currentSettings.CanClaimOnline
                ? $"Mốc {nextMilestone}: có thể nhận ngay"
                : "Không rõ";

        Color onlineColor = claimedOnlineCount > 0 || _currentSettings.OnlineClaimDate == today
            ? Color.FromArgb(34, 139, 34)
            : Color.FromArgb(220, 80, 0);
        Color monthlyColor = status.MonthlyClaimedKey == monthKey
            ? Color.FromArgb(34, 139, 34)
            : Color.FromArgb(30, 30, 30);
        Color continuousColor = status.ContinuousClaimDate == today
            ? Color.FromArgb(34, 139, 34)
            : Color.FromArgb(30, 30, 30);

        _lblState.Text       = $"Trạng thái: {Safe(status.StateText, "Đang tắt")}";
        _lblMonthly.Text      = $"Điểm danh tháng: {monthlyText}";
        _lblMonthly.ForeColor = monthlyColor;
        _lblContinuous.Text   = $"Liên tục: {continuousText}";
        _lblContinuous.ForeColor = continuousColor;
        _lblOnline.Text      = $"Online: {onlineDateText}";
        _lblOnline.ForeColor = onlineColor;
        _lblNextOnline.Text  = $"Mốc tiếp theo: {nextText}";
        _lblLastCheck.Text   = $"Lần kiểm tra: {Safe(status.LastCheckTime, "Chưa có")}";
    }

    private void TickOnlineCountdown()
    {
        if (!_runtimeEnabled || _currentSettings.NextOnlineSeconds <= 0) return;
        _currentSettings.NextOnlineSeconds = Math.Max(0, _currentSettings.NextOnlineSeconds - 1);
        ApplyRuntime(_currentSettings);
    }

    private void RaiseSettingsChanged(object? sender, EventArgs e)
    {
        if (_isApplying) return;
        SettingsChanged?.Invoke(this, EventArgs.Empty);
    }

    private void CopyFrom(AttendanceFeatureSettings settings)
    {
        _currentSettings.Enabled         = settings.Enabled;
        _currentSettings.AutoStart       = settings.AutoStart;
        _currentSettings.ScheduleEnabled = settings.ScheduleEnabled;
        _currentSettings.ScheduleHour    = settings.ScheduleHour;
        _currentSettings.ScheduleMinute  = settings.ScheduleMinute;
        _currentSettings.ClaimMonthly    = settings.ClaimMonthly;
        _currentSettings.ClaimContinuous = settings.ClaimContinuous;
        _currentSettings.ClaimOnline     = settings.ClaimOnline;
        CopyStatus(settings);
    }

    private void CopyStatus(AttendanceFeatureSettings settings)
    {
        _currentSettings.MonthlyClaimedKey    = settings.MonthlyClaimedKey ?? string.Empty;
        _currentSettings.ContinuousClaimDate = settings.ContinuousClaimDate ?? string.Empty;
        _currentSettings.OnlineClaimDate      = settings.OnlineClaimDate ?? string.Empty;
        _currentSettings.OnlineClaimedCount  = settings.OnlineClaimedCount;
        _currentSettings.NextOnlineSeconds    = settings.NextOnlineSeconds;
        _currentSettings.CanClaimOnline       = settings.CanClaimOnline;
        _currentSettings.StateText             = settings.StateText ?? string.Empty;
        _currentSettings.LastCheckTime          = settings.LastCheckTime ?? string.Empty;
    }

    private static string FormatNextOnline(int seconds)
    {
        if (seconds < 0) return "Chưa biết";
        if (seconds == 0) return "Đang kiểm tra...";
        TimeSpan ts = TimeSpan.FromSeconds(seconds);
        if (ts.TotalMinutes >= 1) return $"Còn {(int)ts.TotalMinutes}m {ts.Seconds}s";
        return $"Còn {ts.Seconds}s";
    }

    private static string Safe(string value, string fallback)
    {
        return string.IsNullOrWhiteSpace(value) ? fallback : value;
    }
}
