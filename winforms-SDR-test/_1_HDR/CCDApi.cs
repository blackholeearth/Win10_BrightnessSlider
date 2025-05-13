using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace winformsTests._1_HDR
{

  
    // https://stackoverflow.com/questions/66155083/windows-api-to-get-whether-hdrhigh-dynamic-range-is-active?rq=3
    public static class CCD_Api
    {


        /// <summary>
        /// returns testing console message as string
        /// </summary>
        /// <exception cref="Win32Exception"></exception>
        public static string Dump_eachMonitor_HDR_supported_or_Enabled()
        {
            StringBuilder sbout = new StringBuilder();


            var err = GetDisplayConfigBufferSizes(QDC.QDC_ONLY_ACTIVE_PATHS, out var pathCount, out var modeCount);
            if (err != 0)
                throw new Win32Exception(err);

            var paths = new DISPLAYCONFIG_PATH_INFO[pathCount];
            var modes = new DISPLAYCONFIG_MODE_INFO[modeCount];
            err = QueryDisplayConfig(QDC.QDC_ONLY_ACTIVE_PATHS, ref pathCount, paths, ref modeCount, modes, IntPtr.Zero);
            if (err != 0)
                throw new Win32Exception(err);


            foreach (var path in paths)
            {
                // get display name
                var info = new DISPLAYCONFIG_TARGET_DEVICE_NAME();
                info.header.type = DISPLAYCONFIG_DEVICE_INFO_TYPE.GET_TARGET_NAME;
                info.header.size = Marshal.SizeOf<DISPLAYCONFIG_TARGET_DEVICE_NAME>();
                info.header.adapterId = path.targetInfo.adapterId;
                info.header.id = path.targetInfo.id;
                err = DisplayConfigGetDeviceInfo(ref info);
                if (err != 0)
                    throw new Win32Exception(err);

                //is hdr supported 
                var colorInfo = new DISPLAYCONFIG_GET_ADVANCED_COLOR_INFO();
                colorInfo.header.type = DISPLAYCONFIG_DEVICE_INFO_TYPE.GET_ADVANCED_COLOR_INFO;
                colorInfo.header.size = Marshal.SizeOf<DISPLAYCONFIG_GET_ADVANCED_COLOR_INFO>();
                colorInfo.header.adapterId = path.targetInfo.adapterId;
                colorInfo.header.id = path.targetInfo.id;
                err = DisplayConfigGetDeviceInfo(ref colorInfo);
                if (err != 0)
                    throw new Win32Exception(err);


               


                Console.WriteLine(info.monitorFriendlyDeviceName);
                Console.WriteLine(" Advanced Color Supported: " + colorInfo.advancedColorSupported);
                Console.WriteLine(" Advanced Color Enabled  : " + colorInfo.advancedColorEnabled);
                Console.WriteLine();

                try
                {
                    //this doesnt work need working example-- -
                    var colorInfoSDR_lvl = new DISPLAYCONFIG_SDR_WHITE_LEVEL();
                    colorInfoSDR_lvl.header.type = DISPLAYCONFIG_DEVICE_INFO_TYPE.GET_SDR_WHITE_LEVEL;
                    colorInfoSDR_lvl.header.size = Marshal.SizeOf<DISPLAYCONFIG_SDR_WHITE_LEVEL>();
                    colorInfoSDR_lvl.header.adapterId = path.targetInfo.adapterId;
                    colorInfoSDR_lvl.header.id = path.targetInfo.id;
                    err = DisplayConfigGetDeviceInfo(ref colorInfoSDR_lvl);
                    if (err != 0)
                        throw new Win32Exception(err);
                    sbout.AppendLine("colorInfoSDR_lvl.SDRWhiteLevel       : " + colorInfoSDR_lvl.SDRWhiteLevel);
                    sbout.AppendLine("colorInfoSDR_lvl.SDRWhiteLevelInNits : " + colorInfoSDR_lvl.SDRWhiteLevelInNits);
                }
                catch (Exception ex)
                {
                    sbout.AppendLine("ERROR-FAİLED---  colorInfoSDR_lvl");
                    sbout.AppendLine(ex + "");
                }
                sbout.AppendLine("-----path.targetInfo-----" );
                sbout.AppendLine("p.ti. adapterId        :" + path.targetInfo.adapterId);
                sbout.AppendLine("p.ti. id               :" + path.targetInfo.id);
                sbout.AppendLine("p.ti. modeInfoIdx      :" + path.targetInfo.modeInfoIdx);
                sbout.AppendLine("p.ti. outputTechnology :" + path.targetInfo.outputTechnology);
                //sbout.AppendLine("p.ti. refreshRate    :" + path.targetInfo.refreshRate);
                sbout.AppendLine("p.ti. statusFlags      :" + path.targetInfo.statusFlags);
                sbout.AppendLine("p.ti. targetAvailable  :" + path.targetInfo.targetAvailable);
                sbout.AppendLine("monFriendlyDeviceName  :" + info.monitorFriendlyDeviceName);

                sbout.AppendLine(" Advanced Color Supported: " + colorInfo.advancedColorSupported);
                sbout.AppendLine(" Advanced Color Enabled  : " + colorInfo.advancedColorEnabled);
                sbout.AppendLine();

              




            }

            return sbout.ToString();
        }

        public static string Dump_eachMonitor_SDR_whiteLevel()
        {
            StringBuilder sbout = new StringBuilder();


            var err = GetDisplayConfigBufferSizes(QDC.QDC_ONLY_ACTIVE_PATHS, out var pathCount, out var modeCount);
            if (err != 0)
                throw new Win32Exception(err);

            var paths = new DISPLAYCONFIG_PATH_INFO[pathCount];
            var modes = new DISPLAYCONFIG_MODE_INFO[modeCount];
            err = QueryDisplayConfig(QDC.QDC_ONLY_ACTIVE_PATHS, ref pathCount, paths, ref modeCount, modes, IntPtr.Zero);
            if (err != 0)
                throw new Win32Exception(err);


            foreach (var path in paths)
            {
                // get display name
                var info = new DISPLAYCONFIG_TARGET_DEVICE_NAME();
                info.header.type = DISPLAYCONFIG_DEVICE_INFO_TYPE.GET_TARGET_NAME;
                info.header.size = Marshal.SizeOf<DISPLAYCONFIG_TARGET_DEVICE_NAME>();
                info.header.adapterId = path.targetInfo.adapterId;
                info.header.id = path.targetInfo.id;
                err = DisplayConfigGetDeviceInfo(ref info);
                if (err != 0)
                    throw new Win32Exception(err);

                //is hdr supported 
                var colorInfo = new DISPLAYCONFIG_GET_ADVANCED_COLOR_INFO();
                colorInfo.header.type = DISPLAYCONFIG_DEVICE_INFO_TYPE.GET_ADVANCED_COLOR_INFO;
                colorInfo.header.size = Marshal.SizeOf<DISPLAYCONFIG_GET_ADVANCED_COLOR_INFO>();
                colorInfo.header.adapterId = path.targetInfo.adapterId;
                colorInfo.header.id = path.targetInfo.id;
                err = DisplayConfigGetDeviceInfo(ref colorInfo);
                if (err != 0)
                    throw new Win32Exception(err);


                Console.WriteLine(info.monitorFriendlyDeviceName);
                Console.WriteLine(" Advanced Color Supported: " + colorInfo.advancedColorSupported);
                Console.WriteLine(" Advanced Color Enabled  : " + colorInfo.advancedColorEnabled);
                Console.WriteLine();

                try
                {
                    DISPLAYCONFIG_SDR_WHITE_LEVEL dc_sdrWhiteLevel = Get_SDR_WhiteLevel(path);
                    sbout.AppendLine("colorInfoSDR_lvl.SDRWhiteLevel       : " + dc_sdrWhiteLevel.SDRWhiteLevel);
                    sbout.AppendLine("colorInfoSDR_lvl.SDRWhiteLevelInNits : " + dc_sdrWhiteLevel.SDRWhiteLevelInNits);
                }
                catch (Exception ex)
                {
                    sbout.AppendLine("ERROR-FAİLED---  colorInfoSDR_lvl");
                    sbout.AppendLine(ex + "");
                }

                sbout.AppendLine(" Advanced Color Supported: " + colorInfo.advancedColorSupported);
                sbout.AppendLine(" Advanced Color Enabled  : " + colorInfo.advancedColorEnabled);
                sbout.AppendLine();






            }

            return sbout.ToString();
        }


        public static string SET_eachMonitor_SDR_whiteLevel(int nits)
        {
            StringBuilder sbout = new StringBuilder();

            var paths = QueryDisplayConfig();
            
            int err;
            foreach (var path in paths)
            {
                Set_SDR_WhiteLevel(path , nits);

                Console.WriteLine("set sdr level - for path:" + path.targetInfo.id +"--" +path.targetInfo.adapterId);

                try
                {
                    DISPLAYCONFIG_SDR_WHITE_LEVEL dc_sdrWhiteLevel = Get_SDR_WhiteLevel(path);
                    sbout.AppendLine("colorInfoSDR_lvl.SDRWhiteLevel       : " + dc_sdrWhiteLevel.SDRWhiteLevel);
                    sbout.AppendLine("colorInfoSDR_lvl.SDRWhiteLevelInNits : " + dc_sdrWhiteLevel.SDRWhiteLevelInNits);
                }
                catch (Exception ex)
                {
                    sbout.AppendLine("ERROR-FAİLED---  colorInfoSDR_lvl");
                    sbout.AppendLine(ex + "");
                }




            }

            return sbout.ToString();
        }

        static DISPLAYCONFIG_PATH_INFO[] QueryDisplayConfig()
        {
            DISPLAYCONFIG_PATH_INFO[] paths;


            int err = GetDisplayConfigBufferSizes(QDC.QDC_ONLY_ACTIVE_PATHS, out var pathCount, out var modeCount);
            if (err != 0)
                throw new Win32Exception(err);

            paths = new DISPLAYCONFIG_PATH_INFO[pathCount];
            var modes = new DISPLAYCONFIG_MODE_INFO[modeCount];
            err = QueryDisplayConfig(QDC.QDC_ONLY_ACTIVE_PATHS, ref pathCount, paths, ref modeCount, modes, IntPtr.Zero);
            if (err != 0)
                throw new Win32Exception(err);

            return paths;
        }

        static DISPLAYCONFIG_SDR_WHITE_LEVEL Get_SDR_WhiteLevel(DISPLAYCONFIG_PATH_INFO path)
        {
            var colorInfoSDR_lvl = new DISPLAYCONFIG_SDR_WHITE_LEVEL();
            colorInfoSDR_lvl.header.type = DISPLAYCONFIG_DEVICE_INFO_TYPE.GET_SDR_WHITE_LEVEL;
            colorInfoSDR_lvl.header.size = Marshal.SizeOf<DISPLAYCONFIG_SDR_WHITE_LEVEL>();
            colorInfoSDR_lvl.header.adapterId = path.targetInfo.adapterId;
            colorInfoSDR_lvl.header.id = path.targetInfo.id;
            int err = DisplayConfigGetDeviceInfo(ref colorInfoSDR_lvl);
            if (err != 0)
                throw new Win32Exception(err);

            return colorInfoSDR_lvl;
        }



        [StructLayout(LayoutKind.Sequential)]
        private struct DISPLAYCONFIG_SET_SDR_WHITE_LEVEL
        {
            public DISPLAYCONFIG_DEVICE_INFO_HEADER header;
            public uint SDRWhiteLevel;

            /// <summary>
            /// 1 to make it persistent , 0 : fast but , not persist , wont update win11 setting UI 
            /// </summary>
            public byte finalValue;  
        }
        [DllImport("user32.dll")]
        private static extern int DisplayConfigSetDeviceInfo(ref DISPLAYCONFIG_SET_SDR_WHITE_LEVEL request);
        
        /// <summary>
        /// nits 80 160 --- to 480
        /// </summary>
        /// <param name="path"></param>
        /// <param name="nits"></param>
        /// <returns></returns>
        /// <exception cref="Win32Exception"></exception>
        static DISPLAYCONFIG_SET_SDR_WHITE_LEVEL Set_SDR_WhiteLevel(DISPLAYCONFIG_PATH_INFO path , int nits)
        {
            
            var req = new DISPLAYCONFIG_SET_SDR_WHITE_LEVEL();
            req.header.type = DISPLAYCONFIG_DEVICE_INFO_TYPE.SET_SDR_WHITE_LEVEL;
            req.header.size = Marshal.SizeOf<DISPLAYCONFIG_SET_SDR_WHITE_LEVEL>();
            req.header.adapterId = path.targetInfo.adapterId;
            req.header.id = path.targetInfo.id;

            var val = (uint)(nits * 1000 / 80);
            //req.SDRWhiteLevel = val /1000;
            req.SDRWhiteLevel = val;
            req.finalValue = 1;//1 persist update winSettingUI ; 0 fast


            int err = DisplayConfigSetDeviceInfo(ref req);
            if (err != 0)
                throw new Win32Exception(err);

            return req;
        }



        [StructLayout(LayoutKind.Sequential)]
        struct DISPLAYCONFIG_SDR_WHITE_LEVEL
        {
            public DISPLAYCONFIG_DEVICE_INFO_HEADER header;
            public uint SDRWhiteLevel;
            // https://gregsplaceontheweb.wordpress.com/converting-cpp-types-to-csharp/
            // https://github.com/terrafx/terrafx.interop.windows/blob/ed8ec34dcd615acd7d117ff052a62843d5e4a5d6/sources/Interop/Windows/Windows/um/wingdi/DISPLAYCONFIG_SDR_WHITE_LEVEL.cs
            //public ulong SDRWhiteLevel;

            public float SDRWhiteLevelInNits => (float)SDRWhiteLevel / 1000 * 80;

        }

        public enum DISPLAYCONFIG_DEVICE_INFO_TYPE : uint
        {
            GET_SOURCE_NAME = 1,
            GET_TARGET_NAME = 2,
            GET_TARGET_PREFERRED_MODE = 3,
            GET_ADAPTER_NAME = 4,
            SET_TARGET_PERSISTENCE = 5,
            GET_TARGET_BASE_TYPE = 6,
            GET_SUPPORT_VIRTUAL_RESOLUTION = 7,
            SET_SUPPORT_VIRTUAL_RESOLUTION = 8,
            GET_ADVANCED_COLOR_INFO  = 9,
            SET_ADVANCED_COLOR_STATE = 10,
            GET_SDR_WHITE_LEVEL = 11,
            /*new  https://www.magnumdb.com/search?q=parent:DISPLAYCONFIG_DEVICE_INFO_TYPE */
            GET_MONITOR_SPECIALIZATION = 12,
            SET_MONITOR_SPECIALIZATION = 13,
            SET_SDR_WHITE_LEVEL = 14,   //  14 // Was 0xFFFFFFEE
            GET_ADVANCED_COLOR_INFO_2  = 15,
            SET_HDR_STATE = 16,
            SET_WCG_STATE = 17,

            //from deepseek monitorian
            //SET_SDR_WHITE_LEVEL = 0xFFFFFFEE

        }
        static UInt32 DISPLAYCONFIG_DEVICE_INFO_TYPE_FORCE_UINT32 = 4294967295;

        public enum DISPLAYCONFIG_COLOR_ENCODING
        {
            RGB = 0,
            YCBCR444 = 1,
            YCBCR422 = 2,
            YCBCR420 = 3,
            INTENSITY = 4,
        }

        public enum DISPLAYCONFIG_SCALING
        {
            IDENTITY = 1,
            CENTERED = 2,
            STRETCHED = 3,
            ASPECTRATIOCENTEREDMAX = 4,
            CUSTOM = 5,
            PREFERRED = 128,
        }

        public enum DISPLAYCONFIG_ROTATION
        {
            IDENTITY = 1,
            ROTATE90 = 2,
            ROTATE180 = 3,
        }

        public enum DISPLAYCONFIG_VIDEO_OUTPUT_TECHNOLOGY
        {
            OTHER = -1,
            HD15 = 0,
            SVIDEO = 1,
            COMPOSITE_VIDEO = 2,
            COMPONENT_VIDEO = 3,
            DVI = 4,
            HDMI = 5,
            LVDS = 6,
            D_JPN = 8,
            SDI = 9,
            DISPLAYPORT_EXTERNAL = 10,
            DISPLAYPORT_EMBEDDED = 11,
            UDI_EXTERNAL = 12,
            UDI_EMBEDDED = 13,
            SDTVDONGLE = 14,
            MIRACAST = 15,
            INDIRECT_WIRED = 16,
            INDIRECT_VIRTUAL = 17,
            INTERNAL = unchecked((int)0x80000000),
        }

        enum DISPLAYCONFIG_TOPOLOGY_ID
        {
            INTERNAL = 0x00000001,
            CLONE = 0x00000002,
            EXTEND = 0x00000004,
            EXTERNAL = 0x00000008,
        }

        public enum DISPLAYCONFIG_PATH
        {
            ACTIVE = 0x00000001,
            PREFERRED_UNSCALED = 0x00000004,
            SUPPORT_VIRTUAL_MODE = 0x00000008,
        }

        public enum DISPLAYCONFIG_SOURCE_FLAGS
        {
            IN_USE = 0x00000001,
        }

        public enum DISPLAYCONFIG_TARGET_FLAGS
        {
            IN_USE = 0x00000001,
            FORCIBLE = 0x00000002,
            FORCED_AVAILABILITY_BOOT = 0x00000004,
            FORCED_AVAILABILITY_PATH = 0x00000008,
            FORCED_AVAILABILITY_SYSTEM = 0x00000010,
            IS_HMD = 0x00000020,
        }

        enum QDC
        {
            QDC_ALL_PATHS = 0x00000001,
            QDC_ONLY_ACTIVE_PATHS = 0x00000002,
            QDC_DATABASE_CURRENT = 0x00000004,
            QDC_VIRTUAL_MODE_AWARE = 0x00000010,
            QDC_INCLUDE_HMD = 0x00000020,
        }

        public enum DISPLAYCONFIG_SCANLINE_ORDERING
        {
            UNSPECIFIED = 0,
            PROGRESSIVE = 1,
            INTERLACED = 2,
            INTERLACED_UPPERFIELDFIRST = INTERLACED,
            INTERLACED_LOWERFIELDFIRST = 3,
        }

        enum DISPLAYCONFIG_PIXELFORMAT
        {
            _8BPP = 1,
            _16BPP = 2,
            _24BPP = 3,
            _32BPP = 4,
            _NONGDI = 5,
        }

        enum DISPLAYCONFIG_MODE_INFO_TYPE
        {
            SOURCE = 1,
            TARGET = 2,
            DESKTOP_IMAGE = 3,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DISPLAYCONFIG_DEVICE_INFO_HEADER
        {
            public DISPLAYCONFIG_DEVICE_INFO_TYPE type;
            public int size;
            public LUID adapterId;
            public uint id;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct DISPLAYCONFIG_GET_ADVANCED_COLOR_INFO
        {
            public DISPLAYCONFIG_DEVICE_INFO_HEADER header;
            public uint value;
            public DISPLAYCONFIG_COLOR_ENCODING colorEncoding;
            public int bitsPerColorChannel;

            public bool advancedColorSupported => (value & 0x1) == 0x1;
            public bool advancedColorEnabled => (value & 0x2) == 0x2;
            public bool wideColorEnforced => (value & 0x4) == 0x4;
            public bool advancedColorForceDisabled => (value & 0x8) == 0x8;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct POINTL
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct LUID
        {
            public uint LowPart;
            public int HighPart;

            public long Value => ((long)HighPart << 32) | LowPart;
            public override string ToString() => Value.ToString();
        }

        [StructLayout(LayoutKind.Sequential)]
        struct DISPLAYCONFIG_SOURCE_MODE
        {
            public uint width;
            public uint height;
            public DISPLAYCONFIG_PIXELFORMAT pixelFormat;
            public POINTL position;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DISPLAYCONFIG_RATIONAL
        {
            public uint Numerator;
            public uint Denominator;

            public override string ToString() => Numerator + " / " + Denominator;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct DISPLAYCONFIG_2DREGION
        {
            public uint cx;
            public uint cy;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct DISPLAYCONFIG_DESKTOP_IMAGE_INFO
        {
            public POINTL PathSourceSize;
            public RECT DesktopImageRegion;
            public RECT DesktopImageClip;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct DISPLAYCONFIG_VIDEO_SIGNAL_INFO
        {
            public ulong pixelRate;
            public DISPLAYCONFIG_RATIONAL hSyncFreq;
            public DISPLAYCONFIG_RATIONAL vSyncFreq;
            public DISPLAYCONFIG_2DREGION activeSize;
            public DISPLAYCONFIG_2DREGION totalSize;
            public uint videoStandard;
            public DISPLAYCONFIG_SCANLINE_ORDERING scanLineOrdering;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct DISPLAYCONFIG_TARGET_MODE
        {
            public DISPLAYCONFIG_VIDEO_SIGNAL_INFO targetVideoSignalInfo;
        }

        [StructLayout(LayoutKind.Explicit)]
        struct DISPLAYCONFIG_MODE_INFO_union
        {
            [FieldOffset(0)]
            public DISPLAYCONFIG_TARGET_MODE targetMode;

            [FieldOffset(0)]
            public DISPLAYCONFIG_SOURCE_MODE sourceMode;

            [FieldOffset(0)]
            public DISPLAYCONFIG_DESKTOP_IMAGE_INFO desktopImageInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DISPLAYCONFIG_PATH_SOURCE_INFO
        {
            public LUID adapterId;
            public uint id;
            public uint modeInfoIdx;
            public DISPLAYCONFIG_SOURCE_FLAGS statusFlags;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DISPLAYCONFIG_PATH_TARGET_INFO
        {
            public LUID adapterId;
            public uint id;
            public uint modeInfoIdx;
            public DISPLAYCONFIG_VIDEO_OUTPUT_TECHNOLOGY outputTechnology;
            public DISPLAYCONFIG_ROTATION rotation;
            public DISPLAYCONFIG_SCALING scaling;
            public DISPLAYCONFIG_RATIONAL refreshRate;
            public DISPLAYCONFIG_SCANLINE_ORDERING scanLineOrdering;
            public bool targetAvailable;
            public DISPLAYCONFIG_TARGET_FLAGS statusFlags;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DISPLAYCONFIG_PATH_INFO
        {
            public DISPLAYCONFIG_PATH_SOURCE_INFO sourceInfo;
            public DISPLAYCONFIG_PATH_TARGET_INFO targetInfo;
            public DISPLAYCONFIG_PATH flags;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct DISPLAYCONFIG_MODE_INFO
        {
            public DISPLAYCONFIG_MODE_INFO_TYPE infoType;
            public uint id;
            public LUID adapterId;
            public DISPLAYCONFIG_MODE_INFO_union info;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        struct DISPLAYCONFIG_SOURCE_DEVICE_NAME
        {
            public DISPLAYCONFIG_DEVICE_INFO_HEADER header;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string viewGdiDeviceName;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        struct DISPLAYCONFIG_TARGET_DEVICE_NAME_FLAGS
        {
            public uint value;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        struct DISPLAYCONFIG_TARGET_DEVICE_NAME
        {
            public DISPLAYCONFIG_DEVICE_INFO_HEADER header;
            public DISPLAYCONFIG_TARGET_DEVICE_NAME_FLAGS flags;
            public DISPLAYCONFIG_VIDEO_OUTPUT_TECHNOLOGY outputTechnology;
            public ushort edidManufactureId;
            public ushort edidProductCodeId;
            public uint connectorInstance;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
            public string monitorFriendlyDeviceName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string monitorDevicePat;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [DllImport("user32")]
        static extern int GetDisplayConfigBufferSizes(QDC flags, out int numPathArrayElements, out int numModeInfoArrayElements);

        [DllImport("user32")]
        static extern int QueryDisplayConfig(QDC flags, ref int numPathArrayElements, [In, Out] DISPLAYCONFIG_PATH_INFO[] pathArray, ref int numModeInfoArrayElements, [In, Out] DISPLAYCONFIG_MODE_INFO[] modeInfoArray, out DISPLAYCONFIG_TOPOLOGY_ID currentTopologyId);

        [DllImport("user32")]
        static extern int QueryDisplayConfig(QDC flags, ref int numPathArrayElements, [In, Out] DISPLAYCONFIG_PATH_INFO[] pathArray, ref int numModeInfoArrayElements, [In, Out] DISPLAYCONFIG_MODE_INFO[] modeInfoArray, IntPtr currentTopologyId);

        [DllImport("user32")]
        static extern int DisplayConfigGetDeviceInfo(ref DISPLAYCONFIG_GET_ADVANCED_COLOR_INFO requestPacket);

        /*test  */
        [DllImport("user32")]
        static extern int DisplayConfigGetDeviceInfo(ref DISPLAYCONFIG_SDR_WHITE_LEVEL requestPacket);

        [DllImport("user32")]
        static extern int DisplayConfigGetDeviceInfo(ref DISPLAYCONFIG_SOURCE_DEVICE_NAME requestPacket);

        [DllImport("user32")]
        static extern int DisplayConfigGetDeviceInfo(ref DISPLAYCONFIG_TARGET_DEVICE_NAME requestPacket);

    }

}
