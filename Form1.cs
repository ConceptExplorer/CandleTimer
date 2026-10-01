using System;
using System.Drawing;
using System.Media;
using System.Threading.Tasks;
using System.Windows.Forms;
using CandleTimer.Services;

namespace CandleTimer
{
    public partial class Form1 : Form
    {
        private System.Windows.Forms.Timer clockTimer = null!;
        private Label lblTimer = null!;
        private NumericUpDown numMinutes = null!;
        private Label lblMinutesTag = null!;
        private Label lblCompactTimeframe = null!;
        private CheckBox chkAlwaysOnTop = null!;
        private CheckBox chkSound = null!;
        private CheckBox chkColorWarning = null!;
        private Button btnSync = null!;
        private Button btnCompact = null!;

        private readonly NtpService ntpService = new NtpService();
        private int selectedIntervalSeconds = 60;
        private bool isCompact = false;

        public Form1()
        {
            InitializeComponent();
            SetupCustomUI();

            // Initialize timer directly or pass components if available
            clockTimer = components != null
                ? new System.Windows.Forms.Timer(components)
                : new System.Windows.Forms.Timer();

            clockTimer.Interval = 100;
            clockTimer.Tick += ClockTimer_Tick;
            clockTimer.Start();

            TriggerNtpSync();
        }

        private async void TriggerNtpSync()
        {
            btnSync.Text = "Syncing...";
            btnSync.Enabled = false;

            bool success = await ntpService.SyncAsync();
            if (success)
            {
                btnSync.Text = "Time Synced ✓";
                btnSync.ForeColor = Color.Green;
            }
            else
            {
                btnSync.Text = "Sync Failed";
                btnSync.ForeColor = Color.Red;
            }
            btnSync.Enabled = true;
        }

        private void SetupCustomUI()
        {
            this.Text = "CandleTimer";
            this.Size = new Size(220, 230);
            this.TopMost = true;
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.StartPosition = FormStartPosition.CenterScreen;

            numMinutes = new NumericUpDown
            {
                Location = new Point(12, 12),
                Width = 70,
                Minimum = 1,
                Maximum = 1440,
                Value = 1
            };
            numMinutes.ValueChanged += NumMinutes_ValueChanged;
            this.Controls.Add(numMinutes);

            lblMinutesTag = new Label
            {
                Text = "min(s)",
                Location = new Point(88, 14),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular)
            };
            this.Controls.Add(lblMinutesTag);

            lblCompactTimeframe = new Label
            {
                Text = "[ 1 min ]",
                Location = new Point(12, 48),
                Size = new Size(180, 15),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = Color.DarkGray,
                Visible = false
            };
            this.Controls.Add(lblCompactTimeframe);

            lblTimer = new Label
            {
                Location = new Point(12, 42),
                Size = new Size(180, 45),
                Font = new Font("Consolas", 28, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Text = "00:00"
            };
            this.Controls.Add(lblTimer);

            chkAlwaysOnTop = new CheckBox
            {
                Text = "Always on Top",
                Location = new Point(12, 90),
                AutoSize = true,
                Checked = true
            };
            chkAlwaysOnTop.CheckedChanged += (s, e) => { this.TopMost = chkAlwaysOnTop.Checked; };
            this.Controls.Add(chkAlwaysOnTop);

            chkSound = new CheckBox
            {
                Text = "Audio Alert",
                Location = new Point(12, 112),
                AutoSize = true,
                Checked = false
            };
            this.Controls.Add(chkSound);

            chkColorWarning = new CheckBox
            {
                Text = "Red <10s Warning",
                Location = new Point(12, 134),
                AutoSize = true,
                Checked = true
            };
            this.Controls.Add(chkColorWarning);

            btnSync = new Button
            {
                Text = "Sync NTP",
                Location = new Point(12, 160),
                Width = 95,
                Height = 25
            };
            btnSync.Click += (s, e) => TriggerNtpSync();
            this.Controls.Add(btnSync);

            btnCompact = new Button
            {
                Text = "Compact",
                Location = new Point(112, 160),
                Width = 80,
                Height = 25
            };
            btnCompact.Click += BtnCompact_Click;
            this.Controls.Add(btnCompact);
        }

        private void NumMinutes_ValueChanged(object? sender, EventArgs e)
        {
            int mins = (int)numMinutes.Value;
            if (mins < 1) return;

            selectedIntervalSeconds = mins * 60;
            lblCompactTimeframe.Text = $"[ {mins} min{(mins > 1 ? "s" : "")} ]";
        }

        private void BtnCompact_Click(object? sender, EventArgs e)
        {
            isCompact = !isCompact;

            if (isCompact)
            {
                numMinutes.Visible = false;
                lblMinutesTag.Visible = false;
                chkAlwaysOnTop.Visible = false;
                chkSound.Visible = false;
                chkColorWarning.Visible = false;
                btnSync.Visible = false;

                lblCompactTimeframe.Visible = true;
                btnCompact.Text = "Expand";
                btnCompact.Location = new Point(12, 62);
                btnCompact.Width = 180;

                lblTimer.Location = new Point(12, 2);
                this.Size = new Size(220, 130);
            }
            else
            {
                numMinutes.Visible = true;
                lblMinutesTag.Visible = true;
                chkAlwaysOnTop.Visible = true;
                chkSound.Visible = true;
                chkColorWarning.Visible = true;
                btnSync.Visible = true;

                lblCompactTimeframe.Visible = false;
                btnCompact.Text = "Compact";
                btnCompact.Location = new Point(112, 160);
                btnCompact.Width = 80;

                lblTimer.Location = new Point(12, 42);
                this.Size = new Size(220, 230);
            }
        }

        private void ClockTimer_Tick(object? sender, EventArgs e)
        {
            DateTime now = ntpService.GetCorrectedTime();

            int secondsRemainingInCurrentMinute = 60 - now.Second;
            if (secondsRemainingInCurrentMinute == 60)
            {
                secondsRemainingInCurrentMinute = 0;
            }

            int extraMinutes = (int)numMinutes.Value - 1;
            long displaySeconds = (extraMinutes * 60) + secondsRemainingInCurrentMinute;

            if (displaySeconds == 0 && now.Millisecond < 150)
            {
                if (chkSound.Checked)
                {
                    Task.Run(() => SystemSounds.Asterisk.Play());
                }
            }

            if (chkColorWarning.Checked && displaySeconds <= 10 && displaySeconds > 0)
            {
                lblTimer.ForeColor = Color.Red;
            }
            else
            {
                lblTimer.ForeColor = SystemColors.ControlText;
            }

            TimeSpan ts = TimeSpan.FromSeconds(displaySeconds);
            lblTimer.Text = ts.TotalHours >= 1 ? ts.ToString(@"hh\:mm\:ss") : ts.ToString(@"mm\:ss");
        }
    }
}