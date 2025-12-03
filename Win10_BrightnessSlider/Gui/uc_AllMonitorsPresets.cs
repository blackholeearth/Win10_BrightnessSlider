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

        public uc_AllMonitorsPresets()
        {
            InitializeComponent();
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

            bt_0.BackColor = bg;
            bt_0.ForeColor = text;
            bt_0.FlatAppearance.BorderColor = border;

            bt_25.BackColor = bg;
            bt_25.ForeColor = text;
            bt_25.FlatAppearance.BorderColor = border;

            bt_50.BackColor = bg;
            bt_50.ForeColor = text;
            bt_50.FlatAppearance.BorderColor = border;

            bt_75.BackColor = bg;
            bt_75.ForeColor = text;
            bt_75.FlatAppearance.BorderColor = border;

            bt_100.BackColor = bg;
            bt_100.ForeColor = text;
            bt_100.FlatAppearance.BorderColor = border;

            bt_0.FlatAppearance.BorderSize =
            bt_25.FlatAppearance.BorderSize =
            bt_50.FlatAppearance.BorderSize =
            bt_75.FlatAppearance.BorderSize =
            bt_100.FlatAppearance.BorderSize = 0;

            bt_0.BackColor =
            bt_25.BackColor =
            bt_50.BackColor =
            bt_75.BackColor =
            bt_100.BackColor = border;
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

        private void bt_0_Click(object sender, EventArgs e) => SetAllMonitorsBrightness(0);
        private void bt_25_Click(object sender, EventArgs e) => SetAllMonitorsBrightness(25);
        private void bt_50_Click(object sender, EventArgs e) => SetAllMonitorsBrightness(50);
        private void bt_75_Click(object sender, EventArgs e) => SetAllMonitorsBrightness(75);
        private void bt_100_Click(object sender, EventArgs e) => SetAllMonitorsBrightness(100);
    }
}
