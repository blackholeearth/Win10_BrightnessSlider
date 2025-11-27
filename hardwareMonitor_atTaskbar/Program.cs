using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace hardwareMonitor_atTaskbar
{
	internal static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			// //-- for 4.72 ,  blurry on  rdp / dpi changes
			//WinformsDpiHelper.SetProcess_DPIAware();
			// //-- for 4.8 , not blurry on RDP, but wont resize the form with dpi change while app runs.
			//WinformsDpiHelper.SetProcessDPIAware_PerMonitorV2_withFallback();  

#if NET8_0_OR_GREATER
        // .NET 8: The modern one-line fix
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
#else
			// .NET 4.8: Relies on app.manifest (see below)
#endif

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);


			//Application.Run(new GuestForm());
			Application.Run(new Form1_hw());
		}
	}
}
