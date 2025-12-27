namespace Win10_BrightnessSlider.Gui
{
    partial class ScheduleEditor
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
			this.listBox_Schedules = new System.Windows.Forms.ListBox();
			this.btn_Add = new System.Windows.Forms.Button();
			this.btn_Remove = new System.Windows.Forms.Button();
			this.btn_Save = new System.Windows.Forms.Button();
			this.btn_Cancel = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.timePicker_Hour = new System.Windows.Forms.NumericUpDown();
			this.timePicker_Minute = new System.Windows.Forms.NumericUpDown();
			this.checkBox_Enabled = new System.Windows.Forms.CheckBox();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.label4 = new System.Windows.Forms.Label();
			this.numericUpDown_Brightness = new System.Windows.Forms.NumericUpDown();
			((System.ComponentModel.ISupportInitialize)(this.timePicker_Hour)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.timePicker_Minute)).BeginInit();
			this.groupBox1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDown_Brightness)).BeginInit();
			this.SuspendLayout();
			// 
			// listBox_Schedules
			// 
			this.listBox_Schedules.FormattingEnabled = true;
			this.listBox_Schedules.ItemHeight = 16;
			this.listBox_Schedules.Location = new System.Drawing.Point(14, 13);
			this.listBox_Schedules.Name = "listBox_Schedules";
			this.listBox_Schedules.Size = new System.Drawing.Size(319, 260);
			this.listBox_Schedules.TabIndex = 0;
			this.listBox_Schedules.SelectedIndexChanged += new System.EventHandler(this.listBox_Schedules_SelectedIndexChanged);
			// 
			// btn_Add
			// 
			this.btn_Add.Location = new System.Drawing.Point(14, 279);
			this.btn_Add.Name = "btn_Add";
			this.btn_Add.Size = new System.Drawing.Size(101, 32);
			this.btn_Add.TabIndex = 1;
			this.btn_Add.Text = "Add";
			this.btn_Add.UseVisualStyleBackColor = true;
			this.btn_Add.Click += new System.EventHandler(this.btn_Add_Click);
			// 
			// btn_Remove
			// 
			this.btn_Remove.Location = new System.Drawing.Point(121, 279);
			this.btn_Remove.Name = "btn_Remove";
			this.btn_Remove.Size = new System.Drawing.Size(101, 32);
			this.btn_Remove.TabIndex = 2;
			this.btn_Remove.Text = "Remove";
			this.btn_Remove.UseVisualStyleBackColor = true;
			this.btn_Remove.Click += new System.EventHandler(this.btn_Remove_Click);
			// 
			// btn_Save
			// 
			this.btn_Save.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btn_Save.Location = new System.Drawing.Point(599, 279);
			this.btn_Save.Name = "btn_Save";
			this.btn_Save.Size = new System.Drawing.Size(101, 32);
			this.btn_Save.TabIndex = 3;
			this.btn_Save.Text = "Save";
			this.btn_Save.UseVisualStyleBackColor = true;
			this.btn_Save.Click += new System.EventHandler(this.btn_Save_Click);
			// 
			// btn_Cancel
			// 
			this.btn_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btn_Cancel.Location = new System.Drawing.Point(707, 279);
			this.btn_Cancel.Name = "btn_Cancel";
			this.btn_Cancel.Size = new System.Drawing.Size(101, 32);
			this.btn_Cancel.TabIndex = 4;
			this.btn_Cancel.Text = "Cancel";
			this.btn_Cancel.UseVisualStyleBackColor = true;
			this.btn_Cancel.Click += new System.EventHandler(this.btn_Cancel_Click);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(17, 30);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(41, 16);
			this.label1.TabIndex = 5;
			this.label1.Text = "Time:";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(139, 30);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(10, 16);
			this.label3.TabIndex = 7;
			this.label3.Text = ":";
			// 
			// timePicker_Hour
			// 
			this.timePicker_Hour.Location = new System.Drawing.Point(67, 28);
			this.timePicker_Hour.Maximum = new decimal(new int[] {
            23,
            0,
            0,
            0});
			this.timePicker_Hour.Name = "timePicker_Hour";
			this.timePicker_Hour.Size = new System.Drawing.Size(65, 22);
			this.timePicker_Hour.TabIndex = 8;
			this.timePicker_Hour.ValueChanged += new System.EventHandler(this.timePicker_ValueChanged);
			// 
			// timePicker_Minute
			// 
			this.timePicker_Minute.Location = new System.Drawing.Point(158, 28);
			this.timePicker_Minute.Maximum = new decimal(new int[] {
            59,
            0,
            0,
            0});
			this.timePicker_Minute.Name = "timePicker_Minute";
			this.timePicker_Minute.Size = new System.Drawing.Size(65, 22);
			this.timePicker_Minute.TabIndex = 9;
			this.timePicker_Minute.ValueChanged += new System.EventHandler(this.timePicker_ValueChanged);
			// 
			// checkBox_Enabled
			// 
			this.checkBox_Enabled.AutoSize = true;
			this.checkBox_Enabled.Location = new System.Drawing.Point(17, 111);
			this.checkBox_Enabled.Name = "checkBox_Enabled";
			this.checkBox_Enabled.Size = new System.Drawing.Size(80, 20);
			this.checkBox_Enabled.TabIndex = 11;
			this.checkBox_Enabled.Text = "Enabled";
			this.checkBox_Enabled.UseVisualStyleBackColor = true;
			this.checkBox_Enabled.CheckedChanged += new System.EventHandler(this.checkBox_Enabled_CheckedChanged);
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox1.Controls.Add(this.label4);
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.Controls.Add(this.checkBox_Enabled);
			this.groupBox1.Controls.Add(this.numericUpDown_Brightness);
			this.groupBox1.Controls.Add(this.label3);
			this.groupBox1.Controls.Add(this.timePicker_Minute);
			this.groupBox1.Controls.Add(this.timePicker_Hour);
			this.groupBox1.Location = new System.Drawing.Point(341, 13);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(467, 260);
			this.groupBox1.TabIndex = 12;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Edit Schedule";
			// 
			// label4
			// 
			this.label4.ForeColor = System.Drawing.SystemColors.GrayText;
			this.label4.Location = new System.Drawing.Point(17, 149);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(229, 96);
			this.label4.TabIndex = 12;
			this.label4.Text = "Select a schedule from the list to edit, or click Add to create a new one.\r\n\r\nCha" +
    "nges are saved automatically.";
			// 
			// numericUpDown_Brightness
			// 
			this.numericUpDown_Brightness.Location = new System.Drawing.Point(288, 69);
			this.numericUpDown_Brightness.Name = "numericUpDown_Brightness";
			this.numericUpDown_Brightness.Size = new System.Drawing.Size(111, 22);
			this.numericUpDown_Brightness.TabIndex = 10;
			this.numericUpDown_Brightness.ValueChanged += new System.EventHandler(this.numericUpDown_Brightness_ValueChanged);
			// 
			// ScheduleEditor
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.ClientSize = new System.Drawing.Size(826, 324);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.btn_Cancel);
			this.Controls.Add(this.btn_Save);
			this.Controls.Add(this.btn_Remove);
			this.Controls.Add(this.btn_Add);
			this.Controls.Add(this.listBox_Schedules);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ScheduleEditor";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Scheduled Brightness Editor";
			this.Load += new System.EventHandler(this.ScheduleEditor_Load);
			((System.ComponentModel.ISupportInitialize)(this.timePicker_Hour)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.timePicker_Minute)).EndInit();
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDown_Brightness)).EndInit();
			this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox listBox_Schedules;
        private System.Windows.Forms.Button btn_Add;
        private System.Windows.Forms.Button btn_Remove;
        private System.Windows.Forms.Button btn_Save;
        private System.Windows.Forms.Button btn_Cancel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown timePicker_Hour;
        private System.Windows.Forms.NumericUpDown timePicker_Minute;
        private System.Windows.Forms.CheckBox checkBox_Enabled;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label4;
		private System.Windows.Forms.NumericUpDown numericUpDown_Brightness;
	}
}
