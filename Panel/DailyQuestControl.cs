using System;
using System.Drawing;
using System.Windows.Forms;
using Panel.Models;

namespace Panel;

public class DailyQuestControl : UserControl
{
    public event EventHandler? SettingsChanged;
    public event EventHandler? ToggleAutoRequested;

    private readonly DailyQuestFeatureSettings _currentSettings = new();
    private bool _isApplying;
    private bool _isRuntimeRunning;

    private readonly TabControl _tabControl;
    private readonly Button _btnToggleEnabled;
    private readonly CheckBox _chkScheduleEnabled;
    private readonly NumericUpDown _numStartHour;
    private readonly NumericUpDown _numStartMinute;
    private readonly ComboBox _cboDifficulty;
    private readonly CheckBox _chkCancelKillPlayer;
    private readonly CheckBox _chkCancelTrainGold;
    private readonly CheckBox _chkCancelTrainMonster;
    private readonly Label _lblStatusValue;
    private readonly Label _lblCompletedValue;
    private readonly Label _lblCanceledValue;

    private readonly CheckBox _chkAutoFusion;
    private readonly ComboBox _cboTrainingArmorMode;
    private readonly CheckBox _chkUseTdlt;
    private readonly CheckBox _chkTdltForMonster;
    private readonly CheckBox _chkTdltForPlayer;
    private readonly CheckBox _chkTdltForGold;

    private readonly NumericUpDown _numKillPlayerMapId;
    private readonly NumericUpDown _numKillPlayerZoneId;
    private readonly CheckBox _chkKillPlayerOnlyListedTargets;
    private readonly TextBox _txtKillPlayerTargetNames;

    private readonly RadioButton _rdoGoldTrain;
    private readonly RadioButton _rdoGoldSuicide;
    private readonly GroupBox _grpGoldTrainSettings;
    private readonly GroupBox _grpGoldSuicideSettings;
    private readonly NumericUpDown _numTrainGoldMapId;
    private readonly CheckBox _chkTrainGoldRequireZone;
    private readonly NumericUpDown _numTrainGoldZoneId;
    private readonly NumericUpDown _numTrainGoldSuicideMapId;
    private readonly NumericUpDown _numTrainGoldSuicideZoneId;

    public DailyQuestControl()
    {
        Dock = DockStyle.Fill;
        BackColor = Color.White;

        _tabControl = new TabControl { Dock = DockStyle.Fill };

        var tabBasic = new TabPage("Cơ bản") { BackColor = Color.White };
        var tabAdvanced = new TabPage("Nâng cao") { BackColor = Color.White };
        var tabKillPlayer = new TabPage("NV: Đánh người") { BackColor = Color.White };
        var tabTrainGold = new TabPage("NV: Úp vàng") { BackColor = Color.White };

        _btnToggleEnabled = new Button
        {
            Location = new Point(12, 8),
            Size = new Size(108, 28),
            FlatStyle = FlatStyle.Standard,
            Text = "Bật Auto"
        };

        _chkScheduleEnabled = new CheckBox
        {
            Text = "Chạy theo khung giờ NVHN",
            AutoSize = true,
            Location = new Point(138, 13)
        };

        var grpSchedule = new GroupBox
        {
            Text = "Khung giờ chạy",
            Location = new Point(12, 42),
            Size = new Size(500, 74)
        };
        _numStartHour = CreateHourNumber(4, new Point(160, 30));
        _numStartMinute = CreateMinuteNumber(0, new Point(270, 30));
        grpSchedule.Controls.Add(new Label { Text = "Bắt đầu lúc:", AutoSize = true, Location = new Point(18, 34) });
        grpSchedule.Controls.Add(_numStartHour);
        grpSchedule.Controls.Add(new Label { Text = "giờ", AutoSize = true, Location = new Point(220, 34) });
        grpSchedule.Controls.Add(_numStartMinute);
        grpSchedule.Controls.Add(new Label { Text = "phút", AutoSize = true, Location = new Point(332, 34) });

        _cboDifficulty = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Location = new Point(145, 130),
            Size = new Size(155, 23)
        };
        _cboDifficulty.Items.AddRange(new object[] { "Dễ", "Khó", "Siêu khó" });

