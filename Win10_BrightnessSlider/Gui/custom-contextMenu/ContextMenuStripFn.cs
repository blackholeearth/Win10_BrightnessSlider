using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace Win10_BrightnessSlider
{

    /// <summary>
    /// default is setColors_Dark();
    /// call setcolors dark
    /// </summary>
    public class CMS_colors 
    {
        
        public Font menuItemFont = new Font("Segoe UI", 10);
        public Color backColor = Color.FromArgb(44, 44, 44);  //SystemColors.Window;
        public Color textColor = Color.FromArgb(255, 255, 255);  // SystemColors.ControlText;
        public Color backColor_hovered = Color.FromArgb(61, 61, 61);
        public Color seperatorColor = Color.FromArgb(61, 61, 61);

        //---renderer

        /// <summary>
        /// move text to right - give checkmark more area .... if above 30 text gets clipped.
        /// </summary>
        public int item_TextOffset = 16; //
        /// <summary>
        /// increase item Area Height - not the Fontsize
        /// </summary>
        public int item_verticalPadding = 5; // 14 -- win11 1080p 125%  near


        public CMS_colors() {

            ////padding 14 too big..
            ////win11 1080 125 dpi
            //menuItemFont = new Font("Segoe UI", 10);
            //item_verticalPadding = 14;

            //more compact
            menuItemFont = new Font("Segoe UI", 10);
            item_verticalPadding = 5;

        }

        void setColors_win11_Dark()
        {
            //menuItemFont = new Font("Segoe UI", 9);
            backColor = Color.FromArgb(44, 44, 44);  //SystemColors.Window;
            textColor = Color.FromArgb(255, 255, 255);  //Color.FromArgb(234, 234, 234);  // SystemColors.ControlText;
            backColor_hovered = Color.FromArgb(61, 61, 61);
            seperatorColor = Color.FromArgb(61, 61, 61);
        }
        void setColors_win11_Light()
        {
            //menuItemFont = new Font("Segoe UI", 9);
            backColor = Color.FromArgb(249, 249, 249);  //SystemColors.Window;
            textColor = Color.FromArgb(26, 26, 26);  // SystemColors.ControlText;
            backColor_hovered = Color.FromArgb(240, 240, 240);
            seperatorColor = Color.FromArgb(234, 234, 234);

 
        }

        public static CMS_colors new_Win11_Dark()
        {
            var cmsColors = new CMS_colors();
            cmsColors.setColors_win11_Dark();
            
            return cmsColors;
        }
        public static CMS_colors new_Win11_Light()
        {
            var cmsColors = new CMS_colors();
            cmsColors.setColors_win11_Light();

            return cmsColors;
        }

    }

    public static partial class ContextMenuStripFn
    {
        //static CMS_colors cmsColors_dis =   CMS_colors.new_Win11_Dark();

        //-----------style fix
        public static void CMS_set_win11_DarkTheme(this ContextMenuStrip ctxMenuStrip) 
            => set_win11_Theme_common(ctxMenuStrip, CMS_colors.new_Win11_Dark());

        public static void CMS_set_win11_LightTheme(this ContextMenuStrip ctxMenuStrip) 
            => set_win11_Theme_common(ctxMenuStrip, CMS_colors.new_Win11_Light());

        private static void set_win11_Theme_common(ContextMenuStrip ctxMenuStrip, CMS_colors cmsColors)
        {
            ctxMenuStrip.Renderer = new win11_MenuRenderer(new win11_darkColorTable(cmsColors), cmsColors);
            //ctxMenuStrip.Renderer = new ToolStripProfessionalRenderer(new win11_darkColorTable());

            ctxMenuStrip.ForeColor = cmsColors.textColor;
            ctxMenuStrip.BackColor = cmsColors.backColor;
            ctxMenuStrip.Font = cmsColors.menuItemFont;//doesnt work

            CtxMenuStrip_ItemAdded(ctxMenuStrip, null);

            ctxMenuStrip.ItemAdded -= CtxMenuStrip_ItemAdded;
            ctxMenuStrip.ItemAdded += CtxMenuStrip_ItemAdded;


        }
        //submenu color fix
        private static void CtxMenuStrip_ItemAdded(object sender, ToolStripItemEventArgs e)
        {
            var snd_ctxMenuStrip = sender as ContextMenuStrip;
            var cmsColors = (snd_ctxMenuStrip.Renderer as win11_MenuRenderer).cmsColors;

            var itemsWithDD = snd_ctxMenuStrip.Items.OfType<ToolStripMenuItem>().Where(x => x.HasDropDownItems).ToList();
            //submenu text color fix 
            foreach (ToolStripMenuItem item in itemsWithDD)
            {
                if (item.DropDown != null)
                {
                    item.DropDown.ForeColor = cmsColors.textColor;
                    item.DropDown.BackColor = cmsColors.backColor;
                }

            }
        }

    }




    public static class ContextMenuStripFn2
    {
        public static void Show_win11Style_atlocation(this ContextMenuStrip ctxMenuStrip, NotifyIcon notifyIcon1)
        {
            var notifyicon_Rect = NotifyIconHelpers.GetNotifyIconRectangle(notifyIcon1, true);
            notifyicon_Rect.X = (int)(notifyicon_Rect.X + notifyicon_Rect.Width);

			// if win11
			var gapsize = 1;
			if (Form1.isWindows11) 
				gapsize = 16;
			
			notifyicon_Rect.Inflate(0, gapsize);//16 //4

            //var notifRectClamped = TaskBarLocationFn.Clamp_Into_ScreenWorkingArea(notifyicon_Rect);

            //------------fix position for  1st show.
            var heightBefore = ctxMenuStrip.Height;
            RamLogger.Log($"ctxMenuStrip height:{ctxMenuStrip.Height}");

            //this prevents dummy App icon to be visible at Taskbar. - aka set Focus
            UnsafeNativeMethods.SetForegroundWindow(new HandleRef(ctxMenuStrip, ctxMenuStrip.Handle));
            ctxMenuStrip.Show(notifyicon_Rect.Location, ToolStripDropDownDirection.AboveLeft);
            //UnsafeNativeMethods.SetForegroundWindow(new HandleRef(ctxMenuStrip, ctxMenuStrip.Handle));


            RamLogger.Log($"ctxMenuStrip height:{ctxMenuStrip.Height}");
            
                Task.Run(() =>
                {
                    Thread.Sleep(150);
                    if (heightBefore == ctxMenuStrip.Height)
                        return;

                    ctxMenuStrip.Invoke((Action)delegate
                    {
                        //-----method1 -- risky
                        var diff = Math.Abs(ctxMenuStrip.Height - heightBefore);  //diff doesnt work
                        ctxMenuStrip.Top -= 14;

                        //-----method1 --- this is cleaner but flickers
                        ////ctxMenuStrip.AutoSize = true;
                        //ctxMenuStrip.Hide();
                        //ctxMenuStrip.Show(notifyicon_Rect.Location, ToolStripDropDownDirection.AboveLeft);
                        //UnsafeNativeMethods.SetForegroundWindow(new HandleRef(ctxMenuStrip, ctxMenuStrip.Handle));
                    });
                });


            //no problem location is ok.
            if (Debugger.IsAttached)
                DrawOnScreen.DrawRect(new Rectangle(notifyicon_Rect.Location, new Size(5, 5)));
        }

        /// <summary>
        /// ctxMenuStrip is parent.
        /// </summary>
        /// <param name="toolStripDropDown"></param>
        /// <param name="ctxMenuStrip"></param>
        public static void Show_win11Style_atlocation(this ToolStripDropDown toolStripDropDown, ContextMenuStrip ctxMenuStrip)
        {
            var parent_Rect = ctxMenuStrip.ClientRectangle;
            parent_Rect.X = (int)(parent_Rect.X + parent_Rect.Width);

            parent_Rect.Inflate(0, 16);//16 //4
            //var notifRectClamped = TaskBarLocationFn.Clamp_Into_ScreenWorkingArea(notifyicon_Rect);

            //------------fix position for  1st show.
            var heightBefore = toolStripDropDown.Height;
            RamLogger.Log($"ctxMenuStrip-DD height:{toolStripDropDown.Height}");

            //this prevents dummy App icon to be visible at Taskbar. - aka set Focus
            //UnsafeNativeMethods.SetForegroundWindow(new HandleRef(ctxMenuStrip, ctxMenuStrip.Handle));
            toolStripDropDown.Show(parent_Rect.Location);


            RamLogger.Log($"ctxMenuStrip-DD height:{toolStripDropDown.Height}");

            Task.Run(() =>
            {
                Thread.Sleep(150);
                if (heightBefore == toolStripDropDown.Height)
                    return;

                ctxMenuStrip.Invoke((Action)delegate
                {
                    //-----method1 -- risky
                    var diff = Math.Abs(toolStripDropDown.Height - heightBefore);  //diff doesnt work
                    toolStripDropDown.Top -= 14 ;

                    //-----method1 --- this is cleaner but flickers
                    ////ctxMenuStrip.AutoSize = true;
                    //ctxMenuStrip.Hide();
                    //ctxMenuStrip.Show(notifyicon_Rect.Location, ToolStripDropDownDirection.AboveLeft);
                    //UnsafeNativeMethods.SetForegroundWindow(new HandleRef(ctxMenuStrip, ctxMenuStrip.Handle));
                });
            });


            //no problem locatiion is ok.
            if (Debugger.IsAttached)
                DrawOnScreen.DrawRect(new Rectangle(parent_Rect.Location, new Size(5, 5)));
        }

    }




}
