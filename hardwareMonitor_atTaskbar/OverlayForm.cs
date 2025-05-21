//using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Data;
//using System.Drawing;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows.Forms;
//using System.Runtime.InteropServices;


//namespace hardwareMonitor_atTaskbar
//{
//	public partial class OverlayForm : Form
//	{
//		// --- P/Invoke Declarations (Keep these as they are) ---
//		[DllImport("user32.dll", SetLastError = true)]
//		static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

//		[DllImport("user32.dll")]
//		[return: MarshalAs(UnmanagedType.Bool)]
//		public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

//		[StructLayout(LayoutKind.Sequential)]
//		public struct RECT
//		{
//			public int Left;
//			public int Top;
//			public int Right;
//			public int Bottom;
//			public int Width => Right - Left;
//			public int Height => Bottom - Top;
//		}

//		private const int GWL_EXSTYLE = -20;
//		private const int WS_EX_TOOLWINDOW = 0x00000080;
//		private const int WS_EX_NOACTIVATE = 0x08000000;
//		private const int WS_EX_LAYERED = 0x00080000;
//		private const int WS_EX_TRANSPARENT = 0x00000020; // For click-through

//		[DllImport("user32.dll", SetLastError = true)]
//		static extern int GetWindowLong(IntPtr hWnd, int nIndex);

//		[DllImport("user32.dll")]
//		static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
//		// --- End P/Invoke ---




//		private Image _imageToDisplay;
//		private System.Windows.Forms.Timer _refreshTimer; // Renamed from _updateTimer for clarity
//		private System.Windows.Forms.Timer _bringToFrontTimer; 

//		/// <summary>
//		/// Creates an overlay form to display a single image.
//		/// </summary>
//		/// <param name="image">The image to display. The form will attempt to size itself to this image within taskbar height constraints.</param>
//		public OverlayForm(Image image)
//		{
//			InitializeComponent();



//			if (image == null)
//			{
//				// Create a fallback image if null is passed
//				Bitmap fallbackBmp = new Bitmap(24, 24); // Small default size
//				using (Graphics g = Graphics.FromImage(fallbackBmp))
//				{
//					g.Clear(Color.Red);
//					using (Font f = new Font("Arial", 8)) g.DrawString("ERR", f, Brushes.White, 2, 2);
//				}
//				_imageToDisplay = fallbackBmp;
//				Console.WriteLine("Warning: Null image passed to OverlayForm. Using fallback.");
//			}
//			else
//			{
//				_imageToDisplay = image; // Store the provided image
//			}


//			this.Load += OverlayForm_Load;
//			this.Paint += OverlayForm_Paint;
//			this.DoubleBuffered = true;

//			// Optional: If your image has a specific transparent color
//			// this.BackColor = Color.Magenta;
//			// this.TransparencyKey = Color.Magenta;

//			// Timer for periodic refresh, in case the image content needs to be updated externally
//			// and then re-drawn by calling Invalidate() on this form.
//			// If the image itself never changes once set, this timer is only for potential system redraw needs,
//			// but DoubleBuffered and TopMost usually handle that.
//			// For per-second "tick" effect even if image isn't changing, we keep it.
//			_refreshTimer = new System.Windows.Forms.Timer();
//			_refreshTimer.Interval = 1000; // 1 second "tick"
//			_refreshTimer.Tick += RefreshTimer_Tick;




//			_bringToFrontTimer = new System.Windows.Forms.Timer();
//			_bringToFrontTimer.Interval = 250; // 1 second "tick"
//			_bringToFrontTimer.Tick += _bringToFrontTimer_Tick;

//			// my addititon.
//			//this.TopMost = true;
//			this.FormBorderStyle = FormBorderStyle.None;
//			this.ShowInTaskbar = false;
//		}

//		protected override bool ShowWithoutActivation => true;
//		protected override CreateParams CreateParams
//		{
//			get
//			{
//				CreateParams baseParams = base.CreateParams;

//				const int WS_EX_TOPMOST = 0x00000008;
//				const int WS_EX_NOACTIVATE = 0x08000000;
//				const int WS_EX_TOOLWINDOW = 0x00000080;
//				baseParams.ExStyle |= (int)(WS_EX_NOACTIVATE | WS_EX_TOOLWINDOW | WS_EX_TOPMOST);

//				return baseParams;
//			}
//		}



