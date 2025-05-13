using System.Windows.Forms;
using System.Drawing;
using System;

namespace winformsTests._4_contextMenuStrip
{
	// A minimal, invisible form to own the ContextMenuStrip
	public class MenuOwnerForm : Form
	{
		public MenuOwnerForm()
		{
			// Basic properties to make it invisible and non-interactive
			this.FormBorderStyle = FormBorderStyle.None;
			this.ShowInTaskbar = false;
			this.StartPosition = FormStartPosition.Manual;
			this.Location = new Point(-2000, -2000); // Position off-screen
			this.Size = new Size(1, 1);
			this.Opacity = 0;
			this.ShowIcon = false;
			this.WindowState = FormWindowState.Normal; // Needs to be normal to create handle

			// Ensure the handle is created when the form is instantiated
			// but DON'T show the window. Accessing the Handle property forces creation.
			IntPtr handle = this.Handle;
			// Or alternatively:
			// if (!this.IsHandleCreated) { this.CreateControl(); }

		}

		// Prevent it from ever gaining focus or being activated
		protected override bool ShowWithoutActivation => true;

		// Override CreateParams for good measure, although ShowInTaskbar=false should handle it
		protected override CreateParams CreateParams
		{
			get
			{
				CreateParams cp = base.CreateParams;
				cp.ExStyle |= 0x08000000; // WS_EX_NOACTIVATE
				cp.ExStyle &= ~0x40000; // ~WS_EX_APPWINDOW
				return cp;
			}
		}
	}
}