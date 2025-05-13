using System;
using static Win10_BrightnessSlider.DxvaMonFn;

namespace Win10_BrightnessSlider
{
    public class dxvaMonFn_Example
    {

        public static PHYSICAL_MONITOR[] PhysicalMonitors;

        // create a class scoped array to hold the PHYSICAL_MONITOR structurs for each Physical Monitor
        public static void Form1_FormClosing()
        {
            DxvaMonFn.DestroyAllPhysicalMonitors(PhysicalMonitors);
            // when the form is closing you must destroy the PHYSICAL_MONITOR structurs in the unmanaged memory
        }

        public static void Form1_Load()
        {

            PhysicalMonitors = DxvaMonFn.GetPhysicalMonitors_All_Flattened();
            // call the GetPhysicalMonitors function to fill the class scoped array of PHYSICAL_MONITOR structurs
            // this is just to show the index and discription of each PhysicalMonitor that was found
            foreach (DxvaMonFn.PHYSICAL_MONITOR pm in PhysicalMonitors)
            {
                Console.WriteLine(pm.hPhysicalMonitor.ToString() + "  " + pm.szPhysicalMonitorDescription + "\r\n");
            }

            // if there is at least 1 PhysicalMonitor, then try setting NumericUpDown1 to PhysicalMonitor(0) Brightness value
            if ((PhysicalMonitors.Length > 0))
            {
                double Brightness1 = DxvaMonFn.GetPhysicalMonitorBrightness(PhysicalMonitors[0]);
                // get the Brightness level of PhysicalMonitor(0)
                if (!double.IsNaN(Brightness1))
                {
                    // make sure the Brightness level was successfully retrieved
                    var brSlider1Value = Brightness1;
                }

            }

            // if there is more than 1 PhysicalMonitor, then try setting NumericUpDown2 to PhysicalMonitor(1) Brightness value
            if ((PhysicalMonitors.Length > 1))
            {
                double Brightness2 = DxvaMonFn.GetPhysicalMonitorBrightness(PhysicalMonitors[1]);
                // get the Brightness level of PhysicalMonitor(1)
                if (!double.IsNaN(Brightness2))
                {
                    // make sure the Brightness level was successfully retrieved
                    var brSlider2Value = Brightness2;

                }

            }

        }

        private void NumericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            if ((PhysicalMonitors.Length > 0))
            {
                //  MonitorHelper.SetPhysicalMonitorBrightness(PhysicalMonitors[0], NumericUpDown1.Value);
            }

        }

        private void NumericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            if ((PhysicalMonitors.Length > 1))
            {
                //  MonitorHelper.SetPhysicalMonitorBrightness(PhysicalMonitors[1], NumericUpDown2.Value);
            }

        }
    }


}