//		private void OverlayForm_Load(object sender, EventArgs e)
//		{
//			//int extendedStyle = GetWindowLong(this.Handle, GWL_EXSTYLE);
//			//SetWindowLong(this.Handle, GWL_EXSTYLE, extendedStyle | WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE | WS_EX_LAYERED | WS_EX_TRANSPARENT);

//			PositionWindowAndSetSize_Main();
//			_refreshTimer.Start();

//			_bringToFrontTimer.Start();
//		}


//		/// <summary>
//		/// 0 is Left, 1 is Right
//		/// </summary>
//		int _Location = 0;
//		private static int _Location_atLeft = 0;
//		private static int _Location_atRight = 1;

//		private void PositionWindowAndSetSize_Main()
//		{
//			if (_Location == _Location_atLeft)
//				PositionWindowAndSetSize_atLeft();
//			else if(_Location == _Location_atRight)
//				PositionWindowAndSetSize_atRight();

//		}

//		int Top_Padding = 1;

//		/// <summary>
//		/// win11 center icons at Taskbar.. has empty area at Left. this is Good for Win11.
//		/// </summary>
//		private void PositionWindowAndSetSize_atLeft()
//		{
//			IntPtr taskbarHandle = FindWindow("Shell_TrayWnd", null);
//			RECT taskbarRect;

//			if (taskbarHandle == IntPtr.Zero)
//			{
//				Console.WriteLine("Taskbar (Shell_TrayWnd) not found. Positioning at screen bottom-left.");
//				taskbarRect = new RECT
//				{
//					Left = 0,
//					Top = Screen.PrimaryScreen.Bounds.Height - (_imageToDisplay?.Height ?? 24), // Use image height or default
//					Right = Screen.PrimaryScreen.Bounds.Width,
//					Bottom = Screen.PrimaryScreen.Bounds.Height
//				};
//			}
//			else
//			{
//				GetWindowRect(taskbarHandle, out taskbarRect);
//			}

//			// Determine the display height: taskbar height, but not exceeding image height
//			int displayHeight = Math.Min(taskbarRect.Height, _imageToDisplay.Height);

//			// Determine display width: image width
//			int displayWidth = _imageToDisplay.Width;

//			// If the image is taller than the taskbar, scale it down proportionally to fit taskbar height
//			float aspectRatio = (float)_imageToDisplay.Width / _imageToDisplay.Height;
//			if (_imageToDisplay.Height > taskbarRect.Height)
//			{
//				displayHeight = taskbarRect.Height;
//				displayWidth = (int)(displayHeight * aspectRatio);
//			}
//			else // Image is shorter or equal to taskbar height, use its original dimensions
//			{
//				displayHeight = _imageToDisplay.Height;
//				displayWidth = _imageToDisplay.Width;
//			}


//			this.ClientSize = new Size(displayWidth, displayHeight);
//			this.Location = new Point(
//				taskbarRect.Left  , 
//				taskbarRect.Top + (taskbarRect.Height - displayHeight) / 2
//				+ Top_Padding
//				); // Vertically center within taskbar height

//			//this.Top = this.Top + 1;


//			Console.WriteLine($"Taskbar Rect: L={taskbarRect.Left}, T={taskbarRect.Top}, H={taskbarRect.Height}");
//			Console.WriteLine($"Overlay Positioned at: X={this.Location.X}, Y={this.Location.Y}, W={this.Width}, H={this.Height}");
//		}

//		/// <summary>
//		///  win10 has to use this, it cant center icons at Taskbar..
//		/// </summary>
//		private void PositionWindowAndSetSize_atRight()
//		{
//			//TODO find  right most notify icon are .. subtract that are.. we want to place our form 
//			// to left of trayicons Area...

//			IntPtr taskbarHandle = FindWindow("Shell_TrayWnd", null);
//			RECT taskbarRect;

//			if (taskbarHandle == IntPtr.Zero)
//			{
//				Console.WriteLine("Taskbar (Shell_TrayWnd) not found. Positioning at screen bottom-left.");
//				taskbarRect = new RECT
//				{
//					Left = 0,
//					Top = Screen.PrimaryScreen.Bounds.Height - (_imageToDisplay?.Height ?? 24), // Use image height or default
//					Right = Screen.PrimaryScreen.Bounds.Width,
//					Bottom = Screen.PrimaryScreen.Bounds.Height
//				};
//			}
//			else
//			{
//				GetWindowRect(taskbarHandle, out taskbarRect);
//			}

