using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Win10_BrightnessSlider
{
    /// <summary>
    /// NoFocusTrackBar
    /// </summary>
    public class TrackBarEX : System.Windows.Forms.TrackBar
    {
        public TrackBarEX()
        {
            SetStyle(ControlStyles.UserPaint, true);
            DoubleBuffered = true;

            SetColor_ForDarkTheme();

        }

        #region  Set Colors Light Dark
        Color knobColor;
        Color knobHoverColor;
        Color knobMsDownColor;
        SolidBrush knobBrush;
        SolidBrush knobHoverBrush;
        SolidBrush knobMsDownBrush;
        //channel
        Color Chan_Color ;  
        Color Chan_BorderColor ;  
        SolidBrush Chan_Brush ;
        SolidBrush Chan_BorderBrush ;
        Pen Chan_BorderPen ;

        //Pen knobPen = new Pen(Color.Black);

        public void SetColor_ForDarkTheme()
        {
            //channel
            Chan_Color = Color.FromArgb(231, 234, 234); //original win10   -win11: 134,134,134
            Chan_BorderColor = Color.FromArgb(218, 218, 218); //original win10 

            Chan_Brush = new SolidBrush(Chan_Color);
            Chan_BorderBrush = new SolidBrush(Chan_BorderColor);
            Chan_BorderPen = new Pen(Chan_BorderColor);


            //knobPen = new Pen(knobColor);
            knobColor = Color.FromArgb(0, 120, 215); //original win10 blue
            knobHoverColor = Chan_BorderColor; //original win10 
            knobMsDownColor = Color.Black; //original win10 

            knobBrush = new SolidBrush(knobColor);
            knobHoverBrush = new SolidBrush(knobHoverColor);
            knobMsDownBrush = new SolidBrush(knobMsDownColor);

           
        }
        /// <summary>
        /// TODO:  not implemented.  
        /// </summary>
        public void SetColor_ForLightTheme()
        {
            
        }
        #endregion

        #region disable focus -dashed line
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public extern static int SendMessage(IntPtr hWnd, uint msg, int wParam, int lParam);
        private static int MakeParam(int loWord, int hiWord)
        {
            return (hiWord << 16) | (loWord & 0xffff);
        }
        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            SendMessage(this.Handle, 0x0128, MakeParam(1, 0x1), 0);
        }
        #endregion

        #region get Knob and Channel Rect
        public Rectangle Knob
        {
            get
            {
                RECT rc = new RECT();
                SendMessageRect(this.Handle, TBM_GETTHUMBRECT, IntPtr.Zero, ref rc);
                return new Rectangle(rc.left, rc.top, rc.right - rc.left, rc.bottom - rc.top);
            }
        }
        public Rectangle Channel
        {
            get
            {
                RECT rc = new RECT();
                SendMessageRect(this.Handle, TBM_GETCHANNELRECT, IntPtr.Zero, ref rc);
                return new Rectangle(rc.left, rc.top, rc.right - rc.left, rc.bottom - rc.top);
            }
        }
        private const int TBM_GETCHANNELRECT = 0x400 + 26;
        private const int TBM_GETTHUMBRECT = 0x400 + 25;
        private struct RECT { public int left, top, right, bottom; }
        [DllImport("user32.dll", EntryPoint = "SendMessageW")]
        private static extern IntPtr SendMessageRect(IntPtr hWnd, int msg, IntPtr wp, ref RECT lp);
        #endregion

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            //e.Graphics.FillRectangle(System.Drawing.Brushes.DarkSalmon, ClientRectangle);

            var channel = this.RectangleToClient(RectangleToScreen(Channel));
            var knob = this.RectangleToClient(RectangleToScreen(Knob));

            //----mouseOver
            var selBrush = knobBrush;
            if (knob.Contains(this.PointToClient(Cursor.Position)))
            {
                selBrush = knobHoverBrush;
                if (msDown) //(Control.MouseButtons == MouseButtons.Left) 
                {
                    selBrush = knobMsDownBrush;
                    //knob inflate_val = -5
                }
            }
            if (msDragging)
            {
                selBrush = knobMsDownBrush;
            }

            e.Graphics.FillRectangle(Chan_Brush, channel);
            e.Graphics.DrawRectangle(Chan_BorderPen, channel);
            //e.Graphics.FillEllipse(Brushes.SteelBlue,  knob);

            //draw KNOB
            e.Graphics.FillEllipse(selBrush, knob);
            //e.Graphics.DrawEllipse(knobPen,  knob); //knobBorder

        }


        #region MouseDown - Dragging 
        //original Trackbar Doesnt do it.
        bool msDown = false;
        bool msDragging = false;
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            msDown = true;
            //trigger onpaint to draw black color for mouseDown
            this.Invalidate();
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (msDown)
                msDragging = true;
        }
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            msDown = false;
            msDragging = false;
        }
        #endregion






    }
}
