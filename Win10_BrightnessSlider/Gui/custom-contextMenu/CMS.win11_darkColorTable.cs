using System.Drawing;
using System.Windows.Forms;

namespace Win10_BrightnessSlider
{

    public class win11_darkColorTable : ProfessionalColorTable
    {
        internal CMS_colors cmsColors;
        //Fields
        //private Color backColor;
        private Color leftColumnColor;
        //private Color borderColor;
        //private Color menuItemBorderColor;
        //private Color menuItemSelectedColor;

        public win11_darkColorTable(CMS_colors cms_colors)
        {
            UseSystemColors = false;

            cmsColors = cms_colors;

            //backColor = colors.backColor;
            leftColumnColor = cmsColors.backColor;
            //borderColor = colors.backColor;
            //menuItemBorderColor = colors.backColor_hovered; //primaryColor;
            //menuItemSelectedColor = colors.backColor_hovered;
        }



        //Overrides
        public override Color ToolStripDropDownBackground => cmsColors.backColor;

        public override Color MenuBorder => cmsColors.seperatorColor; //cmsColors.backColor_hovered;//colors.backColor;
        public override Color MenuItemBorder => cmsColors.backColor_hovered;//primaryColor;

        //override item hover colors
        public override Color MenuItemSelected => cmsColors.backColor_hovered;
        public override Color MenuItemSelectedGradientBegin => cmsColors.backColor_hovered;
        public override Color MenuItemSelectedGradientEnd => cmsColors.backColor_hovered;

        //override Check mark back color
        public override Color CheckBackground => cmsColors.backColor;
        public override Color CheckSelectedBackground => cmsColors.backColor;
        public override Color CheckPressedBackground => cmsColors.backColor;


        public override Color ImageMarginGradientBegin => leftColumnColor;
        public override Color ImageMarginGradientMiddle => leftColumnColor;
        public override Color ImageMarginGradientEnd => leftColumnColor;


        public override Color MenuStripGradientBegin => Color.Cyan;
        public override Color MenuStripGradientEnd => Color.DarkCyan;



    }



}
