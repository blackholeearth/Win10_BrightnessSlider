using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms; // For Timer, not for drawing on taskbar

public class TaskbarDrawer
{
	private System.Windows.Forms.Timer 
		_drawChart_Timer,
		_refreshTaskbar_Timer;

	private IntPtr _taskbarHandle;
	private IntPtr _trayAreaHandle; // e.g., TrayNotifyWnd or ClockWindow

	public Bitmap LatestImg;

	public TaskbarDrawer()
	{
		_drawChart_Timer = new System.Windows.Forms.Timer();
		_drawChart_Timer.Interval = 1000;
		_drawChart_Timer.Tick += _drawChart_Timer_Tick;
		_drawChart_Timer.Start();


		_refreshTaskbar_Timer = new System.Windows.Forms.Timer();
		_refreshTaskbar_Timer.Interval = 100; // 350; 
		_refreshTaskbar_Timer.Tick += _refreshTaskbar_Timer_Tick;
		_refreshTaskbar_Timer.Start();



		_taskbarHandle = FindWindow("Shell_TrayWnd", null);
		if (_taskbarHandle == IntPtr.Zero)
		{
			Console.WriteLine("Shell_TrayWnd not found.");
			return;
		}

		// --- Finding the specific area is the TRICKY and UNRELIABLE part ---
		IntPtr trayNotifyWnd = FindWindowEx(_taskbarHandle, IntPtr.Zero, "TrayNotifyWnd", null);
		if (trayNotifyWnd != IntPtr.Zero)
		{
			_trayAreaHandle = trayNotifyWnd;
		}
		else
		{
			Console.WriteLine("Target area (e.g., TrayNotifyWnd) not found.");
			_trayAreaHandle = _taskbarHandle; // Fallback to drawing on main taskbar, which is usually too broad
		}


		
	}


	/// <summary>
	/// rectangle is taskbar locaiton and size
	/// </summary>
	public Action<Rectangle> on_ChartImage_Requested;

	Rectangle rect_Taskbar;
	private void _drawChart_Timer_Tick(object sender, EventArgs e)
	{
		if (rect_Taskbar.IsEmpty)
			return;

		on_ChartImage_Requested?.Invoke(rect_Taskbar);

	}

	/// <summary>
	/// cant draw directly on shelltray wind its not visible...
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private void _refreshTaskbar_Timer_Tick(object sender, EventArgs e)
	{
		//IntPtr desktop_dc = GetDC(IntPtr.Zero);
		//using (Graphics g = Graphics.FromHdc(desktop_dc))
		//{
		//	g.FillRectangle(Brushes.Red, 0, 0, 100, 100);
		//}
		//ReleaseDC(IntPtr.Zero, desktop_dc);


		IntPtr hdc = GetDC(IntPtr.Zero); // Get DC for Desktop..
		if (hdc == IntPtr.Zero)
			return;
		 
		try
		{
			// --- IMPORTANT: Coordinates are relative to the _targetAreaHandle's client area ---
			RECT targetRect;
			GetWindowRect(_taskbarHandle, out targetRect);
			// Get screen coordinates of target
			// To draw within this target, (0,0) is its top-left.

			//--- is taskbar visible
			// --- Define your content's intended area relative to taskbar ---
			int contentOffsetX = 5;  // Where your drawing starts from taskbar's left
			int contentWidth = 200;  // The width of your drawing
			int contentHeight = targetRect.Height; // You draw the full height
														   // ****** USE THE HIT-TEST LOGIC ******
			if (!IsTaskbarAreaVisibleAndTop(targetRect, contentOffsetX, contentWidth, contentHeight))
			{
				System.Diagnostics.Debug.WriteLine("TaskbarDrawer: Target draw area on taskbar is obscured. Drawing paused.");
				return; // Skip drawing
			}


			using (Graphics g = Graphics.FromHdc(hdc)) // Can use Graphics object if you prefer
			{

				rect_Taskbar = new Rectangle(
						//0,
						//0,
						targetRect.Left,
						targetRect.Top,
						targetRect.Right - targetRect.Left,
						targetRect.Bottom - targetRect.Top
					);

				var rectangleTaskbar_TargetLeft = new Rectangle(
						//0,
						//0,
						targetRect.Left,
						targetRect.Top,
						200,
						targetRect.Bottom - targetRect.Top
					);

				//onTaskbar_Paint?.Invoke(g, rectangleTaskbar_Full);

				//this works
				//g.FillRectangle(Brushes.Green, 0, 0, 50, 50);

				//test
				//g.FillRectangle(Brushes.Green, rectangleTaskbar_TargetLeft);

				if (LatestImg != null)
				{
					//g.DrawImage(LatestImg, 0, 0);

					//gTaskbar.DrawImage(img_combined, 0,0,img1.Width,img1.Height);
					g.DrawImage(LatestImg, rect_Taskbar.X, rect_Taskbar.Y, LatestImg.Width, LatestImg.Height);

				}


			}
		}
		finally
		{
			ReleaseDC(_taskbarHandle, hdc);
		}

	}

	public void Stop()
	{
		_drawChart_Timer?.Stop();
		// How to "clean up" what you drew? You can't easily.
		// You might try to force a repaint of the taskbar area,
		// but it might not always work or could cause visual glitches.
		// InvalidateRect(_targetAreaHandle, IntPtr.Zero, true); // Example
	}







