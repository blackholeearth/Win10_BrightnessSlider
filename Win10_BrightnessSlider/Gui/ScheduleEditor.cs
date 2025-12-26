using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace Win10_BrightnessSlider.Gui
{
	
    public partial class ScheduleEditor : Form
    {
        private List<BrightnessSchedule> schedules;
        private BrightnessSchedule currentlyEditing;
        private Dictionary<DayOfWeek, CheckBox> dayCheckboxes;
        private bool isLoading = false;  // Prevents event handlers from firing during load

        // Per-monitor brightness controls
        private RadioButton rbAllMonitors;
        private RadioButton rbPerMonitor;
        private Panel panelPerMonitor;
        private Dictionary<string, NumericUpDown> monitorBrightnessControls;  // MonitorId -> NumericUpDown

        public ScheduleEditor()
        {
            InitializeComponent();
            ModernizeUI();
            CreateDayCheckboxes();
            CreatePerMonitorControls();
		}

		

		

		private void ModernizeUI()
        {
			

            // Modern styling
            this.Font = new Font("Segoe UI", 9F);
            this.BackColor = Color.FromArgb(240, 240, 240);

            // Style groupbox
            groupBox1.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            groupBox1.ForeColor = Color.FromArgb(60, 60, 60);

			// Enlarge form for per-monitor controls
			this.ClientSize = new Size(720 , 450 ).ApplyDpiFix_ToSize(); 

            // Reposition main controls
            listBox_Schedules.Font = new Font("Segoe UI", 9F);
            listBox_Schedules.Size = new Size(300 , 380 ).ApplyDpiFix_ToSize(); 

			groupBox1.Location = new Point(318, 12);
            groupBox1.Size = new Size(390 , 380 ).ApplyDpiFix_ToSize();
            groupBox1.Text = "Edit Schedule";

            // Style buttons
            foreach (var btn in new[] { btn_Add, btn_Remove, btn_Save, btn_Cancel })
            {
                btn.Font = new Font("Segoe UI", 9F);
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderColor = Color.FromArgb(100, 100, 100);
                btn.BackColor = Color.White;
                btn.Cursor = Cursors.Hand;
            }

            // Position buttons at bottom
            btn_Add.Location = new Point(12, 405).ApplyDpiFix_ToPoint();
            btn_Remove.Location = new Point(106, 405).ApplyDpiFix_ToPoint();
			btn_Save.Location = new Point(526, 405).ApplyDpiFix_ToPoint();
			btn_Cancel.Location = new Point(620, 405).ApplyDpiFix_ToPoint();

			// Style numeric controls - hide default brightness control (we'll use per-monitor panel)
			numericUpDown_Brightness.Visible = false;
            label3.Visible = false;  // Hide "Brightness" label
            timePicker_Hour.Font = new Font("Segoe UI", 10F);
            timePicker_Minute.Font = new Font("Segoe UI", 10F);
        }

        private void CreateDayCheckboxes()
        {
            dayCheckboxes = new Dictionary<DayOfWeek, CheckBox>();
            var dayNames = new[] { "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat" };
            var days = new[] { DayOfWeek.Sunday, DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday };

            for (int i = 0; i < days.Length; i++)
            {
                var cb = new CheckBox
                {
                    Text = dayNames[i],
                    Location = new System.Drawing.Point(15, 148 + (i * 24)),  // Vertical stack
                    Size = new System.Drawing.Size(70, 20).ApplyDpiFix_ToSize(),
                    Checked = true,
                    Font = new System.Drawing.Font("Segoe UI", 9F),
                    Cursor = Cursors.Hand,
                    ForeColor = System.Drawing.Color.FromArgb(40, 40, 40)
                };
                cb.CheckedChanged += DayCheckbox_CheckedChanged;
                dayCheckboxes[days[i]] = cb;
                groupBox1.Controls.Add(cb);
            }

            // Add Days label with modern styling
            var lblDays = new Label
            {
                Text = "Active Days:",
                Location = new System.Drawing.Point(15, 125).ApplyDpiFix_ToPoint(),
                Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular),
                ForeColor = System.Drawing.Color.FromArgb(60, 60, 60),
                AutoSize = true
            };
            groupBox1.Controls.Add(lblDays);

            // Add quick select buttons
            var btnWeekdays = new Button
            {
                Text = "Weekdays",
                Location = new System.Drawing.Point(95, 148).ApplyDpiFix_ToPoint(),
                Size = new System.Drawing.Size(75, 24).ApplyDpiFix_ToSize(),
                Font = new System.Drawing.Font("Segoe UI", 8F),
                FlatStyle = FlatStyle.Flat,
                BackColor = System.Drawing.Color.FromArgb(230, 230, 230),
                Cursor = Cursors.Hand
            };
            btnWeekdays.FlatAppearance.BorderSize = 1;
            btnWeekdays.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(180, 180, 180);
            btnWeekdays.Click += (s, e) =>
            {
                dayCheckboxes[DayOfWeek.Monday].Checked = true;
                dayCheckboxes[DayOfWeek.Tuesday].Checked = true;
                dayCheckboxes[DayOfWeek.Wednesday].Checked = true;
                dayCheckboxes[DayOfWeek.Thursday].Checked = true;
                dayCheckboxes[DayOfWeek.Friday].Checked = true;
                dayCheckboxes[DayOfWeek.Saturday].Checked = false;
                dayCheckboxes[DayOfWeek.Sunday].Checked = false;
            };
            groupBox1.Controls.Add(btnWeekdays);

            var btnWeekends = new Button
            {
                Text = "Weekends",
                Location = new System.Drawing.Point(95, 176),
                Size = new System.Drawing.Size(75, 24),
                Font = new System.Drawing.Font("Segoe UI", 8F),
                FlatStyle = FlatStyle.Flat,
                BackColor = System.Drawing.Color.FromArgb(230, 230, 230),
                Cursor = Cursors.Hand
            };
            btnWeekends.FlatAppearance.BorderSize = 1;
            btnWeekends.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(180, 180, 180);
            btnWeekends.Click += (s, e) =>
            {
                dayCheckboxes[DayOfWeek.Saturday].Checked = true;
                dayCheckboxes[DayOfWeek.Sunday].Checked = true;
                dayCheckboxes[DayOfWeek.Monday].Checked = false;
                dayCheckboxes[DayOfWeek.Tuesday].Checked = false;
                dayCheckboxes[DayOfWeek.Wednesday].Checked = false;
                dayCheckboxes[DayOfWeek.Thursday].Checked = false;
                dayCheckboxes[DayOfWeek.Friday].Checked = false;
            };
            groupBox1.Controls.Add(btnWeekends);

            var btnAllDays = new Button
            {
                Text = "All Days",
                Location = new System.Drawing.Point(95, 204).ApplyDpiFix_ToPoint(),
                Size = new System.Drawing.Size(75, 24).ApplyDpiFix_ToSize(),
                Font = new System.Drawing.Font("Segoe UI", 8F),
                FlatStyle = FlatStyle.Flat,
                BackColor = System.Drawing.Color.FromArgb(230, 230, 230),
                Cursor = Cursors.Hand
            };
            btnAllDays.FlatAppearance.BorderSize = 1;
            btnAllDays.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(180, 180, 180);
            btnAllDays.Click += (s, e) =>
            {
                foreach (var cb in dayCheckboxes.Values)
                    cb.Checked = true;
            };
            groupBox1.Controls.Add(btnAllDays);

            // Move and style the Enabled checkbox
            checkBox_Enabled.Location = new System.Drawing.Point(15, 100).ApplyDpiFix_ToPoint();
            checkBox_Enabled.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            checkBox_Enabled.ForeColor = System.Drawing.Color.FromArgb(0, 120, 215);

            // Remove the old label4
            label4.Visible = false;
        }

        private void DayCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            if (!isLoading) UpdateCurrentScheduleDays();
        }

        private void CreatePerMonitorControls()
        {
            monitorBrightnessControls = new Dictionary<string, NumericUpDown>();

            // Create brightness section label
            var lblBrightness = new Label
            {
                Text = "Brightness:",
                Location = new Point(200, 20).ApplyDpiFix_ToPoint(),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 60, 60),
                AutoSize = true
            };
            groupBox1.Controls.Add(lblBrightness);

            // Radio button: All Monitors Same
            rbAllMonitors = new RadioButton
            {
                Text = "All Monitors Same:",
                Location = new Point(200, 45).ApplyDpiFix_ToPoint(),
                Size = new Size(140, 22).ApplyDpiFix_ToSize(),
                Font = new Font("Segoe UI", 9F),
                Checked = true,
                Cursor = Cursors.Hand
            };
            rbAllMonitors.CheckedChanged += (s, e) =>
            {
                if (!isLoading && rbAllMonitors.Checked)
                {
                    numericUpDown_Brightness.Visible = true;
                    numericUpDown_Brightness.Location = new Point(340, 43).ApplyDpiFix_ToPoint();
                    panelPerMonitor.Visible = false;
                    UpdateCurrentSchedule();
                }
            };
            groupBox1.Controls.Add(rbAllMonitors);

            // NumericUpDown for "All Monitors" mode - reuse existing but reposition
            numericUpDown_Brightness.Visible = true;
            numericUpDown_Brightness.Location = new Point(340, 43).ApplyDpiFix_ToPoint();
            numericUpDown_Brightness.Size = new Size(60, 25).ApplyDpiFix_ToSize();

            // Radio button: Per-Monitor
            rbPerMonitor = new RadioButton
            {
                Text = "Per-Monitor:",
                Location = new Point(200, 72).ApplyDpiFix_ToPoint(),
                Size = new Size(140, 22).ApplyDpiFix_ToSize(),
                Font = new Font("Segoe UI", 9F),
                Cursor = Cursors.Hand
            };
            rbPerMonitor.CheckedChanged += (s, e) =>
            {
                if (!isLoading && rbPerMonitor.Checked)
                {
                    numericUpDown_Brightness.Visible = false;
                    panelPerMonitor.Visible = true;
                    UpdateCurrentSchedule();
                }
            };
            groupBox1.Controls.Add(rbPerMonitor);

            // Panel for per-monitor controls
            panelPerMonitor = new Panel
            {
                Location = new Point(200, 96).ApplyDpiFix_ToPoint(),
                Size = new Size(180, 180).ApplyDpiFix_ToSize(),
                AutoScroll = true,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White,
                Visible = false
            };
            groupBox1.Controls.Add(panelPerMonitor);

            // Populate with detected monitors
            PopulateMonitorControls();
        }

        private void PopulateMonitorControls()
        {
            panelPerMonitor.Controls.Clear();
            monitorBrightnessControls.Clear();

            var monitors = Form1.riScreens;
            if (monitors == null || monitors.Count == 0)
            {
                var lblNoMonitors = new Label
                {
                    Text = "No monitors detected",
                    Location = new Point(5, 10),
                    AutoSize = true,
                    ForeColor = Color.Gray
                };
                panelPerMonitor.Controls.Add(lblNoMonitors);
                return;
            }

            int yPos = 5;
            int monitorIndex = 1;

            foreach (var riScreen in monitors)
            {
                string monitorId = 
					riScreen.WMIMonitorID?.InstanceName 
					?? riScreen.dc_TargetDeviceName?.monitorDevicePath 
					?? $"Monitor_{monitorIndex}";
                string displayName = GetMonitorDisplayName(riScreen, monitorIndex);

                // Label for monitor name
                var lbl = new Label
                {
                    Text = displayName,
                    Location = new Point(5, yPos + 3).ApplyDpiFix_ToPoint(),
                    Size = new Size(100, 20).ApplyDpiFix_ToSize(),
                    Font = new Font("Segoe UI", 8F),
                    AutoEllipsis = true
                };
                panelPerMonitor.Controls.Add(lbl);

                // NumericUpDown for brightness
                var nud = new NumericUpDown
                {
                    Location = new Point(110, yPos).ApplyDpiFix_ToPoint(),
                    Size = new Size(50, 22).ApplyDpiFix_ToSize(),
                    Minimum = 0,
                    Maximum = 100,
                    Value = 100,
                    Font = new Font("Segoe UI", 8F),
                    Tag = monitorId
                };
                nud.ValueChanged += PerMonitorBrightness_ValueChanged;
                panelPerMonitor.Controls.Add(nud);

                // % label
                var lblPercent = new Label
                {
                    Text = "%",
                    Location = new Point(162, yPos + 3).ApplyDpiFix_ToPoint(),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 8F)
                };
                panelPerMonitor.Controls.Add(lblPercent);

                monitorBrightnessControls[monitorId] = nud;
                yPos += (int)(28*Dpihelper.multiplier);
                monitorIndex++;
            }
        }

        private string GetMonitorDisplayName(RichInfoScreen riScreen, int index)
        {
            // Try to get user-set name first
            var settings = Settings_json.Get();
            if (settings.monitorNames != null)
            {
                var matchingName = settings.monitorNames.FirstOrDefault(m =>
                    m.wmi_InstanceName == riScreen.WMIMonitorID?.InstanceName ||
                    m.dc_monitorDevicePath == riScreen.dc_TargetDeviceName?.monitorDevicePath);

                if (matchingName != null && !string.IsNullOrEmpty(matchingName.MonitorName))
                    return matchingName.MonitorName;
            }

            // Fallback to model name or generic
            if (!string.IsNullOrEmpty(riScreen.avail_MonitorName_clean))
                return riScreen.avail_MonitorName_clean;

            return $"Monitor {index}";
        }

        private void PerMonitorBrightness_ValueChanged(object sender, EventArgs e)
        {
            if (!isLoading) UpdateCurrentSchedule();
        }

        private void ScheduleEditor_Load(object sender, EventArgs e)
        {
            LoadSchedules();
            UpdateScheduleList();
            ClearEditor();
        }

        private void LoadSchedules()
        {
            var settings = Settings_json.Get();
            schedules = settings.BrightnessSchedules ?? new List<BrightnessSchedule>();
        }

        private void UpdateScheduleList()
        {
            listBox_Schedules.Items.Clear();
            foreach (var schedule in schedules)
            {
                var status = schedule.Enabled ? "" : " (Disabled)";
                var days = schedule.GetDaysString();
                var brightness = schedule.GetBrightnessString();
                listBox_Schedules.Items.Add($"{schedule.Time} → {brightness} - {days}{status}");
            }
        }

        private void listBox_Schedules_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox_Schedules.SelectedIndex >= 0 && listBox_Schedules.SelectedIndex < schedules.Count)
            {
                currentlyEditing = schedules[listBox_Schedules.SelectedIndex];
                LoadScheduleToEditor(currentlyEditing);
            }
        }

        private void LoadScheduleToEditor(BrightnessSchedule schedule)
        {
            isLoading = true;  // Prevent event handlers from firing during load

            var time = schedule.GetTimeSpan();
            timePicker_Hour.Value = time.Hours;
            timePicker_Minute.Value = time.Minutes;
            numericUpDown_Brightness.Value = schedule.BrightnessPercent;
            checkBox_Enabled.Checked = schedule.Enabled;

            // Load days - deduplicate them first
            var daysToLoad = schedule.Days != null && schedule.Days.Count > 0
                ? schedule.Days.Distinct().ToList()
                : new List<DayOfWeek> { DayOfWeek.Sunday, DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday };

            foreach (var kvp in dayCheckboxes)
            {
                kvp.Value.Checked = daysToLoad.Contains(kvp.Key);
            }

            // Clean up the schedule's Days list immediately
            schedule.Days = daysToLoad;

            // Load per-monitor brightness settings
            if (schedule.ApplyToAllMonitors)
            {
                rbAllMonitors.Checked = true;
                numericUpDown_Brightness.Visible = true;
                panelPerMonitor.Visible = false;
            }
            else
            {
                rbPerMonitor.Checked = true;
                numericUpDown_Brightness.Visible = false;
                panelPerMonitor.Visible = true;

                // Load per-monitor values
                if (schedule.PerMonitorBrightness != null)
                {
                    foreach (var entry in schedule.PerMonitorBrightness)
                    {
                        if (monitorBrightnessControls.TryGetValue(entry.MonitorId, out var nud))
                        {
                            nud.Value = Math.Max(0, Math.Min(100, entry.BrightnessPercent));
                        }
                    }
                }
            }

            isLoading = false;  // Re-enable event handlers
        }

        private void ClearEditor()
        {
            isLoading = true;  // Prevent event handlers from firing during clear

            timePicker_Hour.Value = 0;
            timePicker_Minute.Value = 0;
            numericUpDown_Brightness.Value = 100;
            checkBox_Enabled.Checked = true;

            // Reset all day checkboxes
            foreach (var cb in dayCheckboxes.Values)
            {
                cb.Checked = true;
            }

            // Reset per-monitor controls
            rbAllMonitors.Checked = true;
            numericUpDown_Brightness.Visible = true;
            panelPerMonitor.Visible = false;
            foreach (var nud in monitorBrightnessControls.Values)
            {
                nud.Value = 100;
            }

            currentlyEditing = null;
            isLoading = false;
        }

        private void btn_Add_Click(object sender, EventArgs e)
        {
            var selectedDays = dayCheckboxes.Where(kvp => kvp.Value.Checked).Select(kvp => kvp.Key).ToList();

            var newSchedule = new BrightnessSchedule
            {
                Time = $"{(int)timePicker_Hour.Value:D2}:{(int)timePicker_Minute.Value:D2}",
                BrightnessPercent = (int)numericUpDown_Brightness.Value,
                Enabled = checkBox_Enabled.Checked,
                Days = selectedDays,
                ApplyToAllMonitors = rbAllMonitors.Checked,
                PerMonitorBrightness = GetPerMonitorBrightnessEntries()
            };

            schedules.Add(newSchedule);
            SaveSchedules();
            UpdateScheduleList();
            listBox_Schedules.SelectedIndex = schedules.Count - 1;
        }

        private List<MonitorBrightnessEntry> GetPerMonitorBrightnessEntries()
        {
            var entries = new List<MonitorBrightnessEntry>();
            var monitors = Form1.riScreens;

            if (monitors == null) return entries;

            int monitorIndex = 1;
            foreach (var riScreen in monitors)
            {
                string monitorId = riScreen.WMIMonitorID?.InstanceName ?? riScreen.dc_TargetDeviceName?.monitorDevicePath ?? $"Monitor_{monitorIndex}";
                string displayName = GetMonitorDisplayName(riScreen, monitorIndex);

                int brightness = 100;
                if (monitorBrightnessControls.TryGetValue(monitorId, out var nud))
                {
                    brightness = (int)nud.Value;
                }

                entries.Add(new MonitorBrightnessEntry
                {
                    MonitorId = monitorId,
                    MonitorName = displayName,
                    BrightnessPercent = brightness
                });

                monitorIndex++;
            }

            return entries;
        }

        private void btn_Remove_Click(object sender, EventArgs e)
        {
            if (listBox_Schedules.SelectedIndex >= 0)
            {
                schedules.RemoveAt(listBox_Schedules.SelectedIndex);
                SaveSchedules();
                UpdateScheduleList();
                ClearEditor();
            }
        }

        private void btn_Save_Click(object sender, EventArgs e)
        {
            SaveSchedules();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btn_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void SaveSchedules()
        {
            Settings_json.Update(st =>
            {
                st.BrightnessSchedules = schedules;
            });
        }

        private void timePicker_ValueChanged(object sender, EventArgs e)
        {
            if (!isLoading) UpdateCurrentSchedule();
        }

        private void numericUpDown_Brightness_ValueChanged(object sender, EventArgs e)
        {
            if (!isLoading) UpdateCurrentSchedule();
        }

        private void checkBox_Enabled_CheckedChanged(object sender, EventArgs e)
        {
            if (!isLoading) UpdateCurrentSchedule();
        }

        private void UpdateCurrentSchedule()
        {
            if (currentlyEditing != null)
            {
                currentlyEditing.Time = $"{(int)timePicker_Hour.Value:D2}:{(int)timePicker_Minute.Value:D2}";
                currentlyEditing.BrightnessPercent = (int)numericUpDown_Brightness.Value;
                currentlyEditing.Enabled = checkBox_Enabled.Checked;
                currentlyEditing.ApplyToAllMonitors = rbAllMonitors.Checked;
                currentlyEditing.PerMonitorBrightness = GetPerMonitorBrightnessEntries();
                UpdateCurrentScheduleDays();
                SaveSchedules();
                UpdateScheduleList();
            }
        }

        private void UpdateCurrentScheduleDays()
        {
            if (currentlyEditing != null)
            {
                var selectedDays = dayCheckboxes.Where(kvp => kvp.Value.Checked).Select(kvp => kvp.Key).ToList();
                currentlyEditing.Days = selectedDays;
            }
        }
    }
}


public static class Dpihelper
{
	// this programamtic ui setup is for 96dpi iguess..!??!
	public static float design_dpi = 96;
	public static float currentdpi = 120;  // make it programatically i know mine its 120
	public static float multiplier => currentdpi / design_dpi;

	public static Size ApplyDpiFix_ToSize(this Size size)
	{
		return new SizeF(size.Width * multiplier, size.Height * multiplier).ToSize();
	}

	public static Point ApplyDpiFix_ToPoint(this Point pt)
	{
		return new Point((int)(pt.X * multiplier), (int)(pt.Y * multiplier));
	}

}