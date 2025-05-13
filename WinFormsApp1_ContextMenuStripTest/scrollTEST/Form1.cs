using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static winforms_SDR_test.scrollTEST.OverlayForm;

namespace winforms_SDR_test.scrollTEST
{
	public partial class Form1 : Form
	{
		private NotifyIcon notifyIcon1;
		// Remove the old hook if you still have it
		// private TrayMouseHook _trayHook;
		private OverlayForm _overlayForm;
		private System.Windows.Forms.Timer _hideOverlayTimer;
		private bool _isMouseOverIcon = false; // Track if mouse is logically over icon area

		public Form1()
		{
			InitializeComponent();

			// --- Initialize Overlay Form ---
			_overlayForm = new OverlayForm();
			_overlayForm.OverlayScrolled += OverlayForm_OverlayScrolled; // Subscribe to its event

			// --- Initialize Hide Timer ---
			_hideOverlayTimer = new System.Windows.Forms.Timer();
			_hideOverlayTimer.Interval = 300; // ms - How long to wait before hiding after mouse stops moving over icon
			_hideOverlayTimer.Tick += HideOverlayTimer_Tick;

			// --- Initialize NotifyIcon ---
			notifyIcon1 = new NotifyIcon();
			notifyIcon1.Icon = SystemIcons.Information; // Use your icon
			notifyIcon1.Text = "My App - Scroll Here!";
			notifyIcon1.Visible = true;
			notifyIcon1.MouseMove += NotifyIcon_MouseMove;
			// Optional: Detect mouse leaving the general vicinity is harder.
			// We rely on the timer and the overlay's MouseLeave.
			// notifyIcon1.MouseLeave isn't reliable for this.

			// Remove hook initialization if present
			// _trayHook = new TrayMouseHook(this);
			// _trayHook.TrayMouseWheel += TrayHook_TrayMouseWheel;
			// _trayHook.InstallHook();
		}

		// --- NotifyIcon Mouse Move: The Trigger ---
		private void NotifyIcon_MouseMove__old(object sender, MouseEventArgs e)
		{
			// Get current SCREEN coordinates of the mouse
			Point mousePos = Cursor.Position;

			if (!_isMouseOverIcon)
			{
				Debug.WriteLine("Mouse Entered NotifyIcon Area - Showing Overlay");
				_isMouseOverIcon = true;

				// Position the overlay centered roughly on the cursor
				// Adjust offset as needed if (0,0) isn't top-left of icon visually
				_overlayForm.Location = new Point(mousePos.X - _overlayForm.Width / 2,
												  mousePos.Y - _overlayForm.Height / 2);

				// Show the nearly invisible form without activating it
				_overlayForm.Show();
				// Optional: Bring to front just in case, though TopMost should handle it
				// _overlayForm.BringToFront();
			}

			// --- Reset the hide timer every time the mouse moves OVER the icon ---
			// This keeps the overlay visible as long as there's activity
			_hideOverlayTimer.Stop();
			_hideOverlayTimer.Start();
		}

		// --- NotifyIcon Mouse Move: The Trigger ---
		private void NotifyIcon_MouseMove(object sender, MouseEventArgs e)
		{
			// Only try to show/position if the mouse *just* entered the conceptual area
			if (!_isMouseOverIcon)
			{
				Debug.WriteLine("Mouse Entered NotifyIcon Area - Attempting to get fixed Rect...");
				_isMouseOverIcon = true; // Mark that we are now "over" the icon area

				// --- Attempt to get the icon's actual rectangle ---



				try
				{
					var iconRect = NotifyIconHelper.GetIconRect(notifyIcon1);
					int result = 0; // i dont know i dont use this..

					if (result == 0) // S_OK means success
					{
						Rectangle screenRect = iconRect;
						Debug.WriteLine($"Shell_NotifyIconGetRect SUCCESS: {screenRect}");

						// --- Position and Size the overlay EXACTLY over the icon ---
						_overlayForm.Bounds = screenRect; // Sets Location and Size

						if (!_overlayForm.Visible)
						{
							_overlayForm.Show();
							// _overlayForm.BringToFront(); // Might still be needed
							Debug.WriteLine($"Showing Overlay at fixed Rect: {screenRect}");
						}
						// Reset hide timer *only* if we successfully positioned
						_hideOverlayTimer.Stop();
						_hideOverlayTimer.Start();
					}
					else
					{
						// Failed to get Rect - Log error code
						int errorCode = Marshal.GetLastWin32Error(); // Or just use HRESULT 'result'
						Debug.WriteLine($"Shell_NotifyIconGetRect FAILED! HRESULT: {result}, Win32Error: {errorCode}");
						// Fallback maybe? Or do nothing? For now, just log.
						// As a fallback, you *could* revert to centering on the mouse here,
						// but that defeats the purpose of a fixed location.
						_isMouseOverIcon = false; // Reset flag if we failed to position
					}
				}
				catch (Exception ex)
				{
					Debug.WriteLine($"Exception calling Shell_NotifyIconGetRect: {ex.Message}");
					_isMouseOverIcon = false; // Reset flag on exception
				}
			}
			else
			{
				// Mouse is still moving *within* the icon area, keep resetting the timer
				// to prevent hiding while active. Check if overlay is visible first.
				if (_overlayForm.Visible)
				{
					_hideOverlayTimer.Stop();
					_hideOverlayTimer.Start();
				}
			}
		}



