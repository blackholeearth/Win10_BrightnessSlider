using Gma.System.MouseKeyHook;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Management; //add dll to reference
using System.Threading;
using System.Windows.Forms;
using System.Windows.Threading;

namespace Win10_BrightnessSlider
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        //override form as toolwindow
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x80;
                return cp;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.FormBorderStyle = FormBorderStyle.None;

            this.WindowState = FormWindowState.Normal;
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(Screen.PrimaryScreen.WorkingArea.Width + 22,
                Screen.PrimaryScreen.WorkingArea.Height + 22);


            //colors
            BackColor = Color.FromArgb(31, 31, 31);
            label1.ForeColor = Color.White;

            //form show hide event
            notifyIcon1.MouseClick += NotifyIcon1_MouseClick;
            //clicked outside of form
            Deactivate += Form1_Deactivate;


            CreateNotifyIConContexMenu();
            UpdateStatesOnGuiControls();

            OverTrayTimer.Tick += OverTrayTimer_Tick;

            // set Timer to fire every 1/4th second  
            {
                var withBlock = OverTrayTimer;
                withBlock.Interval = 250;
                withBlock.Enabled = true;
            }
        }
     
        
        #region TrayIcon

        private List<Point> TrayIconPoints = new List<Point>();
        private System.Windows.Forms.Timer OverTrayTimer = new System.Windows.Forms.Timer();

        private void NotifyIcon1_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            Point MouseAt = new Point(MousePosition.X, MousePosition.Y);

            // Save point if it hasn't already been saved  
            if (!TrayIconPoints.Contains(MouseAt))
                TrayIconPoints.Add(MouseAt);
        }

        private Rectangle GetTrayIconRectangle()
        {
            Point TopLeft = new Point(10000, 10000);
            Point BottomRight = new Point(-10000, -10000);

            // Find upper left and bottom right of rectangle  
            foreach (var curPoint in TrayIconPoints)
            {
                if (curPoint.X < TopLeft.X)
                    TopLeft.X = curPoint.X;

                if (curPoint.Y < TopLeft.Y)
                    TopLeft.Y = curPoint.Y;

                if (curPoint.X > BottomRight.X)
                    BottomRight.X = curPoint.X;

                if (curPoint.Y > BottomRight.Y)
                    BottomRight.Y = curPoint.Y;
            }

            // Return rectangle representing the location of Tray Icon  
            return new Rectangle(TopLeft, new Size(BottomRight.X - TopLeft.X, BottomRight.Y - TopLeft.Y));
        }

        private void OverTrayTimer_Tick(object sender, EventArgs e)
        {
            // check to see if mouse is over Tray Icon  
            if (GetTrayIconRectangle().Contains(MousePosition))
            {
                Subscribe();
                Console.WriteLine("Is Over Tray Icon");
            }
            else
            {
              //  Unsubscribe();
                Console.WriteLine("Isn't Over Tray Icon");
            }
        }

        #endregion

        #region m_GlobalHook

        private IKeyboardMouseEvents m_GlobalHook;

        public void Subscribe()
        {
            // Note: for the application hook, use the Hook.AppEvents() instead
            m_GlobalHook = Hook.GlobalEvents();

            m_GlobalHook.MouseWheel += ChangheBr;
        }
        public void Unsubscribe()
        {
            m_GlobalHook.MouseWheel += ChangheBr;

            //It is recommened to dispose it
            m_GlobalHook.Dispose();
        }

        #endregion

        private void ChangheBr(object sender, MouseEventArgs e)
        {
            Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() => SetBrightness((byte)(GetBrightness() + ((e.Delta / 15) % 99)))),
             System.Windows.Threading.DispatcherPriority.Background);

            Console.WriteLine("ChangheBr" + e.Delta / 15);
        }

        private void CreateNotifyIConContexMenu()
        {

            var cm = new ContextMenu();
            var mi1 = new MenuItem("Exit", (snd, ev) =>
            {
                Application.Exit();
            });

            var mi2 = new MenuItem("State Of Window", (snd, ev) =>
            {
                var msg =
                "visible:" + this.Visible + "\r\n" +
                "Focused:" + this.Focused + "\r\n" +
                "canFocus:" + this.CanFocus + "\r\n";
                MessageBox.Show(msg);
            });

            var mi3 = new MenuItem("Run At Startup", (snd, ev) =>
            {
                var _mi3 = snd as MenuItem;

                _mi3.Checked = !_mi3.Checked; // toggle

                SetStartup(_mi3.Checked);
            });

            cm.MenuItems.Add(mi1);
            cm.MenuItems.Add(mi3);

            if (System.Diagnostics.Debugger.IsAttached)
            {
                cm.MenuItems.Add(mi2);
            }

            notifyIcon1.ContextMenu = cm;
        }
        private void UpdateStatesOnGuiControls()
        {
            //get current states
            var isRunSttup = isRunAtStartup();
            notifyIcon1.ContextMenu.MenuItems
                .Cast<MenuItem>().Where(x => x.Text == "Run At Startup").FirstOrDefault()
                .Checked = isRunSttup;

            var initBrig = GetBrightness();
            label1.Text = initBrig + "";
            trackBar1.Value = initBrig;
        }


        bool vis = false;
        public void eSetVis(bool visible)
        {
            Console.WriteLine("eSetVis - vis:" + vis);
            this.WindowState = FormWindowState.Normal;
            this.StartPosition = FormStartPosition.Manual;

            var scrWA = Screen.PrimaryScreen.WorkingArea;
            var p = new Point(scrWA.Width, scrWA.Height);

            if (visible)
            {
                p.Offset(-this.Width, -this.Height);
                this.Location = p;

                this.Activate();
                this.Show();
                this.BringToFront();

                vis = true;
            }
            else
            {
                p.Offset(this.Width, this.Height);
                this.Location = p;

                vis = false;

            }



        }

        private void Form1_Deactivate(object sender, EventArgs e)
        {
            eSetVis(false);
        }

        private void NotifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            notifyIcon1.MouseClick -= NotifyIcon1_MouseClick;
            Deactivate -= Form1_Deactivate;

            if (e.Button == MouseButtons.Left)
            {
                Console.WriteLine("Notify Cliiked - vis:" + vis);

                eSetVis(!vis);
            }


            notifyIcon1.MouseClick += NotifyIcon1_MouseClick;
            Deactivate += Form1_Deactivate;

        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            byte g = 0;
            if (byte.TryParse(trackBar1.Value + "", out g))
            {
                SetBrightness(g);
                label1.Text = g + "";
            }


        }

        static void SetBrightness(byte targetBrightness)
        {
            ManagementScope scope = new ManagementScope("root\\WMI");
            SelectQuery query = new SelectQuery("WmiMonitorBrightnessMethods");

            Thread thread = new Thread(() =>
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query))
                {
                    using (ManagementObjectCollection objectCollection = searcher.Get())
                    {
                        foreach (ManagementObject mObj in objectCollection)
                        {
                            mObj.InvokeMethod("WmiSetBrightness",
                                new Object[] { UInt32.MaxValue, targetBrightness });
                            break;
                        }
                    }
                }
            });
            thread.Start();
            thread.Join(); //wait for the thread to finish
        }
        static int GetBrightness()
        {
            ManagementScope scope = new ManagementScope("root\\WMI");
            SelectQuery query = new SelectQuery("WmiMonitorBrightness");

            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query))
            {
                using (ManagementObjectCollection objectCollection = searcher.Get())
                {
                    foreach (ManagementObject mObj in objectCollection)
                    {
                        var br_obj = mObj.Properties["CurrentBrightness"].Value;

                        int br = 0;
                        int.TryParse(br_obj + "", out br);
                        return br;
                        break;
                    }
                }
            }
            return 0;

        }

        private void SetStartup(bool RunAtStartup)
        {
            RegistryKey rk = Registry.CurrentUser.OpenSubKey
                ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            if (RunAtStartup)
                rk.SetValue(Application.ProductName, Application.ExecutablePath);
            else
                rk.DeleteValue(Application.ProductName, false);

        }
        private bool isRunAtStartup()
        {
            RegistryKey rk = Registry.CurrentUser.OpenSubKey
                ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            var val = rk.GetValue(Application.ProductName);

            if (val + "" == Application.ExecutablePath)
                return true;
            else
                return false;

        }



    }



}





