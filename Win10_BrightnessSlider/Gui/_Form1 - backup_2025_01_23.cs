using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Gma.System.MouseKeyHook;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using Win10_BrightnessSlider.Properties;
using static Win10_BrightnessSlider.NativeMethods;


namespace Win10_BrightnessSlider
{
    public partial class Form1 : Form
    {
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x80;  //toolwindow: hides window from alt+tab menu  //override form as toolwindow
                return cp;
            }
        }

        public Form1()
        {
            InitializeComponent();
            DoubleBuffered = true;

            this.FormBorderStyle = FormBorderStyle.None;

            this.WindowState = FormWindowState.Normal;
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(-this.Width, -this.Height);

            //Add_Wifi_Icon();
            //Remove_Wifi_Icon();

            //var st = Settings_litedb.GetSettings();
            var st = Settings_json.Get();
            //st.customTheme = new CustomTheme();
            //st.SaveTo_JsonFile();
            
            //st.SaveTo_TomlFile();


            SetColors_themeAware();
            SetColors_CustomTheme_ifEnabled();


            //if(st.Style != Style.FollowSystem)
            //    isRoundCorners = st.Style == Style.win11; //has to be called first
            //these are set at Create ContexMenu
            //notifyIcon_wifi.Visible = st.WifiIcon;
            //GloabalKeyHookFn.copilotKey_toggle(st.MapCopilotKey);
        }


        /*
          
        v1.8.20
        * rightClick menu is ContextMenuStrip - Themed like Win11
           * but no RoundedBorder.   
         
        */
        static string version = "1.8.20"; //.20

        /// <summary>
        /// is win11
        /// </summary>
        public static bool isRoundCorners = WindowsVersion.IsWindows11();
        //cant use common, evey slider need distinctive one.
        //Bitmap slider_Image_dark => ImageUtils.BytesToBitmap(Resources.brightness_gray);
        Bitmap slider_Image_dark => Resources.sunny_black;

        public ContextMenuStrip_win11 notifyIcon_bright_ContextMenuStrip { get; private set; }
        public ContextMenuStrip_win11 notifyIcon_wifi_ContextMenuStrip { get; private set; }

        Bitmap slider_Image_current() 
        {
            var st = Settings_json.Get();

            //Custom Theme DONOT Touch TaskBAR icon -tray icon
            var img = Resources.sunny_black;
            if (st.customTheme.iconsColor_isLight)
                img = Resources.sunny_white;

            return img;
        }
        Color BorderColor = Color.FromArgb(63, 63, 63);
        Color BackColor1 = Color.FromArgb(31, 31, 31);
        Color TextColor1 = Color.White;
        //customcolor
        Settings settings_forTheme;
        private void SetColors_themeAware()
        {
            //Colors
            this.BackColor = BackColor1;
            notifyIcon_bright.Icon = Resources.sunny_white.BitmapToIcon();

            BorderColor = Color.FromArgb(63, 63, 63);
            BackColor1 = Color.FromArgb(31, 31, 31);
            TextColor1 = Color.White;

            bool isLight = TaskBarUtil.isTaskbarColor_Light();
            if (isLight)
            {
                BorderColor = Color.FromArgb(190, 190, 190);
                BackColor1 = Color.FromArgb(242, 242, 242); //Color.White;  
                TextColor1 = Color.FromArgb(31, 31, 31);


                //notifyIcon_bright.Icon = ImageUtils.BytesToIcon(Resources.bright_gray);
                notifyIcon_bright.Icon = Resources.sunny_black.BitmapToIcon();
            }
        }
        private void SetColors_CustomTheme_ifEnabled()
        {
            var st = Settings_json.Get();
            settings_forTheme = st;

            if (!st.CustomTheme_Enabled)
                return;


            BorderColor = st.customTheme.borderColor;
            BackColor1 = st.customTheme.backColor;
            TextColor1 = st.customTheme.textColor;
            //Colors
            this.BackColor = BackColor1;

            //Custom Theme DONOT Touch TaskBAR icon -tray icon

        }



        bool vis = false;

        ToolStripMenuItem mi_update;
        ToolStripMenuItem mi_wifiToggle;
        bool updateChecked = false;

        private IKeyboardMouseEvents m_GlobalHook;
        private async void Form1_Load(object sender, EventArgs e)
        {
            notifyIcon_bright.Text = "setting up events...";

            //form show hide event
            notifyIcon_bright.MouseClick += NotifyIcon_Bright_MouseClick;
            //clicked outside of form
            Deactivate += Form1_Deactivate;

            //clicked outside windows--  deactivate doesnt work every time.
            // Note: for the application hook, use the Hook.AppEvents() instead
            m_GlobalHook = Hook.GlobalEvents();
            m_GlobalHook.MouseDownExt -= GlobalHookMouseDownExt;
            m_GlobalHook.MouseDownExt += GlobalHookMouseDownExt;
            m_GlobalHook.MouseWheelExt -= M_GlobalHook_MouseWheelExt;
            m_GlobalHook.MouseWheelExt += M_GlobalHook_MouseWheelExt;
            m_GlobalHook.KeyPress -= M_GlobalHook_KeyPress;
            m_GlobalHook.KeyPress += M_GlobalHook_KeyPress;

            //m_GlobalHook.init_remapKeys(true);


            //  -monitor plug unplug
            DeviceNotification.Instance.On_WmDevicechange -= On_WmDevicechange;
            DeviceNotification.Instance.On_WmDevicechange += On_WmDevicechange;
            // monitor -on off
            PowerBroadcastSetting.Instance.On_PowerSettingsChange -= On_PowerSettingsChange;
            PowerBroadcastSetting.Instance.On_PowerSettingsChange += On_PowerSettingsChange;
            //resume suspend hibernate 
            SystemEvents.PowerModeChanged -= SystemEvents_PowerModeChanged;
            SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;

            //theme changed
            SystemEvents.UserPreferenceChanged -= SystemEvents_UserPreferenceChanged;
            SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;


            eSetVis(false);

            notifyIcon_bright.Text = "querying monitors...";
            RePopulateSliders();
            //await on_event_RepopulateSlider_inTaskRun("formload",0);
            //CreateNotifyIcon_ContexMenu();
            CreateNotifyIcon_ContexMenuStrip();
            SetColors_themeAware_RightClickMenus();

            GUI_Update_StatesOnControls();

            //calling this again to fix issue i have to manualy detect monitor
            riScreens = RePopulateSliders();
            //await on_event_RepopulateSlider_inTaskRun("formload", 0);
            notifyIcon_bright.Text = "Win10_BrightnessSlider";

            Dump_Monitor_info_toJsonFile();
            Output_monitor_ids_toJson__AndShowNames_OnGui();
        }

        private static void Dump_Monitor_info_toJsonFile()
        {
            try
            {
                //dump monitors
                var md = new MonitorsDump() { riScreens = riScreens };
                md.SaveTo_JsonFile();
            }
            catch (Exception ex)
            {
                File.WriteAllText(MonitorsDump_json.settingFilePath.Replace(".json", ".error.txt"), ex + "");
            }
        }
        private static void Output_monitor_ids_toJson__AndShowNames_OnGui()
        {
            //output monitor ids to settings.json
            var set = Settings_json.Get();
            if (set.monitorNames is null)
                set.monitorNames = new List<MonitorNames>();

            foreach (var riScr in riScreens)
            {
                var curRi_monName = new MonitorNames()
                {
                    MonitorName = "",
                    dc_monitorDevicePath = riScr.dc_TargetDeviceName?.monitorDevicePath ?? null,
                    wmi_InstanceName = riScr.WMIMonitorID?.InstanceName ?? null,
                };

                //-------- by wmi
                var foundMon = set.monitorNames.FirstOrDefault(x => x.wmi_InstanceName == curRi_monName.wmi_InstanceName);
                if (string.IsNullOrWhiteSpace(curRi_monName.wmi_InstanceName))
                    foundMon = null;

                if (foundMon != null)
                {
                    if (!string.IsNullOrWhiteSpace(foundMon.MonitorName))
                        riScr.MonitorName = foundMon.MonitorName;

                    continue;
                }

                //-------- by dc
                var foundMondc = set.monitorNames.FirstOrDefault(x => x.dc_monitorDevicePath == curRi_monName.dc_monitorDevicePath);
                if (string.IsNullOrWhiteSpace(curRi_monName.dc_monitorDevicePath))
                    foundMondc = null;

                if (foundMondc != null)
                {
                    if (!string.IsNullOrWhiteSpace(foundMondc.MonitorName))
                        riScr.MonitorName = foundMondc.MonitorName;

                    continue;
                }

                //both not found -- add to json
                set.monitorNames.Add(curRi_monName);
            }

            set.SaveTo_JsonFile();
        }


        private void M_GlobalHook_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!Visible)
                return;

            bool keysToClose2 = e.KeyChar == (char)Keys.Escape;
            if (keysToClose2)
            {
                eSetVis(false);
                RamLogger.Log("keypress ESC pressed");
                e.Handled = true;
            };
        }

        public static List<RichInfoScreen> riScreens;
        /// <summary>
        /// scroll to brightnessUp/Down, when mouse over TrayIcon
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void M_GlobalHook_MouseWheelExt(object sender, MouseEventExtArgs e)
        {
            if (!e.WheelScrolled)
                return;

            var notifyicon_Rect = NotifyIconHelpers.GetNotifyIconRectangle(notifyIcon_bright, true);
            var Is_mouseInside_NotifRect = notifyicon_Rect.Contains(e.Location);

            //RamLogger.Log($"M_GlobalHook_MouseWheelEXT - notifyicon_Rect: {notifyicon_Rect} , mouseLoc: {notifyicon_Rect}");

            if (!Is_mouseInside_NotifRect)
                return;

            RamLogger.Log($"M_GlobalHook_MouseWheelEXT - mouseInside_NotifRect  ");

            ////requires ui to be visible, otherwise error.
            //var ucSlderLi = getUCSliderLi(); //var slider1 = ucSlderLi.FirstOrDefault();

            var riScreen1 = riScreens.FirstOrDefault();
            if (riScreen1 is null)
                return;

            //dont freeze Global Key Hook - release it immediately by doing LongRunningProcess on Seperate Thread. (Task.Run)
            Task.Run(() =>
            {
                this.Invoke((Action)delegate
                {
                    var val = riScreen1.GetBrightness();

                    if (e.Delta > 0)
                    {
                        var newval = MathFn.Clamp(val + 5, 0, 100);
                        var ret = riScreen1.SetBrightness(newval, true);
                    }
                    else if (e.Delta < 0)
                    {
                        var newval = MathFn.Clamp(val - 5, 0, 100);
                        var ret = riScreen1.SetBrightness(newval, true);
                    }

                    if (Visible)
                    {
                        GUI_Update_StatesOnControls();
                    }
                    else
                    {
                        Update_NotifyIconText_viaRiScreen();
                    }
                });
            });


        }




        /// <summary>
        /// persist between event suspend resume monitor on off
        /// </summary>
        bool persist_LastBrighnessValue = true;

        public Task on_event_RepopulateSlider_inTaskRun(string exNote, int DelayMs = 1000)
        {
            return Task.Run(() =>
            {
                Thread.Sleep(DelayMs);

                try
                {
                    var riScreens = RePopulateSliders();
                    if (persist_LastBrighnessValue)
                        RestoreBrighnessLevel_fromBackup(riScreens);
                }
                catch (Exception ex)
                {
                    RamLogger.Log("Win10_BriSlider -- " + exNote + "\r\n" + ex + "");
                }

                try
                {
                    //update ui
                    this.Invoke((Action)delegate { GUI_Update_AllSliderControls(); });
                }
                catch (Exception ex)
                {
                    RamLogger.Log("Win10_BriSlider -- " + "Invoke -> UpdateAllSliderControls " + "\r\n" + ex + "");
                }


            });
        }
        private static void RestoreBrighnessLevel_fromBackup(List<RichInfoScreen> riScreens)
        {
            foreach (var bi in RicInfoScreenHolder.BriInfoLi)
            {


                var ris_wmi = riScreens.FirstOrDefault(
                    x => bi.wmi_InstanceName == x.WMIMonitorID.InstanceName);
                var ris_u32dc = riScreens.FirstOrDefault(
                   x => bi.user32dc_DevicePath == x.dc_TargetDeviceName?.monitorDevicePath);

                var ris_li = new[] { ris_wmi, ris_u32dc }.Select(x => x != null).Distinct();

                foreach (var risX in ris_li)
                {
                    ris_wmi.SetBrightness(bi.Brightness, false);
                    Thread.Sleep(60);
                }
            }
        }
        private void On_PowerSettingsChange(byte powerState)
        {
            if (powerState == 2) /* 2: dim , no handle change*/
                return;

            on_event_RepopulateSlider_inTaskRun("powerState: " + powerState);
        }
        private void On_WmDevicechange(int DbtDevice_stat)
        {
            //DeviceNotification.DbtDevice_Arrival == DbtDevice_stat;
            on_event_RepopulateSlider_inTaskRun("DbtDevice_stat: " + DbtDevice_stat);
        }
        private void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            if (e.Mode == PowerModes.Resume)
                on_event_RepopulateSlider_inTaskRun("powermode: " + e.Mode);
        }
        /// theme changed
        private void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            Task.Run(() =>
            {
                Thread.Sleep(1500);//taskbar  pick color needs 1,5sec delay to get right theme color.
                if (e.Category == UserPreferenceCategory.General)
                {
                    Lightweight_Restart__Aka_PopulateSliders_ReapplyColors();
                }

                //Application.Restart(); //setTheme();
            });


        }
        private void Lightweight_Restart__Aka_PopulateSliders_ReapplyColors()
        {
            SetColors_themeAware();
            SetColors_ThemeAware_WifiIcon();

            SetColors_CustomTheme_ifEnabled();

            SetColors_themeAware_RightClickMenus();

            this.Invoke((Action)delegate { RePopulateSliders(); });
        }

        private void SetColors_themeAware_RightClickMenus()
        {

            //notifyIcon_bright_ContextMenuStrip.ApplyRoundCorners();
            //notifyIcon_wifi_ContextMenuStrip.ApplyRoundCorners();

            ////ctx menu colors
            //Console.WriteLine("SetColors_themeAware_RightClickMenus- disabled to test dpi thingiy");
            //return;

            //if i use the code below first show location is little bit problematic
            bool isLight = TaskBarUtil.isTaskbarColor_Light();
            this.Invoke((Action)delegate
            {

                if (isLight)
                {
                    notifyIcon_bright_ContextMenuStrip.CMS_set_win11_LightTheme();
                    notifyIcon_wifi_ContextMenuStrip.CMS_set_win11_LightTheme();
                }
                else
                {
                    notifyIcon_bright_ContextMenuStrip.CMS_set_win11_DarkTheme();
                    notifyIcon_wifi_ContextMenuStrip.CMS_set_win11_DarkTheme();
                }
            });
        }


        //public static List<uc_brSlider2> uc_brSlider2_List;

        /// <summary>
        ///   this.invoke controls handled  for task.run
        /// </summary>
        private List<RichInfoScreen> RePopulateSliders()
        {
            GC.Collect();

            // var monli = RichInfoScreen.GetMonitors();
            var riScreens = RichInfoScreen.Get_RichInfo_Screen();

            ////test -- increases ram usage.. 1 -> 15 , 4-> 35  - 1sider-> 7mb on win11 1080p 125dpi
            //riScreens.AddRange(RichInfoScreen.Get_RichInfo_Screen());
            //riScreens.AddRange(RichInfoScreen.Get_RichInfo_Screen());

            //run on uı thread for controls
            this.Invoke((Action)delegate
            {

                //added at 1.7.5
                foreach (Control ctl in fLayPnl1.Controls)
                {
                    ctl.Dispose();
                }
                fLayPnl1.Controls.Clear();


                if (riScreens != null)
                {
                    foreach (var riscrX in riScreens)
                    {
                        //uc_brSlider2 ucSldr = GetSlider2_UC(riscrX);
                        Control ucSldr = newSlider3_UC(riscrX);


                        fLayPnl1.Controls.Add(ucSldr);
                    }
                    //uc_brSlider2_List = getUCSliderLi();
                }
                else
                {
                    MessageBox.Show("SORRY!!\r\n" + "dxva2 and WMI functions Failed" + "\r\n There is Nothing to do");
                }

                FixFormHeight();

                RestartApp_ifRamUsage_isBiggerThan(150);
            });

            return riScreens;
        }

        private Control newSlider2_UC(RichInfoScreen riscrX)
        {
            var ucSldr = new uc_brSlider2()
            {
                Margin = Padding.Empty,
                BackColor = BackColor1,
            };
            ucSldr.label1.ForeColor = TextColor1;
            ucSldr.lbl_Name.ForeColor = TextColor1;
            ucSldr.FrameColor = new Pen(BorderColor, 1);
            if (TaskBarUtil.isTaskbarColor_Light())
                ucSldr.pictureBox1.Image = slider_Image_dark;

            HelperFn.SetTooltip(ucSldr.pictureBox1, riscrX.TooltipText, riscrX.avail_MonitorName);
            ucSldr.lbl_Name.Text = riscrX.avail_MonitorName;
            //ucSldr.lbl_Name.Text = riscrX.TooltipText;
            ucSldr.riScreen = riscrX;
            return ucSldr;
        }
        private Control newSlider3_UC(RichInfoScreen riscrX)
        {
            var ucSldr = new uc_brSlider3()
            {
                Margin = Padding.Empty,
                BackColor = BackColor1,
            };
            //ucSldr.trackBar1 = ColorSliderFn.setStyle_win10_trackbarColor_withElapsedAccented(ucSldr.trackBar1);
            ucSldr.trackBar1 = ColorSliderFn.setStyle_win10_trackbarColor_withElapsedAccented(ucSldr.trackBar1, settings_forTheme);
            ucSldr.trackBar1.BackColor = BackColor1;


            ucSldr.lbl_value.ForeColor = TextColor1;
            ucSldr.lbl_Name.ForeColor = TextColor1;
            ucSldr.FrameColor = new Pen(BorderColor, 1);
            if (TaskBarUtil.isTaskbarColor_Light())
                ucSldr.pictureBox1.Image = slider_Image_dark;

            //settings_forTheme.CustomTheme_Enabled
            if ( Settings_json.Get().CustomTheme_Enabled)
                ucSldr.pictureBox1.Image = slider_Image_current();

            HelperFn.SetTooltip(ucSldr.pictureBox1, riscrX.TooltipText, riscrX.avail_MonitorName);
            ucSldr.lbl_Name.Text = riscrX.avail_MonitorName_clean;

            riscrX.onMonitorNameChanged += name =>  { ucSldr.Set_MonitorName(name); }; 
            //ucSldr.lbl_Name.Text = riscrX.TooltipText;
            ucSldr.riScreen = riscrX;
            return ucSldr;
        }


        void RestartApp_ifRamUsage_isBiggerThan(int maxAllowed_RamUsage = 150)
        {
            // ram fix. to be added on v..19
            try
            {
                var memoryMb = 0.0;
                using (var proc = Process.GetCurrentProcess())
                {
                    // The proc.PrivateMemorySize64 will returns the private memory usage in byte.
                    // Would like to Convert it to Megabyte? divide it by 2^20
                    memoryMb = proc.PrivateMemorySize64 / (1024 * 1024);
                }
                RamLogger.Log($" App's Ram_Usage: {memoryMb} Mb");
                if (memoryMb > maxAllowed_RamUsage)
                {
                    RamLogger.Log($"Restarting App... cond: Ram_Usage > {maxAllowed_RamUsage} Mb ");
                    Application.Restart();
                }

            }
            catch (Exception ex)
            {

                RamLogger.Log("Exception at Get Proces Ram Usage.: " + ex.Message);
            }

        }


        //slider interface, 2 and 3
        private List<Iuc_brSlider> getUCSliderList()
        {
            return fLayPnl1.Controls.OfType<Iuc_brSlider>().ToList();
        }
        public void FixFormHeight()
        {
            var ucSlderLi = getUCSliderList();

            try
            {
                this.Height = ucSlderLi.Count() * ucSlderLi[0].Height;
            }
            catch (Exception ex)
            {
                this.Height = 100;
            }
        }
        public void GUI_Update_StatesOnControls()
        {
            //get current states
            var isRunSttup = HelperFn.isRunAtStartup();

            notifyIcon_bright_ContextMenuStrip.Items
                .OfType<ToolStripMenuItem>().Where(x => x.Text .StartsWith("Run At Startup")).FirstOrDefault()
                .Checked = isRunSttup;

            //notifyIcon_bright.ContextMenuStrip.Items
            //    .OfType<ToolStripMenuItem>().Where(x => x.Text == "Run At Startup").FirstOrDefault()
            //    .Checked = isRunSttup;

            GUI_Update_AllSliderControls();
        }
        public void GUI_Update_AllSliderControls()
        {
            var ucSlderLi = getUCSliderList();
            foreach (Iuc_brSlider sld in ucSlderLi)
                sld.UpdateSliderControl();

            FixFormHeight();
        }


        //call this when slider modified
        public void GUI_Update_NotifyIconText()
        {
            var ucSlderLi = getUCSliderList();

            var sb = new StringBuilder();
            foreach (Iuc_brSlider sld in ucSlderLi)
                sb.AppendLine(sld.NotifyIconText);

            //remove last-lines \r\n
            try { sb.Remove(sb.Length - 2, 2); }  catch (Exception) { }

            //sb.AppendLine("");
            //sb.AppendLine("W10_BriSlider"); //max64CHar

            var str1 = sb.ToString();
            notifyIcon_bright.Text = str1.Substring(0, Math.Min(str1.Length, 63));
        }
        public void Update_NotifyIconText_viaRiScreen()
        {
            var sb = new StringBuilder();
            foreach (var riScreen in riScreens)
                sb.AppendLine(" " + riScreen.avail_MonitorName + " : " + riScreen.GetBrightness() + "%");

            //remove last-lines \r\n
            try { sb.Remove(sb.Length - 2, 2); } catch (Exception) { }

            //sb.AppendLine("");
            //sb.AppendLine("W10_BriSlider"); //max64CHar

            var str1 = sb.ToString();
            notifyIcon_bright.Text = str1.Substring(0, Math.Min(str1.Length, 63));
        }


        /// <summary>
        /// form show hide related funcs
        /// </summary>
        public void eSetVis(bool visible)
        {
            //Console.WriteLine("eSetVis - vis:" + vis);
            RamLogger.Log($"eSetVis(visible:{visible})  , vis:" + vis);
            this.WindowState = FormWindowState.Normal;
            this.StartPosition = FormStartPosition.Manual;

            var pvis = TaskBarLocationFn.calc_FormLoc_NearToTaskbarDate(this.Size);
            //pvis = TaskBarLocationFn.ofset_FormLocation_p2_for_Win11_RoundBorder(pvis ,this.Size, win11_roundCorners_Enabled,this);
            pvis =
               TaskBarLocationFn.offset_FormLoc_for_RoundCorners(new Rectangle(pvis, this.Size), isRoundCorners)
               .Location;

            if (!TaskBarLocationFn.IsTaskbarVisible())
            {
                pvis = TaskBarLocationFn.calc_FormLoc_NearToTaskbarDate_viaMouseLocation(this.Size);
                pvis = TaskBarLocationFn.offset_FormLoc_for_RoundCorners_CalculateAutoHide(new Rectangle(pvis, this.Size), isRoundCorners)
                    .Location;

            }
            //var phidden = new Point(-this.Width, -this.Height);
           



            if (visible)
            {
                this.Location = pvis;

                this.Region = isRoundCorners ? RoundBorders.GetRegion_ForRoundCorner(this.Size, 16) : null;

                this.TopMost = true;
                this.Show();
                this.BringToFront();
                this.Activate();

                vis = true;
            }
            else
            {
                this.TopMost = false;
                this.Hide();

                vis = false;
            }

        }

        //hides when clicked outside of window.  
        private void GlobalHookMouseDownExt(object sender, MouseEventExtArgs e)
        {
            //dont run , if clicked on tray icon.
            var notifyicon_Rect = NotifyIconHelpers.GetNotifyIconRectangle(notifyIcon_bright, true);
            var Is_mouseInside_NotifRect = notifyicon_Rect.Contains(e.Location);
            if (Is_mouseInside_NotifRect)
                return;

            //RamLogger.Log("GlobalHookMouseDownExt");
            if (!this.Visible)
                return;

            //RamLogger.Log("cond: visible true ");

            if (e.IsMouseButtonDown)
            {
                //var form_ptoSCR = PointToScreen(this.ClientRectangle.Location);
                var form_recttoSCR = RectangleToScreen(this.ClientRectangle);

                RamLogger.Log($"form_clientRect:{this.ClientRectangle}, " +
                    $"form_clientRect_RecttoSCR:{form_recttoSCR}" +
                    $"mouseDow.Loc:{e.Location}"
                    );

                bool clickedinside_theForm = form_recttoSCR.Contains(e.Location);
                if (clickedinside_theForm)
                {
                    return;
                }

                RamLogger.Log("cond: clicked outside of form");
                eSetVis(false);
            }

        }

        DateTime deactivateTime;
        private void Form1_Deactivate(object sender, EventArgs e)
        {
            //dont deactive if mouse clicked on notif icon.
            if (Control.MouseButtons == MouseButtons.Left)//static msDown
            {
                var notifyicon_Rect = NotifyIconHelpers.GetNotifyIconRectangle(notifyIcon_bright, true);
                var msPos = Control.MousePosition;

                var Is_mouseInside_NotifRect = notifyicon_Rect.Contains(msPos);
                if (Is_mouseInside_NotifRect)
                    return;
            }


            deactivateTime = DateTime.Now;
            eSetVis(false);
            RamLogger.Log(" eSetVis(false) ");
        }
        // but if clicked on taskbar icon ,  evt_deactive -> evt_mouseClick  
        // (the chain happens 80 - 136 ms - just to be safe i did 300ms)
        private void NotifyIcon_Bright_MouseClick(object sender, MouseEventArgs e)
        {
            //this sometimes prevent ui closing
            if (DateTime.Now.Subtract(deactivateTime).TotalMilliseconds < 300)
                return;

            RamLogger.Log("NotifyIcon1_MouseClick  v1-older:  Visible + last deactive >300 ");

            notifyIcon_bright.MouseClick -= NotifyIcon_Bright_MouseClick;
            Deactivate -= Form1_Deactivate;

            if (e.Button == MouseButtons.Left)
            {
                //msbutton leftClick aka donw/Up
                GUI_Update_AllSliderControls();
                eSetVis(!vis);

                HelperFn.ColorFormIfActive_inDbgMode(this);
            }


            notifyIcon_bright.MouseClick += NotifyIcon_Bright_MouseClick;
            Deactivate += Form1_Deactivate;
        }
        //debug
        private void Form1_MouseEnter(object sender, EventArgs e)
        {
            eSetVis(true);
            HelperFn.ColorFormIfActive_inDbgMode(this);

            if (Debugger.IsAttached)
            {
                Console.WriteLine("Active Window ProcName:" + HelperFn.GetActiveProcessFileName());
            }

        }


        //other funcs
        private void CheckForUpdate_inTaskRun_Retry()
        {
            Task.Run(() =>
            {
                for (int i = 1; i < 5; i++)
                {
                    Thread.Sleep(1000 * 5);
                    if (!Debugger.IsAttached)
                        Thread.Sleep(1000 * 55 * i);

                    try { updateChecked = CheckForUpdate_ResultAtCtxMenu(); }
                    catch (Exception ex) { RamLogger.Log("@update-check : " + ex); }

                    if (updateChecked)
                        break;
                }

            });
        }
        public bool CheckForUpdate_ResultAtCtxMenu()
        {
            //İstek durduruldu: SSL/TLS güvenli kanalı oluşturulamadı
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;

            var wc = new WebClient();
            wc.Headers.Add("User-Agent", "Win10_BrightnessSlider_cfu_" + Environment.MachineName); //github fixx
            var json_latest = wc.DownloadString("https://api.github.com/repos/blackholeearth/Win10_BrightnessSlider/releases/latest");

            JObject obj = JObject.Parse(json_latest);
            var latest_version = obj["tag_name"].Value<string>().TrimStart('v');
            var latest_html_url = obj["html_url"].Value<string>(); // gives absolute version inside url 1.7.1
            var releases_html_url = @"https://github.com/blackholeearth/Win10_BrightnessSlider/releases";

            var newVersionFound = Version.Parse(latest_version) > Version.Parse(version);
            this.Invoke((Action)delegate
            {
                mi_update.Enabled = true;
                if (newVersionFound)
                {
                    mi_update.Text = $"-- New Version Found: v{latest_version} ";
                    //mi_update.BackColor = mi_update.BackColor.AddValue(0,15,0);
                    mi_update.Font = mi_update.Font.SetBold();
                    mi_update.Click += null;
                    mi_update.Click += (sd, ev) =>
                    {
                        Open_Github_LatestReleasePage();
                        /*MessageTextbox_Show("download Latest From Here:\r\n" + releases_html_url);*/
                        //Clipboard.SetText(releases_html_url);
                    };
                }
                else
                {
                    mi_update.Text = "UpToDate";
                    mi_update.Enabled = false;
                    mi_update.Click += (sd, ev) => { /*MessageBox.Show("Already Up to Date!!!."); */};
                }

            });
            return true; //update done
        }

