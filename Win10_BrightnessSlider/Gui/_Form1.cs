using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using ep_uiauto;
using Gma.System.MouseKeyHook;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using SharpHook;
using Win10_BrightnessSlider.Gui;
using Win10_BrightnessSlider.Properties;
using static ep_uiauto.UIAutoPinvoke;
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

            ////--------important otherwise it says null error.. if someone access customTheme.
            ////------- this is only required for old users not having in their file.
            //var st = Settings_json.Get();
            //st.customTheme = new CustomTheme();
            //st.SaveTo_JsonFile();

            //st.SaveTo_TomlFile();




            //if(st.Style != Style.FollowSystem)
            //    isRoundCorners = st.Style == Style.win11; //has to be called first
            //these are set at Create ContexMenu
            //notifyIcon_wifi.Visible = st.WifiIcon;
            //GloabalKeyHookFn.copilotKey_toggle(st.MapCopilotKey);
        }


		/*

		v1.8.29
		* fix wifi icon not detecting disconnect Reconnect.
		
		v1.8.28
		* fix slider buttons.. (downside: clicking rapidly on + - buttons will be slow for DXVA monitors )

		v1.8.27
		* win+space > everthing ,  added error message.. "you need to install to default folder"

		v1.8.26
		* extras -> slider wButton , width is Back to Previous Size. - #182

		v1.8.25
		* at win10 - gap between contexmenu and taskbar set to 1px;
		* Fixed: slider wButton right part is hidden/outside of screen - #182

		v1.8.24
		* (info discovered - not enabled/ not tested ). scan only for monitor plug_in/out, Not Usb.
		* Added Proxy Detection, and Proxy Icon

		v1.8.23
		* improved: system events are debounced
		* fixed: mouse freezes for 1sec, when app Starts* 
			* migrated globalMouse to  **TolikPylypchuk/SharpHook**

		*/
		static string version = "1.8.29";

        /// <summary>
        /// is win11
        /// </summary>
        public static bool isWindows11 = WindowsVersion.IsWindows11();
        //cant use common, evey slider need distinctive one.
        //Bitmap slider_Image_dark => ImageUtils.BytesToBitmap(Resources.brightness_gray);
        Bitmap slider_Image_dark => Resources.sunny_black;
        Bitmap slider_Image_light => Resources.sunny_white;

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
        Color BorderColor1 = Color.FromArgb(63, 63, 63);
        Color BackColor1 = Color.FromArgb(31, 31, 31);
        Color TextColor1 = Color.White;
        //customcolor
        Settings settings_forTheme;
        private void SetColors_themeAware()
        {
            //Colors
            this.BackColor = BackColor1;
            notifyIcon_bright.Icon = Resources.sunny_white.BitmapToIcon();

            BorderColor1 = Color.FromArgb(63, 63, 63);
            BackColor1 = Color.FromArgb(31, 31, 31);
            TextColor1 = Color.White;

            bool isLight = TaskBarUtil.isTaskbarColor_Light();
            if (isLight)
            {
                BorderColor1 = Color.FromArgb(190, 190, 190);
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

            BorderColor1 = st.customTheme.borderColor;
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

        /// <summary>
        /// debugging helper tool. 
        /// </summary>
        TextBox _tbx_log;
        Form _frm_Log;
        bool _frm_Log__isEnabled = false;
        private void tbxLog_AppendText(string text)
        {
            if (!_frm_Log__isEnabled)
                return;

            if (_frm_Log is null)
            {
                (_frm_Log, _tbx_log) = MessageTextbox_Show("");
                _frm_Log.Visible = false;
                _frm_Log.Height = 700;
                _frm_Log.Top = 40;
            }

            _frm_Log.Visible = true;

            //found the freezer culprit Setting up events;
            Thread.Sleep(2000);
            _tbx_log.AppendText(text);
        }

        private IKeyboardMouseEvents m_GlobalHook;
        private void Form1_Load(object sender, EventArgs e)
        {
            eSetVis(false);

            SetColors_themeAware();
            SetColors_CustomTheme_ifEnabled();

            tbxLog_AppendText("\r\n setting up events...");
            notifyIcon_bright.Text = "setting up events...";
            wire_events();


            notifyIcon_bright.Text = "querying monitors...";
            //await on_event_RepopulateSlider_inTaskRun("formload", 0);

            tbxLog_AppendText("\r\n Get_RichInfo_Screen...");
            RichInfoScreen.Get_RichInfo_Screen();
            //RePopulateSliders();
            //await Task.Delay(250);
            Thread.Sleep(250);
            //calling this again to fix issue, i have to manualy detect monitor
            riScreens = RePopulateSliders();

            tbxLog_AppendText("\r\n GUI_Update__AllSliderControls...");
            GUI_Update__AllSliderControls();

            tbxLog_AppendText("\r\n Dump_Monitor,ShowNamesOnGui ...");

            notifyIcon_bright.Text = "Dump_Monitor,ShowNamesOnGui ...";
            Task.Run(() =>
            {
                Dump_Monitor_info_toJsonFile();
                Output_monitor_ids_toJson_And_ShowNamesOnGui();

                //throw new Exception("test- exception in taskrun... ");
            });


            tbxLog_AppendText("\r\n initializing Context Menu...");
            notifyIcon_bright.Text = "initializing Context Menu...";
            //CreateNotifyIcon_ContexMenu();
            CreateNotifyIcon_ContexMenuStrip();
            SetColors_themeAware_RightClickMenus();
            GUI_Update__CtxMenu_isRunAtStartup();

            //GUI_Update_StatesOnControls(); //also set CtxMenu Run At StartUp

            notifyIcon_bright.Text = "Win10_BrightnessSlider";
        }



        private void wire_events()
        {
            tbxLog_AppendText("\r\n wire_events - mglobal_hook ... ");
            //clicked outside windows--  deactivate doesnt work every time.
            // Note: for the application hook, use the Hook.AppEvents() instead
            //TODO: replaceThisLib------------ - this library freezes mouse for 1 sec ------------
            m_GlobalHook = Hook.GlobalEvents();
            //m_GlobalHook.KeyPress -= M_GlobalHook_KeyPress;
            //m_GlobalHook.KeyPress += M_GlobalHook_KeyPress;

            //BUG... mouse hooking freezes mouse for 1 sec..
            //m_GlobalHook.MouseDownExt -= M_GlobalHook_MouseDownExt;
            //m_GlobalHook.MouseDownExt += M_GlobalHook_MouseDownExt;
            //m_GlobalHook.MouseWheelExt -= M_GlobalHook_MouseWheelExt;
            //m_GlobalHook.MouseWheelExt += M_GlobalHook_MouseWheelExt;

            //test mouse lib
            Task.Run(() =>
            {
                var hook = new TaskPoolGlobalHook(); //block current thread 

                hook.KeyTyped += SHARPHOOK_OnKeyTyped;

                //hook.MouseClicked += SHARPHOOK_OnMouseClicked;    
                hook.MousePressed += SHARPHOOK_OnMousePressed;
                hook.MouseWheel += SHARPHOOK_OnMouseWheel;

                hook.Run();
            });


            tbxLog_AppendText("\r\n wire_events - form_ShowHide ... ");
            //form show hide event
            notifyIcon_bright.MouseClick += NotifyIcon_Bright_MouseClick;
            //clicked outside of form
            Deactivate += Form1_Deactivate;
            Deactivate += Form1_Deactivate1_hideCtxMenuStrips;


            //m_GlobalHook.init_remapKeys(true);

            tbxLog_AppendText("\r\n wire_events - device power events ... ");
            //  -monitor plug unplug
            //old one was using action, parameters name was 
            //DeviceNotification.Instance.On_WmDevicechange -= On_WmDevicechange;
            //DeviceNotification.Instance.On_WmDevicechange += On_WmDevicechange;
            DeviceNotification.Instance.On_WmDeviceChanged -= On_WmDeviceChanged;
            DeviceNotification.Instance.On_WmDeviceChanged += On_WmDeviceChanged;
            // monitor -on off
            PowerBroadcastSetting.Instance.On_PowerSettingsChange -= On_PowerSettingsChange;
            PowerBroadcastSetting.Instance.On_PowerSettingsChange += On_PowerSettingsChange;
            //resume suspend hibernate 
            SystemEvents.PowerModeChanged -= SystemEvents_PowerModeChanged;
            SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;

            //theme changed
            SystemEvents.UserPreferenceChanged -= SystemEvents_UserPreferenceChanged;
            SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;
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
        private void Output_monitor_ids_toJson_And_ShowNamesOnGui()
        {
            //output monitor ids to settings.json
            var settings = Settings_json.Get();
            if (settings.monitorNames is null)
                settings.monitorNames = new List<MonitorNames>();

            foreach (var riScr in riScreens)
            {
                var curRi_monName = new MonitorNames()
                {
                    MonitorName = "",
                    dc_monitorDevicePath = riScr.dc_TargetDeviceName?.monitorDevicePath ?? null,
                    wmi_InstanceName = riScr.WMIMonitorID?.InstanceName ?? null,
                };

                //-------- by wmi
                var foundMon = settings.monitorNames.FirstOrDefault(x => x.wmi_InstanceName == curRi_monName.wmi_InstanceName);
                if (string.IsNullOrWhiteSpace(curRi_monName.wmi_InstanceName))
                    foundMon = null;

                if (foundMon != null)
                {
                    if (!string.IsNullOrWhiteSpace(foundMon.MonitorName))
                        this.Invoke((Action)delegate { riScr.MonitorName = foundMon.MonitorName; });

                    continue;
                }

                //-------- by dc
                var foundMondc = settings.monitorNames.FirstOrDefault(x => x.dc_monitorDevicePath == curRi_monName.dc_monitorDevicePath);
                if (string.IsNullOrWhiteSpace(curRi_monName.dc_monitorDevicePath))
                    foundMondc = null;

                if (foundMondc != null)
                {
                    if (!string.IsNullOrWhiteSpace(foundMondc.MonitorName))
                        riScr.MonitorName = foundMondc.MonitorName;

                    continue;
                }

                //both not found -- add to json
                settings.monitorNames.Add(curRi_monName);
            }

            settings.SaveTo_JsonFile();
        }




        //---global key mouse events.
        private void SHARPHOOK_OnKeyTyped(object sender, KeyboardHookEventArgs e)
        {
            bool keyToClose = e.Data.KeyChar == (char)Keys.Escape;
            onEsc__CloseWindow_N_ctxMenu(keyToClose, out bool isHandled);
            e.SuppressEvent = isHandled;

        }
        private void M_GlobalHook_KeyPress(object sender, KeyPressEventArgs e)
        {
            bool keyToClose = e.KeyChar == (char)Keys.Escape;
            onEsc__CloseWindow_N_ctxMenu(keyToClose, out bool isHandled);
            e.Handled = isHandled;

        }
        private void onEsc__CloseWindow_N_ctxMenu(bool keyToClose, out bool isHandled)
        {
            isHandled = false;

            if (!keyToClose)
                return;

            if (Visible)
            {
                eSetVis(false);
                isHandled = true;
            }
            RamLogger.Log("keypress ESC pressed");

            notifyIcon_bright_ContextMenuStrip?.Close();
            notifyIcon_wifi_ContextMenuStrip?.Close();

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
            onGlobal_MouseWheel__ChangeBrightnes(e.Location, e.Delta > 0);

        }
        private void SHARPHOOK_OnMouseWheel(object sender, MouseWheelHookEventArgs e)
        {
            // https://sharphook.tolik.io/v5.3.8/articles/simulation.html
            onGlobal_MouseWheel__ChangeBrightnes(new Point(e.Data.X, e.Data.Y), e.Data.Rotation > 0);
        }
        private void onGlobal_MouseWheel__ChangeBrightnes(Point _Location, bool isIncrement)
        {
            var notifyicon_Rect = NotifyIconHelpers.GetNotifyIconRectangle(notifyIcon_bright, true);
            var Is_mouseInside_NotifRect = notifyicon_Rect.Contains(_Location);

            //RamLogger.Log($"M_GlobalHook_MouseWheelEXT - notifyicon_Rect: {notifyicon_Rect} , mouseLoc: {notifyicon_Rect}");

            if (!Is_mouseInside_NotifRect)
                return;

            RamLogger.Log($"M_GlobalHook_MouseWheelEXT - mouseInside_NotifRect  ");

            ////requires ui to be visible, otherwise error.
            //var ucSlderLi = getUCSliderLi(); //var slider1 = ucSlderLi.FirstOrDefault();

            // Prefer the screen under the mouse (where the tray icon is),
            // otherwise pick the first screen that reports a valid brightness,
            // and finally fall back to the primary screen.
            RichInfoScreen riScreen1 = null;
            try
            {
                riScreen1 = riScreens.FirstOrDefault(r => r.Screen.Bounds.Contains(_Location));
            }
            catch { }

            if (riScreen1 == null)
            {
                riScreen1 = riScreens.FirstOrDefault(r => r.GetBrightness() >= 0);
            }

            if (riScreen1 == null)
            {
                riScreen1 = riScreens.FirstOrDefault(r => r.Screen.Primary);
            }

            if (riScreen1 is null)
                return;

            //dont freeze Global Key Hook - release it immediately by doing LongRunningProcess on Seperate Thread. (Task.Run)
            Task.Run(() =>
            {
                this.Invoke((Action)delegate
                {
                    var val = riScreen1.GetBrightness();

                    if (isIncrement)
                    {
                        var newval = MathFn.Clamp(val + 5, 0, 100);
                        var ret = riScreen1.SetBrightness(newval, true);
                    }
                    else //if (e.Delta < 0)
                    {
                        var newval = MathFn.Clamp(val - 5, 0, 100);
                        var ret = riScreen1.SetBrightness(newval, true);
                    }

                    if (Visible)
                    {
                        GUI_Update__StatesOnControls();
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

        private DebounceDispatcher debouncer = new DebounceDispatcher();
        public void on_event_RepopulateSlider_inTaskRun__Debounced(string exNote, int DelayMs = 1000)
        {
            RamLogger.Log("on_event_RepopulateSlider_inTaskRun__Debounced:::" + exNote);
            debouncer.Debounce(800, _ =>
            {
                on_event_RepopulateSlider_inTaskRun(exNote, DelayMs);
                RamLogger.Log("on_event_RepopulateSlider_inTaskRun");
            });

        }
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
                    this.Invoke((Action)delegate
                    {
                        this.SuspendLayout();
                        GUI_Update__AllSliderControls();
                        this.ResumeLayout();

                    });


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

            on_event_RepopulateSlider_inTaskRun__Debounced("powerState: " + powerState);
        }

        private void On_WmDeviceChanged(object sender, MessageFormDN.DeviceChangedEventArgs e)
        {
            on_event_RepopulateSlider_inTaskRun__Debounced(
                "DeviceEventType: " + e.EventType + " ," +
                "IsGuid_MonitorOrDisplayAdapter:" + e.IsGuid_MonitorOrDisplayAdapter
                );

        }
        private void On_WmDevicechange(int Status, bool isMonitorRelated)
        {
            //DeviceNotification.DbtDevice_Arrival == DbtDevice_stat;
            on_event_RepopulateSlider_inTaskRun__Debounced("DbtDevice_stat: " + Status);
        }
        private void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            if (e.Mode == PowerModes.Resume)
                on_event_RepopulateSlider_inTaskRun__Debounced("powermode: " + e.Mode);
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
            //Console.WriteLine("SetColors_themeAware_RightClickMenus- disabled to test dpi thingy");
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
            //GC.Collect();
            RestartApp_ifRamUsage_isBiggerThan(150);

            // var monli = RichInfoScreen.GetMonitors();
            var riScreens = RichInfoScreen.Get_RichInfo_Screen();
            if (riScreens == null)
            {
                MessageBox.Show("SORRY!!\r\n" + "dxva2 and WMI functions Failed" + "\r\n There is Nothing to do");
                return null;
            }

            var settings = Settings_json.Get();

            ////test -- increases ram usage.. 1 -> 15 , 4-> 35  - 1slider-> 7mb on win11 1080p 125dpi
            //riScreens.AddRange(RichInfoScreen.Get_RichInfo_Screen());
            //riScreens.AddRange(RichInfoScreen.Get_RichInfo_Screen());

            //run on uı thread for controls
            this.Invoke((Action)delegate
            {
                this.SuspendLayout();
                //added at 1.7.5
                //foreach (Control ctl in fLayPnl1.Controls){ ctl.Dispose(); }
                fLayPnl1.Controls.Clear();

				//foreach (var riscrX in riScreens)
				for (int i = 0; i < riScreens.Count; i++)
                {
                    var riscrX = riScreens[i];

                    //uc_brSlider2 ucSldr = GetSlider2_UC(riscrX);
                    //Control ucSldr = newSlider3_UC(riscrX);

                    Control ucSldr;
                    if (settings.Show_PlusMinusButtons)
                        ucSldr = newSlider3_wButton_UC(riscrX);
                    else if (settings.Show_PlusMinusButtons_v2)
                        ucSldr = newSlider3_buttonsOnly_UC(riscrX);
                    else
                        ucSldr = newSlider3_UC(riscrX);

					// 2. INJECT THE NEW LOGIC HERE
					// Check if this control inherits from our new Base Class
					if (ucSldr is ThemedUserControl themedControl)
					{
						themedControl.IsFirstItem = (i == 0);
						themedControl.IsLastItem = (i == riScreens.Count - 1);

						themedControl.FrameColor = BorderColor1;
					}

					fLayPnl1.Controls.Add(ucSldr);
                }
                //uc_brSlider2_List = getUCSliderLi()

                FixFormHeight();
                this.ResumeLayout();
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
            ucSldr.FrameColor = new Pen(BorderColor1, 1);
            if (TaskBarUtil.isTaskbarColor_Light())
                ucSldr.pictureBox1.Image = slider_Image_dark;
            else
                ucSldr.pictureBox1.Image = slider_Image_light;


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
            ucSldr.trackBar1 = ColorSliderFn.setStyle_win10_trackbarColor_v2(ucSldr.trackBar1, settings_forTheme);
            ucSldr.trackBar1.BackColor = BackColor1;


            ucSldr.lbl_value.ForeColor = TextColor1;
            ucSldr.lbl_Name.ForeColor = TextColor1;
            //ucSldr.FrameColor = new Pen(BorderColor1, 1);
            ucSldr.FrameColor = BorderColor1;
            if (TaskBarUtil.isTaskbarColor_Light())
                ucSldr.pictureBox1.Image = slider_Image_dark;
            else
                ucSldr.pictureBox1.Image = slider_Image_light;


            //settings_forTheme.CustomTheme_Enabled
            if (Settings_json.Get().CustomTheme_Enabled)
                ucSldr.pictureBox1.Image = slider_Image_current();

            HelperFn.SetTooltip(ucSldr.pictureBox1, riscrX.TooltipText, riscrX.avail_MonitorName);
            ucSldr.lbl_Name.Text = riscrX.avail_MonitorName_clean;

            riscrX.onMonitorNameChanged += name => { ucSldr.Set_MonitorName(name); };
            //ucSldr.lbl_Name.Text = riscrX.TooltipText;
            ucSldr.riScreen = riscrX;
            return ucSldr;
        }
        private Control newSlider3_wButton_UC(RichInfoScreen riscrX)
        {
            var ucSldr_wb = new uc_brSlider3_wButton()
            {
                Margin = Padding.Empty,
                BackColor = BackColor1,
            };

            var ucSldr = (uc_brSlider3)newSlider3_UC(riscrX);
            ucSldr_wb.SetGUIColors(BackColor1, TextColor1, BorderColor1, ucSldr);
            //ucSldr_wb._uc_brSlider3.Parent = this;
            this.Width = ucSldr_wb.Width;

			


			return ucSldr_wb;
        }
        private Control newSlider3_buttonsOnly_UC(RichInfoScreen riscrX)
        {
            var ucSldr = new uc_brSlider3_buttonsOnly()
            {
                Margin = Padding.Empty,
                BackColor = BackColor1,
            };
            //ucSldr.trackBar1 = ColorSliderFn.setStyle_win10_trackbarColor_withElapsedAccented(ucSldr.trackBar1);
            ucSldr.trackBar1 = ColorSliderFn.setStyle_win10_trackbarColor_v2(ucSldr.trackBar1, settings_forTheme);
            ucSldr.trackBar1.BackColor = BackColor1;
            ucSldr.trackBar1.Enabled = false;  //test

            ucSldr.lbl_value.ForeColor = TextColor1;
            ucSldr.lbl_Name.ForeColor = TextColor1;
            ucSldr.FrameColor = BorderColor1;
            if (TaskBarUtil.isTaskbarColor_Light())
                ucSldr.pictureBox1.Image = slider_Image_dark;
            else
                ucSldr.pictureBox1.Image = slider_Image_light;


            //settings_forTheme.CustomTheme_Enabled
            if (Settings_json.Get().CustomTheme_Enabled)
                ucSldr.pictureBox1.Image = slider_Image_current();

            HelperFn.SetTooltip(ucSldr.pictureBox1, riscrX.TooltipText, riscrX.avail_MonitorName);
            ucSldr.lbl_Name.Text = riscrX.avail_MonitorName_clean;

            riscrX.onMonitorNameChanged += name => { ucSldr.Set_MonitorName(name); };
            //ucSldr.lbl_Name.Text = riscrX.TooltipText;
            ucSldr.riScreen = riscrX;


            //------additional for 
            ucSldr.SetGUIColors(BackColor1, TextColor1, BorderColor1);
            //ucSldr_wb._uc_brSlider3.Parent = this;
            this.Width = ucSldr.Width;

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
                RamLogger.Log("coudnt get form height");
            }
        }
        public void GUI_Update__StatesOnControls()
        {
            GUI_Update__CtxMenu_isRunAtStartup();

            GUI_Update__AllSliderControls();
        }

        private void GUI_Update__CtxMenu_isRunAtStartup()
        {
            //get current states
            var isRunSttup = HelperFn.isRunAtStartup();

            var itemx = notifyIcon_bright_ContextMenuStrip?.Items?
                .OfType<ToolStripMenuItem>().Where(x => x.Text.StartsWith("Run At Startup")).FirstOrDefault();

            if (itemx != null)
                itemx.Checked = isRunSttup;

            //notifyIcon_bright.ContextMenuStrip.Items
            //    .OfType<ToolStripMenuItem>().Where(x => x.Text == "Run At Startup").FirstOrDefault()
            //    .Checked = isRunSttup;
        }
        public void GUI_Update__AllSliderControls()
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
            try { sb.Remove(sb.Length - 2, 2); } catch (Exception) { }

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
        /// <para/>
        /// if you need to update gui - call this  <para/>
        /// GUI_Update_AllSliderControls(); <para/>
        /// eSetVis(true) <para/>
        /// </summary>
        public void eSetVis(bool visible)
        {
            if (visible)
            {
                //-------only  calc if visible

                //Console.WriteLine("eSetVis - vis:" + vis);
                RamLogger.Log($"eSetVis(visible:{visible})  , vis:{vis}");
                this.WindowState = FormWindowState.Normal;
                this.StartPosition = FormStartPosition.Manual;

                var pvis = TaskBarLocationFn.calc_FormLoc_NearToTaskbarDate(this.Size);
                //pvis = TaskBarLocationFn.ofset_FormLocation_p2_for_Win11_RoundBorder(pvis ,this.Size, win11_roundCorners_Enabled,this);
                pvis = TaskBarLocationFn.offset_FormLoc_for_RoundCorners(new Rectangle(pvis, this.Size), isWindows11)
                   .Location;

                if (!TaskBarLocationFn.IsTaskbarVisible())
                {
                    pvis = TaskBarLocationFn.calc_FormLoc_NearToTaskbarDate_viaMouseLocation(this.Size);
                    pvis = TaskBarLocationFn.offset_FormLoc_for_RoundCorners_CalculateAutoHide(new Rectangle(pvis, this.Size), isWindows11)
                        .Location;

                }
                //var phidden = new Point(-this.Width, -this.Height);



                //---show logic
                this.Location = pvis;

                this.Region = isWindows11 ? RoundBorders.GetRegion_ForRoundCorner(this.Size, 16) : null;

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
        private void M_GlobalHook_MouseDownExt(object sender, MouseEventExtArgs e)
        {
            hide_Window_whenClickedOutside(e.Location, e.IsMouseButtonDown);

        }
        private void SHARPHOOK_OnMousePressed(object sender, MouseHookEventArgs e)
        {
            this.Invoke((Action)delegate
            {
                hide_Window_whenClickedOutside(new Point(e.Data.X, e.Data.Y), true);
            });

        }
        private void SHARPHOOK_OnMouseClicked(object sender, MouseHookEventArgs e)
        {
        }
        private void hide_Window_whenClickedOutside(Point _Location, bool isMsButtonDown)
        {
            //dont run , if clicked on tray icon.
            var notifyicon_Rect = NotifyIconHelpers.GetNotifyIconRectangle(notifyIcon_bright, true);
            var Is_mouseInside_NotifRect = notifyicon_Rect.Contains(_Location);
            if (Is_mouseInside_NotifRect)
                return;

            //RamLogger.Log("GlobalHookMouseDownExt");
            if (!this.Visible)
                return;

            //RamLogger.Log("cond: visible true ");

            if (isMsButtonDown)
            {
                //var form_ptoSCR = PointToScreen(this.ClientRectangle.Location);
                var form_recttoSCR = RectangleToScreen(this.ClientRectangle);

                RamLogger.Log($"form_clientRect:{this.ClientRectangle}, " +
                    $"form_clientRect_RecttoSCR:{form_recttoSCR}" +
                    $"mouseDow.Loc:{_Location}"
                    );

                bool clickedinside_theForm = form_recttoSCR.Contains(_Location);
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
                GUI_Update__AllSliderControls();
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
        void CreateNotifyIcon_ContexMenuStrip()
        {
            //var settingsX = Settings_litedb.GetSettings();
            var settingsX = Settings_json.Get();

            var cms = new ContextMenuStrip_win11();

            var mi0_exit = new ToolStripMenuItem("Exit", null, (snd, ev) => { Application.Exit(); });
            var mi0_restart = new ToolStripMenuItem("Restart", null, (snd, ev) => { Application.Restart(); });

            var mi1_aboutMe = new ToolStripMenuItem($"About Me - (v{version})", null, (snd, ev) =>
            {
                var text =
@"
Developer: blackholeearth 

Official Site: 
https://github.com/blackholeearth/Win10_BrightnessSlider
";
                MessageTextbox_Show(text);
            });

            mi_update = new ToolStripMenuItem("Check For Update, in 1min", null, (snd, ev) => { });
            mi_update.Enabled = false;
            CheckForUpdate_inTaskRun_Retry();
            var mi_log = new ToolStripMenuItem("Show Logs", null, (snd, ev) =>
            {
                var logstr = RamLogger.ToString();
                LogTextbox_Show(
                    logstr,
                    RamLogger.logger.Count + "",
                    "Refresh, For New Logs", () => RamLogger.ToString(),
                    "Delete Logs", () => RamLogger.logger.Clear()
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

            /// ----------- ReMap Keys
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



            var mi_hotkey_everything = new ToolStripMenuItem("Win+Space > Open Everything.exe");
            GloabalKeyHookFn.openEverything_toggle(settingsX.Hotkey_OpenEverything);
            GloabalKeyHookFn.hideEverything_toggle(settingsX.Hotkey_OpenEverything);
            mi_hotkey_everything.Checked = settingsX.Hotkey_OpenEverything;
            mi_hotkey_everything.Click += (snd, ev) =>
            {
                var _mix = snd as ToolStripMenuItem;
                _mix.Checked = !_mix.Checked;
                GloabalKeyHookFn.openEverything_toggle(_mix.Checked);
                GloabalKeyHookFn.hideEverything_toggle(_mix.Checked);

                //Settings_litedb.Update(st => { st.MapCopilotKey = _mix.Checked; });
                Settings_json.Update(st => { st.Hotkey_OpenEverything = _mix.Checked; });
            };

            //---way1---doesnt work-- registering two same keys deletes previous one.
            //hide ContextMenuStrip when win Key PRessed
            //GloabalKeyHookFn.on_LWinPresssed_toggle(true, () => {
            //    notifyIcon_bright_ContextMenuStrip?.Hide();
            //    notifyIcon_wifi_ContextMenuStrip?.Hide();
            //}  );
            //---way1---  works after --- closeEverthingExe
            GloabalKeyHookFn.on_LWinPresssed_ExtraAction = () =>
            {
                notifyIcon_bright_ContextMenuStrip?.Hide();
                notifyIcon_wifi_ContextMenuStrip?.Hide();
            };


            var mi_ShowButtons = new ToolStripMenuItem("Show +/- Buttons wSlider");
            var mi_ShowButtons_v2 = new ToolStripMenuItem("Show +/- Buttons");

            mi_ShowButtons.ToolTipText = "This is intended for TABLET  Users.";
            mi_ShowButtons.Checked = settingsX.Show_PlusMinusButtons;
            mi_ShowButtons.Click += (snd, ev) =>
            {
                var _mix = snd as ToolStripMenuItem;
                _mix.Checked = !_mix.Checked;

                Settings_json.Update(st =>
                {
                    st.Show_PlusMinusButtons = _mix.Checked;

                    //toogle of other -radio style
                    if (st.Show_PlusMinusButtons == true)
                    {
                        st.Show_PlusMinusButtons_v2 = false;
                        mi_ShowButtons_v2.Checked = false;
                    }
                });

                //Application.Restart();
                this.Width = new uc_brSlider3().Width;
                Lightweight_Restart__Aka_PopulateSliders_ReapplyColors();
                GUI_Update__AllSliderControls();
                eSetVis(true);
            };

            mi_ShowButtons_v2.ToolTipText = "This is intended for TABLET  Users.";
            mi_ShowButtons_v2.Checked = settingsX.Show_PlusMinusButtons_v2;
            mi_ShowButtons_v2.Click += (snd, ev) =>
            {
                var _mix = snd as ToolStripMenuItem;
                _mix.Checked = !_mix.Checked;

                Settings_json.Update(st =>
                {
                    st.Show_PlusMinusButtons_v2 = _mix.Checked;

                    //toogle of other -radio style
                    if (st.Show_PlusMinusButtons_v2 == true)
                    {
                        st.Show_PlusMinusButtons = false;
                        mi_ShowButtons.Checked = false;
                    }
                });

                //Application.Restart();
                this.Width = new uc_brSlider3().Width;
                Lightweight_Restart__Aka_PopulateSliders_ReapplyColors();
                GUI_Update__AllSliderControls();
                eSetVis(true);
            };


            var mi_customTheme_onOff = new ToolStripMenuItem("Custom Theme");
            mi_customTheme_onOff.Checked = settingsX.CustomTheme_Enabled;
            mi_customTheme_onOff.Click += (snd, ev) =>
            {
                var _mix = snd as ToolStripMenuItem;
                _mix.Checked = !_mix.Checked;

                Settings_json.Update(st => { st.CustomTheme_Enabled = _mix.Checked; });

                //Application.Restart();
                Lightweight_Restart__Aka_PopulateSliders_ReapplyColors();
                GUI_Update__AllSliderControls();
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


            cms.Items.Add(mi1_aboutMe);
            cms.Items.Add(mi_update);
            cms.Items.Add("-");
            cms.Items.Add(mi_log);
            cms.Items.Add(mi_rescanMon);
            cms.Items.Add("-");
            //cm.Items.Add(mi_RoundCorner);



            var mi_extras = new ToolStripMenuItem("Extras");
            cms.Items.Add(mi_extras);
            {
                //mi_extras.DropDown = new ToolStripDropDown();
                //mi_extras.DropDown = new w11_ToolStripDropDown();
                mi_extras.DropDown.ApplyRoundCorners(true);
                ////test
                //mi_extras.DropDown.VisibleChanged += (s1,e1) => 
                //{
                //    mi_extras.DropDown.Show_win11Style_atlocation(cms);
                //};

                mi_extras.DropDown.Items.Add(mi_customTheme_EditFile);
                mi_extras.DropDown.Items.Add(mi_customTheme_onOff);
                mi_customTheme_onOff.ToolTipText = "Edit Colors in Settings.json File... :)";
                mi_extras.DropDown.Items.Add("-");
                mi_extras.DropDown.Items.Add(new ToolStripMenuItem("___Tablet Buttons___") { Enabled = false });
                mi_extras.DropDown.Items.Add(mi_ShowButtons);
                mi_extras.DropDown.Items.Add(mi_ShowButtons_v2);
                mi_extras.DropDown.Items.Add("-");
                mi_extras.DropDown.Items.Add(new ToolStripMenuItem("___ReMap Keys___") { Enabled = false });
                mi_extras.DropDown.Items.Add(mi_remapKey1);
                mi_extras.DropDown.Items.Add(mi_hotkey_everything);
                //mi_extras.DropDown.Items.Add("-");
            }
            cms.Items.Add(mi_wifiToggle);
            cms.Items.Add(mi_runAtStartUp);

            cms.Items.Add("-");
            cms.Items.Add(mi_darkMode);
            cms.Items.Add(mi0_restart);
            cms.Items.Add(mi0_exit);


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
                if (e1.Button == MouseButtons.Right)
                {
                    //hide Taskbar App icon. 
                    UnsafeNativeMethods.SetForegroundWindow(new HandleRef(cms, cms.Handle));
                    cms.Show_win11Style_atlocation(notifyIcon_bright);
                }

            };

        }
        private void Form1_Deactivate1_hideCtxMenuStrips(object sender, EventArgs e)
        {
            //if user Clicks - Win button -- Windows Menu Opens. This method  Hides RightClickMenus
            //Prevent Dummy App Icon Being Visible In TASKBAR
            notifyIcon_bright_ContextMenuStrip?.Hide();
            notifyIcon_wifi_ContextMenuStrip?.Hide();

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
        /// ------ Forms via Code 
        private (Form frm, TextBox tbx) MessageTextbox_Show(string text)
        {
            var f = new Form() { Width = 650, Height = 250, Padding = new Padding(7), StartPosition = FormStartPosition.CenterScreen };
            var tb = new TextBox() { ReadOnly = true, Dock = DockStyle.Fill, Multiline = true, };
            tb.Font = new Font(tb.Font.OriginalFontName, 12f);
            f.Controls.Add(tb);
            f.Show();

            tb.Text = text;

            return (f, tb);
        }
        private void LogTextbox_Show(string text, string title,
            string btnText, Func<string> btnClickAction,
            string btn2Text, Action btn2ClickAction
            )
        {
            var f = new Form() { Width = 632, Height = 400, Padding = new Padding(7), StartPosition = FormStartPosition.CenterScreen };
            f.Text = "Logs: " + title;

            var tb = new TextBox() { ReadOnly = true, Dock = DockStyle.Fill, Multiline = true, };
            tb.ScrollBars = ScrollBars.Both;
            tb.WordWrap = false;
            tb.BackColor = Color.WhiteSmoke;



            var btn_in = new Button() { Text = "Copy To Clipboard...", Width = 150, Dock = DockStyle.Top, Height = 35 };
            btn_in.Click += (s1, e1) =>
            {
                if (!string.IsNullOrWhiteSpace(tb.Text))
                    Clipboard.SetText(tb.Text);
            };

            var btn = new Button() { Text = btnText, Width = 150, Dock = DockStyle.Top, Height = 35 };
            btn.Click += (s1, e1) =>
            {
                tb.Text = btnClickAction?.Invoke();
                tb.SelectionStart = tb.Text.Length;
                tb.ScrollToCaret();
            };
            var btn2 = new Button() { Text = btn2Text, Width = 150, Dock = DockStyle.Top, Height = 35 };
            btn2.Click += (s1, e1) =>
            {
                btn2ClickAction?.Invoke();
                tb.Text = "";
            };

            btn_in.Dock = btn.Dock = btn2.Dock = DockStyle.Bottom;
            //f.Padding = new Padding(20);
            f.Controls.Add(tb); //dock.fill; tb shoud be added first. or overlap ,
            f.Controls.Add(btn_in);
            f.Controls.Add(btn);
            f.Controls.Add(btn2);
            f.Controls.Add(new Label() { Text = "  ", Dock = btn.Dock }); //easy resize grip
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

            Task.Run(() =>
            {
                NetworkChange.NetworkAvailabilityChanged -= NetworkChange_NetworkAvailabilityChanged;
                NetworkChange.NetworkAvailabilityChanged += NetworkChange_NetworkAvailabilityChanged;

				NetworkChange.NetworkAddressChanged -= NetworkChange_AddressChanged; //gemini says add this.?!
				NetworkChange.NetworkAddressChanged += NetworkChange_AddressChanged;
			});


            var cms = new ContextMenuStrip_win11();

            var mi3 = new ToolStripMenuItem("set Proxy ON", null, (s1, e1) =>
            {
                ProxyON();
                Try_Set_Wifi_StatusText();
                //Task.Run(() => {
                //    Thread.Sleep(1000);
                //    Try_Set_Wifi_StatusText();
                //});

            });
            cms.Items.Add(mi3);

            var mi2 = new ToolStripMenuItem("set Proxy OFF", null, (s1, e1) =>
            {
                ProxyOFF();
                Try_Set_Wifi_StatusText();
                //Task.Run(() => {
                //    Thread.Sleep(1000);
                //    Try_Set_Wifi_StatusText();
                //});
            });
            cms.Items.Add(mi2);

            cms.Items.Add(new ToolStripSeparator());


            var mi1 = new ToolStripMenuItem("Hide Wifi Icon", null, (s1, e1) => { Hide_Wifi_Icon(); mi_wifiToggle.Checked = false; });
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

            Task.Run(() =>
            {
                Try_Set_Wifi_StatusText();
            });
        }



        private void SetColors_ThemeAware_WifiIcon() => set_wifi_Image();

        /// <summary>
        /// if proxy is enabled, returns "proxy:Info" , otherwise ""
        /// </summary>
        /// <returns></returns>
        private string set_wifi_Image()
        {

            var proxyMessage = "";
            //theme
            notifyIcon_wifi.Icon = Resources.wifi_white.BitmapToIcon();
            //notifyIcon_wifi.Icon = ImageUtils.BytesToBitmap(Resources.g7_wifi_wght700).BitmapToIcon();
            bool isLight = TaskBarUtil.isTaskbarColor_Light();
            if (isLight)
            {
                notifyIcon_wifi.Icon = Resources.wifi_black.BitmapToIcon();
                //notifyIcon_wifi.Icon = ImageUtils.BytesToBitmap(Resources.g7_wifi_black_wght700).BitmapToIcon();
            }

            //------ try Set Wifi Proxy İcon
            try
            {                // HKCU\
                var subKey = @"Software\Microsoft\Windows\CurrentVersion\Internet Settings";
                using (var key = Registry.CurrentUser.OpenSubKey(subKey, false)) // False is important!
                {
                    var autoDetect = key.GetValue("AutoDetect") as int?;
                    var proxyOn = key.GetValue("ProxyEnable") as int?;
                    var proxyServer = key.GetValue("ProxyServer") as string;
                    if (proxyOn == 1)
                    {
                        set_wifi_Image_ProxyON();
                        proxyMessage = ("Proxy:On Auto" /*+"," +proxyServer*/).Truncate(63);
                        //set_wifi_Image_ProxyON();
                    }
                }
            }
            catch (Exception ex)  //just for demonstration...it's always best to handle specific exceptions
            {
                proxyMessage = "\r\nERROR -Can't Read Proxy";
            }

            return proxyMessage;
        }

        private void set_wifi_Image_ProxyON()
        {
            //theme
            //notifyIcon_wifi.Icon = Resources.wifi_proxy_white_wght700.BitmapToIcon();
            notifyIcon_wifi.Icon = Resources.wifi_proxy__white_Filled.BitmapToIcon();
            //notifyIcon_wifi.Icon = ImageUtils.BytesToBitmap(Resources.g7_wifi_wght700).BitmapToIcon();
            bool isLight = TaskBarUtil.isTaskbarColor_Light();
            if (isLight)
            {
                //notifyIcon_wifi.Icon = Resources.wifi_proxy_black_wght700.BitmapToIcon();
                notifyIcon_wifi.Icon = Resources.wifi_proxy__black_Filled.BitmapToIcon();
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

		// IP adresi değiştiğinde (örn: Wifi IP aldığında) çalışır.
		// Bu genellikle AvailabilityChanged'den daha sonra çalışır ve daha güvenilirdir.
		private void NetworkChange_AddressChanged(object sender, EventArgs e)
		{
			// Yine debounce/gecikme ile kontrol et
			Task.Run(async () => {
				await Task.Delay(2000); // 2 saniye bekle
				this.Invoke((Action)delegate { Try_Set_Wifi_StatusText(); });
			});
		}
		private void NetworkChange_NetworkAvailabilityChanged(object sender, NetworkAvailabilityEventArgs e)
		{
			if (e.IsAvailable)
			{
				// Olay tetiklendiğinde hemen kontrol etmek yerine,
				// Arka planda birkaç kez deneyerek bağlantının tam oturmasını bekle.
				Task.Run(async () =>
				{
					// 5 deneme yap (her biri 1.5 saniye arayla)
					for (int i = 0; i < 5; i++)
					{
						// Netsh'den veriyi çek ama UI güncelleme (sadece kontrol et)
						var info = show_WLan_info();

						// Eğer durum "Connected" ise döngüyü kır ve UI güncelle
						if (!string.IsNullOrWhiteSpace(info.isConnected) &&
							 info.isConnected.Trim().ToLower().Contains("connected"))
						{
							break; // Bağlantı sağlandı, döngüden çık
						}

						// Bağlı değilse biraz bekle ve tekrar dene
						await Task.Delay(1500);
					}

					// Son durumu ekrana bas (UI Thread içinde)
					this.Invoke((Action)delegate { Try_Set_Wifi_StatusText(); });
				});
			}
			else
			{
				// İnternet koptuysa beklemeye gerek yok, direkt ikonu değiştir.
				this.Invoke((Action)delegate {
					set_wifi_Image_notConnected();
					notifyIcon_wifi.Text = "Network is Not Available !";
				});
			}
		}
		private void NetworkChange_NetworkAvailabilityChanged_old(object sender, NetworkAvailabilityEventArgs e)
        {
            if (e.IsAvailable)
            {
                //set_wifi_Image(); // wifi icon is set inside Try_Set_Wifi_StatusText;
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
                var proxyMessage = set_wifi_Image();

                var msg =
                    ret.SSID
                    + "\r\n" + ret.isConnected   /* + "\r\nSignal: " + ret.Signal; */
                    + "\r\n" + proxyMessage
                    ;

                notifyIcon_wifi.Text = msg.Truncate(63);

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
        private (string SSID, string Signal, string isConnected, string Ghz) show_WLan_info()
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


            string GetValue_FromCmd(string text, string key)
            {
                string s1 = text.Substring(text.IndexOf(key));
                s1 = s1.Substring(s1.IndexOf(":"));
                s1 = s1.Substring(2, s1.IndexOf("\n")).Trim();
                return s1;
            }

            string SSID = GetValue_FromCmd(s, "SSID");
            string signal = GetValue_FromCmd(s, "Signal");
            string isConnected = GetValue_FromCmd(s, "State");
            string Ghz = GetValue_FromCmd(s, "Band");

            // "WIFI connected to " + s1 + "  " + s2;
            return (SSID, signal, isConnected, Ghz);
        }

        private string ProxyON()
        {
            var script =
$@"
@echo off

echo Switching ON proxy . . .

REG ADD ""HKCU\Software\Microsoft\Windows\CurrentVersion\Internet Settings"" /v ProxyEnable /t REG_DWORD /d 1 /f
REG ADD ""HKCU\Software\Microsoft\Windows\CurrentVersion\Internet Settings"" /v AutoDetect /t REG_DWORD /d 1 /f

echo Proxy:ON -  AutoDetect:ON

";
            var (output, err) = ExecuteCMDScript(script);


            return "On";
        }
        private string ProxyOFF()
        {
            var script =
$@"
@echo off

echo Switching OFF proxy . . .

REG ADD ""HKCU\Software\Microsoft\Windows\CurrentVersion\Internet Settings"" /v ProxyEnable /t REG_DWORD /d 0 /f
REG ADD ""HKCU\Software\Microsoft\Windows\CurrentVersion\Internet Settings"" /v AutoDetect /t REG_DWORD /d 0 /f

echo. 

";
            var (ret, ret2) = ExecuteCMDScript(script);


            // "WIFI connected to " + s1 + "  " + s2;
            return "Off";
        }



        /// <summary>
        /// runs CMD batch script - can run multi-line script... 
        /// <para>ie:  ExecuteCommand("echo testing"); </para>
        /// </summary>
        /// <param name="script">  pass the content of your script.bat </param>
        static (string output, string error) ExecuteCMDScript(string script)
        {
            int exitCode;
            ProcessStartInfo psi;
            Process process;

            //psi = new ProcessStartInfo("cmd.exe", "/c " + command);
            psi = new ProcessStartInfo("cmd.exe");
            psi.CreateNoWindow = true;
            psi.UseShellExecute = false;
            //***Redirect the output ***
            psi.RedirectStandardInput = true;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;

            process = Process.Start(psi);

            process.StandardInput.WriteLine(script);
            process.StandardInput.Flush();
            process.StandardInput.Close();
            process.WaitForExit();

            // *** Read the streams ***
            // Warning: This approach can lead to deadlocks, see Edit #2
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            exitCode = process.ExitCode;

            Console.WriteLine("output: " + output);
            Console.WriteLine("error: " +  error);
            Console.WriteLine("ExitCode: " + exitCode.ToString());
            process.Close();

            return (output, error);

        }
        ///-------------- Run - processStart
        private static Process ProcessStart(string url)
        {
            //var url = "ms-availablenetworks:"; //"myprotocl://10.0.0.123";  //ms-settings:personalization-colors
            var psi = new ProcessStartInfo();
            psi.UseShellExecute = true;
            psi.FileName = url;
            var process = Process.Start(psi);
            return process;
        }

        private static void Open_Github_LatestReleasePage()
            => ProcessStart("https://github.com/blackholeearth/Win10_BrightnessSlider/releases");
        private static void Run_MsSettingUri_availableWifis() => ProcessStart("ms-availablenetworks:");
        private static void Run_MsSettingUri_DarkLight() => ProcessStart("ms-settings:personalization-colors");
        public static void Run_EverythingExe()
        {
            //hide previously visible; ---this way it wont show red Colored - cant Bright To Front.
            var handle = UIAutoPinvoke.GetWindowHandle_byText("everything", x => x.className == "EVERYTHING");
            if (handle != IntPtr.Zero)
            {
                //UIAutoPinvoke.ShowWindow(handle, UIAutoPinvoke.SW_HIDE);
                UIAutoPinvoke.ShowWindow(handle, UIAutoPinvoke.SW_SHOW);
                //UIAutoPinvoke.ShowWindow(handle, UIAutoPinvoke.SW_NORMAL);
                //return;
            }



           
            try
            {
                var ps = ProcessStart(@"C:\Program Files\Everything\Everything.exe");
            }
            catch (Exception ex)
            {
                try
                {
                    var ps = ProcessStart(@"D:\Downloads\programs\_Everything-1.4.1.1026.x64\everything.exe");
                }
                catch (Exception ex2)
                {

                    MessageBox.Show(
$@"Error:

you need to install everything to  default path: 
C:\Program Files\Everything\Everything.exe


Error Message: 

  {ex2.Message}

" );
                }
               
            }



        }




    }






}





