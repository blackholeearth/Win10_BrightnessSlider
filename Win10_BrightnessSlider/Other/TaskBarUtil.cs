using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Win10_BrightnessSlider.Monitor.Events;

namespace Win10_BrightnessSlider
{

    /// <summary>
    ///  how to use :   Color taskBarColour = GetColourAt(GetTaskbarPosition().Location);
    /// </summary>
    public static class TaskBarUtil
    {
        static TaskBarUtil()
        {
            try
            {
                var test =  ThemeHelper.ThemeIsLight_viaRegedit();
            }
            catch (Exception ex)
            {
                RamLogger.Log("Exception at ThemeIsLight_viaRegedit :" + ex.Message);
                RegeditFailed = true;
            }
           
        }



        static bool RegeditFailed = false;
        public static bool isTaskbarColor_Light()
        {

            if(RegeditFailed)
                return isTaskbarColor_Light_viaGetColor();
            else
                return ThemeHelper.ThemeIsLight_viaRegedit();


        }

        public static bool isTaskbarColor_Light_viaGetColor()
        {
            var taskBarRect = TaskBarUtil.GetTaskbarPosition();
            taskBarRect.Inflate(-3, -3);
            Color taskBarColour = TaskBarUtil.GetColourAt(taskBarRect.Location);
            bool isLight = taskBarColour.ToGray() > 127;
            return isLight;
        }



        #region SHAppBarMessage -> GetTaskbarPosition , BitBlt -> GetColourAt
        [DllImport("shell32.dll")]
        private static extern IntPtr SHAppBarMessage(int msg, ref APPBARDATA data);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        private static extern int BitBlt(IntPtr hDC, int x, int y, int nWidth, int nHeight, IntPtr hSrcDC, int xSrc, int ySrc, int dwRop);

        private const int ABM_GETTASKBARPOS = 5;
        private struct APPBARDATA
        {
            public int cbSize;
            public IntPtr hWnd;
            public int uCallbackMessage;
            public int uEdge;
            public RECT rc;
            public IntPtr lParam;
        }

        private struct RECT { public int left, top, right, bottom;   }

        public static Rectangle GetTaskbarPosition()
        {
            APPBARDATA data = new APPBARDATA();
            data.cbSize = Marshal.SizeOf(data);

            IntPtr retval = SHAppBarMessage(ABM_GETTASKBARPOS, ref data);
            if (retval == IntPtr.Zero)
            {
                throw new Win32Exception("Please re-install Windows");
            }

            return new Rectangle(data.rc.left, data.rc.top, data.rc.right - data.rc.left, data.rc.bottom - data.rc.top);
        }

        public static Color GetColourAt(Point location)
        {
            using (Bitmap screenPixel = new Bitmap(1, 1, PixelFormat.Format32bppArgb))
            using (Graphics gdest = Graphics.FromImage(screenPixel))
            {
                using (Graphics gsrc = Graphics.FromHwnd(IntPtr.Zero))
                {
                    IntPtr hSrcDC = gsrc.GetHdc();
                    IntPtr hDC = gdest.GetHdc();
                    int retval = BitBlt(hDC, 0, 0, 1, 1, hSrcDC, location.X, location.Y, (int)CopyPixelOperation.SourceCopy);
                    gdest.ReleaseHdc();
                    gsrc.ReleaseHdc();
                }

                return screenPixel.GetPixel(0, 0);
            }
        }
        #endregion


        public static byte ToGray(this Color c) 
        {
            byte gray = (byte)(.299 * c.R + .587 * c.G + .114 * c.B);
            return gray;
        }
        public static Color ToGrayColor(this Color c)
        {
            byte gray = c.ToGray();
            return Color.FromArgb(gray, gray, gray);
        }



    }
}