//        private void CreateNotifyIcon_ContexMenu()
//        {
//            //var settingsX = Settings_litedb.GetSettings();
//            var settingsX = Settings_json.Get();

//            var cm = new ContextMenu();
//            var mi0 = new MenuItem("Exit", (snd, ev) => { Application.Exit(); });
//            var mi0_1 = new MenuItem("Restart", (snd, ev) => { Application.Restart(); });

//            var mi1 = new MenuItem("About Me - (v" + version + ")", (snd, ev) =>
//            {
//                var text =
//@"
//Developer: blackholeearth 

//Official Site: 
//https://github.com/blackholeearth/Win10_BrightnessSlider
//";
//                MessageTextbox_Show(text);
//            });

//            mi_update = new MenuItem("Will Check For Update...(in 60sec)", (snd, ev) => { });
//            mi_update.Enabled = false;
//            CheckForUpdate_inTaskRun_Retry();
//            var mi_log = new MenuItem("Show Logs", (snd, ev) =>
//            {
//                var logstr = RamLogger.ToString();
//                LogTextbox_Show(
//                    logstr, RamLogger.logger.Count + "",
//                    "Refresh, For New Logs", () => RamLogger.ToString()
//                    );
//            });

//            var mi_rescanMon = new MenuItem("Detect Monitors", (snd, ev) => { RePopulateSliders(); });
//            var mi_runAtStartUp = new MenuItem("Run At Startup", (snd, ev) =>
//            {
//                var _mi = snd as MenuItem;
//                _mi.Checked = !_mi.Checked; // toggle
//                HelperFn.SetStartup(_mi.Checked);
//            });


