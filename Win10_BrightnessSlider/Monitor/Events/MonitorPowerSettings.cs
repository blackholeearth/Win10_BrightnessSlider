using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Win10_BrightnessSlider
{
    //class MonitorPowerSettings {}


    public partial class MessageFormPBS : Form
    {
        private IntPtr PowerNotifyHandle = IntPtr.Zero;

        public MessageFormPBS()
        {
            FormClosing += (s1,e1) => { PowerBroadcastSetting.UnregisterPowerSettingNotification(PowerNotifyHandle);  };
        }
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            var settingGuid = new PowerBroadcastSetting.PowerSettingGuid();
            Guid powerGuid = IsWindows8Plus()
                           ? settingGuid.ConsoleDisplayState
                           : settingGuid.MonitorPowerGuid;

            PowerNotifyHandle = PowerBroadcastSetting.RegisterPowerSettingNotification(
                this.Handle, powerGuid, PowerBroadcastSetting.DEVICE_NOTIFY_WINDOW_HANDLE);
        }

        private bool IsWindows8Plus()
        {
            var version = Environment.OSVersion.Version;
            if (version.Major > 6) return true; // Windows 10+
            if (version.Major == 6 && version.Minor > 1) return true; // Windows 8+
            return false;  // Windows 7 or less
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case PowerBroadcastSetting.WM_POWERBROADCAST:
                    if (m.WParam == (IntPtr)PowerBroadcastSetting.PBT_POWERSETTINGCHANGE)
                    {
                        var settings = (PowerBroadcastSetting.POWERBROADCAST_SETTING)m.GetLParam(
                            typeof(PowerBroadcastSetting.POWERBROADCAST_SETTING));

                        On_PowerSettingsChange?.Invoke(settings.Data);

                        switch (settings.Data)
                        {
                            case 0:
                                Console.WriteLine("Monitor Power Off");
                                break;
                            case 1:
                                Console.WriteLine("Monitor Power On");
                                break;
                            case 2:
                                Console.WriteLine("Monitor Dimmed");
                                break;
                        }
                    }
                    m.Result = (IntPtr)1;
                    break;
            }
            base.WndProc(ref m);
        }

        /// <summary>
        /// MonitorPowerStateChanged   par1:  0:Off, 1:_On, 2:Dimmed
        /// </summary>
        public Action<byte> On_PowerSettingsChange;
        
    }


    //winforms helper
    public static partial class PowerBroadcastSetting
    {
        public static MessageFormPBS Instance;
        static PowerBroadcastSetting()
        {
            Instance = new MessageFormPBS();
        }


    }

    partial class PowerBroadcastSetting
    {
        internal const uint DEVICE_NOTIFY_WINDOW_HANDLE = 0x0;
        internal const uint DEVICE_NOTIFY_SERVICE_HANDLE = 0x1;
        internal const int WM_POWERBROADCAST = 0x0218;
        internal const int PBT_POWERSETTINGCHANGE = 0x8013;

        [DllImport("User32.dll", SetLastError = true)]
        internal static extern IntPtr RegisterPowerSettingNotification(IntPtr hWnd, [In] Guid PowerSettingGuid, uint Flags);

        [DllImport("User32.dll", SetLastError = true)]
        internal static extern bool UnregisterPowerSettingNotification(IntPtr hWnd);

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        internal struct POWERBROADCAST_SETTING
        {
            public Guid PowerSetting;
            public uint DataLength;
            public byte Data;
        }

        // https://docs.microsoft.com/en-us/windows/win32/power/power-setting-guids
        public class PowerSettingGuid
        {
            // 0=Powered by AC, 1=Powered by Battery, 2=Powered by short-term source (UPC)
            public Guid AcdcPowerSource { get; } = new Guid("5d3e9a59-e9D5-4b00-a6bd-ff34ff516548");
            // POWERBROADCAST_SETTING.Data = 1-100
            public Guid BatteryPercentageRemaining { get; } = new Guid("a7ad8041-b45a-4cae-87a3-eecbb468a9e1");
            // Windows 8+: 0=Monitor Off, 1=Monitor On, 2=Monitor Dimmed
            public Guid ConsoleDisplayState { get; } = new Guid("6fe69556-704a-47a0-8f24-c28d936fda47");
            // Windows 8+, Session 0 enabled: 0=User providing Input, 2=User Idle
            public Guid GlobalUserPresence { get; } = new Guid("786E8A1D-B427-4344-9207-09E70BDCBEA9");
            // 0=Monitor Off, 1=Monitor On.
            public Guid MonitorPowerGuid { get; } = new Guid("02731015-4510-4526-99e6-e5a17ebd1aea");
            // 0=Battery Saver Off, 1=Battery Saver On.
            public Guid PowerSavingStatus { get; } = new Guid("E00958C0-C213-4ACE-AC77-FECCED2EEEA5");

            // Windows 8+: 0=Off, 1=On, 2=Dimmed
            public Guid SessionDisplayStatus { get; } = new Guid("2B84C20E-AD23-4ddf-93DB-05FFBD7EFCA5");

            // Windows 8+, no Session 0: 0=User providing Input, 2=User Idle
            public Guid SessionUserPresence { get; } = new Guid("3C0F4548-C03F-4c4d-B9F2-237EDE686376");
            // 0=Exiting away mode 1=Entering away mode
            public Guid SystemAwaymode { get; } = new Guid("98a7f580-01f7-48aa-9c0f-44352c29e5C0");

            /* Windows 8+ */
            // POWERBROADCAST_SETTING.Data not used
            public Guid IdleBackgroundTask { get; } = new Guid(0x515C31D8, 0xF734, 0x163D, 0xA0, 0xFD, 0x11, 0xA0, 0x8C, 0x91, 0xE8, 0xF1);

            public Guid PowerSchemePersonality { get; } = new Guid(0x245D8541, 0x3943, 0x4422, 0xB0, 0x25, 0x13, 0xA7, 0x84, 0xF6, 0x79, 0xB7);

            // The Following 3 Guids are the POWERBROADCAST_SETTING.Data result of PowerSchemePersonality
            public Guid MinPowerSavings { get; } = new Guid("8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c");
            public Guid MaxPowerSavings { get; } = new Guid("a1841308-3541-4fab-bc81-f71556f20b4a");
            public Guid TypicalPowerSavings { get; } = new Guid("381b4222-f694-41f0-9685-ff5bb260df2e");
        }

    }


}
