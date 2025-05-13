using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Win10_BrightnessSlider._testArea
{
    public partial class frm1_test : Form
    {
        public frm1_test()
        {
            InitializeComponent();
        }

        LevelMeter level = new LevelMeter();


        private void frm1_test_Load(object sender, EventArgs e)
        {
            this.Controls.Add(level);



        }
    }
}
