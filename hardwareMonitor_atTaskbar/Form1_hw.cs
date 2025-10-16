using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Windows.Forms;

namespace hardwareMonitor_atTaskbar
{
	public partial class Form1_hw : Form
	{
		// Performance Counters
		private PerformanceCounter _cpuCounter;
		private PerformanceCounter _diskReadCounter;
		private PerformanceCounter _diskWriteCounter;
		private PerformanceCounter _networkSentCounter;
		private PerformanceCounter _networkReceivedCounter;

		// Public properties to expose data
		public float CurrentCpuUsage { get; set; }
		public float CurrentDiskSpeedMBps { get; set; }
		public float CurrentNetworkSpeedMBps { get; set; }
		public float CurrentNetworkSpeedKBps { get; set; }
		//public string FormattedStringForTaskbar { get; set; }

		// Event to notify when data is updated
		public event EventHandler DataUpdated;

		// --- NEW: Data history for graphs ---
		private const int MaxHistoryPoints = 50;
		private Queue<float> _cpuHistory = new();
		private Queue<float> _diskHistory = new();
		private Queue<float> _networkHistory = new();
		// --- END NEW ---

		private ContextMenuStrip cms_Tray = new();
		private Timer updateTimer = new Timer();

		public TaskbarDrawer _drawer = new();


		//not used currently.
		//public OverlayForm _overlayForm = new(null);


		public Form1_hw()
		{

			InitializeComponent();
			InitializePerformanceCounters();

			this.Shown += (s1, e1) => { this.Hide(); };

			_drawer = new TaskbarDrawer(); // Instantiate it
										   //_drawer.onTaskbar_Paint += onTaskbar_Paint;
			_drawer.on_ChartImage_Requested += on_ChartImage_Requested;

			//make Graph Start from End---
			for (int i = 0; i < MaxHistoryPoints; i++)
			{
				_cpuHistory.Enqueue(0);
				_diskHistory.Enqueue(0);
				_networkHistory.Enqueue(0);
			}

			// --- NEW: Setup graph panels ---
			panelCpuGraph.Paint += PanelCpuGraph_Paint;
			panelDiskGraph.Paint += PanelDiskGraph_Paint;
			panelNetworkGraph.Paint += PanelNetworkGraph_Paint;

			// Enable double buffering on panels to reduce flicker
			SetDoubleBuffered(panelCpuGraph);
			SetDoubleBuffered(panelDiskGraph);
			SetDoubleBuffered(panelNetworkGraph);
			// --- END NEW ---

			// Setup NotifyIcon
			try
			{
				Bitmap bmp = new Bitmap(16, 16);
				using (Graphics g = Graphics.FromImage(bmp)) { g.Clear(Color.SkyBlue); }
				appNotifyIcon.Icon = Icon.FromHandle(bmp.GetHicon());
			}
			catch (Exception ex) { Console.WriteLine("Error setting notify icon: " + ex.Message); }

			var mi_togglepos = cms_Tray.Items.Add("Toggle Location: Left/Right", null,
				(s1, e1) => { TaskbarDrawer.location_AtRight = !TaskbarDrawer.location_AtRight; });

			var mi_detechScreenRes = cms_Tray.Items.Add("info -> Screen Resolution/Dpi", null, (s1, e1) => DetectScreenChange_RDP());
			var mi_hide = cms_Tray.Items.Add("Settings Show/Hide", null, (s1, e1) => Toggle_ShowHide());
			var mi_sep1 = cms_Tray.Items.Add("-");
			var mi_restart = cms_Tray.Items.Add("Restart", null, (snd, ev) => { Application.Restart(); });
			var mi_exit = cms_Tray.Items.Add("Exit", null, ExitMenuItem_Click);


			appNotifyIcon.Visible = true;

			appNotifyIcon.ContextMenuStrip = cms_Tray;


		}

		string GetDeviceDpi_ofMonitor_ofThisControl() 
		{
			float dx, dy;

			Graphics g = this.CreateGraphics();
			try
			{
				dx = g.DpiX;
				dy = g.DpiY;
			}
			finally
			{
				g.Dispose();
			}

			return $"{dx}, {dy}";
		}