//			// Determine the display height: taskbar height, but not exceeding image height
//			int displayHeight = Math.Min(taskbarRect.Height, _imageToDisplay.Height);

//			// Determine display width: image width
//			int displayWidth = _imageToDisplay.Width;

//			// If the image is taller than the taskbar, scale it down proportionally to fit taskbar height
//			float aspectRatio = (float)_imageToDisplay.Width / _imageToDisplay.Height;
//			if (_imageToDisplay.Height > taskbarRect.Height)
//			{
//				displayHeight = taskbarRect.Height;
//				displayWidth = (int)(displayHeight * aspectRatio);
//			}
//			else // Image is shorter or equal to taskbar height, use its original dimensions
//			{
//				displayHeight = _imageToDisplay.Height;
//				displayWidth = _imageToDisplay.Width;
//			}


//			this.ClientSize = new Size(displayWidth, displayHeight);
//			this.Location = new Point(taskbarRect.Left, taskbarRect.Top + (taskbarRect.Height - displayHeight) / 2); // Vertically center within taskbar height

//			Console.WriteLine($"Taskbar Rect: L={taskbarRect.Left}, T={taskbarRect.Top}, H={taskbarRect.Height}");
//			Console.WriteLine($"Overlay Positioned at: X={this.Location.X}, Y={this.Location.Y}, W={this.Width}, H={this.Height}");
//		}


//		/// <summary>
//		/// Public method to update the image being displayed.
//		/// </summary>
//		public void UpdateImage(Image newImage)
//		{
//			if (newImage == null) return;

//			// Dispose the old image only if it was not the fallback and it's different
//			if (_imageToDisplay != null 
//				&& !_imageToDisplay.Equals(newImage) /* simple check, may need more robust if image is modified externally */
//				)
//			{
//				// Be careful disposing if the image is shared or managed elsewhere
//				// For this example, assume OverlayForm takes ownership if it's not the fallback
//				// If the fallback was used, don't dispose it here as it might be a shared static resource
//				// This logic needs care depending on image source.
//				// _imageToDisplay.Dispose();
//			}

//			_imageToDisplay = newImage;


//			if(!Visible)
//				this.Show();


//			// Recalculate size and position based on new image
//			// This is important if the new image has different dimensions.
//			PositionWindowAndSetSize_Main();

//			this.Invalidate(); // Trigger repaint with the new image
//		}


//		private void RefreshTimer_Tick(object sender, EventArgs e)
//		{
//			// This tick primarily exists to force a repaint every second if desired,
//			// or if you have some other per-second logic.
//			// If the image content is updated externally, you'd call UpdateImage() then this.Invalidate().
//			this.Invalidate();
//		}

//		private void _bringToFrontTimer_Tick(object sender, EventArgs e)
//		{
//			//this.TopMost = true; //without this it doesnt work.. 
//			//but this closes Right click Menues...

//			FormFn.ShowInactiveTopmost(this);

//			//this.BringToFront();
//			//this.Show();

//			if(!this.Visible)
//				this.Show();
//		}

//		private void OverlayForm_Paint(object sender, PaintEventArgs e)
//		{
//			e.Graphics.Clear(this.BackColor); // Important if using TransparencyKey

//			if (_imageToDisplay != null)
//			{
//				// Draw the image to fit the ClientSize (which was set based on image and taskbar)
//				// The image will be scaled if its original dimensions were adjusted in PositionWindowAndSetSize
//				e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic; // For better scaling
//				e.Graphics.DrawImage(_imageToDisplay, this.ClientRectangle);
//			}
//		}

//		protected override void OnFormClosing(FormClosingEventArgs e)
//		{
//			_refreshTimer?.Stop();
//			_refreshTimer?.Dispose();

//			_bringToFrontTimer?.Stop();
//			_bringToFrontTimer?.Dispose();

//			// Dispose the image only if this form is considered its owner
//			// If the image is passed in and managed externally, you might not want to dispose it here.
//			// For this example, let's assume if it's not the initial fallback, we can dispose it.
//			// This needs careful consideration based on your application's image management.
//			// _imageToDisplay?.Dispose(); // Potentially dispose

//			base.OnFormClosing(e);
//		}


//	}
//}



//public static class FormFn
//{