//            //var mi_RoundCorner = new MenuItem("Win11-RoundCorners ");
//            //mi_RoundCorner.Checked = isRoundCorners;          
//            //mi_RoundCorner.Click += (snd, ev) =>
//            //{
//            //    var _mi14 = snd as MenuItem;

//            //    _mi14.Checked = !_mi14.Checked;
//            //    isRoundCorners = _mi14.Checked; // toggle
//            //    GUI_Update_AllSliderControls();
//            //    eSetVis(true);

//            //    Settings_litedb.Update(st => { st.Style = isRoundCorners ? Style.win11 : Style.win10; });
//            //};

//            mi_wifiToggle = new MenuItem("Wifi Icon");
//            Add_Wifi_Icon();
//            //notifyIcon_wifi.Visible = Debugger.IsAttached;
//            //mi_wifiToggle.Checked = Debugger.IsAttached;
//            notifyIcon_wifi.Visible = settingsX.Show_WifiIcon;
//            mi_wifiToggle.Checked = settingsX.Show_WifiIcon;
//            mi_wifiToggle.Click += (snd, ev) =>
//            {
//                var _mix = snd as MenuItem;
//                _mix.Checked = !_mix.Checked;
//                notifyIcon_wifi.Visible = _mix.Checked;
//                //if (_mix.Checked)Show_Wifi_Icon(); else Hide_Wifi_Icon();

