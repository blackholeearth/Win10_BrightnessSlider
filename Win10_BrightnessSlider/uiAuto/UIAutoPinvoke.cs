using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;


namespace ep_uiauto
{
    public static class UIAutoPinvoke 
    {
        public enum Cmp
        {
            Text,
            ClassName
        }


        public static bool Wait_for_Window_toBeDisappear(string processName, string child_WindowCaption, Cmp cmp, int MaxWait_AsSeconds = 15)
        {
            var sleep = 500;
            var retryCount = MaxWait_AsSeconds * 1000 / sleep;
            for (int i = 0; i < retryCount; i++)
            {
                Thread.Sleep(sleep);
                Wait_for_Window_toBeReady__ProgressSTR = $"{child_WindowCaption}; retry:{i}/{retryCount}; ";
                var ptr = GetWindowHandle_byText(processName, s => s.Contains(child_WindowCaption), cmp);
                if (ptr == IntPtr.Zero)
                {
                    Wait_for_Window_toBeReady__ProgressSTR = "";
                    return true;
                }
            }
            Wait_for_Window_toBeReady__ProgressSTR = "";
            return false;
        }


        public static string Wait_for_Window_toBeReady__ProgressSTR ="";
        /// <summary>
        ///  this will search in 1st lvl ( aka immediate child ) windows title.
        /// </summary>
        /// <param name="processName">  enter process name without  .exe (extension) </param>
        /// <param name="WindowCaption">  will search on any child windows  Of the Process </param>
        /// <param name="MaxWait_AsSeconds">after this time elapses, it will stop blocking the thread.</param>
        /// <param name=""></param>
        public static bool Wait_for_Window_toBeReady(string processName, string child_WindowCaption, Cmp cmp, int MaxWait_AsSeconds = 15)
        {
            var sleep = 500;
            var retryCount = MaxWait_AsSeconds * 1000 / sleep;
            for (int i = 0; i < retryCount; i++)
            {
                Thread.Sleep(sleep);
                Wait_for_Window_toBeReady__ProgressSTR = $"{child_WindowCaption}; retry:{i}/{retryCount};";
                var ptr = GetWindowHandle_byText(processName, s => s.Contains(child_WindowCaption), cmp);
                if (ptr != IntPtr.Zero)
                {
                    Wait_for_Window_toBeReady__ProgressSTR = "";
                    return true;
                }
            }
            Wait_for_Window_toBeReady__ProgressSTR = "";
            return false;
        }

        public static (bool found,string Caption) Wait_for_Window_toBeReady(string processName, string []child_WindowCaptions, Cmp cmp, int MaxWait_AsSeconds = 15)
        {
            var sleep = 500;
            var retryCount = MaxWait_AsSeconds * 1000 / sleep;
            for (int i = 0; i < retryCount; i++)
            {
                Thread.Sleep(sleep);
                Wait_for_Window_toBeReady__ProgressSTR = $"{string.Join(", ", child_WindowCaptions)}; retry:{i}/{retryCount};";
                string CaptionFound = null;
                Func<string, bool> compareTitles = s =>
                {
                    foreach (var captionX in child_WindowCaptions) {
                        if (s.Contains(captionX)) {
                            Wait_for_Window_toBeReady__ProgressSTR = "";
                            CaptionFound = captionX;
                            return true;
                        }
                    }
                    return false;
                };
                var ptr = GetWindowHandle_byText(processName, compareTitles , cmp);
                if (ptr != IntPtr.Zero)
                    return (true,CaptionFound);

            }
            Wait_for_Window_toBeReady__ProgressSTR = "";
            return (false,null);
        }




        /// <summary>
        /// arguments of GetWindowHandle_byText
        /// </summary>
        public class WindowHandle_Args 
        {
            /// <summary>
            /// also Called Caption
            /// </summary>
            public string text;
            public string className;
        }

