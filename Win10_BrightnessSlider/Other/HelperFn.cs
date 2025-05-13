using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Win10_BrightnessSlider
{
    public static class HelperFn
    {
        //guiHelp
        public static void SetTooltip(Control ctl, string text, string title = "")
        {
            var yourToolTip = new ToolTip();
            //The below are optional, of course,

            yourToolTip.ToolTipIcon = ToolTipIcon.Info;
            yourToolTip.ShowAlways = true;

            yourToolTip.ToolTipTitle = title;
            yourToolTip.SetToolTip(ctl, text);
        }
         

        //registery
        public static void SetStartup(bool RunAtStartup)
        {
            RegistryKey rk = Registry.CurrentUser.OpenSubKey
                ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            if (RunAtStartup)
                rk.SetValue(Application.ProductName, Application.ExecutablePath);
            else
                rk.DeleteValue(Application.ProductName, false);

        }
        public static bool isRunAtStartup()
        {
            RegistryKey rk = Registry.CurrentUser.OpenSubKey
                ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            var val = rk.GetValue(Application.ProductName);

            if (val + "" == Application.ExecutablePath)
                return true;
            else
                return false;

        }

        //dumpinfo
        public static List<string> GetAllScreensInfo()
        {
            var infoli = new List<string>();

            foreach (var screen in Screen.AllScreens)
            {
                var infoX =
                    "Device Name: " + screen.DeviceName + "\r\n" +
                    "Bounds: " + screen.Bounds.ToString() + "\r\n" +
                    "Type: " + screen.GetType().ToString() + "\r\n" +
                    "Working Area: " + screen.WorkingArea.ToString() + "\r\n" +
                    "Primary Screen: " + screen.Primary.ToString() + "\r\n";
                infoli.Add(infoX);
            }

            return infoli;
        }
        public static List<string> GetAllMonitorInfo()
        {
            ManagementObjectSearcher searcher =
                    new ManagementObjectSearcher("root\\WMI",
                    "SELECT * FROM WmiMonitorBasicDisplayParams");
            var infoLi = new List<string>();
            foreach (ManagementObject queryObj in searcher.Get())
            {
                //  info.Add(queryObj["InstanceName"].ToString());
                var infox =
                queryObj["InstanceName"].ToString() + "\r\n" +
                queryObj["Active"].ToString() + "\r\n" +
                queryObj["DisplayTransferCharacteristic"].ToString() + "\r\n" +
                queryObj["MaxHorizontalImageSize"].ToString() + "\r\n" +
                queryObj["MaxVerticalImageSize"].ToString() + "\r\n" +
                queryObj["SupportedDisplayFeatures"].ToString() + "\r\n" +
                queryObj["VideoInputType"].ToString()
                ;
                infoLi.Add(infox);
            }
            return infoLi;
        }

        #region dllImports

        /// <summary>Returns true if the current application has focus, false otherwise</summary>
        public static bool ApplicationIsActivated()
        {
            var activatedHandle = GetForegroundWindow();
            if (activatedHandle == IntPtr.Zero)
            {
                return false;       // No window is currently activated
            }

            var procId = Process.GetCurrentProcess().Id;
            int activeProcId;
            GetWindowThreadProcessId(activatedHandle, out activeProcId);

            return activeProcId == procId;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowThreadProcessId(IntPtr handle, out int processId);


        public static string GetActiveProcessFileName()
        {
            IntPtr hwnd = GetForegroundWindow();
            uint pid;
            GetWindowThreadProcessId(hwnd, out pid);
            Process p = Process.GetProcessById((int)pid);
            return p.ProcessName;
        }

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, out uint ProcessId);

        #endregion

        public static void ColorFormIfActive_inDbgMode(Control f)
        {
            if (Debugger.IsAttached)
                f.BackColor = ApplicationIsActivated() ? Color.DarkRed : Color.Black;
        }

    }
}