        var grpCancel = new GroupBox
        {
            Text = "Bỏ qua loại nhiệm vụ",
            Location = new Point(12, 168),
            Size = new Size(500, 86)
        };
        _chkCancelKillPlayer = new CheckBox { Text = "Bỏ qua NV đánh người", AutoSize = true, Location = new Point(18, 28) };
        _chkCancelTrainGold = new CheckBox { Text = "Bỏ qua NV nhặt vàng", AutoSize = true, Location = new Point(245, 28) };
        _chkCancelTrainMonster = new CheckBox { Text = "Bỏ qua NV đánh quái", AutoSize = true, Location = new Point(18, 54) };
        grpCancel.Controls.Add(_chkCancelKillPlayer);
        grpCancel.Controls.Add(_chkCancelTrainGold);
        grpCancel.Controls.Add(_chkCancelTrainMonster);

        tabBasic.Controls.Add(_btnToggleEnabled);
        tabBasic.Controls.Add(_chkScheduleEnabled);
        tabBasic.Controls.Add(grpSchedule);
        tabBasic.Controls.Add(new Label { Text = "Mức nhiệm vụ:", AutoSize = true, Location = new Point(12, 134) });
        tabBasic.Controls.Add(_cboDifficulty);
        tabBasic.Controls.Add(grpCancel);

