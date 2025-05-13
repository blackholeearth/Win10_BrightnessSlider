using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Win10_BrightnessSlider
{
    public static class RoundBorders
    {

        #region roundWindow dllimport
        /* example usage. -- call after you set position and size of window . just before window is shown, if possible.
         *  this.FormBorderStyle = FormBorderStyle.None;
            Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 20, 20));
         */
        //set rounded window - also  there is func in the ctor.    
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        public static extern IntPtr CreateRoundRectRgn
        (
           int nLeftRect,     // x-coordinate of upper-left corner
           int nTopRect,      // y-coordinate of upper-left corner
           int nRightRect,    // x-coordinate of lower-right corner
           int nBottomRect,   // y-coordinate of lower-right corner
           int nWidthEllipse, // width of ellipse
           int nHeightEllipse // height of ellipse
        );
        #endregion


        public static Region GetRegion_ForRoundCorner(Size formSize, int borderRadius) 
        {
            var Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, formSize.Width, formSize.Height, borderRadius, borderRadius));
            return Region;
        }


      


    }
}