	// --- hit test if taskbar is not covered by another window... so we can draw on it..
	[DllImport("user32.dll")]
	static extern IntPtr WindowFromPoint(POINTSTRUCT Point); // Using POINTSTRUCT from previous WINDOWPLACEMENT
	[StructLayout(LayoutKind.Sequential)]
	public struct POINTSTRUCT
	{
		public int X;
		public int Y;
	}

	// --- Windows API P/Invoke Signatures ---
	[DllImport("user32.dll", SetLastError = true)]
	public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

	[DllImport("user32.dll", SetLastError = true)]
	static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);



	[DllImport("user32.dll")]
	static extern IntPtr GetDC(IntPtr hWnd);

	[DllImport("user32.dll")]
	static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);


	[DllImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

	[StructLayout(LayoutKind.Sequential)]
	public struct RECT
	{
		public int Left;
		public int Top;
		public int Right;
		public int Bottom;

		public int Width => Right - Left;
		public int Height => Bottom - Top;
	}

	// Ensure GetParent P/Invoke is available:
	[DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
	public static extern IntPtr GetParent(IntPtr hWnd);

	private bool IsTaskbarAreaVisibleAndTop(RECT taskbarScreenRect, int contentOffsetX, int contentWidth, int contentHeight)
	{
		if (taskbarScreenRect.Width <= 0 || taskbarScreenRect.Height <= 0) return false;

		// --- Choose a few test points within your intended drawing area ---
		// These points are in SCREEN coordinates.
		POINTSTRUCT testPoint1 = new POINTSTRUCT // Center of your drawing area
		{
			X = taskbarScreenRect.Left + contentOffsetX + (contentWidth / 2),
			Y = taskbarScreenRect.Top + (contentHeight / 2)
		};

		POINTSTRUCT testPoint2 = new POINTSTRUCT // Top-left of your drawing area
		{
			X = taskbarScreenRect.Left + contentOffsetX + 2, // Small offset in
			Y = taskbarScreenRect.Top + 2
		};

		POINTSTRUCT testPoint3 = new POINTSTRUCT // Bottom-right of your drawing area
		{
			X = taskbarScreenRect.Left + contentOffsetX + contentWidth - 2,
			Y = taskbarScreenRect.Top + contentHeight - 2
		};

		IntPtr[] targetHandles = { _taskbarHandle }; // Primarily expect Shell_TrayWnd
													 // Optionally, add known child window handles if you want to be more granular,
													 // but this makes it more complex as these child handles can change or not exist.
													 // IntPtr trayNotifyWnd = FindWindowEx(_taskbarHandle, IntPtr.Zero, "TrayNotifyWnd", null);
													 // if (trayNotifyWnd != IntPtr.Zero) { /* add to a list of valid handles */ }


		POINTSTRUCT[] pointsToTest = { testPoint1, testPoint2, testPoint3 };
		int matchCount = 0;

		foreach (POINTSTRUCT pt in pointsToTest)
		{
			IntPtr hWndAtPoint = WindowFromPoint(pt);
			if (hWndAtPoint == IntPtr.Zero)
			{
				// No window at this point (highly unlikely for taskbar area unless something is wrong)
				System.Diagnostics.Debug.WriteLine($"IsTaskbarAreaVisible: No window at screen point ({pt.X},{pt.Y}).");
				continue; // Or potentially return false if any point fails
			}

			// Check if the window at the point is the taskbar itself,
			// or one of its direct children if you want to allow that.
			// For the most robust check against being covered by *other apps*,
			// ensuring it's the taskbar itself is usually sufficient.
			bool isTaskbarFamily = false;
			IntPtr currentParent = hWndAtPoint;
			int safetyCounter = 0; // Prevent infinite loop if parentage is weird

			// Walk up the parent chain from hWndAtPoint to see if _taskbarHandle is an ancestor
			// or if hWndAtPoint is _taskbarHandle itself.
			while (currentParent != IntPtr.Zero && safetyCounter < 10) // Max 10 levels up
			{
				if (currentParent == _taskbarHandle)
				{
					isTaskbarFamily = true;
					break;
				}
				currentParent = GetParent(currentParent); // GetParent P/Invoke needed
				safetyCounter++;
			}


			if (isTaskbarFamily)
			{
				matchCount++;
				System.Diagnostics.Debug.WriteLine($"IsTaskbarAreaVisible: Point ({pt.X},{pt.Y}) IS on taskbar/child ({hWndAtPoint}). Taskbar handle: {_taskbarHandle}");
			}
			else
			{
				System.Diagnostics.Debug.WriteLine($"IsTaskbarAreaVisible: Point ({pt.X},{pt.Y}) is NOT on taskbar. Window found: {hWndAtPoint}. Expected: {_taskbarHandle} or its child.");
				// If any critical test point is NOT the taskbar, then something is covering it.
				return false; // Strict: if any point is covered, assume not visible for drawing
			}
		}

		// Require all (or a majority) of test points to be on the taskbar
		return matchCount >= pointsToTest.Length; // Or matchCount > 0 for a less strict check
	}



}


 
 