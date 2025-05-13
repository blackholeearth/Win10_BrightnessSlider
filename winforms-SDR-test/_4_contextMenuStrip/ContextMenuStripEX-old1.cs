using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace winformsTests._4_contextMenuStrip.old1 // Use your namespace
{
	public class ContextMenuStripEX : ContextMenuStrip  //backuppp
	{
		// Padding can be configured here if desired
		private int workingArea_padding_width = 0;
		private int workingArea_padding_height = 8; // Example padding

		/*
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(HandleRef hWnd);
        */

		// Constructor needed for designer support if you add it via designer
		public ContextMenuStripEX(IContainer container) : base(container) { }

		public ContextMenuStripEX() : base() { }

		/// <summary>
		/// Calculates the 'Win11 Style' position (above cursor, clamped to padded working area)
		/// based on the provided click point (usually Cursor.Position).
		/// </summary>
		/// <param name="clickPoint">The screen coordinates where the user clicked.</param>
		/// <returns>The calculated Point where the menu should be shown.</returns>
		private Point CalculateWin11StylePosition(Point clickPoint)
		{
			Screen currentScreen = Screen.FromPoint(clickPoint);
			Rectangle workingArea = currentScreen.WorkingArea;

			// Apply padding
			workingArea.Inflate(-workingArea_padding_width, -workingArea_padding_height);

			Size menuSize = this.GetPreferredSize(Size.Empty);

			// Calculate initial desired position
			int menuX = clickPoint.X - menuSize.Width / 2;
			int menuY = clickPoint.Y - menuSize.Height; // Directly above click point initially

			// Clamp X
			if (menuX < workingArea.Left) menuX = workingArea.Left;
			else if (menuX + menuSize.Width > workingArea.Right) menuX = workingArea.Right - menuSize.Width;

			// Clamp Y (prioritize showing above)
			if (menuY < workingArea.Top) // If menu goes above top edge
			{
				menuY = clickPoint.Y; // Try showing below cursor
				if (menuY + menuSize.Height > workingArea.Bottom) // If still doesn't fit below
				{
					menuY = workingArea.Bottom - menuSize.Height; // Align to bottom
				}
				if (menuY < workingArea.Top) menuY = workingArea.Top; // Final clamp top
			}
			else if (menuY + menuSize.Height > workingArea.Bottom) // If menu goes below bottom edge (when placed above)
			{
				menuY = workingArea.Bottom - menuSize.Height; // Align to bottom
				if (menuY < workingArea.Top) menuY = workingArea.Top; // Final clamp top
			}

			return new Point(menuX, menuY);
		}

		/// <summary>
		/// Shows the context menu using the calculated 'Win11 Style' position.
		/// </summary>
		/// <param name="clickPoint">The screen coordinates where the user clicked (usually Cursor.Position).</param>
		public void ShowWin11Style(Point clickPoint)
		{
			Point finalLocation = CalculateWin11StylePosition(clickPoint);

			// Call the base Show method with the calculated screen coordinates
			base.Show(finalLocation);

			// Optional: Call SetForegroundWindow here if preferred over calling it in NotifyIcon_v2
			/*
            if (this.IsHandleCreated)
            {
                 SetForegroundWindow(new HandleRef(this, this.Handle));
            }
            */
		}

		// You can add other overrides or custom methods here if needed


	 

	}
}











 