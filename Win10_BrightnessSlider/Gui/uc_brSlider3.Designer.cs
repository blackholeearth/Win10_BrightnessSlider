namespace Win10_BrightnessSlider
{
    partial class uc_brSlider3
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lbl_value = new System.Windows.Forms.Label();
            this.lbl_Name = new System.Windows.Forms.Label();
            this.trackBar1 = new Win10_BrightnessSlider.ColorSlider();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // lbl_value
            // 
            this.lbl_value.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lbl_value.AutoSize = true;
            this.lbl_value.Font = new System.Drawing.Font("Segoe UI", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_value.Location = new System.Drawing.Point(300, 26);
            this.lbl_value.Name = "lbl_value";
            this.lbl_value.Size = new System.Drawing.Size(42, 25);
            this.lbl_value.TabIndex = 4;
            this.lbl_value.Text = "000";
            this.lbl_value.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lbl_Name
            // 
            this.lbl_Name.AutoSize = true;
            this.lbl_Name.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.lbl_Name.Location = new System.Drawing.Point(14, 7);
            this.lbl_Name.Name = "lbl_Name";
            this.lbl_Name.Size = new System.Drawing.Size(107, 15);
            this.lbl_Name.TabIndex = 6;
            this.lbl_Name.Text = "Screen/Mon Name";
            this.lbl_Name.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // trackBar1
            // 
            this.trackBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.trackBar1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(77)))), ((int)(((byte)(95)))));
            this.trackBar1.BarOuterColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(60)))), ((int)(((byte)(74)))));
            this.trackBar1.BorderRoundRectSize = new System.Drawing.Size(8, 8);
            this.trackBar1.ElapsedInnerColor = System.Drawing.Color.FromArgb(((int)(((byte)(21)))), ((int)(((byte)(56)))), ((int)(((byte)(152)))));
            this.trackBar1.ElapsedOuterColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(140)))), ((int)(((byte)(180)))));
            this.trackBar1.Font = new System.Drawing.Font("Microsoft Sans Serif", 6F);
            this.trackBar1.ForeColor = System.Drawing.Color.White;
            this.trackBar1.LargeChange = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.trackBar1.Location = new System.Drawing.Point(45, 29);
            this.trackBar1.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.trackBar1.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.trackBar1.Name = "trackBar1";
            this.trackBar1.ScaleDivisions = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.trackBar1.ScaleSubDivisions = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.trackBar1.ShowDivisionsText = false;
            this.trackBar1.ShowSmallScale = false;
            this.trackBar1.Size = new System.Drawing.Size(250, 23);
            this.trackBar1.SmallChange = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.trackBar1.TabIndex = 3;
            this.trackBar1.ThumbInnerColor = System.Drawing.Color.FromArgb(((int)(((byte)(21)))), ((int)(((byte)(56)))), ((int)(((byte)(152)))));
            this.trackBar1.ThumbPenColor = System.Drawing.Color.FromArgb(((int)(((byte)(21)))), ((int)(((byte)(56)))), ((int)(((byte)(152)))));
            this.trackBar1.ThumbRoundRectSize = new System.Drawing.Size(16, 16);
            this.trackBar1.ThumbSize = new System.Drawing.Size(16, 16);
            this.trackBar1.TickAdd = 0F;
            this.trackBar1.TickColor = System.Drawing.Color.White;
            this.trackBar1.TickDivide = 0F;
            this.trackBar1.TickStyle = System.Windows.Forms.TickStyle.None;
            this.trackBar1.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::Win10_BrightnessSlider.Properties.Resources.sunny_white;
            this.pictureBox1.InitialImage = null;
            this.pictureBox1.Location = new System.Drawing.Point(18, 30);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(20, 20);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 5;
            this.pictureBox1.TabStop = false;
            // 
            // uc_brSlider3
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.Controls.Add(this.lbl_Name);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.lbl_value);
            this.Controls.Add(this.trackBar1);
            this.Name = "uc_brSlider3";
            this.Size = new System.Drawing.Size(350, 73);
            this.Load += new System.EventHandler(this.uc_brSlider3_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

       

        #endregion

        public System.Windows.Forms.PictureBox pictureBox1;
        public System.Windows.Forms.Label lbl_value;
        public ColorSlider trackBar1;
        public System.Windows.Forms.Label lbl_Name;
    }
}
