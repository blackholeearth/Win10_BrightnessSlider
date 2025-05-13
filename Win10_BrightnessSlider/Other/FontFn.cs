using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;
using System.Windows.Forms;



public static class ControlFn
{
    


    /// <summary>
    /// Sets the font of the control to bold (and optionally back to normal) if specified, keeping other font properties.
    /// </summary>
    /// <param name="control">The control to modify.</param>
    /// <param name="isBold">True for bold, false for normal. If not specified will be set to bold</param>
    public static void SetFontBold(this Control control, bool isBold = true)
    {
        if (control == null || control.Font == null)
            return;

        if (isBold)
        {
            control.Font = new Font(control.Font, control.Font.Style | FontStyle.Bold);
        }
        else
        {
            control.Font = new Font(control.Font, control.Font.Style & ~FontStyle.Bold);
        }
    }


    /// <summary>
    /// returns New Font as Bold , Sets the font of the to bold (and optionally back to normal) if specified, keeping other font properties.
    /// </summary>
    /// <param name="isBold">True for bold, false for normal. If not specified will be set to bold</param>
    public static Font SetBold(this Font font, bool isBold = true)
    {
        if (font == null)
            return font;

        if (isBold)
            return new Font(font, font.Style | FontStyle.Bold);
        else
            return new Font(font, font.Style & ~FontStyle.Bold);

    }


}

public static class MathFn
{
    public static int Clamp(int value, int min, int max)
    {
        if (value < min)
            return min;

        if (value > max)
            return max;

        return value;
    }
}

public static class ColorFn
{
    /// <summary>
    /// Creates a new color by increasing the R, G, and B values of the given color by the specified value.
    /// </summary>
    /// <param name="color">The original color.</param>
    /// <param name="value">The value to add to each R, G, and B component. Can be positive or negative.</param>
    /// <returns>A new color with increased RGB values, or the original color if the operation would result in invalid color values.</returns>
    public static Color AddValue(this Color color, int value)
    {
        int newR = MathFn.Clamp(color.R + value, 0, 255);
        int newG = MathFn.Clamp(color.G + value, 0, 255);
        int newB = MathFn.Clamp(color.B + value, 0, 255);
        return Color.FromArgb(color.A, newR, newG, newB);
    }


    /// <summary>
    /// Creates a new color by increasing the R, G, and B values of the given color by the specified value.
    /// </summary>
    /// <param name="color">The original color.</param>
    /// <param name="redValue">The value to add to the R component. Can be positive or negative.</param>
    /// <param name="greenValue">The value to add to the G component. Can be positive or negative.</param>
    /// <param name="blueValue">The value to add to the B component. Can be positive or negative.</param>
    /// <returns>A new color with increased RGB values, or the original color if the operation would result in invalid color values.</returns>
    public static Color AddValue(this Color color, int redValue, int greenValue, int blueValue)
    {
        int newR = MathFn.Clamp(color.R + redValue, 0, 255);
        int newG = MathFn.Clamp(color.G + greenValue, 0, 255);
        int newB = MathFn.Clamp(color.B + blueValue, 0, 255);
        return Color.FromArgb(color.A, newR, newG, newB);
    }


}