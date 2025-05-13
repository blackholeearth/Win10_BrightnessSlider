namespace Win10_BrightnessSlider
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.fLayPnl1 = new System.Windows.Forms.FlowLayoutPanel();
            this.notifyIcon_bright = new System.Windows.Forms.NotifyIcon(this.components);
            this.notifyIcon_wifi = new System.Windows.Forms.NotifyIcon(this.components);
            this.SuspendLayout();
            // 
            // notifyIcon1
            // 
            //this.notifyIcon_bright.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
            this.notifyIcon_bright.Text = "notifyIcon1";
            this.notifyIcon_bright.Visible = true;
            // 
            // notifyIcon2
            // 
            this.notifyIcon_wifi.Text = "notifyIcon2";
            this.notifyIcon_wifi.Visible = true;
            // 
            // fLayPnl1
            // 
            this.fLayPnl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fLayPnl1.Location = new System.Drawing.Point(0, 0);
            this.fLayPnl1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.fLayPnl1.Name = "fLayPnl1";
            this.fLayPnl1.Size = new System.Drawing.Size(438, 81);
            this.fLayPnl1.TabIndex = 3;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(438, 81);
            this.Controls.Add(this.fLayPnl1);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.MouseEnter += new System.EventHandler(this.Form1_MouseEnter);
            this.ResumeLayout(false);

        }



        #endregion
        private System.Windows.Forms.FlowLayoutPanel fLayPnl1;
        public System.Windows.Forms.NotifyIcon notifyIcon_bright;
        private System.Windows.Forms.NotifyIcon notifyIcon_wifi;
    }
}

