using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms; // For Timer, not for drawing on taskbar

public static class DpiConverter
{
	// P/Invoke for GetDpiForWindow. Requires Windows 10 Creators Update or later.
	[DllImport("user32.dll")]
	private static extern uint GetDpiForWindow(IntPtr hWnd);

	// The default DPI value if the API is not available.
	private const uint DEFAULT_DPI = 96;

	/// <summary>
	/// Gets the DPI scaling factor for a specific window.
	/// </summary>
	/// <param name="hWnd">The handle of the window to check.</param>
	/// <returns>The scaling factor (e.g., 1.0 for 100%, 1.5 for 150%).</returns>
	private static double GetScalingFactor(IntPtr hWnd)
	{
		// GetDpiForWindow is the most reliable way to get the DPI.
		// It's aware of per-monitor DPI settings.
		uint dpi = GetDpiForWindow(hWnd);

		// A DPI of 0 indicates an error or an invalid window handle.
		if (dpi == 0)
		{
			// Fallback for older systems or if the handle is invalid (e.g., desktop)
			// We can use the primary screen's Graphics object to find the system DPI.
			using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
			{
				dpi = (uint)g.DpiX;
			}
		}

		// The scaling factor is the current DPI divided by the standard DPI (96).
		return dpi / (double)DEFAULT_DPI;
	}

	/// <summary>
	/// Converts a Rectangle in logical (DPI-scaled) coordinates to physical
	/// coordinates usable by a screen HDC.
	/// </summary>
	/// <param name="logicalRect">The rectangle from GetWindowRect or Screen.Bounds.</param>
	/// <param name="windowHandle">The handle of the window the rectangle relates to. Use this.Handle.</param>
	/// <returns>A new Rectangle with physical pixel coordinates.</returns>
	public static Rectangle ToHdcUsableRect(Rectangle logicalRect, IntPtr windowHandle)
	{
		double scale = GetScalingFactor(windowHandle);

		return new Rectangle(
			(int)(logicalRect.Left * scale),
			(int)(logicalRect.Top * scale),
			(int)(logicalRect.Width * scale),
			(int)(logicalRect.Height * scale)
		);
	}

	/// <summary>
	/// Overload for converting the bounds of a Control directly.
	/// </summary>
	public static Rectangle ToHdcUsableRect(Control control)
	{
		if (control == null) throw new ArgumentNullException(nameof(control));

		// Control.Bounds is relative to its parent. We need the screen coordinates.
		Rectangle logicalRectOnScreen = control.RectangleToScreen(control.ClientRectangle);

		return ToHdcUsableRect(logicalRectOnScreen, control.Handle);
	}
}