		// --- Timer Tick: Hide the overlay if mouse hasn't moved over icon recently ---
		private void HideOverlayTimer_Tick(object sender, EventArgs e)
		{
			_hideOverlayTimer.Stop(); // Stop the timer

			// Double-check if the mouse is STILL inside the overlay form's bounds.
			// If it is, the user might just be holding the mouse still over it. Restart timer.
			// Important: Check against Screen coordinates!
			Rectangle overlayScreenBounds = _overlayForm.Bounds; // Bounds gives screen coords for non-child forms
			if (_overlayForm.Visible && overlayScreenBounds.Contains(Cursor.Position))
			{
				Debug.WriteLine("Hide Timer Tick: Mouse still inside overlay, restarting timer.");
				_hideOverlayTimer.Start(); // Restart timer - keep overlay visible
			}
			else
			{
				// Mouse is likely outside the icon/overlay area, or overlay isn't visible
				if (_overlayForm.Visible)
				{
					Debug.WriteLine("Hide Timer Tick: Hiding Overlay");
					_overlayForm.Hide();
				}
				_isMouseOverIcon = false; // Reset the tracking flag
			}
		}


		// --- Event Handler for Scrolls on the Overlay ---
		private void OverlayForm_OverlayScrolled(object sender, MouseEventArgs e)
		{
			// THIS is where your scroll logic goes!
			// e.Delta contains the scroll amount (+ve = up/forward, -ve = down/backward)
			// e.X, e.Y are coordinates RELATIVE TO THE OVERLAY FORM, not the screen here.
			// If you need screen coordinates, use Cursor.Position

			string direction = e.Delta > 0 ? "Up" : "Down";
			Point screenPos = Cursor.Position; // Get current screen position
			Debug.WriteLine($"*** Overlay Scroll DETECTED! Delta: {e.Delta}, Direction: {direction}, ScreenPos: ({screenPos.X},{screenPos.Y}) ***");

			// --- !! Add your action here !! ---
			this.Text = $"Overlay Scroll: {direction} ({e.Delta})"; // Update main form title

			// Optional: Reset the hide timer when scroll occurs over the overlay
			// _hideOverlayTimer.Stop();
			// _hideOverlayTimer.Start();
		}


		// --- Cleanup ---
		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			// Dispose hook if you forgot to remove it
			// _trayHook?.Dispose();

			// Dispose timer
			_hideOverlayTimer?.Stop();
			_hideOverlayTimer?.Dispose();

			// Dispose overlay form
			_overlayForm?.Close(); // Close first
			_overlayForm?.Dispose();

			// Hide and dispose the NotifyIcon
			if (notifyIcon1 != null)
			{
				notifyIcon1.Visible = false;
				notifyIcon1.Dispose();
			}
			base.OnFormClosing(e);
		}



		// --- removedd this for a while  Event Handler for the Hook ---
		private void TrayHook_TrayMouseWheel(object sender, MouseEventArgs e)
		{
			System.Diagnostics.Debug.WriteLine("TrayHook_TrayMouseWheel handler executed!"); // <-- ADD

			// e.Delta contains the scroll amount (+ve = up/forward, -ve = down/backward)
			// e.X, e.Y are SCREEN coordinates where the scroll happened

			string direction = e.Delta > 0 ? "Up" : "Down";
			System.Diagnostics.Debug.WriteLine($"Tray Scroll Detected! Delta: {e.Delta}, Direction: {direction}, Location: ({e.X},{e.Y})");

			// --- !! Add your action here !! ---
			// Example: Adjust volume, change brightness, cycle through options, etc.
			// Make sure any UI updates happen correctly (already on UI thread thanks to BeginInvoke)
			this.Text = $"Tray Scroll: {direction} ({e.Delta})"; // Update form title as feedback
		}

		// Example click handler (unrelated to scroll, just for completeness)
		private void NotifyIcon1_Click(object sender, EventArgs e)
		{
			// Handle clicks if needed
			// Maybe show the main form window if hidden
			this.Show();
			this.WindowState = FormWindowState.Normal;
			this.Activate();
		}









		// --- P/Invoke for Shell_NotifyIconGetRect ---

		[DllImport("shell32.dll", SetLastError = true)]
		static extern int Shell_NotifyIconGetRect([In] ref NOTIFYICONIDENTIFIER identifier, [Out] out RECT iconLocation);

		[StructLayout(LayoutKind.Sequential)]
		public struct RECT
		{
			public int left;
			public int top;
			public int right;
			public int bottom;

			public int Width => right - left;
			public int Height => bottom - top;
			public Size Size => new Size(Width, Height);
			public Point Location => new Point(left, top);
			public Rectangle ToRectangle() => Rectangle.FromLTRB(left, top, right, bottom);
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct NOTIFYICONIDENTIFIER
		{
			public uint cbSize;
			public IntPtr hWnd; // HWND of window that registered the icon
			public uint uID;    // App-defined ID of the icon
			public Guid guidItem; // GUID (alternative identifier) - Set cbSize correctly based on usage
		}

		// We also need a way to potentially *get* the uID. This is the hard part.
		// We might need reflection or to manage the icon entirely via P/Invoke.
		// Let's *assume* for now we could somehow get the ID (e.g., if we managed it ourselves).
		// For this example, we'll *guess* common IDs like 0 or 1, but this is UNRELIABLE.
		const uint MY_ICON_ID = 0; // <<< --- !!! THIS IS LIKELY WRONG !!! ---

		// If using GUID instead (better, but requires P/Invoke icon management):
		// public Guid MyIconGuid = new Guid("YOUR-UNIQUE-GUID-HERE");







	}






}