//	private const int SW_SHOWNOACTIVATE = 4;
//	private const int HWND_TOPMOST = -1;
//	private const uint SWP_NOACTIVATE = 0x0010;

//	[DllImport("user32.dll", EntryPoint = "SetWindowPos")]
//	static extern bool SetWindowPos(
//		 int hWnd,             // Window handle
//		 int hWndInsertAfter,  // Placement-order handle
//		 int X,                // Horizontal position
//		 int Y,                // Vertical position
//		 int cx,               // Width
//		 int cy,               // Height
//		 uint uFlags);         // Window positioning flags

//	[DllImport("user32.dll")]
//	static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

//	public static void ShowInactiveTopmost(this Form frm)
//	{
//		ShowWindow(frm.Handle, SW_SHOWNOACTIVATE);
//		SetWindowPos(frm.Handle.ToInt32(), HWND_TOPMOST,
//		frm.Left, frm.Top, frm.Width, frm.Height,
//		SWP_NOACTIVATE);
//	}

//}





//////////////////test2
//////////////////test2
//////////////////test2
//////////////////test2

using System;
using System.Drawing;
using System.Drawing.Drawing2D; // For InterpolationMode
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Linq; // For Screen.AllScreens.First() if needed

namespace hardwareMonitor_atTaskbar // Ensure this namespace matches your project
{
	public partial class OverlayForm : Form
	{
		// P/Invoke for SetWindowPos (ensure it's available)
		[DllImport("user32.dll", EntryPoint = "SetWindowPos")]
		static extern bool SetWindowPos(
			 IntPtr hWnd,            // Window handle
			 IntPtr hWndInsertAfter, // Placement-order handle
			 int X, int Y, int cx, int cy,
			 uint uFlags);

		// Constants for SetWindowPos
		static readonly IntPtr HWND_TOPMOST_HANDLE = new IntPtr(-1); // For hWndInsertAfter
		const uint SWP_NOMOVE = 0x0002;
		const uint SWP_NOSIZE = 0x0001;
		const uint SWP_NOACTIVATE = 0x0010; // Important!


		// --- P/Invoke Declarations ---
		[DllImport("user32.dll", SetLastError = true)]
		static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

		[DllImport("user32.dll", SetLastError = true)]
		static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

		[StructLayout(LayoutKind.Sequential)]
		public struct RECT
		{
			public int Left; public int Top; public int Right; public int Bottom;
			public int Width => Right - Left; public int Height => Bottom - Top;
		}

		private const int GWL_EXSTYLE = -20;
		private const int WS_EX_TOOLWINDOW = 0x00000080;
		private const int WS_EX_NOACTIVATE = 0x08000000;
		private const int WS_EX_LAYERED = 0x00080000;
		private const int WS_EX_TRANSPARENT = 0x00000020;
		private const int WS_EX_TOPMOST = 0x00000008;


		[DllImport("user32.dll", SetLastError = true)]
		static extern int GetWindowLong(IntPtr hWnd, int nIndex);

		[DllImport("user32.dll")] // SetWindowLong does not return an error code, check GetLastWin32Error
		static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
		// --- End P/Invoke ---

		private Image _imageToDisplay;
		private object _imageLock = new object(); // For thread safety if UpdateImage is called from non-UI thread
		private System.Windows.Forms.Timer _refreshTimer;
		// private System.Windows.Forms.Timer _bringToFrontTimer; // REMOVED - This was likely causing issues

		public int DesiredLocation { get; set; } = LocationSetting.AtLeftOfTaskbar; // Default to right
		public int TopPadding { get; set; } = 1; // Your Top_Padding variable, made public

		public static class LocationSetting
		{
			public const int AtLeftOfTaskbar = 0;
			public const int AtRightOfTaskbar = 1;
		}


