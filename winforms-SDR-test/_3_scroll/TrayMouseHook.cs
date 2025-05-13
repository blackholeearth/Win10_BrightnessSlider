using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel; // Required for Win32Exception

/// <summary>
/// Provides low-level mouse hook capabilities to detect mouse wheel events
/// over the System Tray area.
/// Remember to call Dispose() or UninstallHook() when finished to release the hook.
/// Requires a Control instance (like a Form) for marshalling events back to the UI thread.
/// </summary>
public class TrayMouseHook : IDisposable
{
	// --- Native Constants ---
	private const int WH_MOUSE_LL = 14;      // Hook type: Low-level mouse input events
	private const int HC_ACTION = 0;         // Hook code: Process the message

	// --- Windows Messages (wParam in HookCallback) ---
	// Add more from winuser.h if needed for logging other events
	private const int WM_MOUSEMOVE = 0x0200;
	private const int WM_LBUTTONDOWN = 0x0201;
	private const int WM_LBUTTONUP = 0x0202;
	private const int WM_LBUTTONDBLCLK = 0x0203;
	private const int WM_RBUTTONDOWN = 0x0204;
	private const int WM_RBUTTONUP = 0x0205;
	private const int WM_RBUTTONDBLCLK = 0x0206;
	private const int WM_MBUTTONDOWN = 0x0207;
	private const int WM_MBUTTONUP = 0x0208;
	private const int WM_MBUTTONDBLCLK = 0x0209;
	private const int WM_MOUSEWHEEL = 0x020A;    // Vertical mouse wheel
	private const int WM_XBUTTONDOWN = 0x020B;   // Extended mouse buttons (mouseData HIWORD indicates button)
	private const int WM_XBUTTONUP = 0x020C;
	private const int WM_XBUTTONDBLCLK = 0x020D;
	private const int WM_MOUSEHWHEEL = 0x020E;   // Horizontal mouse wheel

	// --- Native Structures ---
	[StructLayout(LayoutKind.Sequential)]
	private struct POINT
	{
		public int X;
		public int Y;
	}

	[StructLayout(LayoutKind.Sequential)]
	private struct MSLLHOOKSTRUCT
	{
		public POINT pt;          // Coordinates of the cursor, in screen coordinates.
		public uint mouseData;    // If the message is WM_MOUSEWHEEL or WM_MOUSEHWHEEL, the HIWORD of this member is the wheel delta.
								  // If the message is WM_XBUTTONDOWN, WM_XBUTTONUP, etc., the HIWORD specifies which X button was pressed or released (XBUTTON1 or XBUTTON2).
								  // Otherwise, this value is not used.
		public uint flags;        // Event-injected flags (e.g., LLMHF_INJECTED).
		public uint time;         // Time stamp for this message.
		public IntPtr dwExtraInfo; // Additional information associated with the message (GetMessageExtraInfo).
	}

