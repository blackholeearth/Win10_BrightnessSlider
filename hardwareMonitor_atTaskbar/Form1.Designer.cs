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
			((System.ComponentModel.ISupportInitialize)(this.picBox_taskbarPrev)).BeginInit();
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
			this.picBox_taskbarPrev.Size = new System.Drawing.Size(281, 150);
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
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
			this.ClientSize = new System.Drawing.Size(817, 374);
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
	}
}

