using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

public static class NoDesigner { }

public class ContextMenuStrip_win11 : ContextMenuStrip
{
    //https://github.com/dotnet/winforms/issues/9258
    //ContextMenuStrip doesn't scale well for its menu item's checked icon. #9258
    private new void RescaleConstantsForDpi(int deviceDpiOld, int deviceDpiNew)
    {
        // Use reflection to invoke the internal ResetScaling method
        var resetScalingMethod = typeof(System.Windows.Forms.ContextMenuStrip).GetMethod("ResetScaling", BindingFlags.NonPublic | BindingFlags.Instance);
        if (resetScalingMethod != null)
        {
            resetScalingMethod.Invoke(this, new object[] { deviceDpiNew });
        }
    }
    void ctor_common()
    {
        //this is not needed /used
        //RescaleConstantsForDpi(96, DeviceDpi); //device dpi is 96 - its reported wrong here?? - 
        this.ApplyRoundCorners();
    }

    public ContextMenuStrip_win11(IContainer container) : base(container)
    {
        //RescaleConstantsForDpi(96, DeviceDpi); //device dpi is 96 - its reported wrong here?? - 
        ctor_common();
    }
    public ContextMenuStrip_win11()
    {
        //RescaleConstantsForDpi(96, DeviceDpi);
        ctor_common();
    }

    //Better autoSize for Custom Width Height İtems -not so good.
    private void Calculate_And_Set_CMS_Height_Width()
    {
        AutoSize = false;
        //Width = 400;
        //Height = 495;

        //  find Better way to get  checkmark_Size
        int checkMarkSize = 70;
        int width_padding = 20 + checkMarkSize;
        int longestTextWidth = 0;

        int calc_itemHeight = 0;
        int itemHeight_padding = 6 + 3 + 1;

        //calculation 
        Graphics g = this.CreateGraphics();
        //foreach (ToolStripItem item in this.Items){
        //    if (item is ToolStripMenuItem menuItem){
        //        menuItem.AutoSize = false;
        //    }
        //}

        var ts_menuItems = this.Items.Cast<ToolStripItem>().OfType<ToolStripMenuItem>().ToList();
        var ts_Seperator = this.Items.OfType<ToolStripSeparator>().ToList();
        foreach (ToolStripItem menuItem in ts_menuItems)
        {
            menuItem.AutoSize = false;
            var txtSize_calc = g.MeasureString(menuItem.Text, menuItem.Font);
            longestTextWidth = Math.Max(longestTextWidth, (int)txtSize_calc.Width + width_padding);

            calc_itemHeight = (int)txtSize_calc.Height + itemHeight_padding;
        }
        //set all items width 
        foreach (ToolStripItem menuItem in ts_menuItems)
        {
            menuItem.Width = longestTextWidth;
            menuItem.Height = calc_itemHeight;
            menuItem.Padding = new Padding(90, 0, 0, 0);

        }
        Width = longestTextWidth;


        //set all items width 
        int calcHeight = 0;
        foreach (ToolStripItem menuItem in ts_menuItems)
        {
            calcHeight += menuItem.Height;
        }
        foreach (ToolStripSeparator sep in ts_Seperator)
        {
            calcHeight += sep.Height - 4;
        }
        Height = calcHeight;

    }
    protected override void OnOpening(CancelEventArgs e)
    {
        base.OnOpening(e);

        //test
        //Bitmap Bmp = new Bitmap(155, 5);
        //using (var gfx = Graphics.FromImage(Bmp))
        //using (var brush = new SolidBrush(Color.Green))
        //{
        //    gfx.FillRectangle(brush, 0, 0, Bmp.Width, Bmp.Height);
        //}

        //var ts_menuItems = this.Items.Cast<ToolStripItem>().OfType<ToolStripMenuItem>().ToList();
        //var ts_Seperator = this.Items.OfType<ToolStripSeparator>().ToList();

        //var tsmix = ts_menuItems.FirstOrDefault();
        //if (tsmix != null)
        //    tsmix.Image = Bmp;

        //    tsmix.Checked = true;



    }


    protected override void OnItemAdded(ToolStripItemEventArgs e)
    {
        base.OnItemAdded(e);

        //need to add padding to right. easiest way is to add -"white space to end"
        e.Item.Text += new string(' ', 6);
    }



}