		public OverlayForm(Image initialImage = null)
		{
			InitializeComponent();

			this.FormBorderStyle = FormBorderStyle.None;
			this.ShowInTaskbar = false;
			this.TopMost = true; // Set TopMost flag here. CreateParams will reinforce with WS_EX_TOPMOST.
			this.StartPosition = FormStartPosition.Manual;

			//this.BackColor = Color.Magenta; // Example for transparency, adjust if needed
			//this.TransparencyKey = Color.Magenta; // Works with WS_EX_LAYERED
			this.AllowTransparency = false;

			this.DoubleBuffered = true;

			lock (_imageLock)
			{
				if (initialImage != null)
				{
					_imageToDisplay = (Image)initialImage.Clone(); // Clone for local ownership
				}
				else
				{
					Bitmap fallbackBmp = new Bitmap(24, 24);
					using (Graphics g = Graphics.FromImage(fallbackBmp))
					{
						g.Clear(Color.Red);
						using (Font f = new Font("Arial", 8)) g.DrawString("O", f, Brushes.White, 2, 2);
					}
					_imageToDisplay = fallbackBmp;
					Console.WriteLine("OverlayForm: Initialized with fallback image.");
				}
			}

			this.Load += OverlayForm_Load;
			this.Paint += OverlayForm_Paint;

			_refreshTimer = new System.Windows.Forms.Timer();
			_refreshTimer.Interval = 1000; // Your desired data update interval (e.g., for repainting)
			_refreshTimer.Tick += RefreshTimer_Tick;

			// _bringToFrontTimer and its Tick event handler are REMOVED.
		}

		// This is a good way to set initial extended styles.
		protected override CreateParams CreateParams
		{
			get
			{
				CreateParams cp = base.CreateParams;
				cp.ExStyle |= WS_EX_NOACTIVATE | WS_EX_TOOLWINDOW | WS_EX_TOPMOST;
				return cp;
			}
		}

		// This also helps in making the form non-intrusive.
		protected override bool ShowWithoutActivation => true;