        _chkAutoFusion = new CheckBox { Text = "Tự hợp thể khi làm NVHN", AutoSize = true, Location = new Point(12, 18) };
        _cboTrainingArmorMode = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Location = new Point(86, 46),
            Size = new Size(130, 23)
        };
        _cboTrainingArmorMode.Items.AddRange(new object[] { "Không chạy", "Mặc giáp", "Tháo giáp" });
        _chkUseTdlt = new CheckBox { Text = "Dùng TĐLT khi làm NVHN", AutoSize = true, Location = new Point(12, 78) };

        var grpTdlt = new GroupBox
        {
            Text = "Phạm vi dùng TĐLT",
            Location = new Point(12, 108),
            Size = new Size(500, 94)
        };
        _chkTdltForMonster = new CheckBox { Text = "Chỉ bật TĐLT khi làm đánh quái", AutoSize = true, Location = new Point(16, 24) };
        _chkTdltForPlayer = new CheckBox { Text = "Chỉ bật TĐLT khi làm đánh người", AutoSize = true, Location = new Point(16, 47) };
        _chkTdltForGold = new CheckBox { Text = "Chỉ bật TĐLT khi làm úp vàng", AutoSize = true, Location = new Point(16, 70) };
        grpTdlt.Controls.Add(_chkTdltForMonster);
        grpTdlt.Controls.Add(_chkTdltForPlayer);
        grpTdlt.Controls.Add(_chkTdltForGold);

        tabAdvanced.Controls.Add(_chkAutoFusion);
        tabAdvanced.Controls.Add(new Label { Text = "Giáp LT:", AutoSize = true, Location = new Point(12, 50) });
        tabAdvanced.Controls.Add(_cboTrainingArmorMode);
        tabAdvanced.Controls.Add(_chkUseTdlt);
        tabAdvanced.Controls.Add(grpTdlt);

        _numKillPlayerMapId = CreateGenericNumber(0, 9999, 0, new Point(82, 18));
        _numKillPlayerZoneId = CreateGenericNumber(0, 9999, 0, new Point(82, 52));
        tabKillPlayer.Controls.Add(new Label { Text = "MapID:", AutoSize = true, Location = new Point(12, 22) });
        tabKillPlayer.Controls.Add(_numKillPlayerMapId);
        tabKillPlayer.Controls.Add(new Label { Text = "ZoneID:", AutoSize = true, Location = new Point(12, 56) });
        tabKillPlayer.Controls.Add(_numKillPlayerZoneId);

        var grpKillPlayerList = new GroupBox
        {
            Text = "Đánh theo danh sách",
            Location = new Point(12, 92),
            Size = new Size(500, 206)
        };
        _chkKillPlayerOnlyListedTargets = new CheckBox
        {
            Text = "Chỉ đánh những người trong danh sách",
            AutoSize = true,
            Location = new Point(12, 24)
        };
        grpKillPlayerList.Controls.Add(_chkKillPlayerOnlyListedTargets);
        grpKillPlayerList.Controls.Add(new Label { Text = "(Không tích sẽ đánh tất cả cờ đen trong khu)", AutoSize = true, Location = new Point(28, 46) });
        grpKillPlayerList.Controls.Add(new Label { Text = "(Mỗi tên 1 dòng)", AutoSize = true, Location = new Point(28, 64) });
        _txtKillPlayerTargetNames = new TextBox
        {
            Location = new Point(12, 86),
            Size = new Size(470, 102),
            Multiline = true,
            ScrollBars = ScrollBars.Vertical
        };
        grpKillPlayerList.Controls.Add(_txtKillPlayerTargetNames);
        tabKillPlayer.Controls.Add(grpKillPlayerList);

        _rdoGoldTrain = new RadioButton
        {
            Text = "Train quái lụm vàng",
            AutoSize = true,
            Location = new Point(16, 18)
        };
        _rdoGoldSuicide = new RadioButton
        {
            Text = "Tự sát lụm vàng bản thân",
            AutoSize = true,
            Location = new Point(16, 44)
        };

        _grpGoldTrainSettings = new GroupBox
        {
            Text = "Thiết lập train quái",
            Location = new Point(12, 78),
            Size = new Size(500, 104)
        };
        _numTrainGoldMapId = CreateGenericNumber(-1, 9999, 80, new Point(100, 28));
        _chkTrainGoldRequireZone = new CheckBox
        {
            Text = "Khu",
            AutoSize = true,
            Location = new Point(16, 62)
        };
        _numTrainGoldZoneId = CreateGenericNumber(-1, 9999, -1, new Point(100, 60));
        _grpGoldTrainSettings.Controls.Add(new Label { Text = "MapID:", AutoSize = true, Location = new Point(16, 32) });
        _grpGoldTrainSettings.Controls.Add(_numTrainGoldMapId);
        _grpGoldTrainSettings.Controls.Add(_chkTrainGoldRequireZone);
        _grpGoldTrainSettings.Controls.Add(_numTrainGoldZoneId);

        _grpGoldSuicideSettings = new GroupBox
        {
            Text = "Thiết lập tự sát",
            Location = new Point(12, 78),
            Size = new Size(500, 104)
        };
        _numTrainGoldSuicideMapId = CreateGenericNumber(0, 9999, 44, new Point(100, 24));
        _numTrainGoldSuicideZoneId = CreateGenericNumber(-1, 9999, -1, new Point(100, 58));
        _grpGoldSuicideSettings.Controls.Add(new Label { Text = "MapID:", AutoSize = true, Location = new Point(16, 28) });
        _grpGoldSuicideSettings.Controls.Add(_numTrainGoldSuicideMapId);
        _grpGoldSuicideSettings.Controls.Add(new Label { Text = "ZoneID:", AutoSize = true, Location = new Point(16, 62) });
        _grpGoldSuicideSettings.Controls.Add(_numTrainGoldSuicideZoneId);
        _grpGoldSuicideSettings.Controls.Add(new Label { Text = "(Để -1 là tự chọn khu)", AutoSize = true, Location = new Point(190, 62) });

        tabTrainGold.Controls.Add(_rdoGoldTrain);
        tabTrainGold.Controls.Add(_rdoGoldSuicide);
        tabTrainGold.Controls.Add(_grpGoldTrainSettings);
        tabTrainGold.Controls.Add(_grpGoldSuicideSettings);

        var pnlStatus = new System.Windows.Forms.Panel
        {
            Dock = DockStyle.Bottom,
            Height = 42,
            BackColor = Color.AliceBlue,
            Padding = new Padding(10, 8, 10, 8)
        };
        var statusLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 6,
            RowCount = 1,
            BackColor = Color.AliceBlue
        };
        statusLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        statusLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        statusLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        statusLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70));
        statusLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        statusLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70));

        _lblStatusValue = new Label
        {
            AutoSize = false,
            Dock = DockStyle.Fill,
            ForeColor = Color.DarkSlateBlue,
            Text = "Đang tắt",
            TextAlign = ContentAlignment.MiddleLeft
        };
        _lblCompletedValue = new Label
        {
            AutoSize = false,
            Dock = DockStyle.Fill,
            Text = "0",
            TextAlign = ContentAlignment.MiddleLeft
        };
        _lblCanceledValue = new Label
        {
            AutoSize = false,
            Dock = DockStyle.Fill,
            Text = "0",
            TextAlign = ContentAlignment.MiddleLeft
        };
        statusLayout.Controls.Add(new Label { Text = "Trạng thái:", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 0);
        statusLayout.Controls.Add(_lblStatusValue, 1, 0);
        statusLayout.Controls.Add(new Label { Text = "Hoàn thành:", AutoSize = true, Anchor = AnchorStyles.Left }, 2, 0);
        statusLayout.Controls.Add(_lblCompletedValue, 3, 0);
        statusLayout.Controls.Add(new Label { Text = "Đã hủy:", AutoSize = true, Anchor = AnchorStyles.Left }, 4, 0);
        statusLayout.Controls.Add(_lblCanceledValue, 5, 0);
        pnlStatus.Controls.Add(statusLayout);

        _tabControl.TabPages.Add(tabBasic);
        _tabControl.TabPages.Add(tabAdvanced);
        _tabControl.TabPages.Add(tabKillPlayer);
        _tabControl.TabPages.Add(tabTrainGold);

        Controls.Add(_tabControl);
        Controls.Add(pnlStatus);

        AttachEvents();
        ApplySettings(new DailyQuestFeatureSettings());
        ApplyRuntime(new DailyQuestRuntimeStatus());
    }

    public DailyQuestFeatureSettings CurrentSettings => _currentSettings;

    public void ApplySettings(DailyQuestFeatureSettings? settings)
    {
        settings ??= new DailyQuestFeatureSettings();
        _isApplying = true;

        CopySettings(settings, _currentSettings);

        _chkScheduleEnabled.Checked = settings.ScheduleEnabled;
        _numStartHour.Value = Clamp(_numStartHour, settings.StartHour);
        _numStartMinute.Value = Clamp(_numStartMinute, settings.StartMinute);
        _cboDifficulty.SelectedItem = NormalizeDifficultyForUi(settings.Difficulty);
        if (_cboDifficulty.SelectedIndex < 0)
        {
            _cboDifficulty.SelectedIndex = 2;
        }

        _chkCancelKillPlayer.Checked = settings.CancelKillPlayerQuest;
        _chkCancelTrainGold.Checked = settings.CancelTrainGoldQuest;
        _chkCancelTrainMonster.Checked = settings.CancelTrainMonsterQuest;

        _chkAutoFusion.Checked = settings.AutoFusion;
        _cboTrainingArmorMode.SelectedIndex = settings.TrainingArmorMode >= 0 && settings.TrainingArmorMode < _cboTrainingArmorMode.Items.Count
            ? settings.TrainingArmorMode
            : 0;
        _chkUseTdlt.Checked = settings.UseTdltWhenDoingDailyQuest;
        _chkTdltForMonster.Checked = settings.TdltForTrainMonster;
        _chkTdltForPlayer.Checked = settings.TdltForKillPlayer;
        _chkTdltForGold.Checked = settings.TdltForTrainGold;

        _numKillPlayerMapId.Value = Clamp(_numKillPlayerMapId, settings.KillPlayerMapId < 0 ? 0 : settings.KillPlayerMapId);
        _numKillPlayerZoneId.Value = Clamp(_numKillPlayerZoneId, settings.KillPlayerZoneId < 0 ? 0 : settings.KillPlayerZoneId);
        _chkKillPlayerOnlyListedTargets.Checked = settings.KillPlayerOnlyListedTargets;
        _txtKillPlayerTargetNames.Text = settings.KillPlayerTargetNames ?? string.Empty;

        int trainGoldMapId = settings.TrainGoldMapId > 0 ? settings.TrainGoldMapId : 80;
        _numTrainGoldMapId.Value = Clamp(_numTrainGoldMapId, trainGoldMapId);
        _chkTrainGoldRequireZone.Checked = settings.TrainGoldRequireZone;
        _numTrainGoldZoneId.Value = Clamp(_numTrainGoldZoneId, settings.TrainGoldZoneId);
        _numTrainGoldZoneId.Enabled = settings.TrainGoldRequireZone;
        int trainGoldSuicideMapId = settings.TrainGoldSuicideMapId > 0 ? settings.TrainGoldSuicideMapId : 44;
        _numTrainGoldSuicideMapId.Value = Clamp(_numTrainGoldSuicideMapId, trainGoldSuicideMapId);
        _numTrainGoldSuicideZoneId.Value = Clamp(_numTrainGoldSuicideZoneId, settings.TrainGoldSuicideZoneId);
        _rdoGoldSuicide.Checked = settings.UseGoldSuicideMode;
        _rdoGoldTrain.Checked = !settings.UseGoldSuicideMode;
        UpdateTrainGoldModeLayout();

        _isApplying = false;
    }

    public void ApplyRuntime(DailyQuestRuntimeStatus? runtime)
    {
        runtime ??= new DailyQuestRuntimeStatus();

        _isRuntimeRunning = runtime.IsRunning;
        _lblCompletedValue.Text = runtime.CompletedToday.ToString();
        _lblCanceledValue.Text = runtime.CanceledToday.ToString();

        string statusText = runtime.StateText;
        if (runtime.FinishedToday && string.IsNullOrWhiteSpace(statusText))
        {
            statusText = "Đã xong hôm nay";
        }
        else if (string.IsNullOrWhiteSpace(statusText))
        {
            statusText = runtime.IsRunning ? "Đang chạy" : "Đang tắt";
        }

        if (!string.IsNullOrWhiteSpace(runtime.RunMode) && runtime.IsRunning)
        {
            statusText = $"{statusText} ({runtime.RunMode})";
        }

        _lblStatusValue.Text = statusText;
        UpdateToggleButtonText(runtime.IsRunning);
    }

    private void AttachEvents()
    {
        _chkScheduleEnabled.CheckedChanged += Control_Changed;
        _btnToggleEnabled.Click += ToggleEnabled_Click;
        _numStartHour.ValueChanged += Control_Changed;
        _numStartMinute.ValueChanged += Control_Changed;
        _cboDifficulty.SelectedIndexChanged += Control_Changed;
        _chkCancelKillPlayer.CheckedChanged += Control_Changed;
        _chkCancelTrainGold.CheckedChanged += Control_Changed;
        _chkCancelTrainMonster.CheckedChanged += Control_Changed;
        _chkAutoFusion.CheckedChanged += Control_Changed;
        _cboTrainingArmorMode.SelectedIndexChanged += Control_Changed;
        _chkUseTdlt.CheckedChanged += Control_Changed;
        _chkTdltForMonster.CheckedChanged += Control_Changed;
        _chkTdltForPlayer.CheckedChanged += Control_Changed;
        _chkTdltForGold.CheckedChanged += Control_Changed;
        _numKillPlayerMapId.ValueChanged += Control_Changed;
        _numKillPlayerZoneId.ValueChanged += Control_Changed;
        _chkKillPlayerOnlyListedTargets.CheckedChanged += Control_Changed;
        _txtKillPlayerTargetNames.TextChanged += Control_Changed;
        _numTrainGoldMapId.ValueChanged += Control_Changed;
        _chkTrainGoldRequireZone.CheckedChanged += Control_Changed;
        _numTrainGoldZoneId.ValueChanged += Control_Changed;
        _numTrainGoldSuicideMapId.ValueChanged += Control_Changed;
        _numTrainGoldSuicideZoneId.ValueChanged += Control_Changed;
        _rdoGoldTrain.CheckedChanged += Control_Changed;
        _rdoGoldSuicide.CheckedChanged += Control_Changed;
    }

    private void Control_Changed(object? sender, EventArgs e)
    {
        if (_isApplying)
        {
            return;
        }

        _currentSettings.ScheduleEnabled = _chkScheduleEnabled.Checked;
        _currentSettings.StartHour = (int)_numStartHour.Value;
        _currentSettings.StartMinute = (int)_numStartMinute.Value;
        _currentSettings.Difficulty = NormalizeDifficultyForStorage(_cboDifficulty.SelectedItem?.ToString());
        _currentSettings.CancelKillPlayerQuest = _chkCancelKillPlayer.Checked;
        _currentSettings.CancelTrainGoldQuest = _chkCancelTrainGold.Checked;
        _currentSettings.CancelTrainMonsterQuest = _chkCancelTrainMonster.Checked;

        _currentSettings.AutoFusion = _chkAutoFusion.Checked;
        _currentSettings.TrainingArmorMode = _cboTrainingArmorMode.SelectedIndex < 0 ? 0 : _cboTrainingArmorMode.SelectedIndex;
        _currentSettings.UseTdltWhenDoingDailyQuest = _chkUseTdlt.Checked;
        _currentSettings.TdltForTrainMonster = _chkTdltForMonster.Checked;
        _currentSettings.TdltForKillPlayer = _chkTdltForPlayer.Checked;
        _currentSettings.TdltForTrainGold = _chkTdltForGold.Checked;

        _currentSettings.KillPlayerEnabled = true;
        _currentSettings.KillPlayerMapId = (int)_numKillPlayerMapId.Value;
        _currentSettings.KillPlayerZoneId = (int)_numKillPlayerZoneId.Value;
        _currentSettings.KillPlayerOnlyListedTargets = _chkKillPlayerOnlyListedTargets.Checked;
        _currentSettings.KillPlayerTargetNames = _txtKillPlayerTargetNames.Text;

        _currentSettings.TrainGoldEnabled = true;
        _currentSettings.TrainGoldMapId = (int)_numTrainGoldMapId.Value;
        _currentSettings.TrainGoldRequireZone = _chkTrainGoldRequireZone.Checked;
        _currentSettings.TrainGoldZoneId = _chkTrainGoldRequireZone.Checked ? (int)_numTrainGoldZoneId.Value : -1;
        _currentSettings.TrainGoldSuicideMapId = (int)_numTrainGoldSuicideMapId.Value;
        _currentSettings.TrainGoldSuicideZoneId = (int)_numTrainGoldSuicideZoneId.Value;
        _currentSettings.UseGoldSuicideMode = _rdoGoldSuicide.Checked;

        _numTrainGoldZoneId.Enabled = _chkTrainGoldRequireZone.Checked;
        UpdateTrainGoldModeLayout();
        SettingsChanged?.Invoke(this, EventArgs.Empty);
    }

    private void ToggleEnabled_Click(object? sender, EventArgs e)
    {
        if (_isApplying)
        {
            return;
        }

        ToggleAutoRequested?.Invoke(this, EventArgs.Empty);
    }

    private static void CopySettings(DailyQuestFeatureSettings source, DailyQuestFeatureSettings target)
    {
        target.Enabled = source.Enabled;
        target.EnableSync = true;
        target.Difficulty = source.Difficulty;
        target.TrainMonsterEnabled = source.TrainMonsterEnabled;
        target.TrainMonsterMobNames = source.TrainMonsterMobNames ?? string.Empty;
        target.TrainMonsterMapId = source.TrainMonsterMapId;
        target.TrainMonsterZoneId = source.TrainMonsterZoneId;
        target.TrainGoldEnabled = source.TrainGoldEnabled;
        target.TrainGoldMapId = source.TrainGoldMapId;
        target.TrainGoldRequireZone = source.TrainGoldRequireZone;
        target.TrainGoldZoneId = source.TrainGoldZoneId;
        target.TrainGoldSuicideMapId = source.TrainGoldSuicideMapId;
        target.TrainGoldSuicideZoneId = source.TrainGoldSuicideZoneId;
        target.UseGoldSuicideMode = source.UseGoldSuicideMode;
        target.KillPlayerEnabled = source.KillPlayerEnabled;
        target.KillPlayerMapId = source.KillPlayerMapId;
        target.KillPlayerZoneId = source.KillPlayerZoneId;
        target.KillPlayerOnlyListedTargets = source.KillPlayerOnlyListedTargets;
        target.KillPlayerTargetNames = source.KillPlayerTargetNames ?? string.Empty;
        target.AutoFusion = source.AutoFusion;
        target.TrainingArmorMode = source.TrainingArmorMode;
        target.UseTdltWhenDoingDailyQuest = source.UseTdltWhenDoingDailyQuest;
        target.TdltForTrainMonster = source.TdltForTrainMonster;
        target.TdltForKillPlayer = source.TdltForKillPlayer;
        target.TdltForTrainGold = source.TdltForTrainGold;
        target.CancelUnsupportedQuest = source.CancelUnsupportedQuest;
        target.RetryWhenNoSignal = source.RetryWhenNoSignal;
        target.RetryDelayMs = source.RetryDelayMs;
        target.ActionDelayMs = source.ActionDelayMs;
        target.ScheduleEnabled = source.ScheduleEnabled;
        target.StartHour = source.StartHour;
        target.StartMinute = source.StartMinute;
        target.CancelKillPlayerQuest = source.CancelKillPlayerQuest;
        target.CancelTrainGoldQuest = source.CancelTrainGoldQuest;
        target.CancelTrainMonsterQuest = source.CancelTrainMonsterQuest;
    }

    private static NumericUpDown CreateHourNumber(int value, Point location)
    {
        return new NumericUpDown
        {
            Minimum = 0,
            Maximum = 23,
            Value = value,
            Location = location,
            Size = new Size(52, 23)
        };
    }

    private static NumericUpDown CreateMinuteNumber(int value, Point location)
    {
        return new NumericUpDown
        {
            Minimum = 0,
            Maximum = 59,
            Value = value,
            Location = location,
            Size = new Size(52, 23)
        };
    }

    private static NumericUpDown CreateGenericNumber(int minimum, int maximum, int value, Point location)
    {
        return new NumericUpDown
        {
            Minimum = minimum,
            Maximum = maximum,
            Value = value,
            Location = location,
            Size = new Size(70, 23)
        };
    }

    private static decimal Clamp(NumericUpDown control, int value)
    {
        return Math.Max(control.Minimum, Math.Min(control.Maximum, value));
    }

    private void UpdateToggleButtonText(bool isRunning)
    {
        _btnToggleEnabled.Text = isRunning ? "Tắt Auto" : "Bật Auto";
        _btnToggleEnabled.BackColor = isRunning ? Color.MistyRose : Color.Honeydew;
    }

    private void UpdateTrainGoldModeLayout()
    {
        _grpGoldTrainSettings.Visible = _rdoGoldTrain.Checked;
        _grpGoldSuicideSettings.Visible = _rdoGoldSuicide.Checked;
    }

    private static string NormalizeDifficultyForUi(string? difficulty)
    {
        return (difficulty ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "dễ" => "Dễ",
            "khó" => "Khó",
            _ => "Siêu khó"
        };
    }

    private static string NormalizeDifficultyForStorage(string? difficulty)
    {
        return (difficulty ?? string.Empty).Trim() switch
        {
            "Dễ" => "dễ",
            "Khó" => "khó",
            _ => "siêu khó"
        };
    }
}