		private void DetectScreenChange_RDP()
		{
			var virtSize = "virtSize:  " + SystemInformation.VirtualScreen.Size ;
			var scrAtMousePos = "screen at cursor Pos:  " + Screen.FromPoint(Cursor.Position).Bounds.Size;

			var GetSizeRect_taskbar = "dllimport_GetSizeRect (Taskbar): "+ _drawer.last__taskbarRECT_backupForDGB.ToString();
			var GetHDC_ofscreen = "dllimport_GetSizeRect (Taskbar): "+ _drawer.last__getHDC_graphics;
			var Getdpi_this = "GetDeviceDpi_ofMonitor_ofThisControl: " + GetDeviceDpi_ofMonitor_ofThisControl();
			MessageBox.Show(@$"
{Getdpi_this}  
{virtSize}  
{scrAtMousePos}  

{GetSizeRect_taskbar}  

{GetHDC_ofscreen}

");
		}

		private void Toggle_ShowHide()
		{
			if (Visible) this.Hide();
			else this.Show();
		}


		private void Form1_Load(object sender, EventArgs e)
		{
			if (_cpuCounter != null)
			{
				updateTimer.Interval = 1000;
				updateTimer.Enabled = true;
				updateTimer.Tick -= updateTimer_Tick;
				updateTimer.Tick += updateTimer_Tick;

				updateTimer.Start();
			}
			else
			{
				lblCombinedValue.Text = "Counters failed to init.";
				MessageBox.Show("Critical performance counters could not be initialized. The application might not function correctly.", "Initialization Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}

		}

		private void InitializePerformanceCounters()
		{
			try
			{
				//https://stackoverflow.com/questions/23391455/performancecounter-reporting-higher-cpu-usage-than-whats-observed
				//_cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");  //at win11 this give me half of Taskmanager.
				_cpuCounter = new PerformanceCounter("Processor Information", "% Processor Utility", "_Total");
				_cpuCounter.NextValue(); // Prime
				System.Threading.Thread.Sleep(100);

				_diskReadCounter = new PerformanceCounter("PhysicalDisk", "Disk Read Bytes/sec", "_Total");
				_diskWriteCounter = new PerformanceCounter("PhysicalDisk", "Disk Write Bytes/sec", "_Total");

				var activeInterface = GetActiveNetworkInterfaceInstanceName();
				if (!string.IsNullOrEmpty(activeInterface))
				{
					_networkSentCounter = new PerformanceCounter("Network Interface", "Bytes Sent/sec", activeInterface);
					_networkReceivedCounter = new PerformanceCounter("Network Interface", "Bytes Received/sec", activeInterface);
				}
				else
				{
					Console.WriteLine("No active network interface found. Network stats will be 0.");
					lblNetworkValue.Text = "N/A";
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Error initializing performance counters: {ex.Message}", "Performance Counter Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				_cpuCounter = null; // Mark as unusable
			}
		}
		private string GetActiveNetworkInterfaceInstanceName()
		{
			// (Keep your existing GetActiveNetworkInterfaceInstanceName() method here)
			// ... (same as previous version)
			var category = new PerformanceCounterCategory("Network Interface");
			string[] instances = category.GetInstanceNames();

			foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
			{
				if (ni.OperationalStatus == OperationalStatus.Up &&
					ni.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
					ni.NetworkInterfaceType != NetworkInterfaceType.Tunnel)
				{
					string interfaceDescription = ni.Description;
					string sanitizedDescription = interfaceDescription.Replace('/', '_').Replace('(', '[').Replace(')', ']');

					if (instances.Contains(interfaceDescription)) return interfaceDescription;
					if (instances.Contains(sanitizedDescription)) return sanitizedDescription;
				}
			}
			var firstUsable = instances.FirstOrDefault(name => !name.ToLower().Contains("loopback") && !name.ToLower().Contains("isatap") && !name.ToLower().Contains("teredo"));
			if (firstUsable != null)
			{
				Console.WriteLine($"Warning: Using fallback network interface: {firstUsable}");
				return firstUsable;
			}
			Console.WriteLine("Critical: No suitable network interface instance found.");
			return null;
		}

		private void updateTimer_Tick(object sender, EventArgs e)
		{
			try
			{
				if (_cpuCounter != null)
					CurrentCpuUsage = _cpuCounter.NextValue();
				else
					CurrentCpuUsage = 0;

				float diskRead = _diskReadCounter?.NextValue() ?? 0f;
				float diskWrite = _diskWriteCounter?.NextValue() ?? 0f;
				CurrentDiskSpeedMBps = (diskRead + diskWrite) / (1024f * 1024f);

				float netSent = _networkSentCounter?.NextValue() ?? 0f;
				float netReceived = _networkReceivedCounter?.NextValue() ?? 0f;
				//CurrentNetworkSpeedMBps = (netSent + netReceived) / (1024f * 1024f);
				CurrentNetworkSpeedKBps = (netSent + netReceived) / (1024f);

				lblCpuValue.Text = $"{CurrentCpuUsage:F1} %";
				lblDiskValue.Text = $"{CurrentDiskSpeedMBps:F1} MB/s";
				lblNetworkValue.Text = $"{CurrentNetworkSpeedKBps:F1} KB/s";

				//FormattedStringForTaskbar = $"C:{CurrentCpuUsage:F0}% D:{CurrentDiskSpeedMBps:F1} N:{CurrentNetworkSpeedMBps:F1}";
				//lblCombinedValue.Text = FormattedStringForTaskbar;

				// ---  Update history and invalidate graphs ---
				UpdateHistory(_cpuHistory, CurrentCpuUsage);
				UpdateHistory(_diskHistory, CurrentDiskSpeedMBps);
				UpdateHistory(_networkHistory, CurrentNetworkSpeedKBps);

				panelCpuGraph.Invalidate(); // Request repaint
				panelDiskGraph.Invalidate();
				panelNetworkGraph.Invalidate();

				DataUpdated?.Invoke(this, EventArgs.Empty);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error during performance counter update: {ex.Message}");
				lblCombinedValue.Text = "Error reading values";
			}
		}

		private void UpdateHistory(Queue<float> history, float value)
		{
			history.Enqueue(value);
			if (history.Count > MaxHistoryPoints)
			{
				history.Dequeue(); // Remove oldest
			}
		}

		private void PanelCpuGraph_Paint(object sender, PaintEventArgs e)
		{
			DrawGraphBar(e.Graphics, panelCpuGraph, _cpuHistory, 100f, "CPU", "%", CurrentCpuUsage);
		}
		private void PanelDiskGraph_Paint(object sender, PaintEventArgs e)
		{
			float maxDisk = _diskHistory.Any() ? _diskHistory.Max() : 10f; // Use .Any() to check if empty
			if (maxDisk < 1.0f) maxDisk = 1.0f;
			DrawGraphBar(e.Graphics, panelDiskGraph, _diskHistory, maxDisk, "DISK", "MB/s", CurrentDiskSpeedMBps);
		}
		private void PanelNetworkGraph_Paint(object sender, PaintEventArgs e)
		{
			float currentMaxNet = _networkHistory.Any() ? _networkHistory.Max() : 0f;
			float scaleMaxNet;

			// Tiered scaling for Network Graph's Y-axis
			if (currentMaxNet < 50f) scaleMaxNet = 50f;
			else if (currentMaxNet <= 250f) scaleMaxNet = 250f;
			else if (currentMaxNet <= 500f) scaleMaxNet = 500f;
			else if (currentMaxNet <= 1024f) scaleMaxNet = 1024f; // Max 1 MB/s effectively
			else scaleMaxNet = currentMaxNet * 1.1f; // Auto-scale with 10% headroom if above 1024 KB/s

			if (scaleMaxNet < 1.0f) scaleMaxNet = 1.0f; // Ensure a minimum if all values are tiny

			DrawGraphBar(e.Graphics, panelNetworkGraph, _networkHistory, scaleMaxNet, "NET", "KB/s", CurrentNetworkSpeedKBps);
		}


		Bitmap img1, img2 , img3, img_combined;
		Graphics g_img1, g_img2, g_img3, g_img_combined;
		void initBitmaps(Rectangle CliRect) 
		{
			int extraWidth = 8;

			img1 = new Bitmap((MaxHistoryPoints + 2) + extraWidth, CliRect.Height);
			img2 = new Bitmap((MaxHistoryPoints + 2) + extraWidth, CliRect.Height);
			img3 = new Bitmap((MaxHistoryPoints + 2) + extraWidth, CliRect.Height);
			img_combined = new Bitmap(img3.Width * 3, CliRect.Height); //3, there is 3 chart.

			g_img1 = Graphics.FromImage(img1);
			g_img2 = Graphics.FromImage(img2);
			g_img3 = Graphics.FromImage(img3);
			g_img_combined = Graphics.FromImage(img_combined);
		}

		private void on_ChartImage_Requested(Rectangle CliRect)
		{
			if (img1 is null)
				initBitmaps(CliRect);


			//DRAW CPU
			var rect1 = new Rectangle(0, 0, (MaxHistoryPoints + 2), CliRect.Height);
			//DrawGraphBar_onBitmap(g_img1, rect1, _cpuHistory, 100f, "CPU", "%", CurrentCpuUsage);

			// Tiered scaling for CPU Graph's Y-axis
			float currentMaxCPU = _cpuHistory.Any() ? _cpuHistory.Max() : 100f;
			if(currentMaxCPU <= 50f)
				currentMaxCPU = 50f;
			DrawGraphBar_onBitmap(g_img1, rect1, _cpuHistory, currentMaxCPU, "CPU", "%", CurrentCpuUsage);

			//DRAW DISK
			var rect2 = new Rectangle(0, 0, (MaxHistoryPoints + 2), CliRect.Height);

			//float maxDisk = _diskHistory.Any() ? _diskHistory.Max() : 10f; 
			float maxDisk = _diskHistory?.Max() ?? 10f;
			if (maxDisk < 1.0f) maxDisk = 1.0f; //ensure a minimum
			DrawGraphBar_onBitmap(g_img2, rect2, _diskHistory, maxDisk, "DISK", "MB/s", CurrentDiskSpeedMBps);

			//DRAW NET
			var rect3 = new Rectangle(0, 0, (MaxHistoryPoints + 2), CliRect.Height);

			float currentMaxNet = _networkHistory.Any() ? _networkHistory.Max() : 0f;
			float scaleMaxNet;
			// Tiered scaling for Network Graph's Y-axis
			if (currentMaxNet < 50f) scaleMaxNet = 50f;
			else if (currentMaxNet <= 250f) scaleMaxNet = 250f;
			else if (currentMaxNet <= 500f) scaleMaxNet = 500f;
			else if (currentMaxNet <= 1024f) scaleMaxNet = 1024f; // Max 1 MB/s effectively
			else scaleMaxNet = currentMaxNet * 1.1f; // Auto-scale with 10% headroom if above 1024 KB/s

			if (scaleMaxNet < 1.0f) scaleMaxNet = 1.0f; // Ensure a minimum if all values are tiny

			DrawGraphBar_onBitmap(g_img3, rect3, _networkHistory, scaleMaxNet, "NET", "KB/s", CurrentNetworkSpeedKBps);


			// --- to taskbar
			g_img_combined.DrawImage(img1, 0, 0);
			g_img_combined.DrawImage(img2, img1.Width * 1, 0);
			g_img_combined.DrawImage(img3, img1.Width * 2, 0);

			_drawer.LatestImg = img_combined;
			////gTaskbar.DrawImage(img_combined, 0,0,img1.Width,img1.Height);
			//gTaskbar.DrawImage(img_combined, CliRect.X, CliRect.Y, img_combined.Width, img_combined.Height);


			//preview
			picBox_taskbarPrev.Size = img_combined.Size;
			picBox_taskbarPrev.Image = img_combined;
		}


		///  --draw graph funcs
		static Color col_LabelName = Color.FromArgb(68, 68, 68);
		static Color col_LabelValue = Color.FromArgb(156, 214, 51);
		static Color col_Bar = Color.FromArgb(37, 84, 142);
		//static Color col_backColor = Color.FromArgb(28, 28, 28);
		//win10 - 1809
		static Color col_backColor = Color.FromArgb(16, 16, 16);

		//static Color col_Axis = Color.FromArgb(80, 80, 80); // Dim axis color //where did this come From??
		private void DrawGraphBar(
			Graphics g, Panel panel,
			Queue<float> history,
			float yAxisMaxValue,
			string graphName,
			string unitSymbol,
			float currentValue)
		{

			DrawGraphBar_onBitmap(g,panel.DisplayRectangle,history,yAxisMaxValue,graphName,unitSymbol,currentValue);
		}

		private void DrawGraphBar_onBitmap(
		Graphics g, Rectangle rect,
		Queue<float> history,
		float yAxisMaxValue,
		string graphName,
		string unitSymbol,
		float currentValue)
		{
			//g.SmoothingMode = SmoothingMode.AntiAlias;
			//g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit; // Smoother text
			g.Clear(col_backColor);

			if (!history.Any())
			{
				TextRenderer.DrawText(g, "Collecting data...", SystemFonts.DefaultFont, rect, Color.Gray,
					TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
				return;
			}

			float scaleMax = yAxisMaxValue;

			using (SolidBrush barBrush = new SolidBrush(col_Bar))
			//using (Pen axisPen = new Pen(col_Axis, 1f))
			using (Brush textBrush_Value = new SolidBrush(col_LabelValue))
			using (Brush textBrush_name = new SolidBrush(col_LabelName))
			using (Font valueFont = new Font("Segoe UI", 7f, FontStyle.Bold))  // Font for Current Value
			using (Font nameFont = new Font("Segoe UI", 7f, FontStyle.Bold)) // Font for Graph Name
			{
				float[] points = history.ToArray();
				int dataPointCount = points.Length;

				float slotWidth = (float)rect.Width / MaxHistoryPoints;
				float barWidthRatio = 0.8f;
				float actualBarWidth = Math.Max(1f, slotWidth * barWidthRatio);
				float barSpacing = (slotWidth - actualBarWidth) / 2f;

				for (int i = 0; i < dataPointCount; i++)
				{
					float value = points[i];
					float barHeightPercentage = (value / scaleMax);
					if (value > scaleMax) barHeightPercentage = 1f;
					if (value < 0) barHeightPercentage = 0f;

					float barHeight = barHeightPercentage * rect.Height;
					float xPos = i * slotWidth + barSpacing;
					g.FillRectangle(barBrush, xPos, rect.Height - barHeight, actualBarWidth, barHeight);
				}

				// --- Text Drawing with New Positioning ---
				StringFormat sfCenter = new StringFormat
				{
					Alignment = StringAlignment.Center,     // Horizontal Center
					LineAlignment = StringAlignment.Center  // Vertical Center
				};
				StringFormat sfTopCenter = new StringFormat
				{
					Alignment = StringAlignment.Center,     // Horizontal Center
					LineAlignment = StringAlignment.Near    // Vertical Top
				};

				//int paddingTop = 5; // Padding from the top edge
				int paddingTop = 0; // Padding from the top edge

				// 1. Draw Current Value (e.g., "55.3 %") - Top Center
				string valueText = $"{currentValue:F0}{unitSymbol}";
				valueText = Humanize_Mb_Kb_toString(unitSymbol, currentValue, valueText);

				// Create a rectangle for the top area to center the text within
				RectangleF valueRect = new RectangleF(0, paddingTop, rect.Width, valueFont.Height + paddingTop);
				g.DrawString(valueText, valueFont, textBrush_Value, valueRect, sfTopCenter);


				// 2. Draw Graph Name (e.g., "CPU") - Middle Center
				// Create a rectangle for the entire panel to center the text within
				RectangleF nameRect = new RectangleF(0, 0, rect.Width, rect.Height);
				g.DrawString(graphName, nameFont, textBrush_name, nameRect, sfCenter);
			}
		}

		private static string Humanize_Mb_Kb_toString(string unitSymbol, float currentValue, string valueText)
		{
			bool isunit_MB = unitSymbol.Contains("MB/s");
			bool isunit_KB = unitSymbol.Contains("KB/s");
			if (isunit_MB || isunit_KB)
			{
				valueText = $"{currentValue:F1}{unitSymbol}";
				//cur >= 100 (aka 3digit), remove fraction.
				if (currentValue >= 1)
					valueText = $"{currentValue:F0}{unitSymbol}";

				if (currentValue > 999.9 && isunit_KB)
				{
					valueText = $"{currentValue / 1024.0:F1}MB/s";
					//cur/1024 >= 100 (aka 3digit), remove fraction.
					if (currentValue / 1024.0 >= 10)
						valueText = $"{currentValue / 1024.0:F0}MB/s";

				}
				else if (currentValue > 999.9 && isunit_MB)
				{
					valueText = $"{currentValue / 1024.0:F1}GB/s";
				}
			}

			return valueText;
		}

		private void chk_useCustom_ScreenRes_forRDP_CheckedChanged(object sender, EventArgs e)
		{
			//var chkx = sender as CheckBox;
			//var useCustomRes = chkx.Checked;

			TaskbarDrawer.use_CustomScreenSize = chk_useCustom_ScreenRes_forRDP.Checked;

			TaskbarDrawer.myPC_ScrRes_Height = (int)numud_height.Value;
			TaskbarDrawer.myPC_ScrRes_Width = (int)numud_width.Value;
			TaskbarDrawer.myPC_scaleFactor = (float)numud_dpi_scaleFactor.Value;
		}

		private void bt_apply_customRes_Click(object sender, EventArgs e)
		{
			
			chk_useCustom_ScreenRes_forRDP.Checked = true;

			TaskbarDrawer.use_CustomScreenSize = chk_useCustom_ScreenRes_forRDP.Checked;

			TaskbarDrawer.myPC_ScrRes_Width = (int)numud_width.Value;
			TaskbarDrawer.myPC_ScrRes_Height = (int)numud_height.Value;
			TaskbarDrawer.myPC_scaleFactor = (float)numud_dpi_scaleFactor.Value;
		}

		private void button1_Click(object sender, EventArgs e)
		{
			numud_width.Value = 1460;
			numud_height.Value=1069;
			bt_apply_customRes_Click(null, null);
		}


		// --- menu click toggle show hide
		private void ExitMenuItem_Click(object sender, EventArgs e) => Application.Exit();
		private void appNotifyIcon_DoubleClick(object sender, EventArgs e)
		{
			//Toggle_ShowHide(); 
			//this.Show();
		}

		private void AppNotifyIcon_Click(object sender, EventArgs e)
		{
			
		}

		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			if (e.CloseReason == CloseReason.UserClosing)
			{
				this.Hide();
				e.Cancel = true;
				return;
			}
			
			updateTimer?.Stop();
			_cpuCounter?.Dispose();
			_diskReadCounter?.Dispose();
			_diskWriteCounter?.Dispose();
			_networkSentCounter?.Dispose();
			_networkReceivedCounter?.Dispose();
			appNotifyIcon?.Dispose();
			base.OnFormClosing(e);
		}

		private void appNotifyIcon_MouseClick(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
				Toggle_ShowHide();
		}





		// --- Helper to enable double buffering on controls ---
		public static void SetDoubleBuffered(Control control)
		{
			// Call by reflection
			typeof(Control).InvokeMember("DoubleBuffered",
				System.Reflection.BindingFlags.SetProperty |
				System.Reflection.BindingFlags.Instance |
				System.Reflection.BindingFlags.NonPublic,
				null, control, new object[] { true });
		}


	}



}