using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Windows.Forms;

namespace Win10_BrightnessSlider.Monitor.Events
{

    public static class ThemeHelper
    {
        // static ThemeChangeEvents() {
        //    SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;
        //}

        //private void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e) {
        //    if (e.Category == UserPreferenceCategory.General)
        //        setTheme();
        //}

        /// <summary>
        /// NOTICE - please use **TaskbarUtil.isTaskbarColor_Light()**
        /// <para></para>
        /// may throw "RegKey Not Found Exception", when debugging if not found..  !!! app crashes without error  on customer pc.
        /// </summary>
        /// <returns></returns>
        public static bool ThemeIsLight_viaRegedit()
        {
            RegistryKey registry =
                Registry.CurrentUser.OpenSubKey(
                    @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", false);
            return (int)registry.GetValue("SystemUsesLightTheme") == 1;
        }

        public static void setTheme()
        {
            var br = ThemeIsLight_viaRegedit() ? Brushes.White : Brushes.Black;
            MessageBox.Show(br.ToString());



        }
    }



}
