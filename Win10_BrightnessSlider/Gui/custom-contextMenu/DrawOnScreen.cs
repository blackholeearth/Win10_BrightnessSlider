using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace Win10_BrightnessSlider
{
    public static class DrawOnScreen
    {
        [DllImport("User32.dll")]
        static extern IntPtr GetDC(IntPtr hwnd);
        [DllImport("User32.dll")]
        static extern void ReleaseDC(IntPtr hwnd, IntPtr dc);


        /// [JsonIgnore]
        static Pen pen = new Pen(Color.Orange, 2);
        // [JsonIgnore]
        static Pen pendash = new Pen(Color.Orange, 2)
        {
            DashStyle = DashStyle.Custom,
            DashPattern = new float[] { 5, 5 },
        };

        public static void DrawRect( Rectangle rect)
        {
            IntPtr desktopPtr = GetDC(IntPtr.Zero);
            var g = Graphics.FromHdc(desktopPtr);

            //var pen = new Pen(Color.Cyan);
            g.DrawRectangle(pen, rect);

            g.Dispose();
            ReleaseDC(IntPtr.Zero, desktopPtr);
        }


    }
}
