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

namespace Win10_BrightnessSlider
{
    public partial class uc_brSlider2 : UserControl
    {
        public RichInfoScreen riScreen;


        public static bool tracker_jumpToValue_enabled = true;

        public uc_brSlider2()
        {
            InitializeComponent();
            DoubleBuffered = true;

            trackBar1.Scroll += trackBar1_Scroll;

            trackBar1.MouseUp += TrackBar1_MouseUp;
            trackBar1.MouseDown += TrackBar1_MouseDown;

            trackBar1.MouseMove += TrackBar1_MouseMove;

            label1.TextChanged += Label1_TextChanged;

            trackBar1.SmallChange = 5;
            trackBar1.LargeChange = 5;

            if (tracker_jumpToValue_enabled)
            {
                trackBar1.LargeChange = 0;
                trackBar1.MouseDown += TrackBar1_MouseDown__jump_to_value;
            }

            
            if (Debugger.IsAttached)  {
               // trackBar1.BackColor = Color.Red;
            }

            Disposed += Uc_brSlider_Disposed;
        }

      

        public Pen FrameColor = new Pen(Color.Orange, 1); 
        //Draw border
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (Debugger.IsAttached)
                FrameColor = new Pen(Color.Orange, 1);

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

            //win11
            if (Form1.isWindows11) 
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

        bool _isMouseDown = false;
        private void TrackBar1_MouseDown(object sender, MouseEventArgs e) => _isMouseDown = true;
        private void TrackBar1_MouseUp(object sender, MouseEventArgs e)
        {
            _isMouseDown = false;
            trackBar1_Scroll(null, null);
        }

        int msMove_lastX = -1;
        private void TrackBar1_MouseMove(object sender, MouseEventArgs e)
        {
            //doesnt work as expected its jumpy... better use ColorSlider Project.
            return;
            var Threshold = MathFn.Clamp(trackBar1.Width / 100, 10, 500);
            if (Math.Abs(msMove_lastX - e.Location.X) < Threshold)
                return;
            msMove_lastX = e.Location.X;
            TrackBar1_MouseDown__jump_to_value(null, e);
            trackBar1_Scroll(null,null);
        }
        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            var val = riScreen.SetBrightness(trackBar1.Value, _isMouseDown);
            if (val != -1)
            {
                label1.Text = val + "";
            }
        }

        // value 0 to 100, REQUİRES GUİ - made these 3 for mouse hover on notify-rect
        public void Slider_SetBrightness(int value  )
        {
            var val = riScreen.SetBrightness(value, true);
            if (val != -1)
            {
                label1.Text = val + "";
                trackBar1.Value = value;
            }
        }
        public void Slider_SetBrightness_UP()
        {
            var newValue = trackBar1.Value + trackBar1.SmallChange;
            Slider_SetBrightness(newValue);
        }
        public void Slider_SetBrightness_DOWN()
        {
            var newValue = trackBar1.Value - trackBar1.SmallChange;
            Slider_SetBrightness(newValue);
        }


        //https://stackoverflow.com/questions/76014135/mouse-click-near-subdivision-dash-of-trackbar;
        //not jumpy but out of sync.
        private int CalcTrackBarSO(int mouseX)
        {
            double p = mouseX / (double)trackBar1.Width; //if horizontal,

            int newValue = trackBar1.Minimum + (int)(p * (trackBar1.Maximum - trackBar1.Minimum + 1));
            var newVal= Math.Min(trackBar1.Maximum, Math.Max(trackBar1.Minimum, newValue));
            return newVal;
        }
        private int CalcTrackBarV1(int mouseX)
        {
            double val1;
            var centerVal = (trackBar1.Maximum - trackBar1.Minimum) * 0.5;
            var paddingLeft = 7; //detected in paint by eye

            //Jump to the clicked location
            val1 = (double)(mouseX - paddingLeft) / (double)(trackBar1.Width - paddingLeft * 2) * (trackBar1.Maximum - trackBar1.Minimum);
            var sapma = (centerVal - val1) * 0.06;
            int newValue = (int)Math.Round(val1 - sapma);
            return newValue;
        }

        private void TrackBar1_MouseDown__jump_to_value(object sender, MouseEventArgs e)
        {
            if (!tracker_jumpToValue_enabled)
                return;

            if (e.Button != MouseButtons.Left)
                return;

            int newValue = CalcTrackBarV1(e.X);
            //newValue = CalcTrackBarSO(e.X);

            
            try
            {
                trackBar1.Value = newValue;
            }
            catch (Exception ex)
            {
                RamLogger.Log(ex + "");
            }



        }

     

        public void UpdateSliderControl()
        {
            int val = riScreen.GetBrightness();

            label1.Text = val + "";
            if (val != -1)
                trackBar1.Value = val;

        }


        public string NotifyIconText => " " + riScreen.avail_MonitorName + " : " + label1.Text + "%";
        private void Label1_TextChanged(object sender, EventArgs e)
        {
            Task.Run(() =>  {
                try
                {
                    var f1 = (this.FindForm() as Form1); //after slider scroll ends, upate text
                    f1.Invoke((Action) delegate { 
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