        /// <summary>
        /// returns IntPtr.Zero , if not found  --- THİS DOESNT WORK, if 2 same Name is running.
        /// </summary>
        /// <param name="processName"></param>
        /// <param name="compareTitle"></param>
        /// <param name="cmp"></param>
        /// <returns></returns>
        public static IntPtr GetWindowHandle_byText_FirstProcess(string processName, Func<WindowHandle_Args, bool> compareTitle )
        {
            //var processName = "explorer";
            var proc = Process.GetProcessesByName(processName).FirstOrDefault();

            if (proc == null)
                return IntPtr.Zero;

            var shared_args = new WindowHandle_Args();

            var procWindows = EnumerateProcessWindowHandles(proc.Id);
            foreach (var handle in procWindows)
            {
                shared_args.className = GetClassName(handle);
                shared_args.text = GetText(handle);

                if (compareTitle(shared_args))
                {
                    return handle;//found
                }
            }
            return IntPtr.Zero;
        }
        public static IntPtr GetWindowHandle_byText(string processName, Func<WindowHandle_Args, bool> compareTitle)
        {
            //var processName = "explorer";
            var proc_idli = Process.GetProcessesByName(processName).Where(x=> x!= null).Select(x=>x.Id);

            if (proc_idli.Count() == 0)
                return IntPtr.Zero;

            var shared_args = new WindowHandle_Args();

            List<IntPtr> procWindows = new List<IntPtr>();
            foreach (var proc_Id in proc_idli)
            {
                procWindows.AddRange(
                    EnumerateProcessWindowHandles(proc_Id)
                    );
            }

            foreach (var handle in procWindows)
            {
                shared_args.className = GetClassName(handle);
                shared_args.text = GetText(handle);

                if (compareTitle(shared_args))
                {
                    return handle;//found
                }
            }
            return IntPtr.Zero;
        }

        /// <summary>
        /// returns IntPtr.Zero , if not found 
        /// </summary>
        /// <param name="processName"></param>
        /// <param name="compareTitle"></param>
        /// <param name="cmp"></param>
        /// <returns></returns>
        public static IntPtr GetWindowHandle_byText(string processName, Func<string, bool> compareTitle, Cmp cmp)
        {
            //var processName = "explorer";
            var proc = Process.GetProcessesByName(processName).FirstOrDefault();

            if (proc == null)
                return IntPtr.Zero;

            foreach (var handle in EnumerateProcessWindowHandles(proc.Id))
            {
                var text = "";
                if (cmp == Cmp.Text) text = GetText(handle);
                else if (cmp == Cmp.ClassName) text = GetClassName(handle);

                if (compareTitle(text))
                {
                    return handle;//found
                }
            }
            return IntPtr.Zero;
        }

        //----- enum
        delegate bool EnumThreadDelegate(IntPtr hWnd, IntPtr lParam);
        [DllImport("user32.dll")]
        static extern bool EnumThreadWindows(int dwThreadId, EnumThreadDelegate lpfn, IntPtr lParam);

        static IEnumerable<IntPtr> EnumerateProcessWindowHandles(int processId)
        {
            var handles = new List<IntPtr>();

            foreach (ProcessThread thread in Process.GetProcessById(processId).Threads)
                EnumThreadWindows(
                    thread.Id,
                    (hWnd, lParam) => { handles.Add(hWnd); return true; },
                    IntPtr.Zero);

            return handles;
        }


        //----- they also cal it Caption.
        #region GetText , GetClassName

        public static string wm_GetText_progressSTR = "";
        private static string GetText(IntPtr handle, int maxChar = 1000)
        {
            wm_GetText_progressSTR = "wm_GetText - Çalışıyor...";
            StringBuilder message = new StringBuilder(maxChar);
            SendMessage(handle, WM_GETTEXT, message.Capacity, message);
            wm_GetText_progressSTR = "";

            //Console.WriteLine(message);
            return message.ToString();
        }

        private const uint WM_GETTEXT = 0x000D;
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, int wParam, StringBuilder lParam);

        //------
        public static string GetClassName(IntPtr window_handle, int nMaxCount = 256)
        {
            //IntPtr hWnd = GetForegroundWindow();
            var className = new StringBuilder(nMaxCount); //256
            GetClassName(window_handle, className, className.Capacity);
            return className.ToString();
        }

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        #endregion


