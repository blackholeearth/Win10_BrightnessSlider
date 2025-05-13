using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Win10_BrightnessSlider._testArea
{
    public class LevelMeter : Panel
    {

        private int min = 0;
        private int max = 20;
        private float current = 10;

        //drawing
        private Pen levelPen = new Pen(Color.Green);

        private Rectangle levelRect;
        private Rectangle currentlevelRect;
        int _barHeight = 30;
        public int MyProperty {
            get => _barHeight;
            set 
            {
                _barHeight = value;

            } 
        }

        /// <summary>
        /// 0 - 0,5 - 1  , 0,5 is 50 pct;
        /// </summary>
        /// <returns></returns>
        float getRatio() 
        {
            var pct = current / (max - min);

            return pct;
        }

        void setNewCurrent(float newRatio)
        {
            var newCurrent = (max - min) * newRatio +min;
            current = newCurrent;

            Calculate_BarLayout();
            Invalidate();
        }


        public LevelMeter()
        {
            this.DoubleBuffered = true;

            Width = 200;
            Height = 60;
            BackColor = Color.LightCyan;
            Calculate_BarLayout();
        }

        private void Calculate_BarLayout()
        {
            int BarX = (int)((Height - _barHeight) * 0.5);
            //horizontal  -
            //todo
            int lr_offset = 0;//20; // adding offset Breaks msPos_setcurrent_andRedraw

            currentlevelRect = new Rectangle(0, BarX, (int)(Width * getRatio()), _barHeight);
            currentlevelRect.Inflate(-lr_offset, -5);

            levelRect = new Rectangle(0, BarX, Width, _barHeight);
            levelRect.Inflate(-lr_offset, -5);
        }


        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            //Form par = (this.Parent as Form);

            var g = e.Graphics;

            g.FillRectangle(levelPen.Brush,currentlevelRect);
            g.DrawRectangle(Pens.Black, levelRect);
        }

        bool msDown = false;
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            msDown = true;
            msPos_setcurrent_andRedraw(e);


        }
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            msDown = false;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            //redraw .. call on paint
            if (msDown)
            {
                msPos_setcurrent_andRedraw(e);

            }
        }

        private void msPos_setcurrent_andRedraw(MouseEventArgs e)
        {
            //Invalidate();

            //levelRect.Inflate(-20, -5);

            //calculate new level
            var hitinside = levelRect.Contains(e.X, e.Y);
            if (!hitinside)
                return;

            //e.X == levelRect.X + Width ; //100
            //e.X == levelRect.X  ; //0

            var msX_padRemoved = e.X - levelRect.X;
            var msX_prScaled = msX_padRemoved;
            //var new_msRatio = msX_padRemoved / (float)levelRect.Width;
            var new_msRatio = msX_padRemoved / (float)levelRect.Width;
            setNewCurrent(new_msRatio);
        }
    }

}
