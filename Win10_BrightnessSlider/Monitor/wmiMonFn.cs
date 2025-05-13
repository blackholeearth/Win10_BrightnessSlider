using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
 
namespace Win10_BrightnessSlider
{
    public static class wmiMonFn
    {

        //wmi methods
        /// <summary>
        /// value  0 ,100
        /// if name is Null , run for all 
        /// </summary>
        /// <returns></returns>
        public static bool SetBrightness(byte value, string wmi_InstanceName)
        {
            bool isSuccess = false;
            try
            {
                ManagementScope scope = new ManagementScope("root\\WMI");
                SelectQuery query = new SelectQuery("WmiMonitorBrightnessMethods");
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query))
                {
                    using (ManagementObjectCollection objectCollection = searcher.Get())
                    {
                        foreach (ManagementObject mObj in objectCollection)
                        {
                            var cur_InstName = mObj.Properties["InstanceName"].Value + "";
                            if (wmi_InstanceName == cur_InstName)
                            {
                                mObj.InvokeMethod("WmiSetBrightness", new Object[] { UInt32.MaxValue, value });
                                isSuccess = true;
                                break;
                            }

                        }
                    }
                }

                return isSuccess;
            }
            catch (Exception ex)
            {
                RamLogger.Log(ex + "");
                
                return false;
            }

        }
        public static int GetBrightness(string wmi_InstanceName)
        {
            try
            {
                ManagementScope scope = new ManagementScope("root\\WMI");
                SelectQuery query = new SelectQuery("WmiMonitorBrightness");
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query))
                {
                    using (ManagementObjectCollection objectCollection = searcher.Get())
                    {
                        foreach (ManagementObject mObj in objectCollection)
                        {
                            var cur_InstName = mObj.Properties["InstanceName"].Value + "";
                            if (wmi_InstanceName == cur_InstName)
                            {
                                var br_obj = mObj.Properties["CurrentBrightness"].Value;

                                double br = Convert.ToDouble( br_obj );
                                return (int)br;
                                //break;

                            }

                        }
                    }
                }

                return -1;
            }
            catch (Exception ex)
            {
                RamLogger.Log(ex+"");
                return -1;
            }
           
        }


        /// <summary>
        /// if null ,"" 
        /// </summary>
        /// <param name="arr"></param>
        /// <returns></returns>
        public static string ToCharArray(ushort[] arr)
        {
            if (arr == null)
                return "";

            var output = new char[arr.Length];
            for (int i = 0; i < arr.Length; ++i)
            {
                output[i] = (char)arr[i];
            }
            return new string(output).TrimEnd('\0');
        }

        public class WMIMonitorID2
        {
            public string InstanceName;
            public string ManufacturerName;
            public string ProductCodeID;
            public string SerialNumberID;
            public string UserFriendlyName;
            public string YearOfManufacture;
        }
        public static List<WMIMonitorID2> GetWMIMonitorIDs()
        {
            var objList = new List<WMIMonitorID2>();

            ManagementScope scope = new ManagementScope("root\\WMI");
            SelectQuery query = new SelectQuery("WMIMonitorID");
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query))
            {
                using (ManagementObjectCollection objectCollection = searcher.Get())
                {
                    foreach (ManagementObject mObj in objectCollection)
                    {
                        var MonObj1 = new WMIMonitorID2();

                        MonObj1.InstanceName = mObj.Properties["InstanceName"].Value + "";
                        MonObj1.ManufacturerName = ToCharArray((ushort[])mObj.Properties["ManufacturerName"].Value);
                        MonObj1.ProductCodeID = ToCharArray((ushort[])mObj.Properties["ProductCodeID"].Value);
                        MonObj1.SerialNumberID = ToCharArray((ushort[])mObj.Properties["SerialNumberID"].Value);
                        MonObj1.UserFriendlyName = ToCharArray((ushort[])mObj.Properties["UserFriendlyName"].Value);
                        MonObj1.YearOfManufacture = mObj.Properties["YearOfManufacture"].Value + "";

                        objList.Add(MonObj1);

                    }
                }
            }

            return objList;
        }

    }


  

}
