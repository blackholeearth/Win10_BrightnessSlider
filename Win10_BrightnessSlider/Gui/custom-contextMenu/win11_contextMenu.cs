using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

public static class win11_contextMenu
{

    public static void ContextMenu_set_win11_DarkStyle(this ContextMenu ctxmenu, Font menuItemFont=null)
    {
        ContextMenu_set_win11_DarkStyle(ctxmenu, menuItemFont, Color.Empty, Color.Empty);
    }

    public static void ContextMenu_set_win11_DarkStyle(this ContextMenu ctxmenu, Font menuItemFont, Color textColor, Color backColor)
    {
        if (menuItemFont != null)
            m_menuItemFont = menuItemFont;
        if (backColor != Color.Empty)
            m_backColor = backColor;
        if (textColor != Color.Empty)
            m_textColor = textColor;

        foreach (MenuItem item in ctxmenu.MenuItems)
        {
            item.OwnerDraw = true;
            item.DrawItem -= item_DrawItem;
            item.DrawItem += item_DrawItem;
            item.MeasureItem -= MeasureMenuItem;
            item.MeasureItem += MeasureMenuItem;
            
            //mouse over item
            //item.select event doesnt give mouse hovered item.
        }

    }

    static MenuItem currentHover_mi;


    static Font m_menuItemFont = new Font("Segoe UI", 11);
    static Color m_backColor = Color.FromArgb(44, 44, 44);  //SystemColors.Window;
    static Color m_textColor = Color.FromArgb(255,255,255);  // SystemColors.ControlText;
    static Color m_backColor_hovered = SystemColors.ControlText;
    static Color m_seperatorColor = Color.FromArgb(61,61,61);

    static void item_DrawItem_backup(object sender, DrawItemEventArgs e)
    {
        MenuItem cmb = sender as MenuItem;
        string color = SystemColors.Window.ToString();

        e.DrawBackground();
        e.Graphics.FillRectangle(new SolidBrush(ColorTranslator.FromHtml(color)), e.Bounds);
        e.Graphics.DrawString(color, m_menuItemFont, new SolidBrush(ColorTranslator.FromHtml(color)), e.Bounds);
    }


    static void item_DrawItem(object sender, DrawItemEventArgs e)
    {
        MenuItem mi = sender as MenuItem;

        if (mi.Text == "-")
        {
            //drawBG
            e.Graphics.FillRectangle(new SolidBrush(m_backColor), e.Bounds);

            //e.Graphics.DrawRectangle(  new Pen(m_seperatorColor), e.Bounds); //draw thick line

            var lineBounds = e.Bounds;
            lineBounds.Inflate(0, -(int)Math.Ceiling(seperatorHeight/2.0) );
            lineBounds.Height = 1;
            e.Graphics.DrawRectangle(  new Pen(m_seperatorColor), lineBounds); //draw thick line
            
            return;
        }

        if (mi == currentHover_mi)
            e.DrawFocusRectangle();
        else
        {
            //e.DrawBackground();
            e.Graphics.FillRectangle(new SolidBrush(m_backColor), e.Bounds);
        }

        StringFormat sf1 = new StringFormat();
        //sf1.Alignment = StringAlignment.Center;      // Horizontal Alignment
        sf1.LineAlignment = StringAlignment.Center;  // Vertical Alignment

        // leave space for CheckBox.
        var eb = e.Bounds;
        var mi_DrawText_Rect = new Rectangle(eb.Left  + textStartLeft, eb.Top, eb.Width- textStartLeft, eb.Height);

        e.Graphics.DrawString(mi.Text, m_menuItemFont, new SolidBrush(m_textColor), mi_DrawText_Rect, sf1);
    }


    static int pad_left = 5;
    static int pad_right => pad_left ;
    static int checkmark_width = 49;
    static int textStartLeft => pad_left + checkmark_width;

    //static int item_text_height = 13;
    static int item_top_pad = 6; //12.5
    static int item_bottom_pad => item_top_pad;

    static int seperatorHeight => 7;
    static void MeasureMenuItem(object sender, MeasureItemEventArgs e)
    {
        MenuItem mi = (MenuItem)sender;

        if (mi.Text == "-")
        {
            e.ItemHeight = seperatorHeight;//33;
            e.ItemWidth =  -1;
            return;
        }
        Font font = m_menuItemFont;
        SizeF textSize = e.Graphics.MeasureString(mi.Text, font);
        e.ItemHeight =
            +item_top_pad
            + (int)textSize.Height
            + item_bottom_pad
            ;


        e.ItemWidth = 
            pad_left + checkmark_width
            + (int)textSize.Width
            + pad_right
            ;

       
    }


}
