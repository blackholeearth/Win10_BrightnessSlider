using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Win10_BrightnessSlider.Gui; // Ensure this namespace exists for ScheduleEditor
using Win10_BrightnessSlider.Properties; // For Resources if needed

namespace Win10_BrightnessSlider.z_Schedule
{
	public class SchedulerAddon
	{
		private Form1 _form;
		private System.Windows.Forms.Timer schedulerTimer;
		private DateTime lastScheduleCheck = DateTime.MinValue;

		public SchedulerAddon(Form1 form)
		{
			_form = form;
		}

		public void Initialize()
		{
			// 1. Inject Menu Items
			InjectMenuItems();

			// 2. Start Timer
			StartTimer();
		}

		private void InjectMenuItems()
		{
			// Safety check for the Context Menu
			if (_form.notifyIcon_bright_ContextMenuStrip == null)
				throw new Exception("Context Menu is null. Form1_Load might be running out of order.");

			var cms = _form.notifyIcon_bright_ContextMenuStrip;

			// 1. Find the "Extras" menu item
			// Using 'Contains' is safer than '==' in case of hidden characters or ampersands (&Extras)
			var extrasMenu = cms.Items.OfType<ToolStripMenuItem>()
								.FirstOrDefault(x => x.Text.Contains("Extras"));

			if (extrasMenu == null)
			{
				// Debugging: List available items in the error message
				string available = string.Join(", ", cms.Items.OfType<ToolStripItem>().Select(x => x.Text));
				throw new Exception($"'Extras' menu item not found. Found: [{available}]");
			}

			// 2. Calculate the Exact Insertion Index
			// We want to insert BEFORE "___ReMap Keys___" to match your original code location.
			var dropdownItems = extrasMenu.DropDown.Items;
			int insertIndex = -1;

			for (int i = 0; i < dropdownItems.Count; i++)
			{
				// Search for the ReMap Keys header
				if (dropdownItems[i].Text.Contains("ReMap Keys"))
				{
					insertIndex = i;
					break;
				}
			}

			// If "ReMap Keys" isn't found (e.g., deleted), default to the end of the list
			if (insertIndex == -1) insertIndex = dropdownItems.Count;

			// 3. Create the Scheduler Items
			var headerItem = new ToolStripMenuItem("___Scheduled Brightness___") { Enabled = false };
			var schedItem = CreateScheduledBrightnessMenuItem();
			var editItem = CreateEditSchedulesMenuItem();

			// --- FIX IS HERE: Create a real Separator object ---
			var separator = new ToolStripSeparator();
			//var separator = new ToolStripMenuItem("-");  //this doesnt show seperator. it adds "-" as text //my custom code doesnt know how to handle it.
			//mi_extras.DropDown.Items.Add("-");// this code works but here it says eeror cant find mi_extrast

			// 4. Insert them in order
			// The original order was: Header -> Scheduler -> Edit -> Separator -> ReMap Keys...

			// Note: We increment 'insertIndex' after each add to keep them sequential
			extrasMenu.DropDown.Items.Insert(insertIndex++, headerItem);
			extrasMenu.DropDown.Items.Insert(insertIndex++, schedItem);
			extrasMenu.DropDown.Items.Insert(insertIndex++, editItem);
			extrasMenu.DropDown.Items.Insert(insertIndex++, separator);
		}
		private void StartTimer()
		{
			schedulerTimer = new System.Windows.Forms.Timer();
			schedulerTimer.Interval = 60000; // Check every minute
			schedulerTimer.Tick += SchedulerTimer_Tick;
			schedulerTimer.Start();

			// Run check immediately
			Task.Run(() => CheckAndApplySchedulesAtStartup());
		}

		// ... [Rest of the logic remains the same] ...

		// PASTE THE REST OF THE HELPER FUNCTIONS HERE (CreateScheduledBrightnessMenuItem, etc.)
		// Ensure you include the Settings_json logic below:

		private ToolStripMenuItem CreateScheduledBrightnessMenuItem()
		{
			var settings = Settings_json.Get();
			var mi_scheduler = new ToolStripMenuItem("Auto-Dim Schedule");
			mi_scheduler.Checked = settings.EnableScheduledBrightness;
			mi_scheduler.ToolTipText = "Automatically adjust brightness at scheduled times";

			mi_scheduler.Click += (snd, ev) =>
			{
				var _mi = snd as ToolStripMenuItem;
				_mi.Checked = !_mi.Checked;

				Settings_json.Update(st =>
				{
					st.EnableScheduledBrightness = _mi.Checked;

					if (_mi.Checked && (st.BrightnessSchedules == null || st.BrightnessSchedules.Count == 0))
					{
						st.BrightnessSchedules = new List<BrightnessSchedule>
						{
							new BrightnessSchedule { Time = "23:00", BrightnessPercent = 25, Enabled = true },
							new BrightnessSchedule { Time = "07:00", BrightnessPercent = 100, Enabled = true }
						};
					}
				});

				if (_mi.Checked)
					MessageBox.Show("Scheduled brightness enabled!", "Auto-Dim Schedule", MessageBoxButtons.OK, MessageBoxIcon.Information);
			};

			return mi_scheduler;
		}

