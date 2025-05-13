using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Win10_BrightnessSlider
{

    public class win11_MenuRenderer : ToolStripProfessionalRenderer
    {
        private int arrowThickness = 3; //2;

        public CMS_colors cmsColors;
        public Color _checkMark_color => cmsColors.textColor; //Color.SkyBlue;
        public Color _textColor => cmsColors.textColor;
        public Color _seperatorColor => cmsColors.seperatorColor;

        //if above 30 text gets clipped.
        int item_TextOffset => cmsColors.item_TextOffset;
        /// increase item Area Height - not the Fontsize
        int item_verticalPadding => cmsColors.item_verticalPadding;  // 14 -- win11 1080p 125%  near.


        public win11_MenuRenderer(ProfessionalColorTable colorTable, CMS_colors _cmsColors)
            : base(colorTable)
        {
            cmsColors = _cmsColors;
        }


        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            base.OnRenderMenuItemBackground(e);
        }

        readonly TextFormatFlags flags = TextFormatFlags.LeftAndRightPadding | TextFormatFlags.VerticalCenter;

        //override
        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Text))
                return;

            //set-FontSize
            //e.Item.Font = cmsColors.menuItemFont;
            //e.TextFont = cmsColors.menuItemFont;

            // The size of the Item is provided here
            e.Item.AutoSize = false;

            // An Item's viewport is 3 pixels higher than the text's Height
            var textSize = TextRenderer.MeasureText(
            e.Graphics, e.Text, e.TextFont, new Size(e.TextRectangle.Width, int.MaxValue), flags
            );
            var textHeight = textSize.Height + 3 - 3 + item_verticalPadding;

            //var textRect = new Rectangle(e.TextRectangle.Left, e.TextRectangle.Top, e.TextRectangle.Width, textHeight);
            var textRect = new Rectangle(e.TextRectangle.Left, e.TextRectangle.Top, e.TextRectangle.Width, textHeight);


            // Bounds of the Item's text
            textRect.Offset(item_TextOffset, 0);
            e.TextRectangle = textRect;
            // Overall height of the MenuItem: text's viewport.Height + 3 pixels
            e.Item.Height = textHeight + 3;


            base.OnRenderItemText(e);
        }


        //protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        //{
        //    //base.OnRenderItemText(e);
        //    //hovering item - text color
        //    e.Item.ForeColor = e.Item.Selected ? _textColor : _textColor;  //Color.Green : textColor;
        //    e.Item.Font = cmsColors.m_menuItemFont;
        //}


        Bitmap ImageCheck_sizer = new Bitmap(55, 1);
        protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e)
        {
            //base.OnRenderItemCheck(e);
            //return;


            if (e.Item is ToolStripMenuItem item && item.Checked)
            {
                //Rectangle rect = e.Item.ContentRectangle;
                Rectangle rect = e.ImageRectangle;
                //rect.Width += ;  //0; //tset
                rect.X += item_TextOffset;
                int offset = 2;

                Pen pen = new Pen(_checkMark_color, 2f);

                Point p1 = new Point(rect.Left + offset, rect.Bottom - offset * 3);
                Point p2 = new Point(rect.Left + offset * 3, rect.Bottom - offset);
                Point p3 = new Point(rect.Right - offset * 2, rect.Top + offset);
                var path = new GraphicsPath();
                path.AddLine(p1, p2);
                path.AddLine(p2, p3);
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.DrawPath(pen, path);



            }
        }


        protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
        {
            //base.OnRenderSeparator(e);
            var g = e.Graphics;
            Rectangle rect = e.Item.ContentRectangle;

            int middle = rect.Top + rect.Height / 2;

            Pen pen = new Pen(_seperatorColor, 1);
            g.DrawLine(pen, rect.Left + 3, middle, rect.Right - 3, middle);


        }

        protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
        {
            //Fields
            var graph = e.Graphics;
            var arrowSize = new Size(5, 12);
            var arrowColor =  Color.White; //e.Item.Selected ?  Color.Cyan : Color.White ;
            var rect = new Rectangle(e.ArrowRectangle.Location.X, (e.ArrowRectangle.Height - arrowSize.Height) / 2,
                arrowSize.Width, arrowSize.Height);
            using (GraphicsPath path = new GraphicsPath())
            using (Pen pen = new Pen(arrowColor, arrowThickness))
            {
                //Drawing
                graph.SmoothingMode = SmoothingMode.AntiAlias;
                int rect_Tmiddle = rect.Top + rect.Height / 2;
                path.AddLine(rect.Left, rect.Top, rect.Right, rect_Tmiddle);
                path.AddLine(rect.Right, rect_Tmiddle, rect.Left, rect.Top + rect.Height);
                graph.DrawPath(pen, path);
            }
        }


        //renderer Calc - not so stable
        public int Calculate_ItemHeights(ContextMenuStrip menu)
        {

            if (menu == null || menu as ContextMenuStrip_win11 == null || cmsColors.menuItemFont == null)
                return -1;

            int CalcHeight = -1;
            foreach (ToolStripItem item in menu.Items)
            {
                if (item is ToolStripSeparator)
                {
                    CalcHeight += item.Height;

                    continue;
                }
                //if (string.IsNullOrEmpty(item.Text))
                //    continue;

                // An Item's viewport is 3 pixels higher than the text's Height
                var textSize = TextRenderer.MeasureText(menu.CreateGraphics(),
                    item.Text,
                    cmsColors.menuItemFont,
                    new Size(item.Width, int.MaxValue),
                    flags);

                var textHeight = textSize.Height + 3 - 3 + item_verticalPadding;
                var item_Height = textHeight + 3;

                CalcHeight += item_Height;
            }

            return CalcHeight + 7;

        }
        public static Point Resize(/*this*/ Point point, double resizeFactor)
        {
            return new Point((int)(point.X * resizeFactor), (int)(point.Y * resizeFactor));
        }


    }





}