//                //Settings_litedb.Update(st => { st.WifiIcon = _mix.Checked; });
//                Settings_json.Update(st => { st.Show_WifiIcon = _mix.Checked; });
//            };

//            var mi_remapKey1 = new MenuItem("ReMap Copilot Key To RightClickMenu");
//            //mi_remapKey.Checked = true;
//            m_GlobalHook.init_remapKeys();
//            //GloabalKeyHookFn.copilotKey_toggle(true);
//            GloabalKeyHookFn.copilotKey_toggle(settingsX.MapCopilotKey);
//            mi_remapKey1.Checked = settingsX.MapCopilotKey;

//            mi_remapKey1.Click += (snd, ev) =>
//            {
//                var _mix = snd as MenuItem;
//                _mix.Checked = !_mix.Checked;
//                GloabalKeyHookFn.copilotKey_toggle(_mix.Checked);

//                //Settings_litedb.Update(st => { st.MapCopilotKey = _mix.Checked; });
//                Settings_json.Update(st => { st.MapCopilotKey = _mix.Checked; });
//            };

//            var mi_customTheme_onOff = new MenuItem("Custom Theme - Enabled");
//            mi_customTheme_onOff.Checked = settingsX.CustomTheme_Enabled;
//            mi_customTheme_onOff.Click += (snd, ev) =>
//            {
//                var _mix = snd as MenuItem;
//                _mix.Checked = !_mix.Checked;

