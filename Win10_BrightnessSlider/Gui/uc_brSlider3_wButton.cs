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
    public partial class uc_brSlider3_wButton : UserControl , Iuc_brSlider
    {
        //interface
        RichInfoScreen Iuc_brSlider.richInfoScreen 
        { 
            get => _uc_brSlider3.richInfoScreen; 
            set => _uc_brSlider3.richInfoScreen = value;
        }
        string Iuc_brSlider.NotifyIconText => _uc_brSlider3.NotifyIconText;
        void Iuc_brSlider.UpdateSliderControl() => _uc_brSlider3.UpdateSliderControl();
        void Iuc_brSlider.Set_MonitorName(string name) => _uc_brSlider3.Set_MonitorName(name);
      



        public uc_brSlider3_wButton()
        {
            InitializeComponent();

			// ---  WIRE THE EVENTS HERE ---
			// Remove first to prevent double-clicking if run multiple times
			bt_increase.Click -= bt_increase_Click;
			bt_increase.Click += bt_increase_Click;

			bt_decrease.Click -= bt_decrease_Click;
			bt_decrease.Click += bt_decrease_Click;
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
                if (Form1.riScreens.Count == 1)
                    shape = DrawingFn.RoundedRect(CliRect, radius);
                else
                {
                    bool _First = Form1.riScreens[0] == this._uc_brSlider3.riScreen;
                    bool _Last = Form1.riScreens[Form1.riScreens.Count - 1] == this._uc_brSlider3.riScreen;

                    if (_First) //first
                        shape = DrawingFn.RoundedRect(CliRect, radius, 0);
                    else if (_Last) //last
                        shape = DrawingFn.RoundedRect(CliRect, 0, radius);
                    else //middle
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

			ucBrSlider3.Width = panel_temp.Width; //sliderWBtn. panel_temp.width is shorter than slider

			//var bg = uc_brSlider3.lbl_Name.BackColor;
			//var text = uc_brSlider3.lbl_Name.ForeColor;
			FrameColor = ucBrSlider3.FrameColor;
            ucBrSlider3.DrawFrame_isEnabled = false;

            this.BackColor = bg;
            this.ForeColor = text;

            bt_increase.BackColor = bg;
            bt_increase.ForeColor = text;
            bt_increase.FlatAppearance.BorderColor = border;

            bt_decrease.BackColor = bg;
            bt_decrease.ForeColor = text;
            bt_decrease.FlatAppearance.BorderColor = border;

            bt_increase.FlatAppearance.BorderSize =
            bt_decrease.FlatAppearance.BorderSize = 0;
            bt_increase.BackColor =
            bt_decrease.BackColor = border;

        }
        private void uc_brSlider3_wButton_Load(object sender, EventArgs e)  {  }




        private void bt_decrease_Click(object sender, EventArgs e) => _uc_brSlider3.Slider_SetBrightness_DOWN();
        private void bt_increase_Click(object sender, EventArgs e) => _uc_brSlider3.Slider_SetBrightness_UP();


    }
}
