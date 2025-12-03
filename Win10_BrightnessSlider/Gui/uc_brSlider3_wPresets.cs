using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Win10_BrightnessSlider.Gui
{
    public partial class uc_brSlider3_wPresets : UserControl , Iuc_brSlider
    {
        //interface
        RichInfoScreen Iuc_brSlider.richInfoScreen
        {
            get => _uc_brSlider3.riScreen;
            set => _uc_brSlider3.riScreen = value;
        }
        string Iuc_brSlider.NotifyIconText => _uc_brSlider3.NotifyIconText;
        void Iuc_brSlider.UpdateSliderControl() => _uc_brSlider3.UpdateSliderControl();
        void Iuc_brSlider.Set_MonitorName(string name) => _uc_brSlider3.Set_MonitorName(name);



        public uc_brSlider3_wPresets()
        {
            InitializeComponent();
        }

        public uc_brSlider3 _uc_brSlider3;

        /// [JsonIgnore]
        public Pen FrameColor = new Pen(Color.Orange, 1);
        // [JsonIgnore]
        Pen FrameColorDebug = new Pen(Color.Orange, 1)
        {
            DashStyle = DashStyle.Custom,
            DashPattern = new float[] { 10, 10 },
        };
        //Draw border
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (Debugger.IsAttached)
                FrameColor = FrameColorDebug;

            var CliRect = this.ClientRectangle;
            CliRect.Height = CliRect.Height - 2;
            CliRect.Width = CliRect.Width - 2;


            ////win11 enabled
            //if (Form1.isRoundCorners)
            //{
            //    int offset = -2;
            //    pw.Offset(offset, 0);
            //    ph.Offset(0, offset);
            //    var phw = new Point(CliRect.Width + offset, CliRect.Height + offset);

            //    //win11 padding, wee need bottom and right
            //    e.Graphics.DrawLine(FrameColor, ph, phw);
            //    e.Graphics.DrawLine(FrameColor, pw, phw); //no problem. works great on light theme

            //}

            if (Form1.riScreens == null)
                return;

            //win11
            if (Form1.isWindows11)
            {
                var radius = 8;
                GraphicsPath shape = new GraphicsPath();

                // Check if "All Monitors" control exists (preset buttons with multiple monitors)
                var parentForm = this.FindForm() as Form1;
                bool hasAllMonitorsControl = parentForm?.HasAllMonitorsControl() ?? false;

                if (Form1.riScreens.Count == 1 && !hasAllMonitorsControl)
                {
                    // Single monitor, no "All Monitors" control - full rounded corners
                    shape = DrawingFn.RoundedRect(CliRect, radius);
                }
                else if (hasAllMonitorsControl)
                {
                    // Multiple monitors with "All Monitors" at top
                    bool _Last = Form1.riScreens[Form1.riScreens.Count - 1] == this._uc_brSlider3.riScreen;

                    if (_Last)
                        // Last monitor - bottom rounded corners only
                        shape = DrawingFn.RoundedRect(CliRect, 0, radius);
                    else
                        // First or middle monitors - no rounded corners
                        shape = DrawingFn.RoundedRect(CliRect, 0, 0);
                }
                else
                {
                    // Multiple monitors, no "All Monitors" control (shouldn't happen with preset buttons)
                    bool _First = Form1.riScreens[0] == this._uc_brSlider3.riScreen;
                    bool _Last = Form1.riScreens[Form1.riScreens.Count - 1] == this._uc_brSlider3.riScreen;

                    if (_First)
                        shape = DrawingFn.RoundedRect(CliRect, radius, 0);
                    else if (_Last)
                        shape = DrawingFn.RoundedRect(CliRect, 0, radius);
                    else
                        shape = DrawingFn.RoundedRect(CliRect, 0, 0);
                }

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




        public void SetGUIColors(Color bg, Color text , Color border , uc_brSlider3 ucBrSlider3)
        {
            this.Padding = Padding.Empty;
            this.Margin = Padding.Empty;

            _uc_brSlider3 = ucBrSlider3;


            this.Controls.Add(ucBrSlider3);
            ucBrSlider3.Top = panel_temp.Top;
            ucBrSlider3.Left = panel_temp.Left;
            panel_temp.Visible = false;

            ucBrSlider3.Width = panel_temp.Width;

            FrameColor = ucBrSlider3.FrameColor;
            ucBrSlider3.DrawFrame_isEnabled = false;

            this.BackColor = bg;
            this.ForeColor = text;

            // Style all preset buttons
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
        private void uc_brSlider3_wPresets_Load(object sender, EventArgs e)  {  }




        private void bt_0_Click(object sender, EventArgs e)
        {
            if (_uc_brSlider3 != null && _uc_brSlider3.riScreen != null)
                _uc_brSlider3.Slider_SetBrightness(0);
        }
        private void bt_25_Click(object sender, EventArgs e)
        {
            if (_uc_brSlider3 != null && _uc_brSlider3.riScreen != null)
                _uc_brSlider3.Slider_SetBrightness(25);
        }
        private void bt_50_Click(object sender, EventArgs e)
        {
            if (_uc_brSlider3 != null && _uc_brSlider3.riScreen != null)
                _uc_brSlider3.Slider_SetBrightness(50);
        }
        private void bt_75_Click(object sender, EventArgs e)
        {
            if (_uc_brSlider3 != null && _uc_brSlider3.riScreen != null)
                _uc_brSlider3.Slider_SetBrightness(75);
        }
        private void bt_100_Click(object sender, EventArgs e)
        {
            if (_uc_brSlider3 != null && _uc_brSlider3.riScreen != null)
                _uc_brSlider3.Slider_SetBrightness(100);
        }


    }
}
