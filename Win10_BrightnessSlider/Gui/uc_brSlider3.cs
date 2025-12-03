using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Threading;
using System.Drawing.Drawing2D;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Win10_BrightnessSlider
{
    [JsonObject(MemberSerialization.OptIn)] //newtonsoft Json Ignore All Prop/field
    public partial class uc_brSlider3 : UserControl, Iuc_brSlider
    {
        public RichInfoScreen riScreen { get; set; }


        public static bool tracker_jumpToValue_enabled = true;

        public uc_brSlider3()
        {
            InitializeComponent();
            DoubleBuffered = true;

            trackBar1.Scroll += trackBar1_Scroll;
            trackBar1.MouseWheel += TrackBar1_MouseWheel;

            trackBar1.MouseUp += TrackBar1_MouseUp;
            trackBar1.MouseDown += TrackBar1_MouseDown;

            

            lbl_value.TextChanged += Label1_TextChanged;

            trackBar1.SmallChange = 5;
            trackBar1.LargeChange = 5;
            trackBar1.Minimum = 0;
            trackBar1.Maximum = 100;

            if (tracker_jumpToValue_enabled)
            {
                trackBar1.LargeChange = 0;
            }


            if (Debugger.IsAttached)  {
               // trackBar1.BackColor = Color.Red;
            }

            Disposed += Uc_brSlider_Disposed;
        }

        private void uc_brSlider3_Load(object sender, EventArgs e) { }

        //private void TrackBar1_KeyPress(object sender, KeyPressEventArgs e) {
        //    Slider_SetBrightness_UP();
        //}

        //private void TrackBar1_KeyDown(object sender, KeyEventArgs e) {
        //    Slider_SetBrightness_UP();
        //}


        public bool DrawFrame_isEnabled = true;
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

            if (!DrawFrame_isEnabled)
                return;

            if (Debugger.IsAttached)
                FrameColor = FrameColorDebug;

            var CliRect = this.ClientRectangle;
            CliRect.Height= CliRect.Height-2;
            CliRect.Width = CliRect.Width-2;


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

            if ( Form1.riScreens == null)
                return;

            //win11 - theming styling.
            if (Form1.isWindows11 ) 
            {
                var radius = 8;
                GraphicsPath shape= new GraphicsPath();
                if (Form1.riScreens.Count==1)
                    shape= DrawingFn.RoundedRect(CliRect, radius);
                else
                {
                    bool _First = Form1.riScreens[0] == this.riScreen;
                    bool _Last = Form1.riScreens[Form1.riScreens.Count - 1] == this.riScreen;

                    if (_First) //first
                        shape = DrawingFn.RoundedRect(CliRect, radius ,0);
                    else if (_Last) //last
                        shape = DrawingFn.RoundedRect(CliRect, 0, radius);
                    else //middle
                        shape = DrawingFn.RoundedRect(CliRect, 0,0);

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
   

        private void Uc_brSlider_Disposed(object sender, EventArgs e)
        {
            pictureBox1.Image?.Dispose();
            riScreen = null;
        }

        bool _isMouseDown { get; set; } = false;
        private void TrackBar1_MouseDown(object sender, MouseEventArgs e) => _isMouseDown = true;
        private void TrackBar1_MouseUp(object sender, MouseEventArgs e) => _isMouseDown = false;

        private void TrackBar1_MouseWheel(object sender, MouseEventArgs e)
        {
            trackBar1_Scroll(null, null);
            //if (e.Delta > 0)  Slider_SetBrightness_UP() else  Slider_SetBrightness_DOWN();
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            var safevalue = (int)MathFn.Clamp(trackBar1.Value, trackBar1.Minimum, trackBar1.Maximum); //without this. trackbar goes into negatives.
            var val = riScreen.SetBrightness((int)safevalue, _isMouseDown);
            if (val != -1)
            {
                lbl_value.Text = val + "";
            }
        }

        // value 0 to 100, REQUİRES GUİ - made these 3 for mouse hover on notify-rect
        public void Slider_SetBrightness(int value  )
        {
            var safevalue = (int)MathFn.Clamp(value, trackBar1.Minimum, trackBar1.Maximum); //without this. trackbar goes into negatives.
            var val = riScreen.SetBrightness(safevalue, false); // false = treat as final value, not dragging
            if (val != -1)
            {
                lbl_value.Text = val + "";
                trackBar1.Value = val;
            }
        }
        public void Slider_SetBrightness_UP()
        {
            var newValue = trackBar1.Value + trackBar1.SmallChange;
            Slider_SetBrightness((int)newValue);
        }
        public void Slider_SetBrightness_DOWN()
        {
            var newValue = trackBar1.Value - trackBar1.SmallChange;
            Slider_SetBrightness((int)newValue);
        }

     

        public void UpdateSliderControl()
        {
            int val = riScreen.GetBrightness();

            lbl_value.Text = val + "";
            if (val != -1)
                trackBar1.Value = val;

        }

        public void Set_MonitorName(string monitorName) => lbl_Name.Text = monitorName;


        public string NotifyIconText => " " + riScreen.avail_MonitorName + " : " + lbl_value.Text + "%";

        public RichInfoScreen richInfoScreen { 
            get => throw new NotImplementedException(); 
            set => throw new NotImplementedException(); 
        }

        private void Label1_TextChanged(object sender, EventArgs e)
        {
            Task.Run(() =>  {
                try
                {
                    var f1 = (this.FindForm() as Form1); //after slider scroll ends, upate text

                    ////slider with buttons
                    //if (f1 is null)
                    //    f1 = (this.Parent.FindForm() as Form1); //after slider scroll ends, upate text
                    
                    f1?.Invoke((Action)delegate {
                        f1?.GUI_Update_NotifyIconText();
                    });
                    
                }
                catch (Exception ex)
                {
                    RamLogger.Log("  @Label1_TextChanged . not important. ignore this \r\n\r\n" + ex);
                }


            });
        }

       



    }
}
