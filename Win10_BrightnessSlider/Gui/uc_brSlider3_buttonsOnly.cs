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
    public partial class uc_brSlider3_buttonsOnly : Gui.ThemedUserControl, Iuc_brSlider
    {
        public RichInfoScreen riScreen { get; set; }


        public static bool tracker_jumpToValue_enabled = true;

        public uc_brSlider3_buttonsOnly()
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

			// ---  WIRE THE EVENTS HERE ---
			// Remove first to prevent double-clicking if run multiple times
			bt_increase.Click -= bt_increase_Click;
			bt_increase.Click += bt_increase_Click;

			bt_decrease.Click -= bt_decrease_Click;
			bt_decrease.Click += bt_decrease_Click;

            if (Debugger.IsAttached)  {
               // trackBar1.BackColor = Color.Red;
            }

            Disposed += Uc_brSlider_Disposed;
        }

        private void uc_brSlider3_Load(object sender, EventArgs e) { }


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
            //if (e.Delta > 0)  Slider_SetBrightness_UP() else  Slider_SetBrightness_DOWN();

            trackBar1_Scroll(null,null);
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
            var val = riScreen.SetBrightness(safevalue, _isMouseDown:false);
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

        public RichInfoScreen richInfoScreen { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

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

        //----------- added for new design
        public void SetGUIColors(Color bg, Color text, Color border)
        {
            this.Padding = Padding.Empty;
            this.Margin = Padding.Empty;


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

            //remove knob
            this.trackBar1.ThumbSize = new Size(1, 6);
            this.trackBar1.ThumbRoundRectSize = new Size(1, 1);
            this.trackBar1.ThumbInner2Color =
            this.trackBar1.ThumbInnerColor = 
            this.trackBar1.ThumbPenColor = this.trackBar1.ElapsedInnerColor;

        }


		private void bt_decrease_Click(object sender, EventArgs e) => this.Slider_SetBrightness_DOWN();
		private void bt_increase_Click(object sender, EventArgs e) => this.Slider_SetBrightness_UP();


	}
}
