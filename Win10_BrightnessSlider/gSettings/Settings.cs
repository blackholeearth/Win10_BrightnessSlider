using System.Collections.Generic;
using System.Drawing.Printing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System.Drawing;

namespace Win10_BrightnessSlider
{
    public enum Style
    {
        FollowSystem,
        win11,
        win10,
    }


    public class MonitorsDump
    {
        public List<RichInfoScreen> riScreens { get;  set; }
    }

    ///---------
    public class Settings
    {
        public int _id => 1; //dont let user modify it. litedb needs it.

        //public Style Style { get; set; } = Style.FollowSystem;

        public bool Hotkey_OpenEverything { get; set; } = false;
        public bool MapCopilotKey { get; set; } = false;
        public bool Show_WifiIcon { get; set; } = false;
        public bool Show_PlusMinusButtons { get; set; } = false;
        public bool Show_PlusMinusButtons_v2 { get; set; } = false;


        public bool CustomTheme_Enabled { get; set; } = false;
        public CustomTheme customTheme { get; set; } = new CustomTheme();

        public List<MonitorNames> monitorNames { get; set; }
    }

    public class MonitorNames
    {
        public string MonitorName { get; set; } = "";
        public bool useWMI_tomatch { get; set; }
        public bool useDC_toMatch { get; set; }

        public string wmi_InstanceName { get; set; }
        public string dc_monitorDevicePath { get; set; }
    }

    public class CustomTheme
    {
        public Color borderColor { get; set; } = Color.FromArgb(63, 63, 63);
        
        public bool iconsColor_isLight { get; set; } = true;
        public Color textColor { get; set; } = Color.FromArgb(255, 255, 255);
        public Color backColor { get; set; } = Color.FromArgb(40, 16, 20);

        public Color knobColor { get; set; } = Color.FromArgb(207, 145, 182);
        public Color barRemainingColor { get; set; } = Color.FromArgb(199, 199, 199);
        public Color barBorderColor { get; set; } = Color.FromArgb(199, 199, 199);
    }



}
