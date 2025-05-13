using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
//using Win10_BrightnessSlider._testArea;

namespace Win10_BrightnessSlider
{
    static class Program
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


            //new frm1_test().Show();

            Application.Run(new Form1());

        }
    }
}