		private ToolStripMenuItem CreateEditSchedulesMenuItem()
		{
			var mi_editSchedules = new ToolStripMenuItem("Edit Schedules...");
			mi_editSchedules.Click += (snd, ev) =>
			{
				using (var editor = new Gui.ScheduleEditor())
				{
					editor.ShowDialog();
				}
			};
			return mi_editSchedules;
		}

		private void SchedulerTimer_Tick(object sender, EventArgs e)
		{
			CheckAndApplySchedules();
		}

		private void CheckAndApplySchedules()
		{
			try
			{
				var settings = Settings_json.Get();
				if (!settings.EnableScheduledBrightness || settings.BrightnessSchedules == null)
					return;

				var now = DateTime.Now;
				var currentMinute = new TimeSpan(now.Hour, now.Minute, 0);

				if (lastScheduleCheck.Hour == now.Hour && lastScheduleCheck.Minute == now.Minute)
					return;

				lastScheduleCheck = now;

				foreach (var schedule in settings.BrightnessSchedules.Where(s => s.Enabled && s.AppliesToday()))
				{
					var scheduleTime = schedule.GetTimeSpan();
					var scheduleMinute = new TimeSpan(scheduleTime.Hours, scheduleTime.Minutes, 0);

					if (scheduleMinute == currentMinute)
					{
						ApplyScheduledBrightnessFromSchedule(schedule);
						break;
					}
				}
			}
			catch (Exception ex) { RamLogger.Log("Scheduler Tick Error: " + ex.Message); }
		}

		private void CheckAndApplySchedulesAtStartup()
		{
			try
			{
				var settings = Settings_json.Get();
				if (!settings.EnableScheduledBrightness || settings.BrightnessSchedules == null || settings.BrightnessSchedules.Count == 0)
					return;

				var now = DateTime.Now;
				var currentTime = now.TimeOfDay;

				BrightnessSchedule mostRecentSchedule = null;
				TimeSpan? mostRecentTime = null;

				// Simple logic: Find the latest schedule that has already passed today
				foreach (var schedule in settings.BrightnessSchedules.Where(s => s.Enabled && s.AppliesToday()))
				{
					var scheduleTime = schedule.GetTimeSpan();
					if (scheduleTime <= currentTime)
					{
						if (mostRecentTime == null || scheduleTime > mostRecentTime.Value)
						{
							mostRecentTime = scheduleTime;
							mostRecentSchedule = schedule;
						}
					}
				}

				if (mostRecentSchedule != null)
				{
					System.Threading.Thread.Sleep(500);
					ApplyScheduledBrightnessFromSchedule(mostRecentSchedule);
				}
			}
			catch (Exception ex) { RamLogger.Log("Scheduler Startup Error: " + ex.Message); }
		}

		private void ApplyScheduledBrightnessFromSchedule(BrightnessSchedule schedule)
		{
			if (Form1.riScreens == null || Form1.riScreens.Count == 0) return;

			if (schedule.ApplyToAllMonitors)
				ApplyScheduledBrightnessToAll(schedule.BrightnessPercent);
			else
				ApplyPerMonitorBrightness(schedule);
		}

		private void ApplyScheduledBrightnessToAll(int brightness)
		{
			var tasks = new List<Task>();
			foreach (var riScreen in Form1.riScreens)
			{
				var screen = riScreen;
				tasks.Add(Task.Run(() => screen.SetBrightness(brightness, false)));
			}
			Task.WaitAll(tasks.ToArray());
			_form.Invoke((Action)(() => { _form.GUI_Update__AllSliderControls(); _form.GUI_Update_NotifyIconText(); }));
		}

		private void ApplyPerMonitorBrightness(BrightnessSchedule schedule)
		{
			if (schedule.PerMonitorBrightness == null) return;
			var tasks = new List<Task>();
			foreach (var riScreen in Form1.riScreens)
			{
				var monitorId = riScreen.WMIMonitorID?.InstanceName ?? riScreen.dc_TargetDeviceName?.monitorDevicePath;
				var entry = schedule.PerMonitorBrightness.FirstOrDefault(e => e.MonitorId == monitorId);
				int brightness = entry?.BrightnessPercent ?? schedule.BrightnessPercent;
				var screen = riScreen;
				tasks.Add(Task.Run(() => screen.SetBrightness(brightness, false)));
			}
			Task.WaitAll(tasks.ToArray());
			_form.Invoke((Action)(() => { _form.GUI_Update__AllSliderControls(); _form.GUI_Update_NotifyIconText(); }));
		}
	}
}