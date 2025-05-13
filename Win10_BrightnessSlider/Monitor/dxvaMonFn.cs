using System.ComponentModel;
using System.Runtime.InteropServices;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

using System.Linq;

namespace Win10_BrightnessSlider
{
   
    public static class DxvaMonFn
    {
        #region dll imports enums etc.  
        public enum MonitorOptions : uint
        {
            MONITOR_DEFAULTTONULL = 0x00000000,
            MONITOR_DEFAULTTOPRIMARY = 0x00000001,
            MONITOR_DEFAULTTONEAREST = 0x00000002  // is used
        }
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr MonitorFromPoint(Point pt, MonitorOptions dwFlags);

        [DllImport("dxva2.dll", EntryPoint = "GetPhysicalMonitorsFromHMONITOR")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetPhysicalMonitorsFromHMONITOR(IntPtr hMonitor, uint dwPhysicalMonitorArraySize, [Out] PHYSICAL_MONITOR[] pPhysicalMonitorArray);

        [DllImport("dxva2.dll", SetLastError = true)]
        public static extern bool GetNumberOfPhysicalMonitorsFromHMONITOR(IntPtr hMonitor, ref uint pdwNumberOfPhysicalMonitors);


        [DllImport("dxva2.dll", SetLastError = true)]
        public static extern bool DestroyPhysicalMonitors(uint dwPhysicalMonitorArraySize, PHYSICAL_MONITOR[] pPhysicalMonitorArray);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct PHYSICAL_MONITOR
        {
            public IntPtr hPhysicalMonitor;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string szPhysicalMonitorDescription;
        }

        [DllImport("dxva2.dll", SetLastError = true)]
        public static extern bool GetMonitorCapabilities(
            IntPtr hMonitor, ref uint pdwMonitorCapabilities,  ref uint pdwSupportedColorTemperatures
            );


        [DllImport("dxva2.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetMonitorBrightness(IntPtr handle, out uint minBrightness, out uint currentBrightness, out uint maxBrightness);


        [DllImport("dxva2.dll", SetLastError = true)]
        public static extern bool SetMonitorBrightness(IntPtr hMonitor, uint dwNewBrightness);

        [DllImport("dxva2.dll", SetLastError = true)]
        public static extern bool GetMonitorContrast(IntPtr hMonitor, ref uint pdwMinContrast, ref uint pdwCurrentContrast, ref uint pdwMaxContrast);


        [DllImport("dxva2.dll", SetLastError = true)]
        public static extern bool SetMonitorContrast(IntPtr hMonitor, uint dwNewContrast);

        [DllImport("dxva2.dll", SetLastError = true)]
        public static extern bool SaveCurrentMonitorSettings(IntPtr hMonitor);
      

        #endregion

        #region example

        //public static double GetPhysicalMonitorContrast(PHYSICAL_MONITOR physicalMonitor)
        //{
        //    uint dwMinContrast = 0;
        //    uint dwCurContrast = 0;
        //    uint dwMaxContrast = 0;
        //    DxvaMonFn.GetMonitorContrast(physicalMonitor.hPhysicalMonitor, ref dwMinContrast, ref dwCurContrast, ref dwMaxContrast);
        //    return   ( dwCurContrast - dwMinContrast) / (double)(dwMaxContrast - dwMinContrast) ;
        //}

        //public static void SetPhysicalMonitorContrast(PHYSICAL_MONITOR physicalMonitor, double contrast)
        //{
        //    uint dwMinContrast = 0;
        //    uint dwCurContrast = 0;
        //    uint dwMaxContrast = 0;
        //    if (!DxvaMonFn.GetMonitorContrast(physicalMonitor.hPhysicalMonitor, ref dwMinContrast, ref dwCurContrast, ref dwMaxContrast);
        //    {
        //        throw new Win32Exception(Marshal.GetLastWin32Error());
        //    }

        //    var contrastVal = dwMinContrast + (dwMaxContrast - dwMinContrast) * contrast;
        //    if (!DxvaMonFn.SetMonitorContrast(physicalMonitor.hPhysicalMonitor,   (uint)contrastVal) )
        //    {
        //        throw new Win32Exception(Marshal.GetLastWin32Error());
        //    }

        //}

        #endregion

        public static PHYSICAL_MONITOR[] GetPhysicalMonitors_All_Flattened()
        {
            var PhysicalMonitorList = new List<PHYSICAL_MONITOR>();
            foreach (var scrx in Screen.AllScreens)  {
                PhysicalMonitorList.AddRange(GetPhysicalMonitors(scrx));
            }

            return PhysicalMonitorList.ToArray();
        }


        public static PHYSICAL_MONITOR[] GetPhysicalMonitors(Screen screen)
        {
            IntPtr hMonitor = DxvaMonFn.MonitorFromPoint(screen.Bounds.Location, MonitorOptions.MONITOR_DEFAULTTONEAREST);
            uint NumberOfPhysicalMonitors = 0u;
            GetNumberOfPhysicalMonitorsFromHMONITOR(hMonitor, ref NumberOfPhysicalMonitors);

            PHYSICAL_MONITOR[] PhysicalMonitors = new PHYSICAL_MONITOR[NumberOfPhysicalMonitors];
            GetPhysicalMonitorsFromHMONITOR(hMonitor, NumberOfPhysicalMonitors, PhysicalMonitors);

            return PhysicalMonitors;
        }

        public static bool DestroyAllPhysicalMonitors(PHYSICAL_MONITOR[] PhysicalMonitors)
        {
            return DxvaMonFn.DestroyPhysicalMonitors(Convert.ToUInt32(PhysicalMonitors.Length), PhysicalMonitors);
        }

        private static uint GetPhysicalMonitorCapabilities(PHYSICAL_MONITOR physicalMonitor)
        {
            uint dwMonitorCapabilities = 0;
            uint dwSupportedColorTemperatures = 0;
            DxvaMonFn.GetMonitorCapabilities(physicalMonitor.hPhysicalMonitor, ref dwMonitorCapabilities, ref dwSupportedColorTemperatures);
            return dwMonitorCapabilities;
        }


        /// <summary>
        /// if  returnValue double.isNAN()  then doesnt suppot or ddc/ci not enabled
        /// </summary>
        /// <param name="physicalMonitor"></param>
        /// <returns></returns>
        public static int GetPhysicalMonitorBrightness(PHYSICAL_MONITOR physicalMonitor)
        {
            uint dwMinBri = 0;
            uint dwCurBri = 0;
            uint dwMaxBri = 0;

            if (!DxvaMonFn.GetMonitorBrightness(physicalMonitor.hPhysicalMonitor, out dwMinBri, out dwCurBri, out dwMaxBri))
            {
                RamLogger.Log(new Win32Exception(Marshal.GetLastWin32Error()) + "");
                return -1;
                //throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            return (int)( (dwCurBri - dwMinBri) / (double)(dwMaxBri - dwMinBri) * 100);
        }

        public static bool SetPhysicalMonitorBrightness(PHYSICAL_MONITOR physicalMonitor, double brightness)
        {
            //if i dont set first , GetMonitorBrightness  doesnt work!!!! need help!!
            var fakesetBri_success = DxvaMonFn.SetMonitorBrightness(physicalMonitor.hPhysicalMonitor, Convert.ToUInt32(brightness)  ); //  Convert.ToUInt32(50)
            System.Threading.Thread.Sleep(60);
            if(!fakesetBri_success)
                RamLogger.Log("you can ignore this one....\r\n" +new Win32Exception(Marshal.GetLastWin32Error()) + "");


            uint dwMinBri = 0;
            uint dwCurBri = 0;
            uint dwMaxBri = 0;
            if (!DxvaMonFn.GetMonitorBrightness(physicalMonitor.hPhysicalMonitor, out dwMinBri, out dwCurBri, out dwMaxBri))
            {
                //throw new Win32Exception(Marshal.GetLastWin32Error());
                RamLogger.Log(new Win32Exception(Marshal.GetLastWin32Error()) + "");
                return false;
            }

            var newBrig = (dwMinBri + ((dwMaxBri - dwMinBri) * (brightness / 100.0))) ;
            if (!DxvaMonFn.SetMonitorBrightness(physicalMonitor.hPhysicalMonitor, Convert.ToUInt32(newBrig)  ))
            {
                //throw new Win32Exception(Marshal.GetLastWin32Error());
                RamLogger.Log(new Win32Exception(Marshal.GetLastWin32Error()) + "");
                return false;
            }

            return true;
        }

        public static bool SaveCurrentMonitorSettings(PHYSICAL_MONITOR physicalMonitor)
        {
            if (!DxvaMonFn.SaveCurrentMonitorSettings(physicalMonitor.hPhysicalMonitor))
            {
                //throw new Win32Exception(Marshal.GetLastWin32Error());
                RamLogger.Log(new Win32Exception(Marshal.GetLastWin32Error()) + "");
                return false;
            }
            return true;

        }

    }


}