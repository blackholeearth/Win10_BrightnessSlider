namespace winforms_SDR_test.scrollTEST
{
	using System;
	using System.Windows.Forms;
	using System.Diagnostics;
	using System.Drawing;
	using System.Runtime.InteropServices;

	public partial class OverlayForm : Form
	{
		// Custom event to notify the main form about scrolling
		public event EventHandler<MouseEventArgs> OverlayScrolled;

		public OverlayForm()
		{
			//InitializeComponent();

			// Additional setup if not done in designer (redundancy is fine)
			this.FormBorderStyle = FormBorderStyle.None;
			this.ShowInTaskbar = false;
			this.StartPosition = FormStartPosition.Manual;
			this.Opacity = 0.01; // Near invisible
			this.Opacity = 0.5; // Near invisible
								 //this.BackColor = Color.Fuchsia; // Example if using TransparencyKey
								 //this.TransparencyKey = Color.Fuchsia; // Match BackColor if using this method
			this.TopMost = true;
			this.Size = new System.Drawing.Size(32, 32); // Small size

			// Attach the mouse wheel handler
			this.MouseWheel += OverlayForm_MouseWheel;
			// Optional: Handler for when mouse leaves THIS form
			this.MouseLeave += OverlayForm_MouseLeave;
		}

		// Crucial: Prevent this window from stealing focus when shown
		protected override bool ShowWithoutActivation => true;

		private void OverlayForm_MouseWheel(object sender, MouseEventArgs e)
		{
			Debug.WriteLine($"--- OverlayForm MouseWheel! Delta: {e.Delta}");
			// Raise the custom event, passing the original arguments
			OverlayScrolled?.Invoke(this, e);
		}

		private void OverlayForm_MouseLeave(object sender, EventArgs e)
		{
			Debug.WriteLine("--- Mouse Left OverlayForm ---");
			// Optionally, you could immediately hide here, but using a timer
			// in the main form is usually safer to handle brief exits/re-entries.
			// this.Hide(); // Or let the main form's timer handle hiding
		}

		// Optional: Add debug output on Show/Hide if needed
		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);
			Debug.WriteLine($"OverlayForm Visible: {this.Visible}");
		}















	}
}