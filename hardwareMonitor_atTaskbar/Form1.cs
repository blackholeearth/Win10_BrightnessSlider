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
	public partial class Form1 : Form
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
		public string FormattedStringForTaskbar { get; set; }

		// Event to notify when data is updated
		public event EventHandler DataUpdated;

		// --- NEW: Data history for graphs ---
		private const int MaxHistoryPoints = 50;
		private Queue<float> _cpuHistory = new();
		private Queue<float> _diskHistory = new();
		private Queue<float> _networkHistory = new();
		// --- END NEW ---

		private ContextMenuStrip trayContextMenuStrip = new();
		private Timer updateTimer = new Timer();

		public TaskbarDrawer _drawer = new();


		//not used currently.
		//public OverlayForm _overlayForm = new(null);


		public Form1()
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

			var mi_pos = trayContextMenuStrip.Items.Add("Toggle Location: Left/Right", null,
				(s1, e1) => { TaskbarDrawer.location_AtRight = !TaskbarDrawer.location_AtRight; });

			var mi_hide = trayContextMenuStrip.Items.Add("Show/Hide", null, (s1, e1) => Toggle_ShowHide());
			var mi_sep1 = trayContextMenuStrip.Items.Add("-");
			var mi_exit = trayContextMenuStrip.Items.Add("Exit", null, ExitMenuItem_Click);
			appNotifyIcon.Visible = true;

			appNotifyIcon.ContextMenuStrip = trayContextMenuStrip;
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
				_cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
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
			DrawGraphBar_onBitmap(g_img1, rect1, _cpuHistory, 100f, "CPU", "%", CurrentCpuUsage);

			//DRAW DISK
			var rect2 = new Rectangle(0, 0, (MaxHistoryPoints + 2), CliRect.Height);

			float maxDisk = _diskHistory.Any() ? _diskHistory.Max() : 10f; 
			if (maxDisk < 1.0f) maxDisk = 1.0f;
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


		private void onTaskbar_Paint__dis(Graphics gTaskbar, Rectangle CliRect)
		{
			if(img1 is null)
				initBitmaps(CliRect);


			//DRAW CPU
			var rect1 = new Rectangle(0, 0, (MaxHistoryPoints + 2), CliRect.Height);
			DrawGraphBar_onBitmap(g_img1, rect1, _cpuHistory, 100f, "CPU", "%", CurrentCpuUsage);

			//DRAW DISK
			var rect2 = new Rectangle(0, 0, (MaxHistoryPoints + 2), CliRect.Height);

			float maxDisk = _diskHistory.Any() ? _diskHistory.Max() : 10f; // Use .Any() to check if empty
			if (maxDisk < 1.0f) maxDisk = 1.0f;
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

			//to taskbar
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

		//private void DrawGraphLine(Graphics g, Panel panel, Queue<float> history, Color lineColor, float maxValue, string unitLabel)
		//{
		//	g.SmoothingMode = SmoothingMode.AntiAlias;
		//	g.Clear(panel.BackColor); // Or a specific graph background like Color.FromArgb(30,30,30)

		//	if (history.Count < 2) // Not enough points to draw a line
		//	{
		//		TextRenderer.DrawText(g, "Collecting data...", panel.Font, panel.ClientRectangle, Color.Gray, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
		//		return;
		//	}


		//	// Determine actual max value in current history for scaling, but cap by provided maxValue
		//	float currentDynamicMax = history.Max();
		//	if (currentDynamicMax == 0) currentDynamicMax = maxValue; // Avoid division by zero if all are 0
		//	else currentDynamicMax = Math.Max(currentDynamicMax, 0.1f); // Ensure a small minimum scale

		//	// If explicit maxValue is given (like 100 for CPU), use that for the ceiling.
		//	// Otherwise, scale to the dynamic max.
		//	float scaleMax = (unitLabel == "CPU %") ? 100f : Math.Max(currentDynamicMax, maxValue); // Use 100 for CPU, dynamic for others but ensure it's at least 'maxValue' if passed
		//	if (scaleMax < 1.0f && unitLabel != "CPU %") scaleMax = 1.0f; // Min scale for MB/s to avoid overly sensitive graphs for tiny values


		//	using (Pen linePen = new Pen(lineColor, 1.5f))
		//	using (Pen axisPen = new Pen(Color.FromArgb(100, Color.Gray), 1f)) // Lighter axis pen
		//	using (Brush textBrush = new SolidBrush(Color.FromArgb(200, panel.ForeColor))) // Slightly transparent text
		//	using (Font smallFont = new Font(panel.Font.FontFamily, 7f))
		//	{
		//		float pointWidth = (float)panel.Width / Math.Max(1, MaxHistoryPoints - 1); // Width per point slot
		//		PointF? prevPoint = null;

		//		for (int i = 0; i < history.Count; i++)
		//		{
		//			float x = i * pointWidth;
		//			// Invert Y: 0 is at top, panel.Height is at bottom
		//			// Scale value: (history[i] / scaleMax) gives ratio, multiply by panel.Height
		//			float y = panel.Height - (Math.Min(history.ElementAt(i), scaleMax) / scaleMax) * panel.Height; // Cap value at scaleMax

		//			PointF currentPoint = new PointF(x, y);

		//			if (prevPoint.HasValue)
		//			{
		//				g.DrawLine(linePen, prevPoint.Value, currentPoint);
		//			}
		//			prevPoint = currentPoint;
		//		}

		//		// Draw a faint horizontal line for 0 if needed (our current Y=0 is bottom of panel)
		//		// g.DrawLine(axisPen, 0, panel.Height-1, panel.Width, panel.Height-1);
		//		// Optionally, draw a faint line for the max scale
		//		g.DrawLine(axisPen, 0, 0, panel.Width, 0);


		//		// Draw Y-axis labels (0 and max)
		//		g.DrawString("0", smallFont, textBrush, 2, panel.Height - smallFont.Height - 2);
		//		g.DrawString($"{scaleMax:F0} {unitLabel.Split(' ')[1]}", smallFont, textBrush, 2, 2); // Display unit on max label
		//	}
		//}


		//color theme for  win10 dark.

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
			//g.SmoothingMode = SmoothingMode.AntiAlias;
			//g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit; // Smoother text
			g.Clear(col_backColor);

			if (!history.Any())
			{
				TextRenderer.DrawText(g, "Collecting data...", panel.Font, panel.ClientRectangle, Color.Gray,
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

				float slotWidth = (float)panel.Width / MaxHistoryPoints;
				float barWidthRatio = 0.8f;
				float actualBarWidth = Math.Max(1f, slotWidth * barWidthRatio);
				float barSpacing = (slotWidth - actualBarWidth) / 2f;

				for (int i = 0; i < dataPointCount; i++)
				{
					float value = points[i];
					float barHeightPercentage = (value / scaleMax);
					if (value > scaleMax) barHeightPercentage = 1f;
					if (value < 0) barHeightPercentage = 0f;

					float barHeight = barHeightPercentage * panel.Height;
					float xPos = i * slotWidth + barSpacing;
					g.FillRectangle(barBrush, xPos, panel.Height - barHeight, actualBarWidth, barHeight);
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
				string valueText = $"{currentValue:F0} {unitSymbol}";
				if (unitSymbol.Contains("MB/s") || unitSymbol.Contains("KB/s"))
				{
					valueText = $"{currentValue:F1} {unitSymbol}";
					if (currentValue > 999.9 && unitSymbol.Contains("KB/s"))
					{
						valueText = $"{currentValue / 1024.0:F1} MB/s";
					}
					else if (currentValue > 999.9 && unitSymbol.Contains("MB/s"))
					{
						valueText = $"{currentValue / 1024.0:F1} GB/s";
					}
				}
				// Create a rectangle for the top area to center the text within
				RectangleF valueRect = new RectangleF(0, paddingTop, panel.Width, valueFont.Height + paddingTop);
				g.DrawString(valueText, valueFont, textBrush_Value, valueRect, sfTopCenter);


				// 2. Draw Graph Name (e.g., "CPU") - Middle Center
				// Create a rectangle for the entire panel to center the text within
				RectangleF nameRect = new RectangleF(0, 0, panel.Width, panel.Height);
				g.DrawString(graphName, nameFont, textBrush_name, nameRect, sfCenter);
			}
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





		// --- NEW: Helper to enable double buffering on controls ---
		public static void SetDoubleBuffered(Control control)
		{
			// Call by reflection
			typeof(Control).InvokeMember("DoubleBuffered",
				System.Reflection.BindingFlags.SetProperty |
				System.Reflection.BindingFlags.Instance |
				System.Reflection.BindingFlags.NonPublic,
				null, control, new object[] { true });
		}
		// --- END NEW ---


	}



}