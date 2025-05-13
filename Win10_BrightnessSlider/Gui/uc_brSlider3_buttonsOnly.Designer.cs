namespace Win10_BrightnessSlider
{
    partial class uc_brSlider3_buttonsOnly
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
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.lbl_value = new System.Windows.Forms.Label();
            this.lbl_Name = new System.Windows.Forms.Label();
            this.bt_increase = new System.Windows.Forms.Button();
            this.bt_decrease = new System.Windows.Forms.Button();
            this.trackBar1 = new Win10_BrightnessSlider.ColorSlider();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::Win10_BrightnessSlider.Properties.Resources.sunny_white;
            this.pictureBox1.Location = new System.Drawing.Point(22, 55);
            this.pictureBox1.Margin = new System.Windows.Forms.Padding(4);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(25, 25);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 5;
            this.pictureBox1.TabStop = false;
            // 
            // lbl_value
            // 
            this.lbl_value.AutoSize = true;
            this.lbl_value.Font = new System.Drawing.Font("Segoe UI", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_value.Location = new System.Drawing.Point(375, 50);
            this.lbl_value.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lbl_value.Name = "lbl_value";
            this.lbl_value.Size = new System.Drawing.Size(50, 31);
            this.lbl_value.TabIndex = 4;
            this.lbl_value.Text = "000";
            this.lbl_value.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lbl_Name
            // 
            this.lbl_Name.AutoSize = true;
            this.lbl_Name.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.lbl_Name.Location = new System.Drawing.Point(18, 11);
            this.lbl_Name.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lbl_Name.Name = "lbl_Name";
            this.lbl_Name.Size = new System.Drawing.Size(133, 20);
            this.lbl_Name.TabIndex = 6;
            this.lbl_Name.Text = "Screen/Mon Name";
            this.lbl_Name.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // bt_increase
            // 
            this.bt_increase.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.bt_increase.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bt_increase.Location = new System.Drawing.Point(221, 36);
            this.bt_increase.Name = "bt_increase";
            this.bt_increase.Size = new System.Drawing.Size(129, 59);
            this.bt_increase.TabIndex = 8;
            this.bt_increase.TabStop = false;
            this.bt_increase.Text = "+";
            this.bt_increase.UseVisualStyleBackColor = true;
            this.bt_increase.Click += new System.EventHandler(this.bt_increase_Click);
            // 
            // bt_decrease
            // 
            this.bt_decrease.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.bt_decrease.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bt_decrease.Location = new System.Drawing.Point(92, 36);
            this.bt_decrease.Name = "bt_decrease";
            this.bt_decrease.Size = new System.Drawing.Size(129, 59);
            this.bt_decrease.TabIndex = 7;
            this.bt_decrease.TabStop = false;
            this.bt_decrease.Text = "-";
            this.bt_decrease.UseVisualStyleBackColor = true;
            this.bt_decrease.Click += new System.EventHandler(this.bt_decrease_Click);
            // 
            // trackBar1
            // 
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
            this.trackBar1.Location = new System.Drawing.Point(92, 92);
            this.trackBar1.Margin = new System.Windows.Forms.Padding(4);
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
            this.trackBar1.Size = new System.Drawing.Size(258, 11);
            this.trackBar1.SmallChange = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.trackBar1.TabIndex = 3;
            this.trackBar1.ThumbInnerColor = System.Drawing.Color.FromArgb(((int)(((byte)(21)))), ((int)(((byte)(56)))), ((int)(((byte)(152)))));
            this.trackBar1.ThumbPenColor = System.Drawing.Color.FromArgb(((int)(((byte)(21)))), ((int)(((byte)(56)))), ((int)(((byte)(152)))));
            this.trackBar1.ThumbRoundRectSize = new System.Drawing.Size(1, 1);
            this.trackBar1.ThumbSize = new System.Drawing.Size(1, 1);
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
            // uc_brSlider3_buttonsOnly
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.Controls.Add(this.bt_increase);
            this.Controls.Add(this.bt_decrease);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.lbl_value);
            this.Controls.Add(this.trackBar1);
            this.Controls.Add(this.lbl_Name);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "uc_brSlider3_buttonsOnly";
            this.Size = new System.Drawing.Size(438, 114);
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
        private System.Windows.Forms.Button bt_increase;
        private System.Windows.Forms.Button bt_decrease;
    }
}
