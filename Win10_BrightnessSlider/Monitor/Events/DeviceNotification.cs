using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Win10_BrightnessSlider.DeviceNotification;

namespace Win10_BrightnessSlider
{

    /// <summary>
    /// WndProc
    /// </summary>
    public class MessageFormDN : Form
    {
        public MessageFormDN()
        {
            DeviceNotification.RegisterDeviceNotification(this.Handle, monitorOnly: true);
            this.FormClosing += (s1,e1)=> {  DeviceNotification.UnregisterDeviceNotification();  };
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == DeviceNotification.WmDevicechange)
            {

                //pass1
                bool isGuid_MonitorOrDisplayAdapter = false; 
                switch ((int)m.WParam)
                {
                    case DbtDevice_RemoveComplete:
                    case DbtDevice_Arrival:

                        //---getType
                        DeviceEventType status_asEventType = DeviceEventType.Unknown;
                        if ((int)m.WParam == DbtDevice_RemoveComplete)
                            status_asEventType = DeviceEventType.Disconnected;
                        else if ((int)m.WParam == DbtDevice_Arrival)
                            status_asEventType = DeviceEventType.Connected;

                        //---TryGetInterface
                        //HandleMessage(m.LParam);
                        var devInterface = HandleMessage__TryGet_DeviceInterface(m.LParam);
                        if (devInterface != null) 
                        {
                            isGuid_MonitorOrDisplayAdapter = 
                                DeviceNotification.isGuid_MonitorOrDisplayAdapter( 
                                    ((DEV_BROADCAST_DEVICEINTERFACE)devInterface).dbcc_classguid
                                    );
                        }

                        On_WmDeviceChanged?.Invoke(null, new DeviceChangedEventArgs(status_asEventType, isGuid_MonitorOrDisplayAdapter));
                        break;
                }

                ////pass2
                ////int status = -1;
                //DeviceEventType status_asEventType =DeviceEventType.Unknown;
                //switch ((int)m.WParam)
                //{
                //    case DeviceNotification.DbtDevice_RemoveComplete:
                //        //status = 0;
                //        status_asEventType = DeviceEventType.Disconnected;
                //        DeviceRemoved?.Invoke(); 
                //        break;
                //    case DeviceNotification.DbtDevice_Arrival:
                //        //status = 1;
                //        status_asEventType = DeviceEventType.Connected;
                //        DeviceAdded?.Invoke();  
                //        break;
                //}

                //On_WmDevicechange?.Invoke((int)m.WParam);
                //On_WmDevicechange?.Invoke(status , isGuid_MonitorOrDisplayAdapter);
                //On_WmDevicechange?.Invoke(status , isGuid_MonitorOrDisplayAdapter);
               
            }
        }


      


        //events??
        /// <summary>
        ///   Occurs When Device Plug Unplug. Device:Monitor Only , ... On_Device_Add_Remove , par1: DbtDevice_RemoveComplete , DbtDevice_Arrival
        ///  <para></para> int status: ( 0 removed ,1 added , -1 unknown) 
        ///  <para></para> bool isGuid_MonitorOrDisplayAdapter: if true its monitor related..
        /// </summary>
        public Action<int, bool> On_WmDevicechange;
        public Action DeviceAdded;
        public Action DeviceRemoved;

        
        /// <summary>
        /// this is Event New version
        /// </summary>
        public event EventHandler<DeviceChangedEventArgs> On_WmDeviceChanged; // Renamed event
        public class DeviceChangedEventArgs : EventArgs
        {
            /// <summary>
            /// connected , or disconnected
            /// </summary>
            public DeviceEventType EventType { get; }
            public bool IsGuid_MonitorOrDisplayAdapter { get; }

