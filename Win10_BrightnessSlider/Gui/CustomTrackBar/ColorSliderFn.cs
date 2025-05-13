using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Win10_BrightnessSlider.Properties;
using Win10_BrightnessSlider.Monitor.Events;


namespace Win10_BrightnessSlider
{
    public class CS_Colors 
    {
        public Color knobColor ;
        public Color barElapsedColor ;
        public Color barColor;
        public Color bar_BorderColor;
    }


    public static class ColorSliderFn
    {
        public static ColorSlider New_ColorSlider() => init_ColorSlider(new ColorSlider());

        public static ColorSlider init_ColorSlider(this ColorSlider cslider)
        {
            //importan vals
            cslider.LargeChange = 5;
            cslider.SmallChange = 5;
            //cslider.MouseWheelBarPartitions = 5; //partition so archaic. opposite or Steps. disabled it in the override msWheel
            cslider.Maximum = 100;
            cslider.Minimum = 0;

            //colors---
            cslider.ForeColor = Color.White;
            cslider.BackColor = Color.FromArgb(70, 77, 95);

            cslider.BarInnerColor = Color.FromArgb(65, 65, 65);
            cslider.BarOuterColor = Color.FromArgb(65, 65, 65);
            cslider.ElapsedInnerColor = Color.Silver;
            cslider.ElapsedOuterColor = Color.FromArgb(224, 224, 224);

            cslider.BorderRoundRectSize = new Size(8, 8);

            cslider.ThumbInnerColor = Color.FromArgb(21, 56, 152);
            cslider.ThumbInner2Color = Color.FromArgb(21, 56, 152);
            cslider.ThumbPenColor = Color.FromArgb(11, 46, 142);

            //knob Style
            cslider.ThumbRoundRectSize = new Size(16, 16);
            cslider.ThumbSize = new Size(16, 16);



            //unnecessary  visuals  - but shoud be set like this.
            cslider.Margin = new Padding(4);
            cslider.DrawSemitransparentThumb = false;

            cslider.ScaleDivisions = 1;
            cslider.ScaleSubDivisions = 1;
            cslider.ShowDivisionsText = false;
            cslider.ShowSmallScale = false;
            cslider.TickAdd = 0F;
            cslider.TickColor = Color.White;
            cslider.TickDivide = 0F;
            cslider.TickStyle = TickStyle.None;

            return cslider;
        }


        public static ColorSlider setStyle_win11_fluent_LightColor(this ColorSlider cslider)
        {

            cslider = ColorSliderFn.init_ColorSlider(cslider);
            cslider.ThumbRoundRectSize = new System.Drawing.Size(8, 8);
            cslider.ThumbSize = new System.Drawing.Size(12, 25);
            //cslider.BackColor = BackColor1;

            Color thumbColor = Color.FromArgb(0, 103, 192); // win11 light 
            Color thumbBorderColor = Color.FromArgb(69, 69, 69);  // 

            Color barBackColor = Color.FromArgb(134, 134, 134); // win11
            Color barElapsedColor = Color.FromArgb(0, 103, 192); // win11

            cslider.ThumbInnerColor = thumbColor;
            cslider.ThumbInner2Color = thumbColor;
            cslider.ThumbPenColor = thumbBorderColor;

            cslider.BarInnerColor = barBackColor;
            cslider.BarOuterColor = barBackColor;

            cslider.ElapsedInnerColor = barElapsedColor;
            cslider.ElapsedOuterColor = barElapsedColor;

            return cslider;
        }
        public static ColorSlider setStyle_win10_trackbarColor(this ColorSlider cslider , CS_Colors colors)
        {
            cslider = ColorSliderFn.init_ColorSlider(cslider);
            cslider.ThumbRoundRectSize = new System.Drawing.Size(8, 8);
            cslider.ThumbSize = new System.Drawing.Size(12, 28);
            //cslider.BackColor = BackColor1;

         
            //trackbarEX
            var barColor = Color.FromArgb(231, 234, 234); //original win10   -win11: 134,134,134
          
            var bar_BorderColor = Color.FromArgb(218, 218, 218); //original win10 

            //input
            var knobColor = Color.FromArgb(0, 120, 215); //original win10 blue
            var knobBorderColor = Color.FromArgb(69, 69, 69);   
            
            //input--end;
            var barElapsedColor = knobColor; // win11

            //---customcolors
            if (colors != null)
            {
                knobColor = colors.knobColor;
                barElapsedColor = colors.barElapsedColor;
                barColor = colors.barColor;
                bar_BorderColor = colors.bar_BorderColor;
            }


            cslider.ThumbInnerColor = knobColor;
            cslider.ThumbInner2Color = knobColor;
            cslider.ThumbPenColor = knobBorderColor;

            cslider.BarInnerColor = barColor;
            cslider.BarOuterColor = barColor;

            cslider.ElapsedInnerColor = barElapsedColor;
            cslider.ElapsedOuterColor = barElapsedColor;

            return cslider;
        }

        /// <summary>
        /// Light/Dark Theme Aware  + custom Theme aware. 
        /// </summary>
        /// <param name="cslider"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static ColorSlider setStyle_win10_trackbarColor_v2(this ColorSlider cslider, Settings settings = null)
        {
            var colors = new CS_Colors();
            var isLight = TaskBarUtil.isTaskbarColor_Light();

            //---customcolors
            if (settings?.CustomTheme_Enabled ?? false)
            {
                var cusThm = settings.customTheme;

                colors.knobColor = cusThm.knobColor;
                colors.barElapsedColor = cusThm.knobColor;
                colors.barColor = cusThm.barRemainingColor;
                colors.bar_BorderColor = cusThm.barBorderColor;
            }
            else if (isLight)
            {

                colors.knobColor = Color.FromArgb(0, 120, 215); //win10 blue
                colors.barElapsedColor = colors.knobColor;
                colors.barColor = Color.FromArgb(134, 134, 134); // win11 light-theme
                //colors.bar_BorderColor = ;
            }
            else
            {
                colors = null;;
            }



            cslider = setStyle_win10_trackbarColor(cslider , colors);


            
            //var knobColor = Color.FromArgb(0, 120, 215); //win10 blue
            //var barColor = Color.FromArgb(134, 134, 134); // win11 light-theme

            //Color barElapsedColor = knobColor; // win11

            //cslider.ElapsedInnerColor = barElapsedColor;
            //cslider.ElapsedOuterColor = barElapsedColor;

            //cslider.BarInnerColor = barColor;
            //cslider.BarOuterColor = barColor;

            return cslider;
        }



    }










}
