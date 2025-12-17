using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using static Win10_BrightnessSlider.User32_MonFn;

namespace Win10_BrightnessSlider
{
    public class BrightnessInfo
    {
        public int Brightness = -1;
        public string wmi_InstanceName  ;
        public string user32dc_DevicePath  ;
    }
    public static class RicInfoScreenHolder 
    {
        public static List<BrightnessInfo> BriInfoLi  = new List<BrightnessInfo>();

        /// <summary>
        ///  RemeberBrightness  by MonitorId
        /// </summary>
        /// <param name="ris"></param>
        /// <param name="briValue"></param>
        public static void RememberBrightness(RichInfoScreen ris , int briValue) 
        {
            try
            {
                var biFound_dc = BriInfoLi.FirstOrDefault(
                    x => x.user32dc_DevicePath== ris.dc_TargetDeviceName?.monitorDevicePath);
                if (biFound_dc != null) 
                {
                    biFound_dc.Brightness = briValue;
                    return;
                }

                var biFound_wmi = BriInfoLi.FirstOrDefault(
                   x => x.wmi_InstanceName == ris.WMIMonitorID.InstanceName);
                if (biFound_wmi != null)
                {
                    biFound_wmi.Brightness = briValue;
                    return;
                }

                //both null
                BriInfoLi.Add(new BrightnessInfo() {
                    Brightness = briValue,
                    user32dc_DevicePath = ris.dc_TargetDeviceName?.monitorDevicePath,
                    wmi_InstanceName = ris.WMIMonitorID.InstanceName
                });

            }
            catch (Exception ex)
            {
                RamLogger.Log(ex+"");
            }
        }


    }


    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class RichInfoScreen
    {
        [JsonProperty]
        public System.Windows.Forms.Screen Screen;
        [JsonProperty]
        public DISPLAYCONFIG_TARGET_DEVICE_NAME? dc_TargetDeviceName;
        /// <summary>
        /// maybe  one screen  is displayed on multiple physicalscreen half split to mon1 half split to mon2??
        /// generally this array will have 1 item
        /// </summary>
        [JsonProperty]
        public DxvaMonFn.PHYSICAL_MONITOR[] PhysicalMonitors;
        [JsonProperty]
        public wmiMonFn.WMIMonitorID2 WMIMonitorID;

        /// <summary>
        /// returns the 1st one
        /// </summary>
        public DxvaMonFn.PHYSICAL_MONITOR? PhysicalMonitor { get { return PhysicalMonitors?.FirstOrDefault(); } }


        string _monitorName;
        /// <summary>
        /// user prefence, 
        /// subscribe  onMonitorNameChanged to update - monitorName
        /// ---this trigger json Cannot Serialize Pen FrameCap  Error.
        /// </summary>
        public string MonitorName
        {
            get => _monitorName;
            set
            {
                _monitorName = value;
                onMonitorNameChanged?.Invoke(_monitorName);
            }
        }
        /// <summary>
        /// string monitorName
        /// </summary>
        public Action<string> onMonitorNameChanged;

        public string TooltipText { get; set; } = ".";
        public string avail_MonitorName
        {
            get
            {
                try
                {
                    if (WMIMonitorID?.InstanceName != null)
                    {
                        if (!string.IsNullOrWhiteSpace(WMIMonitorID.UserFriendlyName))
                            return "wmi." + WMIMonitorID.UserFriendlyName;
                        else
                            return "wmi." + WMIMonitorID.ManufacturerName + "-" + WMIMonitorID.ProductCodeID;
                    }
                    else if (dc_TargetDeviceName != null)
                    {
                        var tdev = ((User32_MonFn.DISPLAYCONFIG_TARGET_DEVICE_NAME)dc_TargetDeviceName);
                        return "user32_dc." + tdev.edidManufactureId + "-" + tdev.edidProductCodeId + "";
                    }
                    else if (PhysicalMonitor?.hPhysicalMonitor != IntPtr.Zero)
                    {
                        return "dxva-ddci." + PhysicalMonitor?.hPhysicalMonitor + "";
                    }
                    else
                    {
                        return "noName";
                    }
                }
                catch (Exception ex)
                {
                    RamLogger.Log(ex+"");
                    return "Ex:" + ex;
                }
              
            }

            set { avail_MonitorName = value; }
               
        }
        public string avail_MonitorName_clean
        {
            get
            {
                try
                {
                    if (WMIMonitorID?.InstanceName != null)
                    {
                        if (!string.IsNullOrWhiteSpace(WMIMonitorID.UserFriendlyName))
                            return "" + WMIMonitorID.UserFriendlyName;
                        else
                            return "" + WMIMonitorID.ManufacturerName + "-" + WMIMonitorID.ProductCodeID;
                    }
                    else if (dc_TargetDeviceName != null)
                    {
                        var tdev = ((User32_MonFn.DISPLAYCONFIG_TARGET_DEVICE_NAME)dc_TargetDeviceName);
                        return "" + tdev.edidManufactureId + "-" + tdev.edidProductCodeId + "";
                    }
                    else if (PhysicalMonitor?.hPhysicalMonitor != IntPtr.Zero)
                    {
                        return "" + PhysicalMonitor?.hPhysicalMonitor + "";
                    }
                    else
                    {
                        return "noName";
                    }
                }
                catch (Exception ex)
                {
                    RamLogger.Log(ex + "");
                    return "Ex:" + ex;
                }

            }

            set { avail_MonitorName = value; }

        }



		/// <summary>
		///  value : 0 -100 ,  if fails return -1
		/// </summary>
		/// <param name="value">0-100</param>
		/// <param name="_isMouseDown">dxva is  only allowed to run if  MouseIsReleased -( aka  msDown:False) </param>
		/// <returns></returns>
		public int SetBrightness(int value, bool _isMouseDown)
        {
            byte val_byte = (byte)value;

            bool IsWmiSucceed = false;
            bool isDxvaSucceed = false;
            if (WMIMonitorID != null)
            {
                IsWmiSucceed = wmiMonFn.SetBrightness(val_byte, WMIMonitorID.InstanceName);
            }

            if (!IsWmiSucceed & !_isMouseDown && PhysicalMonitor?.hPhysicalMonitor != IntPtr.Zero)
            {
                isDxvaSucceed = DxvaMonFn.SetPhysicalMonitorBrightness(PhysicalMonitor.Value, val_byte);
                if (isDxvaSucceed)
                    DxvaMonFn.SaveCurrentMonitorSettings(PhysicalMonitor.Value); //domusW didnt work
            }

            RicInfoScreenHolder.RememberBrightness(this,value) ;
            
            return value;

        }

        /// <summary>
        /// returns  0-100, return -1 if fails
        /// </summary>
        /// <returns></returns>
        public int GetBrightness()
        {
            int curBrigh = -1;

            if (WMIMonitorID?.InstanceName != null)
            {
                curBrigh = wmiMonFn.GetBrightness(WMIMonitorID.InstanceName);
            }
            
            if (PhysicalMonitor?.hPhysicalMonitor != null   && curBrigh == -1 )
            {
                curBrigh = DxvaMonFn.GetPhysicalMonitorBrightness(PhysicalMonitor.Value);
            }

            return curBrigh;
        }



        /// <summary>
        /// populate for later usage. to easyly match infos from different Sources by Winform.Screen
        /// </summary>
        public static List<RichInfoScreen> Get_RichInfo_Screen()
        {
            var RichInfo_Screen = new List<RichInfoScreen>();

            var WMIMonitorIDs = new List<wmiMonFn.WMIMonitorID2>();
            try { WMIMonitorIDs = wmiMonFn.GetWMIMonitorIDs(); }
            catch (Exception ex) { RamLogger.Log("Exception:\r\n" + ex); }

            foreach (var scr in System.Windows.Forms.Screen.AllScreens)
            {
                //  phyMon  by screen   (low Possibility)
                var phyMons = new DxvaMonFn.PHYSICAL_MONITOR[] { };
                try { phyMons = DxvaMonFn.GetPhysicalMonitors(scr); }
                catch (Exception ex) { RamLogger.Log("Exception:\r\n" + ex);  }

                wmiMonFn.WMIMonitorID2 wmiMonid = null;

                //user32mon  by screen  100%
                var dc_tarDevName = User32_MonFn.dc_TargetDeviceName(scr);
                if (dc_tarDevName != null)
                {
                    //  get  wmi  by  phyMon   
                    wmiMonid = GetWmiMonitorID_by_TargetDeviceName(WMIMonitorIDs, 
                        (DISPLAYCONFIG_TARGET_DEVICE_NAME)dc_tarDevName);
                    WMIMonitorIDs.Remove(wmiMonid);
                }

                RichInfo_Screen.Add(
                    new RichInfoScreen()
                    {
                        Screen = scr,
                        dc_TargetDeviceName = dc_tarDevName,
                        PhysicalMonitors = phyMons,
                        WMIMonitorID = wmiMonid,
                    });
            }

            if (WMIMonitorIDs.Count() > 0)
            {
                RamLogger.Log( 
                "[Info] " + "WMIMonitorIDs.Count > 0: " + WMIMonitorIDs + ". Remaining wmis will be added to below. \r\n" +
                "but wmi works better if you have supported device. "
               );

                var remaining_Wmi_asRiScr = 
                    WMIMonitorIDs.Select(remWmi => new RichInfoScreen() { WMIMonitorID = remWmi } ).ToList();
                RichInfo_Screen.InsertRange(0, remaining_Wmi_asRiScr);
            }


            return RichInfo_Screen;

        }

        private static wmiMonFn.WMIMonitorID2 GetWmiMonitorID_by_TargetDeviceName(
            List<wmiMonFn.WMIMonitorID2> WMIMonitorIDs,
            DISPLAYCONFIG_TARGET_DEVICE_NAME dc_tarDevName)
        {
            string dc_tarDevName_monDevPath_asInstace = "none";

            //convert path to instance name;
            var indx = dc_tarDevName.monitorDevicePath.IndexOf(@"DISPLAY");
            var indx2 = dc_tarDevName.monitorDevicePath.IndexOf(@"#{");

            if (indx <0)  // substr DISPLAY  not found in  monitorDevicePath ;
                return null;

            var tmpins = dc_tarDevName.monitorDevicePath.Substring(indx, indx2 - indx).Replace("#", @"\");
            dc_tarDevName_monDevPath_asInstace = tmpins;

            var wmiMonid = WMIMonitorIDs.FirstOrDefault(x => x.InstanceName.StartsWith(dc_tarDevName_monDevPath_asInstace));
            return wmiMonid;
        }



        /// <summary>
        /// _deprecated MatchingProblems
        /// </summary>
        /// <returns></returns>
        public static List<RichInfoScreen> GetMonitors()
        {
            var monList = new List<RichInfoScreen>();

            var dxvaPhyMons = new DxvaMonFn.PHYSICAL_MONITOR[]{};
            var wmiMonIDs = new List<wmiMonFn.WMIMonitorID2>();

            bool wmiFail = false;
            bool dxvaFail = false;
            //populate WMI
            try { wmiMonIDs = wmiMonFn.GetWMIMonitorIDs(); }           catch (Exception ex) { RamLogger.Log("WMI:\r\n" + ex);   wmiFail = true;  }
            try { dxvaPhyMons = DxvaMonFn.GetPhysicalMonitors_All_Flattened(); } catch (Exception ex) { RamLogger.Log("dxva2:\r\n" + ex); dxvaFail = true; }

            // both api support and equal
            if (wmiMonIDs.Count() == dxvaPhyMons.Length && wmiMonIDs.Count() > 0)
            {
                for (int i = 0; i < dxvaPhyMons.Length; i++)
                {
                    var monX = new RichInfoScreen();
                    monX.WMIMonitorID = wmiMonIDs[i];
                    monX.PhysicalMonitors = new DxvaMonFn.PHYSICAL_MONITOR[] { dxvaPhyMons[i] };
                    monX.TooltipText = wmiMonIDs[i].InstanceName;
                    monX.avail_MonitorName = "WMI, Fallback to dxva2";
                    monList.Add(monX);
                }
            }
            else
            {
                if (!wmiFail)
                    foreach (wmiMonFn.WMIMonitorID2 monIdX in wmiMonIDs) {
                        var monX = new RichInfoScreen();
                        monX.WMIMonitorID = monIdX;
                        //monX.physicalMonitor = dxvaPhyMons[i];
                        monX.TooltipText = monIdX.InstanceName;
                        monX.avail_MonitorName = "WMI";
                        monList.Add(monX);
                    }


                if (!dxvaFail)
                    for (int i = 0; i < dxvaPhyMons.Length; i++) {
                        var monX = new RichInfoScreen();
                        //monX.WMIMonitorID = monIdX;
                        monX.PhysicalMonitors = new DxvaMonFn.PHYSICAL_MONITOR[] { dxvaPhyMons[i] };
                        monX.TooltipText = dxvaPhyMons[i].hPhysicalMonitor + "  " + dxvaPhyMons[i].szPhysicalMonitorDescription;
                        monX.avail_MonitorName = "dxva2";
                        monList.Add(monX);
                    }
            }
            return  (dxvaFail && wmiFail) ? null : monList;

        }


    }
}
