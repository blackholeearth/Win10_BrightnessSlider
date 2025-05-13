using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace winformsTests._1_HDR
{
    public partial class frmSDR_test : Form 
    {
        public frmSDR_test()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void button1_Click(object sender, EventArgs e)
        {
            HDR.Set_HDR_Brightness(1);

        }

        
        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            HDR.Set_HDR_Brightness(trackBar1.Value);

            label1.Text = trackBar1.Value+"";

            textBox1.Text = 
                "Now: "+DateTime.Now.ToString("HH:mm:ss.fff")
                + "\r\n" + CCD_Api.Dump_eachMonitor_SDR_whiteLevel();

        }

      

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            //DeepSeekR1_HdrBrightnessController.SetSdrBrightnessAdvanced(nits: 80 * trackBar1.Value);
            CCD_Api.SET_eachMonitor_SDR_whiteLevel(trackBar1.Value * 80);


            label1.Text = trackBar1.Value + " \r\n input-> nits:"+(trackBar1.Value * 80);

            textBox1.Text =
                "Now: " + DateTime.Now.ToString("HH:mm:ss.fff")
                + "\r\n" + CCD_Api.Dump_eachMonitor_SDR_whiteLevel();
        }




        private void bt_get_monitor_HDR_support_Click(object sender, EventArgs e)
        {
            textBox1.Text = CCD_Api.Dump_eachMonitor_HDR_supported_or_Enabled();
        }







    }
}
