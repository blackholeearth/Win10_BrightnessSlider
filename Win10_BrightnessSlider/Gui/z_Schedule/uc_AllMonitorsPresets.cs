using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Win10_BrightnessSlider.Gui
{
    public partial class uc_AllMonitorsPresets : UserControl
    {
        public Pen FrameColor = new Pen(Color.Orange, 1);
        Pen FrameColorDebug = new Pen(Color.Orange, 1)
        {
            DashStyle = DashStyle.Custom,
            DashPattern = new float[] { 10, 10 },
        };

        private List<Button> presetButtons = new List<Button>();

        public uc_AllMonitorsPresets()
        {
            InitializeComponent();
            CreatePresetButtons();
        }

        private void CreatePresetButtons()
        {
            // Remove old hardcoded buttons if they exist
            if (bt_0 != null) { this.Controls.Remove(bt_0); bt_0.Dispose(); }
            if (bt_25 != null) { this.Controls.Remove(bt_25); bt_25.Dispose(); }
            if (bt_50 != null) { this.Controls.Remove(bt_50); bt_50.Dispose(); }
            if (bt_75 != null) { this.Controls.Remove(bt_75); bt_75.Dispose(); }
            if (bt_100 != null) { this.Controls.Remove(bt_100); bt_100.Dispose(); }

            presetButtons.Clear();

            var settings = Settings_json.Get();
            var percentages = settings.PresetButtonPercentages ?? new List<int> { 0, 10, 25, 50, 75, 100 };

            // Limit to 6 buttons max for width
            if (percentages.Count > 6) percentages = percentages.Take(6).ToList();

            int buttonWidth = 44;
            int buttonHeight = 30;
            int startX = 100;  // After the label
            int spacing = 4;

            for (int i = 0; i < percentages.Count; i++)
            {
                int percent = percentages[i];
                var btn = new Button
                {
                    Text = $"{percent}%",
                    Size = new Size(buttonWidth, buttonHeight),
                    Location = new Point(startX + i * (buttonWidth + spacing), 5),
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 8F),
                    TabStop = false,
                    Anchor = AnchorStyles.Left,
                    Tag = percent
                };
                btn.Click += PresetButton_Click;
                presetButtons.Add(btn);
                this.Controls.Add(btn);
            }
        }

        private void PresetButton_Click(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.Tag is int percent)
            {
                SetAllMonitorsBrightness(percent);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (Debugger.IsAttached)
                FrameColor = FrameColorDebug;

            var CliRect = this.ClientRectangle;
            CliRect.Height = CliRect.Height - 2;
            CliRect.Width = CliRect.Width - 2;

            try
            {
                if (Form1.isWindows11)
                {
                    var radius = 8;
                    GraphicsPath shape = DrawingFn.RoundedRect(CliRect, radius, 0);
                    e.Graphics.DrawPath(FrameColor, shape);
                }
                else
                {
                    var p00 = new Point(0, 0);
                    var pw = new Point(CliRect.Width, 0);
                    var ph = new Point(0, CliRect.Height);

                    e.Graphics.DrawLine(FrameColor, p00, pw);
                    e.Graphics.DrawLine(FrameColor, p00, ph);
                }
            }
            catch
            {
                // Ignore paint errors
            }
        }

        public void SetGUIColors(Color bg, Color text, Color border)
        {
            this.Padding = Padding.Empty;
            this.Margin = Padding.Empty;

            FrameColor = new Pen(border, 1);

            this.BackColor = bg;
            this.ForeColor = text;

            lbl_Title.BackColor = bg;
            lbl_Title.ForeColor = text;

            // Style all dynamic preset buttons
            foreach (var btn in presetButtons)
            {
                btn.BackColor = border;
                btn.ForeColor = text;
                btn.FlatAppearance.BorderColor = border;
                btn.FlatAppearance.BorderSize = 0;
            }
        }

        private void uc_AllMonitorsPresets_Load(object sender, EventArgs e) { }

        private void SetAllMonitorsBrightness(int value)
        {
            try
            {
                if (Form1.riScreens == null || Form1.riScreens.Count == 0)
                    return;

                // Apply to all monitors simultaneously using parallel tasks
                var tasks = new List<Task>();
                foreach (var riScreen in Form1.riScreens)
                {
                    var screen = riScreen; // Capture for closure
                    tasks.Add(Task.Run(() => screen.SetBrightness(value, false)));
                }

                // Wait for all monitors to finish updating
                Task.WaitAll(tasks.ToArray());

                // Update UI
                var f1 = this.FindForm() as Form1;
                if (f1 != null && !f1.IsDisposed)
                {
                    try
                    {
                        f1.Invoke((Action)(() =>
                        {
                            f1.GUI_Update__AllSliderControls();
                            f1.GUI_Update_NotifyIconText();
                        }));
                    }
                    catch (Exception ex)
                    {
                        FileLogger.Log("SetAllMonitorsBrightness UI update error: " + ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                FileLogger.Log("SetAllMonitorsBrightness outer error: " + ex.Message);
            }
        }

        // Legacy handlers kept for designer compatibility - not used anymore
        private void bt_0_Click(object sender, EventArgs e) { }
        private void bt_25_Click(object sender, EventArgs e) { }
        private void bt_50_Click(object sender, EventArgs e) { }
        private void bt_75_Click(object sender, EventArgs e) { }
        private void bt_100_Click(object sender, EventArgs e) { }
    }
}