		private void OverlayForm_Load(object sender, EventArgs e)
		{
			// Apply WS_EX_LAYERED and WS_EX_TRANSPARENT via SetWindowLong
			// CreateParams has already set WS_EX_TOPMOST, WS_EX_NOACTIVATE, WS_EX_TOOLWINDOW
			int currentExStyle = GetWindowLong(this.Handle, GWL_EXSTYLE);
			int newExStyle = currentExStyle | WS_EX_LAYERED | WS_EX_TRANSPARENT;
			SetWindowLong(this.Handle, GWL_EXSTYLE, newExStyle);

			PositionWindowAndSetSize_Main(); // Position it first

			// ****** CRITICAL PART for consistent TopMost *******
			// After the window is created, positioned, and initial styles are set,
			// explicitly set its position in the Z-order.
			SetWindowPos(this.Handle, HWND_TOPMOST_HANDLE, 0, 0, 0, 0,
						 SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
			// This tells Windows: "Take this window (which should already be styled as topmost)
			// and put it at the very top of the topmost band, but don't move, resize, or activate it."
			// ******************************************************

			_refreshTimer.Start();
			Console.WriteLine($"OverlayForm Loaded. Location: {this.Location}, Size: {this.ClientSize}, TopMost: {this.TopMost}, Visible: {this.Visible}");
		}


		private void PositionWindowAndSetSize_Main()
		{
			if (this.IsDisposed || !this.IsHandleCreated) return;

			lock (_imageLock)
			{
				if (_imageToDisplay == null)
				{
					this.ClientSize = new Size(1, 1); return;
				}

				if (DesiredLocation == LocationSetting.AtLeftOfTaskbar)
					PositionWindowAndSetSize_atLeft();
				else if (DesiredLocation == LocationSetting.AtRightOfTaskbar)
					PositionWindowAndSetSize_atRight();
			}
		}

		private void PositionWindowAndSetSize_atLeft()
		{
			IntPtr taskbarHandle = FindWindow("Shell_TrayWnd", null);
			RECT taskbarRect;

			if (taskbarHandle == IntPtr.Zero || !GetWindowRect(taskbarHandle, out taskbarRect) || taskbarRect.Height <= 0)
			{
				Console.WriteLine("Taskbar (Shell_TrayWnd) not found for AtLeft. Using fallback.");
				SetupFallbackPosition(); return;
			}
			CalculateAndSetDimensions(taskbarRect, taskbarRect.Left, taskbarRect.Top);
		}

		private void PositionWindowAndSetSize_atRight()
		{
			IntPtr taskbarHandle = FindWindow("Shell_TrayWnd", null);
			RECT taskbarRect;

			if (taskbarHandle == IntPtr.Zero || !GetWindowRect(taskbarHandle, out taskbarRect) || taskbarRect.Height <= 0)
			{
				Console.WriteLine("Taskbar (Shell_TrayWnd) not found for AtRight. Using fallback.");
				SetupFallbackPosition(); return;
			}

			IntPtr trayNotifyWnd = FindWindowEx(taskbarHandle, IntPtr.Zero, "TrayNotifyWnd", null);
			IntPtr sysPager = IntPtr.Zero;
			IntPtr toolbarWnd = IntPtr.Zero;

			if (trayNotifyWnd != IntPtr.Zero)
			{
				sysPager = FindWindowEx(trayNotifyWnd, IntPtr.Zero, "SysPager", null);
				toolbarWnd = FindWindowEx(sysPager != IntPtr.Zero ? sysPager : trayNotifyWnd, IntPtr.Zero, "ToolbarWindow32", null);
			}

			RECT trayIconsRect;
			int targetX;
			int imageWidth = _imageToDisplay?.Width ?? 0; // Cache width

			if (toolbarWnd != IntPtr.Zero && GetWindowRect(toolbarWnd, out trayIconsRect) && trayIconsRect.Width > 0)
			{
				Console.WriteLine($"Tray Icons Area Rect: L={trayIconsRect.Left}, T={trayIconsRect.Top}, W={trayIconsRect.Width}, H={trayIconsRect.Height}");
				targetX = trayIconsRect.Left - imageWidth - 2; // 2px margin
			}
			else
			{
				Console.WriteLine("Could not find System Tray Icons (ToolbarWindow32). Falling back to taskbar right edge.");
				targetX = taskbarRect.Right - imageWidth - 5;
			}

			Screen currentScreen = Screen.FromPoint(new Point(taskbarRect.Left + taskbarRect.Width / 2, taskbarRect.Top + taskbarRect.Height / 2)) ?? Screen.PrimaryScreen;
			if (targetX < currentScreen.WorkingArea.Left) targetX = currentScreen.WorkingArea.Left;
			if (targetX + imageWidth > currentScreen.WorkingArea.Right) targetX = currentScreen.WorkingArea.Right - imageWidth;

			CalculateAndSetDimensions(taskbarRect, targetX, taskbarRect.Top);
		}

		private void CalculateAndSetDimensions(RECT referenceTaskbarRect, int targetX, int targetYBase)
		{
			if (_imageToDisplay == null) return;

			int displayHeight = _imageToDisplay.Height;
			int displayWidth = _imageToDisplay.Width;

			if (displayHeight > referenceTaskbarRect.Height && referenceTaskbarRect.Height > 0)
			{
				float aspectRatio = (float)_imageToDisplay.Width / _imageToDisplay.Height;
				displayHeight = referenceTaskbarRect.Height;
				displayWidth = (int)(displayHeight * aspectRatio);
				if (displayWidth == 0 && _imageToDisplay.Width > 0) displayWidth = 1;
			}

			if (displayWidth <= 0) displayWidth = 1;
			if (displayHeight <= 0) displayHeight = 1;

			this.ClientSize = new Size(displayWidth, displayHeight);
			int finalY = targetYBase + (referenceTaskbarRect.Height - displayHeight) / 2 + TopPadding;
			this.Location = new Point(targetX, finalY);

			Console.WriteLine($"Overlay Positioned. Taskbar: L{referenceTaskbarRect.Left} T{referenceTaskbarRect.Top} H{referenceTaskbarRect.Height}. Overlay SetTo: Loc({this.Location.X},{this.Location.Y}), ClientSize({this.ClientSize.Width},{this.ClientSize.Height})");
		}

		private void SetupFallbackPosition()
		{
			lock (_imageLock)
			{
				if (_imageToDisplay == null) { this.ClientSize = new Size(1, 1); return; }
				Screen primaryScreen = Screen.PrimaryScreen ?? Screen.AllScreens.FirstOrDefault() ?? Screen.FromHandle(this.Handle);
				int h = Math.Max(1, Math.Min(_imageToDisplay.Height, primaryScreen.WorkingArea.Height));
				int w = Math.Max(1, Math.Min(_imageToDisplay.Width, primaryScreen.WorkingArea.Width));

				this.ClientSize = new Size(w, h);
				this.Location = new Point(primaryScreen.WorkingArea.Left + 5, primaryScreen.WorkingArea.Bottom - this.ClientSize.Height - 5);
				Console.WriteLine($"Overlay Fallback Position: {this.Location}, Size: {this.ClientSize}");
			}
		}

		public void UpdateImage(Image newImage)
		{
			if (newImage == null)
			{
				Console.WriteLine("OverlayForm: UpdateImage called with null.");
				return;
			}

			Image oldImage = null;
			bool dimensionsChanged = false;

			lock (_imageLock)
			{
				oldImage = _imageToDisplay; // Keep reference to old one for disposal
				if (_imageToDisplay == null || newImage.Width != _imageToDisplay.Width || newImage.Height != _imageToDisplay.Height)
				{
					dimensionsChanged = true;
				}
				_imageToDisplay = (Image)newImage.Clone(); // Take ownership by cloning
			}

			oldImage?.Dispose(); // Dispose the previous image this form owned

			if (this.IsHandleCreated && !this.IsDisposed)
			{
				// If UpdateImage is called when form is not visible, ensure it becomes visible
				// This should only happen if it was explicitly hidden, not just covered.
				if (!this.Visible)
				{
					// Use ShowWindow with SW_SHOWNOACTIVATE to make it visible without stealing focus
					// This is generally preferred over this.Show() for overlays.
					FormFn.ShowWindow(this.Handle, FormFn.SW_SHOWNOACTIVATE_CONST); // Using constant from FormFn
				}

				if (dimensionsChanged)
				{
					if (this.InvokeRequired)
					{
						this.BeginInvoke(new Action(PositionWindowAndSetSize_Main));
					}
					else
					{
						PositionWindowAndSetSize_Main();
					}
				}
				this.Invalidate();
			}
		}

		private void RefreshTimer_Tick(object sender, EventArgs e)
		{
			if (this.IsHandleCreated && !this.IsDisposed && this.Visible) // Only invalidate if visible
			{
				this.Invalidate();
			}
		}

		// _bringToFrontTimer_Tick REMOVED

		private void OverlayForm_Paint(object sender, PaintEventArgs e)
		{
			if (this.TransparencyKey == this.BackColor) // Check if transparency is active
			{
				e.Graphics.Clear(this.BackColor); // Clear with the transparency key color
			}
			else // Otherwise clear with a default or make it fully transparent if using layered windows
			{
				e.Graphics.Clear(Color.Transparent); // Assuming WS_EX_LAYERED is used for other alpha effects
			}


			lock (_imageLock)
			{
				if (_imageToDisplay != null)
				{
					try
					{
						e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
						e.Graphics.DrawImage(_imageToDisplay, this.ClientRectangle);
					}
					catch (Exception ex)
					{
						Console.WriteLine($"OverlayForm: Error painting image: {ex.Message}");
						// e.Graphics.FillRectangle(Brushes.Red, this.ClientRectangle); // Optional: draw error directly
					}
				}
			}
		}

		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			_refreshTimer?.Stop();
			_refreshTimer?.Dispose();
			// _bringToFrontTimer?.Dispose(); // REMOVED

			lock (_imageLock)
			{
				_imageToDisplay?.Dispose(); // Dispose the image this form owns
				_imageToDisplay = null;
			}
			base.OnFormClosing(e);
		}
	}

