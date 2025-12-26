using System;
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
        public bool Show_PresetButtons { get; set; } = false;
        public List<int> PresetButtonPercentages { get; set; } = new List<int> { 0, 10, 25, 50, 75, 99 };  // Max 6 buttons

        public bool EnableScheduledBrightness { get; set; } = false;
        public List<BrightnessSchedule> BrightnessSchedules { get; set; } = new List<BrightnessSchedule>();

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

    /// <summary>
    /// Stores brightness value for a specific monitor in a schedule
    /// </summary>
    public class MonitorBrightnessEntry
    {
        public string MonitorId { get; set; }      // WMI InstanceName for matching
        public string MonitorName { get; set; }    // User-friendly display name
        public int BrightnessPercent { get; set; }
    }

    public class BrightnessSchedule
    {
        public string Time { get; set; }  // HH:mm format (e.g., "23:00" for 11 PM)
        public int BrightnessPercent { get; set; }  // Used when ApplyToAllMonitors = true
        public bool Enabled { get; set; } = true;
        public List<DayOfWeek> Days { get; set; } = new List<DayOfWeek> { DayOfWeek.Sunday, DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday };

        // Per-monitor brightness support
        public bool ApplyToAllMonitors { get; set; } = true;  // true = same brightness for all, false = use PerMonitorBrightness
        public List<MonitorBrightnessEntry> PerMonitorBrightness { get; set; } = new List<MonitorBrightnessEntry>();

        public TimeSpan GetTimeSpan()
        {
            if (TimeSpan.TryParse(Time, out var result))
                return result;
            return TimeSpan.Zero;
        }

        public bool AppliesToday()
        {
            if (Days == null || Days.Count == 0)
                return true;  // If no days specified, apply every day
            return Days.Contains(DateTime.Now.DayOfWeek);
        }

        public string GetDaysString()
        {
            if (Days == null || Days.Count == 0 || Days.Count == 7)
                return "Every day";
            if (Days.Count == 5 && !Days.Contains(DayOfWeek.Saturday) && !Days.Contains(DayOfWeek.Sunday))
                return "Weekdays";
            if (Days.Count == 2 && Days.Contains(DayOfWeek.Saturday) && Days.Contains(DayOfWeek.Sunday))
                return "Weekends";

            var dayAbbr = new Dictionary<DayOfWeek, string>
            {
                { DayOfWeek.Sunday, "Sun" },
                { DayOfWeek.Monday, "Mon" },
                { DayOfWeek.Tuesday, "Tue" },
                { DayOfWeek.Wednesday, "Wed" },
                { DayOfWeek.Thursday, "Thu" },
                { DayOfWeek.Friday, "Fri" },
                { DayOfWeek.Saturday, "Sat" }
            };

            return string.Join(", ", Days.Distinct().OrderBy(d => (int)d).Select(d => dayAbbr[d]));
        }

        /// <summary>
        /// Gets a display string for the brightness setting (e.g., "75%" or "Per-Monitor")
        /// </summary>
        public string GetBrightnessString()
        {
            if (ApplyToAllMonitors)
                return $"{BrightnessPercent}%";

            if (PerMonitorBrightness == null || PerMonitorBrightness.Count == 0)
                return $"{BrightnessPercent}%";

            return "Per-Monitor";
        }
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
