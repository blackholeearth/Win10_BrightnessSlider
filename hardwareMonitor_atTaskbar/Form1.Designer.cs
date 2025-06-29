using System.Drawing;

namespace hardwareMonitor_atTaskbar
{
	partial class Form1
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.panelCpuGraph = new System.Windows.Forms.Panel();
			this.panelDiskGraph = new System.Windows.Forms.Panel();
			this.panelNetworkGraph = new System.Windows.Forms.Panel();
			this.appNotifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
			this.lblCpuValue = new System.Windows.Forms.Label();
			this.lblDiskValue = new System.Windows.Forms.Label();
			this.lblNetworkValue = new System.Windows.Forms.Label();
			this.lblCombinedValue = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.picBox_taskbarPrev = new System.Windows.Forms.PictureBox();
			this.label4 = new System.Windows.Forms.Label();
			this.numud_width = new System.Windows.Forms.NumericUpDown();
			this.numud_height = new System.Windows.Forms.NumericUpDown();
			this.chk_useCustom_ScreenRes_forRDP = new System.Windows.Forms.CheckBox();
			this.numud_dpi_scaleFactor = new System.Windows.Forms.NumericUpDown();
			this.label5 = new System.Windows.Forms.Label();
			this.bt_apply_customRes = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.picBox_taskbarPrev)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numud_width)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numud_height)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numud_dpi_scaleFactor)).BeginInit();
			this.SuspendLayout();
			// 
			// panelCpuGraph
			// 
			this.panelCpuGraph.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
			this.panelCpuGraph.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panelCpuGraph.Location = new System.Drawing.Point(44, 60);
			this.panelCpuGraph.Name = "panelCpuGraph";
			this.panelCpuGraph.Size = new System.Drawing.Size(114, 70);
			this.panelCpuGraph.TabIndex = 0;
			// 
			// panelDiskGraph
			// 
			this.panelDiskGraph.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
			this.panelDiskGraph.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panelDiskGraph.Location = new System.Drawing.Point(44, 166);
			this.panelDiskGraph.Name = "panelDiskGraph";
			this.panelDiskGraph.Size = new System.Drawing.Size(114, 70);
			this.panelDiskGraph.TabIndex = 1;
			// 
			// panelNetworkGraph
			// 
			this.panelNetworkGraph.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
			this.panelNetworkGraph.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panelNetworkGraph.Location = new System.Drawing.Point(44, 272);
			this.panelNetworkGraph.Name = "panelNetworkGraph";
			this.panelNetworkGraph.Size = new System.Drawing.Size(114, 70);
			this.panelNetworkGraph.TabIndex = 1;
			// 
			// appNotifyIcon
			// 
			this.appNotifyIcon.Text = "Speed Monitor";
			this.appNotifyIcon.Visible = true;
			this.appNotifyIcon.Click += new System.EventHandler(this.AppNotifyIcon_Click);
			this.appNotifyIcon.DoubleClick += new System.EventHandler(this.appNotifyIcon_DoubleClick);
			this.appNotifyIcon.MouseClick += new System.Windows.Forms.MouseEventHandler(this.appNotifyIcon_MouseClick);
			// 
			// lblCpuValue
			// 
			this.lblCpuValue.AutoSize = true;
			this.lblCpuValue.ForeColor = System.Drawing.Color.LightGray;
			this.lblCpuValue.Location = new System.Drawing.Point(188, 88);
			this.lblCpuValue.Name = "lblCpuValue";
			this.lblCpuValue.Size = new System.Drawing.Size(80, 16);
			this.lblCpuValue.TabIndex = 2;
			this.lblCpuValue.Text = "lblCpuValue";
			// 
			// lblDiskValue
			// 
			this.lblDiskValue.AutoSize = true;
			this.lblDiskValue.ForeColor = System.Drawing.Color.LightGray;
			this.lblDiskValue.Location = new System.Drawing.Point(188, 194);
			this.lblDiskValue.Name = "lblDiskValue";
			this.lblDiskValue.Size = new System.Drawing.Size(83, 16);
			this.lblDiskValue.TabIndex = 3;
			this.lblDiskValue.Text = "lblDiskValue";
			// 
			// lblNetworkValue
			// 
			this.lblNetworkValue.AutoSize = true;
			this.lblNetworkValue.ForeColor = System.Drawing.Color.LightGray;
			this.lblNetworkValue.Location = new System.Drawing.Point(188, 300);
			this.lblNetworkValue.Name = "lblNetworkValue";
			this.lblNetworkValue.Size = new System.Drawing.Size(105, 16);
			this.lblNetworkValue.TabIndex = 4;
			this.lblNetworkValue.Text = "lblNetworkValue";
			// 
			// lblCombinedValue
			// 
			this.lblCombinedValue.AutoSize = true;
			this.lblCombinedValue.ForeColor = System.Drawing.Color.LightGray;
			this.lblCombinedValue.Location = new System.Drawing.Point(41, 18);
			this.lblCombinedValue.Name = "lblCombinedValue";
			this.lblCombinedValue.Size = new System.Drawing.Size(118, 16);
			this.lblCombinedValue.TabIndex = 5;
			this.lblCombinedValue.Text = "lblCombinedValue";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.ForeColor = System.Drawing.Color.LightGray;
			this.label1.Location = new System.Drawing.Point(188, 272);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(28, 16);
			this.label1.TabIndex = 8;
			this.label1.Text = "Net";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.ForeColor = System.Drawing.Color.LightGray;
			this.label2.Location = new System.Drawing.Point(185, 166);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(34, 16);
			this.label2.TabIndex = 7;
			this.label2.Text = "Disk";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.ForeColor = System.Drawing.Color.LightGray;
			this.label3.Location = new System.Drawing.Point(188, 60);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(31, 16);
			this.label3.TabIndex = 6;
			this.label3.Text = "Cpu";
			// 
			// picBox_taskbarPrev
			// 
			this.picBox_taskbarPrev.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.picBox_taskbarPrev.Location = new System.Drawing.Point(460, 60);
			this.picBox_taskbarPrev.Name = "picBox_taskbarPrev";
			this.picBox_taskbarPrev.Size = new System.Drawing.Size(281, 122);
			this.picBox_taskbarPrev.TabIndex = 9;
			this.picBox_taskbarPrev.TabStop = false;
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.ForeColor = System.Drawing.Color.LightGray;
			this.label4.Location = new System.Drawing.Point(457, 18);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(112, 16);
			this.label4.TabIndex = 10;
			this.label4.Text = "Taskbar Preview:";
			// 
			// numud_width
			// 
			this.numud_width.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.numud_width.Location = new System.Drawing.Point(494, 268);
			this.numud_width.Maximum = new decimal(new int[] {
            9500,
            0,
            0,
            0});
			this.numud_width.Minimum = new decimal(new int[] {
            500,
            0,
            0,
            0});
			this.numud_width.Name = "numud_width";
			this.numud_width.Size = new System.Drawing.Size(110, 24);
			this.numud_width.TabIndex = 11;
			this.numud_width.Value = new decimal(new int[] {
            500,
            0,
            0,
            0});
			// 
			// numud_height
			// 
			this.numud_height.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.numud_height.Location = new System.Drawing.Point(637, 268);
			this.numud_height.Maximum = new decimal(new int[] {
            9500,
            0,
            0,
            0});
			this.numud_height.Minimum = new decimal(new int[] {
            500,
            0,
            0,
            0});
			this.numud_height.Name = "numud_height";
			this.numud_height.Size = new System.Drawing.Size(110, 24);
			this.numud_height.TabIndex = 12;
			this.numud_height.Value = new decimal(new int[] {
            500,
            0,
            0,
            0});
			// 
			// chk_useCustom_ScreenRes_forRDP
			// 
			this.chk_useCustom_ScreenRes_forRDP.AutoSize = true;
			this.chk_useCustom_ScreenRes_forRDP.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
			this.chk_useCustom_ScreenRes_forRDP.Location = new System.Drawing.Point(471, 235);
			this.chk_useCustom_ScreenRes_forRDP.Name = "chk_useCustom_ScreenRes_forRDP";
			this.chk_useCustom_ScreenRes_forRDP.Size = new System.Drawing.Size(214, 20);
			this.chk_useCustom_ScreenRes_forRDP.TabIndex = 14;
			this.chk_useCustom_ScreenRes_forRDP.Text = "useCustom ScreenRes forRDP";
			this.chk_useCustom_ScreenRes_forRDP.UseVisualStyleBackColor = true;
			this.chk_useCustom_ScreenRes_forRDP.CheckedChanged += new System.EventHandler(this.chk_useCustom_ScreenRes_forRDP_CheckedChanged);
			// 
			// numud_dpi_scaleFactor
			// 
			this.numud_dpi_scaleFactor.DecimalPlaces = 2;
			this.numud_dpi_scaleFactor.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.numud_dpi_scaleFactor.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
			this.numud_dpi_scaleFactor.Location = new System.Drawing.Point(617, 322);
			this.numud_dpi_scaleFactor.Maximum = new decimal(new int[] {
            5,
            0,
            0,
            0});
			this.numud_dpi_scaleFactor.Minimum = new decimal(new int[] {
            50,
            0,
            0,
            131072});
			this.numud_dpi_scaleFactor.Name = "numud_dpi_scaleFactor";
			this.numud_dpi_scaleFactor.Size = new System.Drawing.Size(92, 24);
			this.numud_dpi_scaleFactor.TabIndex = 15;
			this.numud_dpi_scaleFactor.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.ForeColor = System.Drawing.Color.LightGray;
			this.label5.Location = new System.Drawing.Point(491, 326);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(112, 16);
			this.label5.TabIndex = 16;
			this.label5.Text = "DPI - ScaleFactor";
			// 
			// bt_apply_customRes
			// 
			this.bt_apply_customRes.Location = new System.Drawing.Point(494, 396);
			this.bt_apply_customRes.Name = "bt_apply_customRes";
			this.bt_apply_customRes.Size = new System.Drawing.Size(151, 37);
			this.bt_apply_customRes.TabIndex = 17;
			this.bt_apply_customRes.Text = "apply customRes";
			this.bt_apply_customRes.UseVisualStyleBackColor = true;
			this.bt_apply_customRes.Click += new System.EventHandler(this.bt_apply_customRes_Click);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
			this.ClientSize = new System.Drawing.Size(817, 548);
			this.Controls.Add(this.bt_apply_customRes);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.numud_dpi_scaleFactor);
			this.Controls.Add(this.chk_useCustom_ScreenRes_forRDP);
			this.Controls.Add(this.numud_height);
			this.Controls.Add(this.numud_width);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.picBox_taskbarPrev);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.lblCombinedValue);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.lblNetworkValue);
			this.Controls.Add(this.lblDiskValue);
			this.Controls.Add(this.lblCpuValue);
			this.Controls.Add(this.panelDiskGraph);
			this.Controls.Add(this.panelNetworkGraph);
			this.Controls.Add(this.panelCpuGraph);
			this.Name = "Form1";
			this.Text = "Speed Monitor";
			this.Load += new System.EventHandler(this.Form1_Load);
			((System.ComponentModel.ISupportInitialize)(this.picBox_taskbarPrev)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numud_width)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numud_height)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numud_dpi_scaleFactor)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Panel panelCpuGraph;
		private System.Windows.Forms.Panel panelDiskGraph;
		private System.Windows.Forms.Panel panelNetworkGraph;
		private System.Windows.Forms.NotifyIcon appNotifyIcon;
		private System.Windows.Forms.Label lblCpuValue;
		private System.Windows.Forms.Label lblDiskValue;
		private System.Windows.Forms.Label lblNetworkValue;
		private System.Windows.Forms.Label lblCombinedValue;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.PictureBox picBox_taskbarPrev;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.NumericUpDown numud_width;
		private System.Windows.Forms.NumericUpDown numud_height;
		private System.Windows.Forms.CheckBox chk_useCustom_ScreenRes_forRDP;
		private System.Windows.Forms.NumericUpDown numud_dpi_scaleFactor;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Button bt_apply_customRes;
	}
}

