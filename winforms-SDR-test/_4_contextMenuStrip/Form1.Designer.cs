namespace  winformsTests._4_contextMenuStrip
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
			this.notifyIconEx1 = new NotifyIconEX(this.components);
			this.ContextMenuStripEx1 = new ContextMenuStripEX(this.components);
			this.helloToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.worldToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.ContextMenuStripEx1.SuspendLayout();
			this.SuspendLayout();
			// 
			// notifyIcon1
			// 
			this.notifyIconEx1.Text = "notifyIcon_ex1";
			this.notifyIconEx1.Visible = true;
			// 
			// contextMenuStrip1
			// 
			this.ContextMenuStripEx1.ImageScalingSize = new System.Drawing.Size(20, 20);
			this.ContextMenuStripEx1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.helloToolStripMenuItem,
            this.worldToolStripMenuItem});
			this.ContextMenuStripEx1.Name = "contextMenuStrip1";
			this.ContextMenuStripEx1.Size = new System.Drawing.Size(117, 52);
			// 
			// helloToolStripMenuItem
			// 
			this.helloToolStripMenuItem.Name = "helloToolStripMenuItem";
			this.helloToolStripMenuItem.Size = new System.Drawing.Size(116, 24);
			this.helloToolStripMenuItem.Text = "hello";
			// 
			// worldToolStripMenuItem
			// 
			this.worldToolStripMenuItem.Name = "worldToolStripMenuItem";
			this.worldToolStripMenuItem.Size = new System.Drawing.Size(116, 24);
			this.worldToolStripMenuItem.Text = "world";
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(630, 351);
			this.Name = "Form1";
			this.Text = "Form1";
			this.Load += new System.EventHandler(this.Form1_Load);
			this.ContextMenuStripEx1.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private NotifyIconEX notifyIconEx1;
		private ContextMenuStripEX ContextMenuStripEx1;
		private System.Windows.Forms.ToolStripMenuItem helloToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem worldToolStripMenuItem;
	}
}