//                Settings_json.Update(st => { st.CustomTheme_Enabled = _mix.Checked; });

//                //Application.Restart();

//                //refresh
//                Lightweight_Restart__Aka_PopulateSliders_ReapplyColors();
//                eSetVis(true);

//            };

//            var mi_customTheme_EditFile = new MenuItem("Edit Setting.json ...");
//            mi_customTheme_EditFile.Click += (snd, ev) =>
//            {
//                var _mix = snd as MenuItem;
//                //Process.Start("explorer.exe",  Path.GetDirectoryName( Settings_json.settingFilePath) );
//                Process.Start("explorer.exe", $"/select,\"{Settings_json.settingFilePath}\"");
//            };


//            var mi_darkMode = new MenuItem("Choose Dark/Light Mode ...", (snd, ev) => { Run_MsSettingUri_DarkLight(); });

//            cm.MenuItems.Add(mi0);
//            cm.MenuItems.Add(mi0_1);
//            cm.MenuItems.Add(mi_darkMode);
//            cm.MenuItems.Add("-");
//            cm.MenuItems.Add(mi1);
//            cm.MenuItems.Add(mi_update);
//            cm.MenuItems.Add(mi_runAtStartUp);
//            cm.MenuItems.Add("-");
//            cm.MenuItems.Add(mi_log);
//            cm.MenuItems.Add(mi_rescanMon);
//            cm.MenuItems.Add("-");
//            //cm.MenuItems.Add(mi_RoundCorner);
//            cm.MenuItems.Add(mi_wifiToggle);
//            cm.MenuItems.Add(mi_remapKey1);
//            cm.MenuItems.Add("-");
//            cm.MenuItems.Add(mi_customTheme_onOff);
//            cm.MenuItems.Add(mi_customTheme_EditFile);


