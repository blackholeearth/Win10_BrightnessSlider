using System;
using System.Diagnostics; // Added for Debugger check
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Win10_BrightnessSlider.Gui
{
	public class ThemedUserControl : UserControl
	{
		// RESTORED: This flag lets you disable the border for embedded controls
		public bool DrawFrame_isEnabled { get; set; } = true;

		public Color FrameColor { get; set; } = Color.FromArgb(63, 63, 63);

		public bool IsFirstItem { get; set; }
		public bool IsLastItem { get; set; }

		public ThemedUserControl()
		{
			this.DoubleBuffered = true;
			this.Resize += (s, e) => this.Invalidate();
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			// This prevents the "Double Border" inside your wButton wrapper.
			if (!DrawFrame_isEnabled)
				return;


			var rect = this.ClientRectangle;
			//rect.Width -= 1;
			//rect.Height -= 1;
			// --- FIX: Subtract 2 to ensure the 1px border is fully inside the canvas ---
			rect.Width -= 2;
			rect.Height -= 2;

			Pen penToUse = new Pen(this.FrameColor, 1);
			
			if (Debugger.IsAttached) {
				penToUse = new Pen(Color.Orange, 1)
				{
					DashStyle = DashStyle.Custom,
					DashPattern = new float[] { 10, 10 }
				};
			}

			using (penToUse)
			{
				if (Form1.isWindows11)
				{
					DrawRoundedBorders(e.Graphics, penToUse, rect);
				}
				else
				{
					// Standard Win10 Lines
					e.Graphics.DrawLine(penToUse, 0, 0, rect.Width, 0);
					e.Graphics.DrawLine(penToUse, 0, 0, 0, rect.Height);
				}
			}
		}

		private void DrawRoundedBorders(Graphics g, Pen pen, Rectangle rect)
		{
			int radius = 8;
			g.SmoothingMode = SmoothingMode.AntiAlias;

			GraphicsPath path;

			if (IsFirstItem && IsLastItem)
				path = DrawingFn.RoundedRect(rect, radius);
			else if (IsFirstItem)
				path = DrawingFn.RoundedRect(rect, radius, 0);
			else if (IsLastItem)
				path = DrawingFn.RoundedRect(rect, 0, radius);
			else
				path = DrawingFn.RoundedRect(rect, 0, 0);

			g.DrawPath(pen, path);
			path.Dispose();
		}


	}
}