	// Your FormFn class (ensure constants are accessible or replicated)
	public static class FormFn
	{
		public const int SW_SHOWNOACTIVATE_CONST = 4; // Renamed for clarity
		private const int HWND_TOPMOST = -1;
		private const uint SWP_NOACTIVATE = 0x0010;
		private const uint SWP_NOMOVE = 0x0002;
		private const uint SWP_NOSIZE = 0x0001;


		[DllImport("user32.dll", EntryPoint = "SetWindowPos")]
		static extern bool SetWindowPos(
			 IntPtr hWnd,            // Window handle (use IntPtr)
			 IntPtr hWndInsertAfter, // Placement-order handle (use IntPtr)
			 int X,
			 int Y,
			 int cx,
			 int cy,
			 uint uFlags);

		[DllImport("user32.dll")]
		public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow); // Made public for UpdateImage

		public static void ShowInactiveTopmost(this Form frm) // Keep this if you need it elsewhere, but avoid in a rapid timer
		{
			ShowWindow(frm.Handle, SW_SHOWNOACTIVATE_CONST);
			SetWindowPos(frm.Handle, (IntPtr)HWND_TOPMOST, // Cast HWND_TOPMOST to IntPtr
			frm.Left, frm.Top, frm.Width, frm.Height,
			SWP_NOACTIVATE | SWP_NOMOVE | SWP_NOSIZE); // Added NOMOVE and NOSIZE to only affect Z-order
		}
	}
}