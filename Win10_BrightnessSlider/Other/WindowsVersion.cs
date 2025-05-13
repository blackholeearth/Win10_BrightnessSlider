using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace Win10_BrightnessSlider
{

    /// <summary>
    /// Static class that adds convenient methods for getting information on the running computers basic hardware and os setup.
    /// </summary>
    public static class WindowsVersion
    {

        public static bool IsWindows11()
        {
            var reg = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion",false);

            var currentBuildStr = (string)reg.GetValue("CurrentBuild");
            var currentBuild = int.Parse(currentBuildStr);

            return currentBuild >= 22000;
        }



        /// <summary>
        ///     Returns the Windows major version number for this computer.
        /// </summary>
        public static int WinMajorVersion
        {
            get
            {
                dynamic major;
                // The 'CurrentMajorVersionNumber' string value in the CurrentVersion key is new for Windows 10, 
                // and will most likely (hopefully) be there for some time before MS decides to change this - again...
                if (TryGetRegistryKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", "CurrentMajorVersionNumber", out major))
                {
                    return (int)(uint)major;
                }

                // When the 'CurrentMajorVersionNumber' value is not present we fallback to reading the previous key used for this: 'CurrentVersion'
                dynamic version;
                if (!TryGetRegistryKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", "CurrentVersion", out version))
                    return 0;

                var versionParts = ((string)version).Split('.');
                if (versionParts.Length != 2) return 0;
                uint majorAsUInt;
                var result = uint.TryParse(versionParts[0], out majorAsUInt) ? majorAsUInt : 0;

                return (int)result;
            }
        }

        /// <summary>
        ///     Returns the Windows minor version number for this computer.
        /// </summary>
        public static int WinMinorVersion
        {
            get
            {
                dynamic minor;
                // The 'CurrentMinorVersionNumber' string value in the CurrentVersion key is new for Windows 10, 
                // and will most likely (hopefully) be there for some time before MS decides to change this - again...
                if (TryGetRegistryKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", "CurrentMinorVersionNumber",
                    out minor))
                {
                    return (int)(uint)minor;
                }

                // When the 'CurrentMinorVersionNumber' value is not present we fallback to reading the previous key used for this: 'CurrentVersion'
                dynamic version;
                if (!TryGetRegistryKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", "CurrentVersion", out version))
                    return 0;

                var versionParts = ((string)version).Split('.');
                if (versionParts.Length != 2) return 0;
                uint minorAsUInt;
                return (int)(uint.TryParse(versionParts[1], out minorAsUInt) ? minorAsUInt : 0);
            }
        }

        /// <summary>
        ///     Returns whether or not the current computer is a server or not.
        /// </summary>
        public static bool IsServer
        {
            get
            {
                dynamic installationType;
                if (TryGetRegistryKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", "InstallationType",
                    out installationType))
                {
                    return (installationType.Equals("Client") ? false : true);
                }

                return false;
            }
        }

        private static bool TryGetRegistryKey(string path, string key, out dynamic value)
        {
            value = null;
            try
            {
                using (var rk = Registry.LocalMachine.OpenSubKey(path))
                {
                    if (rk == null) return false;
                    value = rk.GetValue(key);
                    return value != null;
                }
            }
            catch
            {
                return false;
            }
        }



    }
}
