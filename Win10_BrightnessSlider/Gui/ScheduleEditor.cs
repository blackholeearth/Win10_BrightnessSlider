using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Win10_BrightnessSlider.Gui
{
    public partial class ScheduleEditor : Form
    {
        private List<BrightnessSchedule> schedules;
        private BrightnessSchedule currentlyEditing;
        private Dictionary<DayOfWeek, CheckBox> dayCheckboxes;

        public ScheduleEditor()
        {
            InitializeComponent();
            ModernizeUI();
            CreateDayCheckboxes();
        }

        private void ModernizeUI()
        {
            // Modern styling
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);

            // Style groupbox
            groupBox1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            groupBox1.ForeColor = System.Drawing.Color.FromArgb(60, 60, 60);

            // Enlarge form for better spacing
            this.ClientSize = new System.Drawing.Size(560, 350);

            // Reposition main controls
            listBox_Schedules.Font = new System.Drawing.Font("Segoe UI", 9F);
            listBox_Schedules.Size = new System.Drawing.Size(300, 280);

            groupBox1.Location = new System.Drawing.Point(318, 12);
            groupBox1.Size = new System.Drawing.Size(230, 280);

            // Style buttons
            foreach (var btn in new[] { btn_Add, btn_Remove, btn_Save, btn_Cancel })
            {
                btn.Font = new System.Drawing.Font("Segoe UI", 9F);
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(100, 100, 100);
                btn.BackColor = System.Drawing.Color.White;
                btn.Cursor = Cursors.Hand;
            }

            // Position buttons at bottom
            btn_Add.Location = new System.Drawing.Point(12, 300);
            btn_Remove.Location = new System.Drawing.Point(106, 300);
            btn_Save.Location = new System.Drawing.Point(366, 300);
            btn_Cancel.Location = new System.Drawing.Point(460, 300);

            // Style numeric controls
            numericUpDown_Brightness.Font = new System.Drawing.Font("Segoe UI", 10F);
            timePicker_Hour.Font = new System.Drawing.Font("Segoe UI", 10F);
            timePicker_Minute.Font = new System.Drawing.Font("Segoe UI", 10F);
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
                    Size = new System.Drawing.Size(70, 20),
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
                Location = new System.Drawing.Point(15, 125),
                Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular),
                ForeColor = System.Drawing.Color.FromArgb(60, 60, 60),
                AutoSize = true
            };
            groupBox1.Controls.Add(lblDays);

            // Add quick select buttons
            var btnWeekdays = new Button
            {
                Text = "Weekdays",
                Location = new System.Drawing.Point(95, 148),
                Size = new System.Drawing.Size(75, 24),
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
                Location = new System.Drawing.Point(95, 204),
                Size = new System.Drawing.Size(75, 24),
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
            checkBox_Enabled.Location = new System.Drawing.Point(15, 100);
            checkBox_Enabled.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            checkBox_Enabled.ForeColor = System.Drawing.Color.FromArgb(0, 120, 215);

            // Remove the old label4
            label4.Visible = false;
        }

        private void DayCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            UpdateCurrentScheduleDays();
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
                listBox_Schedules.Items.Add($"{schedule.Time} → {schedule.BrightnessPercent}% - {days}{status}");
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
            // Temporarily unhook events to prevent duplicate saves during load
            foreach (var kvp in dayCheckboxes)
            {
                kvp.Value.CheckedChanged -= DayCheckbox_CheckedChanged;
            }

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

            // Re-hook events
            foreach (var kvp in dayCheckboxes)
            {
                kvp.Value.CheckedChanged += DayCheckbox_CheckedChanged;
            }
        }

        private void ClearEditor()
        {
            // Temporarily unhook day checkbox events
            foreach (var kvp in dayCheckboxes)
            {
                kvp.Value.CheckedChanged -= DayCheckbox_CheckedChanged;
            }

            timePicker_Hour.Value = 0;
            timePicker_Minute.Value = 0;
            numericUpDown_Brightness.Value = 100;
            checkBox_Enabled.Checked = true;

            // Reset all day checkboxes
            foreach (var cb in dayCheckboxes.Values)
            {
                cb.Checked = true;
            }

            currentlyEditing = null;

            // Re-hook day checkbox events
            foreach (var kvp in dayCheckboxes)
            {
                kvp.Value.CheckedChanged += DayCheckbox_CheckedChanged;
            }
        }

        private void btn_Add_Click(object sender, EventArgs e)
        {
            var selectedDays = dayCheckboxes.Where(kvp => kvp.Value.Checked).Select(kvp => kvp.Key).ToList();

            var newSchedule = new BrightnessSchedule
            {
                Time = $"{(int)timePicker_Hour.Value:D2}:{(int)timePicker_Minute.Value:D2}",
                BrightnessPercent = (int)numericUpDown_Brightness.Value,
                Enabled = checkBox_Enabled.Checked,
                Days = selectedDays
            };

            schedules.Add(newSchedule);
            SaveSchedules();
            UpdateScheduleList();
            listBox_Schedules.SelectedIndex = schedules.Count - 1;
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
            UpdateCurrentSchedule();
        }

        private void numericUpDown_Brightness_ValueChanged(object sender, EventArgs e)
        {
            UpdateCurrentSchedule();
        }

        private void checkBox_Enabled_CheckedChanged(object sender, EventArgs e)
        {
            UpdateCurrentSchedule();
        }

        private void UpdateCurrentSchedule()
        {
            if (currentlyEditing != null)
            {
                currentlyEditing.Time = $"{(int)timePicker_Hour.Value:D2}:{(int)timePicker_Minute.Value:D2}";
                currentlyEditing.BrightnessPercent = (int)numericUpDown_Brightness.Value;
                currentlyEditing.Enabled = checkBox_Enabled.Checked;
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
