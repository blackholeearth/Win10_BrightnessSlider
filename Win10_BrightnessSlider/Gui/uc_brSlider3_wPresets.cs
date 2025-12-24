using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Win10_BrightnessSlider.Gui
{
    public partial class uc_brSlider3_wPresets : UserControl , Iuc_brSlider
    {
        //interface
        RichInfoScreen Iuc_brSlider.richInfoScreen
        {
            get => _uc_brSlider3.riScreen;
            set => _uc_brSlider3.riScreen = value;
        }
        string Iuc_brSlider.NotifyIconText => _uc_brSlider3.NotifyIconText;
        void Iuc_brSlider.UpdateSliderControl() => _uc_brSlider3.UpdateSliderControl();
        void Iuc_brSlider.Set_MonitorName(string name) => _uc_brSlider3.Set_MonitorName(name);

        private List<Button> presetButtons = new List<Button>();

        public uc_brSlider3_wPresets()
        {
            InitializeComponent();
            CreatePresetButtons();
        }

        private void CreatePresetButtons()
        {
            // Remove old hardcoded buttons if they exist
            if (bt_0 != null) { this.Controls.Remove(bt_0); bt_0.Dispose(); }
            if (bt_25 != null) { this.Controls.Remove(bt_25); bt_25.Dispose(); }
            if (bt_50 != null) { this.Controls.Remove(bt_50); bt_50.Dispose(); }
            if (bt_75 != null) { this.Controls.Remove(bt_75); bt_75.Dispose(); }
            if (bt_100 != null) { this.Controls.Remove(bt_100); bt_100.Dispose(); }

            presetButtons.Clear();

            var settings = Settings_json.Get();
            var percentages = settings.PresetButtonPercentages ?? new List<int> { 0, 10, 25, 50, 75, 100 };

            // Limit to 6 buttons max for width
            if (percentages.Count > 6) percentages = percentages.Take(6).ToList();

            int buttonWidth = 44;
            int buttonHeight = 66;
            int startX = 8;
            int spacing = 4;

            for (int i = 0; i < percentages.Count; i++)
            {
                int percent = percentages[i];
                var btn = new Button
                {
                    Text = $"{percent}%",
                    Size = new Size(buttonWidth, buttonHeight),
                    Location = new Point(startX + i * (buttonWidth + spacing), 6),
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 8F),
                    TabStop = false,
                    Anchor = AnchorStyles.Left,
                    Tag = percent  // Store the percentage in Tag
                };
                btn.Click += PresetButton_Click;
                presetButtons.Add(btn);
                this.Controls.Add(btn);
            }

            // Reposition the panel_temp to make room for the buttons
            int buttonsWidth = percentages.Count * (buttonWidth + spacing) + startX;
            if (panel_temp != null)
            {
                panel_temp.Left = buttonsWidth;
            }
        }

        private void PresetButton_Click(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.Tag is int percent)
            {
                if (_uc_brSlider3 != null && _uc_brSlider3.riScreen != null)
                    _uc_brSlider3.Slider_SetBrightness(percent);
            }
        }

        public uc_brSlider3 _uc_brSlider3;

        /// [JsonIgnore]
        public Pen FrameColor = new Pen(Color.Orange, 1);
        // [JsonIgnore]
        Pen FrameColorDebug = new Pen(Color.Orange, 1)
        {
            DashStyle = DashStyle.Custom,
            DashPattern = new float[] { 10, 10 },
        };
        //Draw border
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (Debugger.IsAttached)
                FrameColor = FrameColorDebug;

            var CliRect = this.ClientRectangle;
            CliRect.Height = CliRect.Height - 2;
            CliRect.Width = CliRect.Width - 2;


            ////win11 enabled
            //if (Form1.isRoundCorners)
            //{
            //    int offset = -2;
            //    pw.Offset(offset, 0);
            //    ph.Offset(0, offset);
            //    var phw = new Point(CliRect.Width + offset, CliRect.Height + offset);

            //    //win11 padding, wee need bottom and right
            //    e.Graphics.DrawLine(FrameColor, ph, phw);
            //    e.Graphics.DrawLine(FrameColor, pw, phw); //no problem. works great on light theme

            //}

            if (Form1.riScreens == null)
                return;

            //win11
            if (Form1.isWindows11)
            {
                var radius = 8;
                GraphicsPath shape = new GraphicsPath();

                // Check if "All Monitors" control exists (preset buttons with multiple monitors)
                var parentForm = this.FindForm() as Form1;
                bool hasAllMonitorsControl = parentForm?.HasAllMonitorsControl() ?? false;

                if (Form1.riScreens.Count == 1 && !hasAllMonitorsControl)
                {
                    // Single monitor, no "All Monitors" control - full rounded corners
                    shape = DrawingFn.RoundedRect(CliRect, radius);
                }
                else if (hasAllMonitorsControl)
                {
                    // Multiple monitors with "All Monitors" at top
                    bool _Last = Form1.riScreens[Form1.riScreens.Count - 1] == this._uc_brSlider3.riScreen;

                    if (_Last)
                        // Last monitor - bottom rounded corners only
                        shape = DrawingFn.RoundedRect(CliRect, 0, radius);
                    else
                        // First or middle monitors - no rounded corners
                        shape = DrawingFn.RoundedRect(CliRect, 0, 0);
                }
                else
                {
                    // Multiple monitors, no "All Monitors" control (shouldn't happen with preset buttons)
                    bool _First = Form1.riScreens[0] == this._uc_brSlider3.riScreen;
                    bool _Last = Form1.riScreens[Form1.riScreens.Count - 1] == this._uc_brSlider3.riScreen;

                    if (_First)
                        shape = DrawingFn.RoundedRect(CliRect, radius, 0);
                    else if (_Last)
                        shape = DrawingFn.RoundedRect(CliRect, 0, radius);
                    else
                        shape = DrawingFn.RoundedRect(CliRect, 0, 0);
                }

                e.Graphics.DrawPath(FrameColor, shape);
            }
            else
            {
                var p00 = new Point(0, 0);
                var pw = new Point(CliRect.Width, 0);
                var ph = new Point(0, CliRect.Height);

                e.Graphics.DrawLine(FrameColor, p00, pw);
                e.Graphics.DrawLine(FrameColor, p00, ph);
            }



        }




        public void SetGUIColors(Color bg, Color text , Color border , uc_brSlider3 ucBrSlider3)
        {
            this.Padding = Padding.Empty;
            this.Margin = Padding.Empty;

            _uc_brSlider3 = ucBrSlider3;


            this.Controls.Add(ucBrSlider3);
            ucBrSlider3.Top = panel_temp.Top;
            ucBrSlider3.Left = panel_temp.Left;
            panel_temp.Visible = false;

            // Calculate available width for slider (control width minus buttons area minus small margin)
            ucBrSlider3.Width = this.Width - ucBrSlider3.Left - 4;

            FrameColor = ucBrSlider3.FrameColor;
            ucBrSlider3.DrawFrame_isEnabled = false;

            this.BackColor = bg;
            this.ForeColor = text;

            // Style all dynamic preset buttons
            foreach (var btn in presetButtons)
            {
                btn.BackColor = border;
                btn.ForeColor = text;
                btn.FlatAppearance.BorderColor = border;
                btn.FlatAppearance.BorderSize = 0;
            }
        }
        private void uc_brSlider3_wPresets_Load(object sender, EventArgs e)  {  }

        // Legacy handlers kept for designer compatibility - not used anymore
        private void bt_0_Click(object sender, EventArgs e) { }
        private void bt_25_Click(object sender, EventArgs e) { }
        private void bt_50_Click(object sender, EventArgs e) { }
        private void bt_75_Click(object sender, EventArgs e) { }
        private void bt_100_Click(object sender, EventArgs e) { }
    }
}