            public DeviceChangedEventArgs(DeviceEventType eventType , bool isGuid_MonitorOrDisplayAdapter)
            {
                EventType = eventType;
                IsGuid_MonitorOrDisplayAdapter = isGuid_MonitorOrDisplayAdapter;
            }
        }
        public enum DeviceEventType
        {
            Unknown,
            Connected,
            Disconnected
        }

    }


    //winforms helper
    public static partial class DeviceNotification 
    {
        public static MessageFormDN Instance;
        static  DeviceNotification()
        {
            Instance = new MessageFormDN();
        }
    }



    static partial class DeviceNotification
    {
        //https://msdn.microsoft.com/en-us/library/aa363480(v=vs.85).aspx
        public const int DbtDevice_Arrival = 0x8000; // system detected a new device        
        public const int DbtDevice_RemoveComplete = 0x8004; // device is gone     
        public const int DbtDevNodesChanged = 0x0007; //A device has been added to or removed from the system.

        public const int WmDevicechange = 0x0219; // device change event      
        private const int DbtDevtypDeviceinterface = 5;

        //https://msdn.microsoft.com/en-us/library/aa363431(v=vs.85).aspx
        private const int DEVICE_NOTIFY_ALL_INTERFACE_CLASSES = 4;
        private static readonly Guid GUID_DEVINTERFACE_USB_DEVICE = new Guid("A5DCBF10-6530-11D2-901F-00C04FB951ED"); // USB devices
        private static readonly Guid GUID_DEVINTERFACE_MONITOR = new Guid("E6F07B5F-EE97-4a90-B076-33F57BF4EAA7"); // monitor
        private static readonly Guid GUID_DEVINTERFACE_DISPLAY_ADAPTER = new Guid("5B45201D-F2F2-4F3B-85BB-30FF1F953599"); // displayAdapter
        private static IntPtr notificationHandle;
        public static bool isGuid_MonitorOrDisplayAdapter(Guid guid) 
        {
            if (guid == GUID_DEVINTERFACE_MONITOR ||
                guid == GUID_DEVINTERFACE_DISPLAY_ADAPTER )
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Registers a window to receive notifications when devices are plugged or unplugged.
        /// </summary>
        /// <param name="windowHandle">Handle to the window receiving notifications.</param>
        /// <param name="usbOnly">true to filter to USB devices only, false to be notified for all devices.</param>
        public static void RegisterDeviceNotification(IntPtr windowHandle, bool monitorOnly = false)
        {
            var dbi = new Dev_Broadcast_Deviceinterface
            {
                DeviceType = DbtDevtypDeviceinterface,
                Reserved = 0,
                ClassGuid = GUID_DEVINTERFACE_MONITOR,
                Name = 0
            };

            dbi.Size = Marshal.SizeOf(dbi);
            IntPtr buffer = Marshal.AllocHGlobal(dbi.Size);
            Marshal.StructureToPtr(dbi, buffer, true);


            var flags = monitorOnly ? 0 : DEVICE_NOTIFY_ALL_INTERFACE_CLASSES;
            notificationHandle = RegisterDeviceNotification(windowHandle, buffer, flags);
        }

        /// <summary>
        /// Unregisters the window for device notifications
        /// </summary>
        public static void UnregisterDeviceNotification() => UnregisterDeviceNotification(notificationHandle);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr RegisterDeviceNotification(IntPtr recipient, IntPtr notificationFilter, int flags);

        [DllImport("user32.dll")]
        private static extern bool UnregisterDeviceNotification(IntPtr handle);
        //previosly private
        [StructLayout(LayoutKind.Sequential)]
        public struct Dev_Broadcast_Deviceinterface
        {
            internal int Size;
            internal int DeviceType;
            internal int Reserved;
            internal Guid ClassGuid;
            internal short Name;
        }



        // https://learn.microsoft.com/en-us/windows/win32/api/dbt/ns-dbt-dev_broadcast_hdr
        //https://gist.github.com/emoacht/73eff195317e387f4cda
        [StructLayoutAttribute(LayoutKind.Sequential)]
        private struct DEV_BROADCAST_HDR
        {
            public uint dbch_size;
            public uint dbch_devicetype;
            public uint dbch_reserved;
        }
        private const uint DBT_DEVTYP_DEVICEINTERFACE = 0x00000005;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct DEV_BROADCAST_DEVICEINTERFACE
        {
            public uint dbcc_size;
            public uint dbcc_devicetype;
            public uint dbcc_reserved;
            public Guid dbcc_classguid;

            // To get value from lParam of WM_DEVICECHANGE, this length must be longer than 1.
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 255)]
            public string dbcc_name;
        }


        /// <summary>
        /// Not every Message is DeviceInterface,  maybe null
        /// </summary>
        /// <param name="lParam"></param>
        public static DEV_BROADCAST_DEVICEINTERFACE? HandleMessage__TryGet_DeviceInterface(IntPtr lParam)
        {
            var header = (DEV_BROADCAST_HDR)Marshal.PtrToStructure(lParam, typeof(DEV_BROADCAST_HDR));

            if (header.dbch_devicetype != DBT_DEVTYP_DEVICEINTERFACE)
                return null;


            var deviceInterface = (DEV_BROADCAST_DEVICEINTERFACE)Marshal.PtrToStructure(lParam, typeof(DEV_BROADCAST_DEVICEINTERFACE));
            return deviceInterface;

            ////fcking finally 
            //bool isGuid_MonitorOrDisplayAdapter = DeviceNotification.isGuid_MonitorOrDisplayAdapter(deviceInterface.dbcc_classguid);
        }


        public static void HandleMessage(IntPtr lParam)
        {
            var header = (DEV_BROADCAST_HDR)Marshal.PtrToStructure(lParam, typeof(DEV_BROADCAST_HDR));

            switch (header.dbch_devicetype)
            {
                case DBT_DEVTYP_DEVICEINTERFACE:
                    var deviceInterface = (DEV_BROADCAST_DEVICEINTERFACE)Marshal.PtrToStructure(lParam, typeof(DEV_BROADCAST_DEVICEINTERFACE));

                    //fcking finally 
                    bool isGuid_MonitorOrDisplayAdapter = DeviceNotification.isGuid_MonitorOrDisplayAdapter(deviceInterface.dbcc_classguid);

                    //var vid = GetVid(deviceInterface.dbcc_name);
                    //var pid = GetPid(deviceInterface.dbcc_name);
                    //Debug.WriteLine("XXX device changed. VID:{0} PID:{1}", vid, pid);

                    break;
            }
        }

        #region getVidPid
        private static readonly Regex _patternVid = new Regex("VID_[0-9A-Z]{4}", RegexOptions.Compiled);
        private static readonly Regex _patternPid = new Regex("PID_[0-9A-Z]{4}", RegexOptions.Compiled);
        private static string GetVid(string source)
        {
            var match = _patternVid.Match(source);
            return match.Success ? match.Value : String.Empty;
        }
        private static string GetPid(string source)
        {
            var match = _patternPid.Match(source);
            return match.Success ? match.Value : String.Empty;
        }
        #endregion










    }


}
