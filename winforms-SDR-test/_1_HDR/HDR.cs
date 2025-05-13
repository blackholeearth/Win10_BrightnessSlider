using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

//   https://stackoverflow.com/questions/74594751/controlling-sdr-content-brightness-programmatically-in-windows-11

namespace winformsTests
{
    public static class HDR
    {
        [DllImport("user32.dll")]
        static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);
        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        static extern IntPtr LoadLibrary(string lpFileName);
        [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        static extern IntPtr GetProcAddress(IntPtr hModule, int address);

        private delegate void DwmpSDRToHDRBoostPtr(IntPtr monitor, double brightness);

        public static void Set_HDR_Brightness(double brightness =1.0)
        {
            var primaryMonitor = MonitorFromWindow(IntPtr.Zero, 1);
            var hmodule_dwmapi = LoadLibrary("dwmapi.dll");
            DwmpSDRToHDRBoostPtr changeBrightness = Marshal.GetDelegateForFunctionPointer<DwmpSDRToHDRBoostPtr>(GetProcAddress(hmodule_dwmapi, 171));

            //double brightness = 1.0;
            changeBrightness(primaryMonitor, brightness);

        }


    }

}





 