using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


public static class DWM_RoundedCorners
{
    static DWM_RoundedCorners() { }

    /// <summary>
    /// uses dllimport DwmSetWindowAttribute
    /// </summary>
    public static void ApplyRoundCorners(this Control control , bool Round =true)
    {
        //ContextMenuStrip notifyIconMenu = new ContextMenuStrip();
        var attribute = DWM_windowAttribute.DWMWA_WINDOW_CORNER_PREFERENCE;
        var preference = DWM_windowCornerPreference.DWMWCP_ROUND;

        if (!Round)
            preference = DWM_windowCornerPreference.DWMWCP_DONOTROUND;

        DwmSetWindowAttribute(control.Handle, attribute, ref preference, sizeof(uint));
    }

    /// <summary>
    /// apply to  RoundedCorners to ContextMenuStrip and its 1st level Dropdown.
    /// </summary>
    public static void ApplyRoundCorners_toContextMenuStrip(ContextMenuStrip ctxMenuStrip)
    {
        //var submenu = new ToolStripMenuItem("submenu");
        //ctxMenuStrip.Items.Add(submenu);
        //var submenuItem1 = new ToolStripMenuItem("submenu1");
        //submenu.DropDownItems.Add(submenuItem1);

        ApplyRoundCorners(ctxMenuStrip);
        //ApplyRoundCorners(submenu.DropDown);

        foreach (var item in ctxMenuStrip.Items)
        {
            if ((item as ToolStripMenuItem) == null)
                continue;

            var tsmi = (ToolStripMenuItem)item;
            if (tsmi.DropDown.Items.Count <= 0)
                continue;

            ApplyRoundCorners(tsmi.DropDown);


        }

    }


    #region dllimport and enums

    // The enum flag for DwmSetWindowAttribute's second parameter, which tells the function what attribute to set.
    public enum DWM_windowAttribute
    {
        DWMWA_WINDOW_CORNER_PREFERENCE = 33
    }

    // The DWM_WINDOW_CORNER_PREFERENCE enum for DwmSetWindowAttribute's third parameter, which tells the function
    // what value of the enum to set.
    public enum DWM_windowCornerPreference
    {
        DWMWCP_DEFAULT = 0,
        DWMWCP_DONOTROUND = 1,
        DWMWCP_ROUND = 2,
        DWMWCP_ROUNDSMALL = 3
    }

    [DllImport("dwmapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern long DwmSetWindowAttribute(
        IntPtr hwnd,
        DWM_windowAttribute attribute,
        ref DWM_windowCornerPreference pvAttribute,
        uint cbAttribute);

    #endregion


}

