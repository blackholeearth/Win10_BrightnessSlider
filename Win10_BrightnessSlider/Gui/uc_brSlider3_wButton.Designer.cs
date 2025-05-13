namespace Win10_BrightnessSlider.Gui
{
    partial class uc_brSlider3_wButton
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
            this.bt_decrease = new System.Windows.Forms.Button();
            this.bt_increase = new System.Windows.Forms.Button();
            this.panel_temp = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // bt_decrease
            // 
            this.bt_decrease.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.bt_decrease.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.bt_decrease.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bt_decrease.Location = new System.Drawing.Point(8, 6);
            this.bt_decrease.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.bt_decrease.Name = "bt_decrease";
            this.bt_decrease.Size = new System.Drawing.Size(56, 66);
            this.bt_decrease.TabIndex = 1;
            this.bt_decrease.TabStop = false;
            this.bt_decrease.Text = "-";
            this.bt_decrease.UseVisualStyleBackColor = true;
            this.bt_decrease.Click += new System.EventHandler(this.bt_decrease_Click);
            // 
            // bt_increase
            // 
            this.bt_increase.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.bt_increase.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.bt_increase.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bt_increase.Location = new System.Drawing.Point(64, 6);
            this.bt_increase.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.bt_increase.Name = "bt_increase";
            this.bt_increase.Size = new System.Drawing.Size(56, 66);
            this.bt_increase.TabIndex = 2;
            this.bt_increase.TabStop = false;
            this.bt_increase.Text = "+";
            this.bt_increase.UseVisualStyleBackColor = true;
            this.bt_increase.Click += new System.EventHandler(this.bt_increase_Click);
            // 
            // panel_temp
            // 
            this.panel_temp.Location = new System.Drawing.Point(125, 2);
            this.panel_temp.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.panel_temp.Name = "panel_temp";
            this.panel_temp.Size = new System.Drawing.Size(329, 74);
            this.panel_temp.TabIndex = 3;
            // 
            // uc_brSlider3_wButton
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.bt_increase);
            this.Controls.Add(this.bt_decrease);
            this.Controls.Add(this.panel_temp);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "uc_brSlider3_wButton";
            this.Size = new System.Drawing.Size(456, 78);
            this.Load += new System.EventHandler(this.uc_brSlider3_wButton_Load);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button bt_decrease;
        private System.Windows.Forms.Button bt_increase;
        public System.Windows.Forms.Panel panel_temp;
    }
}