	// --- Native Functions (P/Invoke) ---
	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, uint dwThreadId);

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool UnhookWindowsHookEx(IntPtr hhk);

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

	[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern IntPtr GetModuleHandle(string lpModuleName);

	[DllImport("user32.dll")]
	private static extern IntPtr WindowFromPoint(POINT Point);

	[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
	private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

	// --- Delegate & Hook Handle ---
	// Delegate signature for the hook procedure
	private delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);
	private IntPtr _hookHandle = IntPtr.Zero; // The handle to the installed hook
	private readonly HookProc _hookProc; // Keep a managed reference to prevent garbage collection

	// --- Event & Synchronization ---
	/// <summary>
	/// Raised when a vertical mouse wheel event occurs while the cursor is over a recognized System Tray window.
	/// This event is raised on the UI thread of the control provided in the constructor.
	/// </summary>
	public event EventHandler<MouseEventArgs> TrayMouseWheel;
	private readonly Control _syncControl; // Control used to marshal event calls back to the UI thread

	/// <summary>
	/// Initializes a new instance of the <see cref="TrayMouseHook"/> class.
	/// Requires a control (like the main form) to synchronize event calls to the UI thread.
	/// </summary>
	/// <param name="syncControl">The control (e.g., your Form) used for invoking events on the UI thread. Cannot be null.</param>
	/// <exception cref="ArgumentNullException">Thrown if syncControl is null.</exception>
	public TrayMouseHook(Control syncControl)
	{
		_syncControl = syncControl ?? throw new ArgumentNullException(nameof(syncControl), "A synchronization control (like a Form) is required.");
		// Create the delegate instance pointing to our callback method
		_hookProc = HookCallback;
	}

	/// <summary>
	/// Installs the global low-level mouse hook.
	/// </summary>
	/// <exception cref="Win32Exception">Thrown if SetWindowsHookEx fails, containing the Win32 error code.</exception>
	/// <exception cref="InvalidOperationException">Thrown if the hook delegate is unexpectedly null.</exception>
	public void InstallHook()
	{
		if (_hookHandle != IntPtr.Zero)
		{
			Debug.WriteLine("[TrayMouseHook] Hook already installed.");
			return; // Prevent installing multiple times
		}

		Debug.WriteLine("[TrayMouseHook] Attempting to install hook...");
		using (Process curProcess = Process.GetCurrentProcess())
		using (ProcessModule curModule = curProcess.MainModule)
		{
			IntPtr moduleHandle = GetModuleHandle(curModule.ModuleName);
			Debug.WriteLine($"[TrayMouseHook] Got Module Handle: {moduleHandle} for module {curModule.ModuleName}");

			if (moduleHandle == IntPtr.Zero)
			{
				int errorCode = Marshal.GetLastWin32Error();
				Debug.WriteLine($"[TrayMouseHook] *** ERROR: GetModuleHandle failed! Error Code: {errorCode}");
				throw new Win32Exception(errorCode, $"Failed to get module handle for {curModule.ModuleName}.");
			}

			// Ensure the delegate is assigned (should be from constructor)
			if (_hookProc == null)
			{
				// This should ideally not happen if constructor logic is correct
				Debug.WriteLine("[TrayMouseHook] *** ERROR: Hook delegate is null!");
				throw new InvalidOperationException("Hook delegate was not initialized.");
			}

			Debug.WriteLine("[TrayMouseHook] Delegate assigned. Calling SetWindowsHookEx...");
			// Set the hook: Type=WH_MOUSE_LL, Callback=_hookProc, Module=Current App, Thread=0 (Global)
			_hookHandle = SetWindowsHookEx(WH_MOUSE_LL, _hookProc, moduleHandle, 0);
		}

		// Check if the hook installation was successful
		if (_hookHandle == IntPtr.Zero)
		{
			// Get the last Win32 error code
			int errorCode = Marshal.GetLastWin32Error();
			string errorMsg = $"[TrayMouseHook] *** HOOK INSTALLATION FAILED! Win32 Error Code: {errorCode}";
			Debug.WriteLine(errorMsg);
			// Throw an exception to notify the caller
			throw new Win32Exception(errorCode, "Failed to install low-level mouse hook (SetWindowsHookEx failed).");
		}
		else
		{
			Debug.WriteLine($"[TrayMouseHook] *** HOOK INSTALLED SUCCESSFULLY. Handle: {_hookHandle}");
		}
	}

	/// <summary>
	/// Uninstalls the low-level mouse hook if it is currently installed.
	/// Safe to call even if the hook is not installed.
	/// </summary>
	public void UninstallHook()
	{
		if (_hookHandle != IntPtr.Zero)
		{
			Debug.WriteLine($"[TrayMouseHook] Uninstalling hook: {_hookHandle}");
			// Attempt to uninstall the hook
			bool success = UnhookWindowsHookEx(_hookHandle);
			_hookHandle = IntPtr.Zero; // Clear the handle regardless of success to prevent reuse

			if (!success)
			{
				// Log an error if uninstallation failed, but don't throw (usually called during cleanup)
				int errorCode = Marshal.GetLastWin32Error();
				Debug.WriteLine($"[TrayMouseHook] --- Warning: UnhookWindowsHookEx failed! Win32 Error Code: {errorCode}");
			}
			else
			{
				Debug.WriteLine("[TrayMouseHook] Hook uninstalled successfully.");
			}
		}
		else
		{
			// Optional: Log that it wasn't installed
			// Debug.WriteLine("[TrayMouseHook] Hook already uninstalled or was never installed.");
		}
	}

	/// <summary>
	/// The callback procedure that Windows calls for low-level mouse events.
	/// This method runs in the context of the thread that installed the hook,
	/// but may receive messages for other threads/processes.
	/// </summary>
	private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
	{
		// According to MSDN documentation:
		// If nCode is less than zero, the hook procedure must pass the message to CallNextHookEx
		// without further processing and should return the value returned by CallNextHookEx.
		if (nCode >= HC_ACTION) // Process the message only if nCode >= 0 (HC_ACTION)
		{
			// --- Extract Common Information ---
			int messageCode = wParam.ToInt32(); // The mouse message identifier (e.g., WM_MOUSEMOVE)
			MSLLHOOKSTRUCT hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT)); // Mouse data

			// --- Get Window Information ---
			IntPtr hWndUnderCursor = WindowFromPoint(hookStruct.pt); // Window handle under cursor
			string classNameStr = GetWindowClass(hWndUnderCursor); // Class name of that window

			// --- Comprehensive Logging for Debugging ---
			// Log details for *every* message received when nCode >= 0
			if(messageCode != WM_MOUSEMOVE)
				Debug.WriteLine($"[HOOK] -> Msg: {messageCode} ({GetMessageName(messageCode)}), " + // Message Code & Name
							$"Pt: ({hookStruct.pt.X},{hookStruct.pt.Y}), " +                   // Screen Coordinates
							$"hWnd: {hWndUnderCursor}, Class: '{classNameStr}', " +            // Window Handle & Class
							$"Data: {hookStruct.mouseData}, " +                                // mouseData field
							$"Flags: {hookStruct.flags}, " +                                   // LLMHF flags
							$"Time: {hookStruct.time}");                                       // Message Timestamp

			// --- Specific Handling for VERTICAL MOUSE WHEEL ---
			// You could add a similar block for WM_MOUSEHWHEEL if needed
			if (messageCode == WM_MOUSEWHEEL)
			{
				Debug.WriteLine("  [HOOK] >>> WM_MOUSEWHEEL Event Detected!");

				// Check if the mouse is over one of the windows we consider the "System Tray"
				if (IsSystemTrayWindow(hWndUnderCursor))
				{
					Debug.WriteLine($"  [HOOK] >>> Over recognized Tray Window ('{classNameStr}')!");

					// Extract the wheel delta from the high-order word of mouseData
					// Positive value = Scroll Up/Forward; Negative value = Scroll Down/Backward
					// Standard wheel click is often +/- 120, touchpads might send smaller values more frequently.
					int delta = (short)((hookStruct.mouseData >> 16) & 0xFFFF);

					// Create standard MouseEventArgs to pass to the event handler
					// Note: Button state isn't directly available here, usually None for wheel events.
					// X, Y coordinates are screen coordinates from the hook structure.
					MouseEventArgs args = new MouseEventArgs(MouseButtons.None, 0, hookStruct.pt.X, hookStruct.pt.Y, delta);

					// --- CRITICAL: Marshal the event call to the UI thread ---
					// The hook callback doesn't run on the UI thread. Direct UI access is unsafe.
					// Use BeginInvoke on the control provided in the constructor.
					_syncControl?.BeginInvoke(new Action(() =>
					{
						// This code now runs safely on the UI thread
						Debug.WriteLine($"  [HOOK] >>> Invoking TrayMouseWheel event on UI thread. Delta: {delta}");
						TrayMouseWheel?.Invoke(this, args); // Raise the event
					}));

					// --- Optional: Prevent further processing / "Swallow" the message ---
					// If you return a non-zero value (e.g., 1), the system and other applications
					// might not receive this mouse wheel event. Use this cautiously.
					// Typically, you only do this if you completely handle the scroll and
					// want to prevent default behavior (like scrolling a window underneath).
					// return (IntPtr)1;
				}
				else
				{
					// Log if the wheel event occurred but not over a window we care about
					Debug.WriteLine($"  [HOOK] --- Wheel event ignored. Window class '{classNameStr}' not in IsSystemTrayWindow list.");
				}
			}
		} // End of if (nCode >= HC_ACTION)

		// ALWAYS call the next hook in the chain.
		// This allows other hooks and the system to process the message.
		// Failing to do this will break mouse input for other applications.
		return CallNextHookEx(_hookHandle, nCode, wParam, lParam);
	}

	/// <summary>
	/// Safely retrieves the class name for a given window handle.
	/// </summary>
	/// <param name="hWnd">The window handle.</param>
	/// <returns>The class name, or an error/status string if retrieval fails.</returns>
	private string GetWindowClass(IntPtr hWnd)
	{
		if (hWnd == IntPtr.Zero) return "N/A (Zero Handle)";
		try
		{
			// Pre-allocate buffer for the class name
			StringBuilder className = new StringBuilder(256);
			// Call GetClassName API
			int length = GetClassName(hWnd, className, className.Capacity);
			if (length > 0)
			{
				return className.ToString(); // Success
			}
			else
			{
				// Optional: Check Marshal.GetLastWin32Error() for specific reason
				return "N/A (GetClassName Failed or Empty)";
			}
		}
		catch (Exception ex)
		{
			// Catch potential exceptions (though less likely with GetClassName itself)
			Debug.WriteLine($"[TrayMouseHook] Exception in GetWindowClass for hWnd {hWnd}: {ex.Message}");
			return $"N/A (Exception)";
		}
	}

	/// <summary>
	/// Converts common Windows mouse message codes into human-readable strings for logging.
	/// </summary>
	/// <param name="messageCode">The integer message code (wParam).</param>
	/// <returns>A string representation of the message name or "Unknown".</returns>
	private string GetMessageName(int messageCode)
	{
		switch (messageCode)
		{
			case WM_MOUSEMOVE: return "WM_MOUSEMOVE";
			case WM_LBUTTONDOWN: return "WM_LBUTTONDOWN";
			case WM_LBUTTONUP: return "WM_LBUTTONUP";
			case WM_LBUTTONDBLCLK: return "WM_LBUTTONDBLCLK";
			case WM_RBUTTONDOWN: return "WM_RBUTTONDOWN";
			case WM_RBUTTONUP: return "WM_RBUTTONUP";
			case WM_RBUTTONDBLCLK: return "WM_RBUTTONDBLCLK";
			case WM_MBUTTONDOWN: return "WM_MBUTTONDOWN";
			case WM_MBUTTONUP: return "WM_MBUTTONUP";
			case WM_MBUTTONDBLCLK: return "WM_MBUTTONDBLCLK";
			case WM_MOUSEWHEEL: return "WM_MOUSEWHEEL";
			case WM_XBUTTONDOWN: return "WM_XBUTTONDOWN";
			case WM_XBUTTONUP: return "WM_XBUTTONUP";
			case WM_XBUTTONDBLCLK: return "WM_XBUTTONDBLCLK";
			case WM_MOUSEHWHEEL: return "WM_MOUSEHWHEEL";
			// Add more messages here if needed for debugging
			default: return $"Unknown ({messageCode})"; // Show code if name unknown
		}
	}

	/// <summary>
	/// Determines if the given window handle belongs to a known class name associated with the System Tray.
	/// IMPORTANT: This list is a starting point and likely needs adjustment based on observed class names
	/// from the debug logs on specific Windows versions or configurations.
	/// </summary>
	/// <param name="hWnd">The window handle to check.</param>
	/// <returns>True if the window's class name is in the recognized list, false otherwise.</returns>
	private bool IsSystemTrayWindow(IntPtr hWnd)
	{
		if (hWnd == IntPtr.Zero) return false; // Cannot be a tray window if handle is null

		string name = GetWindowClass(hWnd); // Get the class name using our helper

		// --- !!! UPDATE THIS LIST BASED ON YOUR DEBUG OUTPUT !!! ---
		// Check against a list of known/observed class names for tray-related windows.
		// Use the || (OR) operator to add more classes as you discover them.
		return name == "Shell_TrayWnd" ||              // The main taskbar window itself
			   name == "NotifyIconOverflowWindow" ||   // The flyout window for hidden icons ("overflow")
			   name == "TrayNotifyWnd" ||              // Often contains the actual notification icons area
			   name == "ToolbarWindow32";              // Common class for toolbars, sometimes used for tray elements

		// ---- ADD MORE DISCOVERED CLASS NAMES HERE ----
		// Example: || name == "YourDiscoveredClassName1"
		// Example: || name == "AnotherClassName"
	}


	// --- IDisposable Implementation ---

	private bool _disposed = false; // Flag to detect redundant calls to Dispose

	/// <summary>
	/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
	/// This includes uninstalling the mouse hook.
	/// </summary>
	public void Dispose()
	{
		// Dispose of managed and unmanaged resources
		Dispose(true);
		// Prevent the finalizer from running (since cleanup is already done)
		GC.SuppressFinalize(this);
	}

	/// <summary>
	/// Protected Dispose pattern method for releasing resources.
	/// </summary>
	/// <param name="disposing">True if called explicitly from Dispose(), false if called from the finalizer.</param>
	protected virtual void Dispose(bool disposing)
	{
		if (!_disposed)
		{
			if (disposing)
			{
				// --- Dispose managed state (managed objects) here ---
				// If TrayMouseHook held references to other managed IDisposable objects,
				// they would be disposed here. In this case, there are none directly owned.
				// The event handler references are managed by the subscriber (the Form).
			}

			// --- Free unmanaged resources (unmanaged objects) ---
			// **Crucially, uninstall the hook here.**
			UninstallHook();

			_disposed = true; // Mark as disposed
		}
	}

	/// <summary>
	/// Finalizer (destructor) - provides a safety net to release the hook if Dispose() is not called.
	/// It's generally better practice to ensure Dispose() is called explicitly (e.g., in FormClosing).
	/// </summary>
	~TrayMouseHook()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method above.
		Debug.WriteLine("[TrayMouseHook] Finalizer (~TrayMouseHook) called. Ensure Dispose() is being called for proper cleanup.");
		Dispose(disposing: false); // Call Dispose with false, indicating finalizer call
	}
}