//            ifdebugger_add_extraInfo(cm);

//            cm.ContextMenu_set_win11_DarkStyle(null );

//            notifyIcon_bright.ContextMenu = cm;
//        }

        private void CreateNotifyIcon_ContexMenuStrip()
        {
            //var settingsX = Settings_litedb.GetSettings();
            var settingsX = Settings_json.Get();

            var cms = new ContextMenuStrip_win11();
             
            var mi0 = new ToolStripMenuItem("Exit", null, (snd, ev) => { Application.Exit(); });
            var mi0_1 = new ToolStripMenuItem("Restart", null, (snd, ev) => { Application.Restart(); });

            var mi1 = new ToolStripMenuItem($"About Me - (v{version})", null, (snd, ev) =>
            {
                var text =
@"
Developer: blackholeearth 

Official Site: 
https://github.com/blackholeearth/Win10_BrightnessSlider
";
                MessageTextbox_Show(text);
            });

            mi_update = new ToolStripMenuItem("Will Check For Update...(in 60sec)", null, (snd, ev) => { });
            mi_update.Enabled = false;
            CheckForUpdate_inTaskRun_Retry();
            var mi_log = new ToolStripMenuItem("Show Logs", null, (snd, ev) =>
            {
                var logstr = RamLogger.ToString();
                LogTextbox_Show(
                    logstr, RamLogger.logger.Count + "",
                    "Refresh, For New Logs", () => RamLogger.ToString()
                    );
            });

            var mi_rescanMon = new ToolStripMenuItem("Detect Monitors", null, (snd, ev) => { RePopulateSliders(); });
            var mi_runAtStartUp = new ToolStripMenuItem("Run At Startup", null, (snd, ev) =>
            {
                var _mi = snd as ToolStripMenuItem;
                _mi.Checked = !_mi.Checked; // toggle
                HelperFn.SetStartup(_mi.Checked);
            });



            mi_wifiToggle = new ToolStripMenuItem("Wifi Icon");
            Add_Wifi_Icon();

            notifyIcon_wifi.Visible = settingsX.Show_WifiIcon;
            mi_wifiToggle.Checked = settingsX.Show_WifiIcon;
            mi_wifiToggle.Click += (snd, ev) =>
            {
                var _mix = snd as ToolStripMenuItem;
                _mix.Checked = !_mix.Checked;
                notifyIcon_wifi.Visible = _mix.Checked;

                //Settings_litedb.Update(st => { st.WifiIcon = _mix.Checked; });
                Settings_json.Update(st => { st.Show_WifiIcon = _mix.Checked; });
            };

            var mi_remapKey1 = new ToolStripMenuItem("ReMap Copilot Key To RightClickMenu");

            m_GlobalHook.init_remapKeys();
            //GloabalKeyHookFn.copilotKey_toggle(true);
            GloabalKeyHookFn.copilotKey_toggle(settingsX.MapCopilotKey);
            mi_remapKey1.Checked = settingsX.MapCopilotKey;

            mi_remapKey1.Click += (snd, ev) =>
            {
                var _mix = snd as ToolStripMenuItem;
                _mix.Checked = !_mix.Checked;
                GloabalKeyHookFn.copilotKey_toggle(_mix.Checked);

                //Settings_litedb.Update(st => { st.MapCopilotKey = _mix.Checked; });
                Settings_json.Update(st => { st.MapCopilotKey = _mix.Checked; });
            };

            var mi_customTheme_onOff = new ToolStripMenuItem("Custom Theme - Enabled");
            mi_customTheme_onOff.Checked = settingsX.CustomTheme_Enabled;
            mi_customTheme_onOff.Click += (snd, ev) =>
            {
                var _mix = snd as ToolStripMenuItem;
                _mix.Checked = !_mix.Checked;

                Settings_json.Update(st => { st.CustomTheme_Enabled = _mix.Checked; });

                //Application.Restart();
                Lightweight_Restart__Aka_PopulateSliders_ReapplyColors();
                eSetVis(true);
            };

            var mi_customTheme_EditFile = new ToolStripMenuItem("Edit Settings.json ...");
            mi_customTheme_EditFile.Click += (snd, ev) =>
            {
                var _mix = snd as ToolStripMenuItem;
                //Process.Start("explorer.exe",  Path.GetDirectoryName( Settings_json.settingFilePath) );
                Process.Start("explorer.exe", $"/select,\"{Settings_json.settingFilePath}\"");
            };


            var mi_darkMode = new ToolStripMenuItem("Choose Dark/Light Mode ...", null, (snd, ev) => { Run_MsSettingUri_DarkLight(); });

            cms.Items.Add(mi0);
            cms.Items.Add(mi0_1);
            cms.Items.Add(mi_darkMode);
            cms.Items.Add("-");
            cms.Items.Add(mi1);
            cms.Items.Add(mi_update);
            cms.Items.Add(mi_runAtStartUp);
            cms.Items.Add("-");
            cms.Items.Add(mi_log);
            cms.Items.Add(mi_rescanMon);
            cms.Items.Add("-");
            //cm.Items.Add(mi_RoundCorner);
            cms.Items.Add(mi_wifiToggle);
            cms.Items.Add(mi_remapKey1);
            cms.Items.Add("-");
            cms.Items.Add(mi_customTheme_onOff);
            cms.Items.Add(mi_customTheme_EditFile);


            ifdebugger_add_extraInfo(cms);

            //cms.ContextMenuStrip_set_win11_DarkTheme();

            //----old
            //cms.Opening += (s1, e1) => {
            //    cms.Show_atwin11Style_location(notifyIcon_bright); 
            //};
            //notifyIcon_bright.ContextMenuStrip = cms;

            notifyIcon_bright_ContextMenuStrip = cms;
            notifyIcon_bright.MouseClick += (s1, e1) => 
            {
                if(e1.Button == MouseButtons.Right)
                {
                    //hide Taskbar App icon. 
                    UnsafeNativeMethods.SetForegroundWindow(new HandleRef(cms, cms.Handle));
                    cms.Show_win11Style_atlocation(notifyIcon_bright);
                }

            };

        }



        private void ifdebugger_add_extraInfo(ContextMenuStrip cm)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                var mi2t = new ToolStripMenuItem("State Of Window", null, (snd, ev) =>
                {
                    var msg =
                    "visible:" + this.Visible + "\r\n" +
                    "Focused:" + this.Focused + "\r\n" +
                    "canFocus:" + this.CanFocus + "\r\n";
                    MessageBox.Show(msg);
                });

                var mi4t = new ToolStripMenuItem("All Screens Info", null, (snd, ev) =>
                {
                    var msg = string.Join("--------\r\n", HelperFn.GetAllMonitorInfo().ToArray());
                    var msg2 = string.Join("--------\r\n", HelperFn.GetAllScreensInfo().ToArray());
                    MessageBox.Show(
                        "------------GetAllMonitorInfo----------\r\n\r\n"
                        + msg
                        + "\r\n\r\n------------GetAllScreensInfo----------\r\n\r\n"
                        + msg2);
                });

                //cm.Items.Add(new ToolStripMenuItem("-")); // this creates normal dash
                cm.Items.Add("-");
                cm.Items.Add(mi2t);
                cm.Items.Add(mi4t);

            }
        }

        private void MessageTextbox_Show(string text)
        {
            var f = new Form() { Width = 650, Height = 250, Padding = new Padding(7), StartPosition = FormStartPosition.CenterScreen };
            var tb = new TextBox() { ReadOnly = true, Dock = DockStyle.Fill, Multiline = true, };
            tb.Font = new Font(tb.Font.OriginalFontName, 12f);
            f.Controls.Add(tb);
            f.Show();

            tb.Text = text;
        }
        private void LogTextbox_Show(string text, string title, string btnText, Func<string> btnClickAction)
        {
            var f = new Form() { Width = 632, Height = 400, Padding = new Padding(7), StartPosition = FormStartPosition.CenterScreen };
            f.Text = "Logs: " + title;

            var tb = new TextBox() { ReadOnly = true, Dock = DockStyle.Fill, Multiline = true, };
            tb.ScrollBars = ScrollBars.Both;
            tb.WordWrap = false;
            tb.BackColor = Color.WhiteSmoke;



            var btn = new Button() { Text = "Copy To Clipboard...", Width = 150, Dock = DockStyle.Top, Height = 35 };
            btn.Click += (s1, e1) =>
            {
                if (!string.IsNullOrWhiteSpace(tb.Text))
                    Clipboard.SetText(tb.Text);
            };
            var btn2 = new Button() { Text = btnText, Width = 150, Dock = DockStyle.Top, Height = 35 };
            btn2.Click += (s1, e1) =>
            {
                tb.Text = btnClickAction?.Invoke();
                tb.SelectionStart = tb.Text.Length;
                tb.ScrollToCaret();
            };

            btn.Dock =
            btn2.Dock = DockStyle.Bottom;
            //f.Padding = new Padding(20);
            f.Controls.Add(tb); //dock.fill; tb shoud be added first. or overlap ,
            f.Controls.Add(btn);
            f.Controls.Add(btn2);
            f.Controls.Add(new Label() { Text="  " , Dock = btn2.Dock }); //easy resize grip
            f.Show();

            //needs form to be visible for scrolltoCaret.
            tb.Text = text;
            tb.SelectionStart = tb.Text.Length;
            tb.ScrollToCaret();

        }




        ///-----------------region Show wifi İcon.
        private void Add_Wifi_Icon()
        {
            //ntficon_wifi = new NotifyIcon(); //cretes new everytime
            //add wifi icon
            notifyIcon_wifi.Text = "Win11 WiFi Icon";
            notifyIcon_wifi.Visible = true;
            notifyIcon_wifi.MouseClick += (s1, e1) =>
            {
                if (e1.Button == MouseButtons.Left)
                {
                    Run_MsSettingUri_availableWifis();
                }
            };

            NetworkChange.NetworkAvailabilityChanged -= NetworkChange_NetworkAvailabilityChanged;
            NetworkChange.NetworkAvailabilityChanged += NetworkChange_NetworkAvailabilityChanged;


            var cms = new ContextMenuStrip_win11();

            var mi1 = new ToolStripMenuItem("Hide This", null, (s1, e1) => { Hide_Wifi_Icon(); mi_wifiToggle.Checked = false; });
            cms.Items.Add(mi1);
            //padding for contextmenu hidden behind taskbar on windows 11
            cms.Items.Add(new ToolStripMenuItem("__"));

            //cms.ContextMenuStrip_set_win11_DarkTheme();

            //---old -- to show in CMS at specific location, i use other other.
            //notifyIcon_wifi.ContextMenuStrip = cms;
            //cms.Opening += (s1, e1) => { cms.Show_atwin11Style_location(notifyIcon_wifi); };
            
            //----new 
            notifyIcon_wifi_ContextMenuStrip = cms;
            notifyIcon_wifi.MouseClick += (s1, e1) =>
            {
                if (e1.Button == MouseButtons.Right)
                {
                    //hide Taskbar App icon. 
                    cms.Show_win11Style_atlocation(notifyIcon_wifi);
                }

            };


            SetColors_ThemeAware_WifiIcon();

            Task.Run(() => {
                Try_Set_Wifi_StatusText(); 
                });
        }

 

        private void SetColors_ThemeAware_WifiIcon()
        {
            set_wifi_Image();
        }

        private void set_wifi_Image()
        {
            //theme
            notifyIcon_wifi.Icon = Resources.wifi_white.BitmapToIcon();
            //notifyIcon_wifi.Icon = ImageUtils.BytesToBitmap(Resources.g7_wifi_wght700).BitmapToIcon();
            bool isLight = TaskBarUtil.isTaskbarColor_Light();
            if (isLight)
            {
                notifyIcon_wifi.Icon = Resources.wifi_black.BitmapToIcon();
                //notifyIcon_wifi.Icon = ImageUtils.BytesToBitmap(Resources.g7_wifi_black_wght700).BitmapToIcon();
            }
        }

        private void set_wifi_Image_notConnected()
        {
            //theme
            notifyIcon_wifi.Icon = Resources.wifi_not_connected_wght700.BitmapToIcon();
            bool isLight = TaskBarUtil.isTaskbarColor_Light();
            if (isLight)
            {
                notifyIcon_wifi.Icon = Resources.wifi_not_connected_black_wght700.BitmapToIcon();
            }
        }

        private void NetworkChange_NetworkAvailabilityChanged(object sender, NetworkAvailabilityEventArgs e)
        {
            if (e.IsAvailable)
            {
                set_wifi_Image();
                Try_Set_Wifi_StatusText();
            }
            else
            {
                set_wifi_Image_notConnected();
                notifyIcon_wifi.Text = "Network is Not Available !";
            }

        }

        private void Try_Set_Wifi_StatusText()
        {
            try
            {
                var ret = show_WLan_info();
                notifyIcon_wifi.Text = ret.SSID+ "\r\n"+ ret.isConnected;  // + "\r\nSignal: " + ret.Signal;

                set_wifi_Image();
            }
            catch (Exception ex)
            {
                RamLogger.Log("showConnectedId Failed:" + ex.Message);
                notifyIcon_wifi.Text = " - ";

                set_wifi_Image_notConnected();
            }
        }

        private void Hide_Wifi_Icon()
        {
            notifyIcon_wifi.Visible = false;
        }
        private void Show_Wifi_Icon()
        {
            notifyIcon_wifi.Visible = true;
            //theme
            notifyIcon_wifi.Icon = Resources.wifi_white.BitmapToIcon();
            bool isLight = TaskBarUtil.isTaskbarColor_Light();
            if (isLight)
            {
                notifyIcon_wifi.Icon = Resources.wifi_black.BitmapToIcon();
            }
        }

        // Show SSID and Signal Strength
        private (string SSID, string Signal ,string isConnected,string Ghz) show_WLan_info()
        {
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.FileName = "netsh.exe";
            p.StartInfo.Arguments = "wlan show interfaces";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;

            // Stop the process from opening a new window
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;


            p.Start();
            string s = p.StandardOutput.ReadToEnd();
            p.WaitForExit();

            string SSID = GetValue_FromCmd(s,"SSID");
            string signal = GetValue_FromCmd(s,"Signal");
            string isConnected = GetValue_FromCmd(s,"State");
            string Ghz = GetValue_FromCmd(s,"Band");

            // "WIFI connected to " + s1 + "  " + s2;
            return (SSID, signal, isConnected, Ghz);

            string GetValue_FromCmd(string text , string key)
            {
                string s1 = text.Substring(text.IndexOf(key));
                s1 = s1.Substring(s1.IndexOf(":"));
                s1 = s1.Substring(2, s1.IndexOf("\n")).Trim();
                return s1;
            }

        }




        ///-------------- Run - processStart
        private static void ProcessStart(string url)
        {
            //var url = "ms-availablenetworks:"; //"myprotocl://10.0.0.123";  //ms-settings:personalization-colors
            var psi = new ProcessStartInfo();
            psi.UseShellExecute = true;
            psi.FileName = url;
            Process.Start(psi);
        }
        private static void Open_Github_LatestReleasePage()
        {
            ProcessStart("https://github.com/blackholeearth/Win10_BrightnessSlider/releases");
        }
        private static void Run_MsSettingUri_availableWifis()
        {
            ProcessStart("ms-availablenetworks:");
        }
        private static void Run_MsSettingUri_DarkLight()
        {
            ProcessStart("ms-settings:personalization-colors");

        }

        


    }






}





