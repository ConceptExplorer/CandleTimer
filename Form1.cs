using System;
using System.Drawing;
using System.Media;
using System.Windows.Forms;

namespace CandleTimer
{
    public partial class Form1 : Form
    {
        private System.Windows.Forms.Timer clockTimer;
        private Label lblTimer;
        private ComboBox cmbTimeframe;
        private CheckBox chkAlwaysOnTop;
        private int selectedIntervalSeconds = 60; // Default to 1-Minute

        public Form1()
        {
            InitializeComponent();
            SetupCustomUI();

            // Set up background timer loop
            clockTimer = new System.Windows.Forms.Timer();
            clockTimer.Interval = 100; // Fast 100ms refresh for smooth display
            clockTimer.Tick += ClockTimer_Tick;
            clockTimer.Start();
        }

        private void SetupCustomUI()
        {
            // Form properties
            this.Text = "CandleTimer";
            this.Size = new Size(220, 160);
            this.TopMost = true; // Always on top by default
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.StartPosition = FormStartPosition.CenterScreen;

            // Timeframe Selector Dropdown
            cmbTimeframe = new ComboBox
            {
                Location = new Point(12, 12),
                Width = 180,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbTimeframe.Items.AddRange(new object[] { "1 Minute", "2 Minutes", "3 Minutes", "5 Minutes", "15 Minutes" });
            cmbTimeframe.SelectedIndex = 0;
            cmbTimeframe.SelectedIndexChanged += CmbTimeframe_SelectedIndexChanged;
            this.Controls.Add(cmbTimeframe);

            // Timer Display Label
            lblTimer = new Label
            {
                Location = new Point(12, 45),
                Size = new Size(180, 45),
                Font = new Font("Consolas", 28, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Text = "00:00"
            };
            this.Controls.Add(lblTimer);

            // Always On Top Checkbox
            chkAlwaysOnTop = new CheckBox
            {
                Text = "Always on Top",
                Location = new Point(15, 95),
                AutoSize = true,
                Checked = true
            };
            chkAlwaysOnTop.CheckedChanged += (s, e) => { this.TopMost = chkAlwaysOnTop.Checked; };
            this.Controls.Add(chkAlwaysOnTop);
        }

        private void CmbTimeframe_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (cmbTimeframe.SelectedIndex)
            {
                case 0: selectedIntervalSeconds = 60; break;   // 1m
                case 1: selectedIntervalSeconds = 120; break;  // 2m
                case 2: selectedIntervalSeconds = 180; break;  // 3m
                case 3: selectedIntervalSeconds = 300; break;  // 5m
                case 4: selectedIntervalSeconds = 900; break;  // 15m
            }
        }

        private void ClockTimer_Tick(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            int totalSeconds = (now.Minute * 60) + now.Second;
            int remaining = selectedIntervalSeconds - (totalSeconds % selectedIntervalSeconds);

            // Trigger chime on the exact candle close
            if (remaining == selectedIntervalSeconds && now.Millisecond < 150)
            {
                SystemSounds.Asterisk.Play();
            }

            // Standard display logic
            int displaySeconds = remaining == selectedIntervalSeconds ? 0 : remaining;
            TimeSpan ts = TimeSpan.FromSeconds(displaySeconds);
            lblTimer.Text = ts.ToString(@"mm\:ss");
        }
    }
}