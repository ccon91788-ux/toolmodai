using System;
using System.Drawing;
using System.Windows.Forms;
using Panel.Models;

namespace Panel
{
    public partial class UpSkhControl : UserControl
    {
        private DateTime _targetTime = DateTime.MinValue;

        public UpSkhControl()
        {
            InitializeComponent();
            SetupSproutUI();
            HookEvents();
        }

        private PictureBox? picSprout;


        private void SetupSproutUI()
        {
            picSprout = new PictureBox();
            picSprout.Size = new Size(24, 24);
            picSprout.Location = new Point(14, 12);
            picSprout.SizeMode = PictureBoxSizeMode.Zoom;
            this.Controls.Add(picSprout);

            lblCountdown.Location = new Point(44, 15);
            lblCountdown.Font = new Font("Consolas", 14F, FontStyle.Bold);

            try
            {
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                using (var stream = assembly.GetManifestResourceStream("Panel.Resource.5427.png"))
                {
                    if (stream != null)
                    {
                        picSprout.Image = Image.FromStream(stream);
                    }
                }
            }
            catch { }
        }

        public event EventHandler? SettingsChanged;


        private void HookEvents()
        {
            chkUsePrivateTicket.CheckedChanged += OnSettingsChanged;
            chkUseTdlt.CheckedChanged += OnSettingsChanged;
            chkUseCoMayMan.CheckedChanged += OnSettingsChanged;

            chkAutoBuyPrivateTicket.CheckedChanged += OnSettingsChanged;
            chkAutoBuyTdlt.CheckedChanged += OnSettingsChanged;
            chkAutoBuyCoMayMan.CheckedChanged += OnSettingsChanged;
        }

        private void OnSettingsChanged(object? sender, EventArgs e)

        {
            if (_isBinding) return;
            SettingsChanged?.Invoke(this, EventArgs.Empty);
        }

        private bool _isBinding = false;

        public void ApplySettings(TrainFeatureSettings train, ItemSettings item)
        {
            _isBinding = true;
            StopCountdown();
            try
            {
                chkUsePrivateTicket.Checked = train.UsePrivateTicket;
                chkUseTdlt.Checked = train.UseTDLT;
                chkUseCoMayMan.Checked = item.Use4LeafClover;

                chkAutoBuyPrivateTicket.Checked = item.AutoBuyPrivateTicket;
                chkAutoBuyTdlt.Checked = item.AutoBuyTdlt;
                chkAutoBuyCoMayMan.Checked = item.AutoBuyCoBonLa;
            }
            finally
            {
                _isBinding = false;
            }
        }

        public void GetSettings(TrainFeatureSettings train, ItemSettings item)
        {
            train.UsePrivateTicket = chkUsePrivateTicket.Checked;
            train.UseTDLT = chkUseTdlt.Checked;
            item.Use4LeafClover = chkUseCoMayMan.Checked;

            item.AutoBuyPrivateTicket = chkAutoBuyPrivateTicket.Checked;
            item.AutoBuyTdlt = chkAutoBuyTdlt.Checked;
            item.AutoBuyCoBonLa = chkAutoBuyCoMayMan.Checked;
        }

        public void StartCountdown(DateTime target)
        {
            _targetTime = target;
            if (_targetTime > DateTime.Now)
            {
                tmrCountdown.Start();
                UpdateCountdownDisplay();
            }
            else
            {
                tmrCountdown.Stop();
                lblCountdown.Text = "Đã hết thời gian mầm";
                lblCountdown.ForeColor = Color.DarkOrange;
                if (picSprout != null) picSprout.Visible = false;
            }
        }

        public void StopCountdown()
        {
            tmrCountdown.Stop();
            lblCountdown.Text = "Đã hết thời gian mầm";
            lblCountdown.ForeColor = Color.DarkOrange;
            if (picSprout != null) picSprout.Visible = false;
        }

        private void TmrCountdown_Tick(object sender, EventArgs e)
        {
            UpdateCountdownDisplay();
        }

        private void UpdateCountdownDisplay()
        {
            TimeSpan remaining = _targetTime - DateTime.Now;
            if (remaining.TotalSeconds <= 0)
            {
                tmrCountdown.Stop();
                lblCountdown.Text = "Đã hết thời gian mầm";
                lblCountdown.ForeColor = Color.DarkOrange;
                if (picSprout != null) picSprout.Visible = false;
            }
            else
            {
                int days = remaining.Days;
                int hours = remaining.Hours;
                int mins = remaining.Minutes;
                int secs = remaining.Seconds;
                lblCountdown.Text = $"{days}d {hours}h {mins}m {secs}s";
                lblCountdown.ForeColor = Color.LimeGreen;
                if (picSprout != null) picSprout.Visible = true;
            }
        }

        public void UpdateSkhData(string[] names, string[] values, int total)
        {
            if (names.Length != 5 || values.Length != 5) return;

            lblSet1.Text = $"{names[0].PadRight(15)} [{values[0]}]";
            lblSet2.Text = $"{names[1].PadRight(15)} [{values[1]}]";
            lblSet3.Text = $"{names[2].PadRight(15)} [{values[2]}]";
            lblSet4.Text = $"{names[3].PadRight(15)} [{values[3]}]";
            lblSet5.Text = $"{names[4].PadRight(15)} [{values[4]}]";

            // Highlight if collection is complete (>= 1 of all 5 slots)
            CheckAndHighlight(lblSet1);
            CheckAndHighlight(lblSet2);
            CheckAndHighlight(lblSet3);
            CheckAndHighlight(lblSet4);
            CheckAndHighlight(lblSet5);

            lblTotalSkh.Text = $"Tổng số món Kích Hoạt đang có: {total}";
        }

        private void CheckAndHighlight(Label lbl)
        {
            // Format expected: "Set Name [1-1-1-1-1]"
            string text = lbl.Text;
            int startIdx = text.IndexOf('[');
            int endIdx = text.IndexOf(']');
            if (startIdx >= 0 && endIdx > startIdx)
            {
                string arrayStr = text.Substring(startIdx + 1, endIdx - startIdx - 1);
                var parts = arrayStr.Split('-');
                bool allComplete = parts.Length == 5;
                if (allComplete)
                {
                    foreach (var part in parts)
                    {
                        if (int.TryParse(part, out int val))
                        {
                            if (val < 1)
                            {
                                allComplete = false;
                                break;
                            }
                        }
                        else
                        {
                            allComplete = false;
                            break;
                        }
                    }
                }

                if (allComplete)
                {
                    lbl.ForeColor = Color.LimeGreen;
                }
                else
                {
                    lbl.ForeColor = SystemColors.ControlText; // Default color
                }
            }
        }
    }
}
