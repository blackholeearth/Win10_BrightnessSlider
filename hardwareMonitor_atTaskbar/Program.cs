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
			WinformsDpiHelper.SetProcess_DPIAware();

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);


			//Application.Run(new GuestForm());
			Application.Run(new Form1());
		}
	}
}
