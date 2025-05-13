using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace winformsTests
{
    internal static class Program
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // ***fix blurry text on High-Dpi Screen
            if (Environment.OSVersion.Version.Major >= 6)
                SetProcessDPIAware();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
			
			
			 
			Application.Run(new _4_contextMenuStrip.Form1());
			return;


			new _3_scroll.Form1().ShowDialog() ;
			
            Application.Run(new _1_HDR.frmSDR_test());
        }
    }
}
