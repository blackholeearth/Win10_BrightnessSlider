namespace winformsTests._4_contextMenuStrip // Use your namespace
{

	using System;
	using System.ComponentModel;
	using System.Drawing;
	using System.Windows.Forms;
	using System.Runtime.InteropServices;
	using static System.Windows.Forms.VisualStyles.VisualStyleElement;

	// This class WRAPS a NotifyIcon, it doesn't inherit.
	public class NotifyIconEX : Component // Inherit from Component for designer/container support
	{
		// --- Internal NotifyIcon Instance ---
		private NotifyIcon _internalNotifyIcon;

		// --- Custom Properties ---
		private bool _useWin11StylePosition;
		private ContextMenuStripEX _customContextMenuStrip;

		#region route the properties , back to internal notifyicon

		[DefaultValue(false)]
		[Category("Behavior")]
		[Description("Indicates whether to use custom Windows 11 style positioning.")]
		public bool UseWin11StylePosition
		{
			get => _useWin11StylePosition;
			set => _useWin11StylePosition = value;
		}

		[DefaultValue(null)]
		[Category("Behavior")]
		[Description("The custom ContextMenuStrip (V2) to associate.")]
		public ContextMenuStripEX ContextMenuStrip
		{
			get => _customContextMenuStrip;
			set
			{
				_customContextMenuStrip = value;
				// Important: We DO NOT assign this to the internal icon's property
				// because we handle the showing manually.
				_internalNotifyIcon.ContextMenuStrip = null; // Ensure base icon doesn't show default
			}
		}

		// --- Properties Delegated to Internal NotifyIcon ---
		[Category("Appearance")]
		[Localizable(true)]
		[DefaultValue(null)]
		public Icon Icon
		{
			get => _internalNotifyIcon.Icon;
			set => _internalNotifyIcon.Icon = value;
		}

		[Category("Appearance")]
		[Localizable(true)]
		[DefaultValue("")]
		public string Text
		{
			get => _internalNotifyIcon.Text;
			set => _internalNotifyIcon.Text = value;
		}

		[Category("Behavior")]
		[Localizable(true)]
		[DefaultValue(false)] // Match NotifyIcon's actual default
		public bool Visible
		{
			get => _internalNotifyIcon.Visible;
			set => _internalNotifyIcon.Visible = value;
		}

		[Category("Data")]
		[DefaultValue(null)]
		public object Tag
		{
			get => _internalNotifyIcon.Tag;
			set => _internalNotifyIcon.Tag = value;
		}

		#endregion


		// --- Events Delegated (Add others as needed) ---
		// We need to subscribe to the internal icon's events and raise our own
		public event MouseEventHandler MouseClick
		{
			add => _internalNotifyIcon.MouseClick += value;
			remove => _internalNotifyIcon.MouseClick -= value;
		}

		// Constructor
		public NotifyIconEX(IContainer container) : this()
		{
			container?.Add(this);
		}

		public NotifyIconEX()
		{
			_internalNotifyIcon = new NotifyIcon();
			// Handle the internal icon's MouseDown to implement custom menu logic
			_internalNotifyIcon.MouseDown += InternalNotifyIcon_MouseDown;
		}

		private void InternalNotifyIcon_MouseDown(object sender, MouseEventArgs e)
		{
			// If right-click, show our custom menu if configured
			if (e.Button == MouseButtons.Right && _customContextMenuStrip != null)
			{
				if (this.UseWin11StylePosition)
				{
					_customContextMenuStrip.ShowWin11Style(Cursor.Position);

					// Set foreground window after show
					if (_customContextMenuStrip.IsHandleCreated)
					{
						SetForegroundWindow(new HandleRef(_customContextMenuStrip, _customContextMenuStrip.Handle));
					}
				}
				else
				{
					

					_customContextMenuStrip.Show(Cursor.Position); // Example: Basic show near cursor

					// Optional: Implement default showing if flag is false
					// Requires calculating position manually or using a simpler Show overload
					if (_customContextMenuStrip.IsHandleCreated)
					{
						SetForegroundWindow(new HandleRef(_customContextMenuStrip, _customContextMenuStrip.Handle));
					}

				}
			}
			// Note: Other mouse events (like Left Click) are passed through via the event delegation above.
		}

		// --- Dispose ---
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				// Dispose the internal NotifyIcon
				_internalNotifyIcon?.Dispose();
				_internalNotifyIcon = null; // Suppress warnings
				_customContextMenuStrip = null; // Clear reference
			}
			base.Dispose(disposing);
		}






		// --- Win32 API Declarations ---
		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool SetForegroundWindow(IntPtr hWnd);
		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool SetForegroundWindow(HandleRef hWnd);





	}
}