        //---- get window rect
        //[DllImport("user32.dll")]
        //[return: MarshalAs(UnmanagedType.Bool)]
        //static extern bool GetWindowRect(HandleRef hWnd, out RECT lpRect);

        //---- get window rect  -- ver2
        [DllImport("user32.dll")]
        static extern bool GetWindowRect(IntPtr hwnd, out RECT rectangle);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;        // x position of upper-left corner
            public int Top;         // y position of upper-left corner
            public int Right;       // x position of lower-right corner
            public int Bottom;      // y position of lower-right corner
        }


        public static Rectangle? GetWindowRectangle(IntPtr handle)
        {
            Rectangle myRect = new Rectangle();
            RECT rct;
            //if (!GetWindowRect(new HandleRef(this, this.Handle), out rct))
            if (!GetWindowRect(handle, out rct))
            {
                //MessageBox.Show("ERROR");
                return null;
            }
            myRect.X = rct.Left;
            myRect.Y = rct.Top;
            myRect.Width = rct.Right - rct.Left;
            myRect.Height = rct.Bottom - rct.Top;

            myRect = Rectangle.FromLTRB(rct.Left, rct.Top, rct.Right, rct.Bottom);
            return myRect;

        }


        ///---draw Rect on Screen
        #region draw Rect on Screen
        [DllImport("User32.dll")]
        public static extern IntPtr GetDC(IntPtr hwnd);
        [DllImport("User32.dll")]
        static extern int ReleaseDC(IntPtr hwnd, IntPtr dc);


        /*  Red (#F65058FF), Yellow (#FBDE44FF) and Navy (#28334AFF)
            Red ( 246, 80, 88), Yellow (251, 222, 68) and Navy (40, 51, 74)

            Knockout Pink (#FF3EA5), Safety Yellow (#EDFF00) and Out of the Blue (#00A4CC)
        */
        //static Pen pen1 = new Pen(Color.Red,3f);
        //static Pen pen2 = new Pen(Color.Green,3f);
        static Pen pen1 = new Pen(Color.FromArgb(0,0,0), 2f);
        static Pen pen2 = new Pen(Color.FromArgb(254, 225, 1), 2f);
        public static void DrawRect_OnScreen(Rectangle box)
        {
            IntPtr desktop = GetDC(IntPtr.Zero);
            using (Graphics g = Graphics.FromHdc(desktop))
            {
                int pad =2;
                //g.FillRectangle(Brushes.Red, 0, 0, 100, 100);
                g.DrawRectangle(pen2, box);
                box.Inflate(+pad, +pad);
                
                g.DrawRectangle(pen1, box);
                box.Inflate(+pad, +pad);
                g.DrawRectangle(pen2, box);
                box.Inflate(+pad, +pad);
            }
            ReleaseDC(IntPtr.Zero, desktop);
        }

        static Pen circ_pen1 = new Pen(Color.White, 2f);
        static Pen circ_pen2 = new Pen(Color.Red, 2f);
        public static void DrawCircle_OnScreen(Rectangle box)
        {
            IntPtr desktop = GetDC(IntPtr.Zero);
            using (Graphics g = Graphics.FromHdc(desktop))
            {
                int pad = 2;

                for (int i = 0; i < 3; i++)
                {
                    g.DrawEllipse(circ_pen2, box);
                    box.Inflate(+pad, +pad);

                    g.DrawEllipse(circ_pen1, box);
                    box.Inflate(+pad, +pad);
                }
             
            }
            ReleaseDC(IntPtr.Zero, desktop);
        }

        public static void DrawString_OnScreen(Rectangle box, string text)
        {
            IntPtr desktop = GetDC(IntPtr.Zero);
            using (Graphics g = Graphics.FromHdc(desktop))
            {
                ////yazmaya Yer aç   -eski- yedek
                //g.FillRectangle(Pens.DarkBlue.Brush, box);
                //g.DrawString(v, new Font( FontFamily.GenericSansSerif,11), Brushes.WhiteSmoke, box);
                //g.DrawRectangle(Pens.WhiteSmoke, box);

                var rect2_Tmp = new Rectangle(0, 0, box.Width , box.Height );

                var bmp_tmp = new Bitmap(rect2_Tmp.Width, rect2_Tmp.Height,g);
                using (Graphics g2 = Graphics.FromImage(bmp_tmp))
                {
                    //ekranda flicker önlemek için, önce bmpye yaz -buffer
                    //yazmaya Yer aç
                    g2.FillRectangle(Pens.DarkBlue.Brush, rect2_Tmp);
                    g2.DrawString(text, new Font(FontFamily.GenericSansSerif, 11), Brushes.WhiteSmoke, rect2_Tmp);
                    rect2_Tmp.Inflate(-2,-2);
                    g2.DrawRectangle(Pens.WhiteSmoke, rect2_Tmp);

                }



                g.DrawImage(bmp_tmp, box.Left, box.Top, box.Width, box.Height);
                //bmp_tmp.Dispose();
                
                //int pad = 2;
                //for (int i = 0; i < 1; i++)
                //{
                //    g.DrawRectangle(pen2, box);
                //    box.Inflate(+pad, +pad);

                //    g.DrawRectangle(circ_pen1, box);
                //    box.Inflate(+pad, +pad);
                //}

            }
            ReleaseDC(IntPtr.Zero, desktop);
        }

        public static void DrawString_OnScreen_yedek(Rectangle box, string text)
        {
            IntPtr desktop = GetDC(IntPtr.Zero);
            using (Graphics g = Graphics.FromHdc(desktop))
            {
                //yazmaya Yer aç   -eski- yedek
                g.FillRectangle(Pens.DarkBlue.Brush, box);
                g.DrawString(text, new Font(FontFamily.GenericSansSerif, 11), Brushes.WhiteSmoke, box);
                g.DrawRectangle(Pens.WhiteSmoke, box);

            }
            ReleaseDC(IntPtr.Zero, desktop);
        }

        #endregion



        //--------Show Hide Window By Handle
        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int SW_value);
        public const int SW_HIDE = 0;
        public const int SW_NORMAL = 1;
        public const int SW_SHOWNOACTIVATE = 4;
        public const int SW_SHOW = 5;
        public const int SW_RESTORE = 9;
        

        //----- perform mouse click on Screen Cordinates

        /// -This is a replacement for Cursor.Position in WinForms
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern bool SetCursorPos(int x, int y);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        public const int MOUSEEVENTF_LEFTDOWN = 0x02;
        public const int MOUSEEVENTF_LEFTUP = 0x04;

        /// <summary>
        /// This simulates a left mouse click
        /// </summary>
        /// <param name="xpos"></param>
        /// <param name="ypos"></param>
        public static void Do_MouseLeftClick(int xpos, int ypos)
        {
            SetCursorPos(xpos, ypos);
            mouse_event(MOUSEEVENTF_LEFTDOWN, xpos, ypos, 0, 0);
            mouse_event(MOUSEEVENTF_LEFTUP, xpos, ypos, 0, 0);
        }

        public static void Do_MouseLeftClick(Point pos) => Do_MouseLeftClick(pos.X, pos.Y);

        public static void Do_MouseLeftClick_LongTap(int xpos, int ypos , int tapDurationMs=500)
        {
            SetCursorPos(xpos, ypos);
            mouse_event(MOUSEEVENTF_LEFTDOWN, xpos, ypos, 0, 0);
            System.Threading.Thread.Sleep(tapDurationMs);
            mouse_event(MOUSEEVENTF_LEFTUP, xpos, ypos, 0, 0);
        }

        /// <summary>
        /// uses inflate , preserves center point.
        /// </summary>
        /// <param name="clickPos"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static Rectangle MakeRectFromPoint(this Point clickPos, int size = 8)
        {
            var ClicPos_drawRect = new Rectangle(clickPos, new Size(1, 1));
            ClicPos_drawRect.Inflate(size, size); //inflate preserves center point.
            return ClicPos_drawRect;
        }

        public static Point GetCenter_ofRect(this Rectangle calc_textboxRect)
        {
            return new Point(
                calc_textboxRect.X + calc_textboxRect.Width / 2,
                calc_textboxRect.Y + calc_textboxRect.Height / 2
                );
        }

       
    }




}











