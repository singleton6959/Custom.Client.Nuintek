using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace Custom.Client.Nuintek.Fmea
{
    public partial class ProgressForm : Form
    {
        
        
        public ProgressForm()
        {
            InitializeComponent();
            
        }

        void CountUp(int count, int maxCount)
        {
            progressBar1.Maximum = maxCount;
            progressBar1.PerformStep();
        }

        public void SetMax(int max)
        {
            label4.Text = max.ToString();
        }
        public void SetCouunt(int count)
        {
            label2.Text = count.ToString();
        }

        private void ProgressForm_Load(object sender, EventArgs e)
        {
            //erpService.CountUp += new ERPServices.MyEventHandler(erpService_CountUp);
            progressBar1.Style = ProgressBarStyle.Continuous;
            progressBar1.Maximum = 10;
            progressBar1.Minimum = 0;
            progressBar1.Step = 1;
            progressBar1.Value = 0;
            Application.DoEvents();
        }

        public void label4_TextChanged(object sender, EventArgs e)
        {
            progressBar1.Maximum = Int32.Parse(label4.Text);
            Application.DoEvents();
        }

        public void label2_TextChanged(object sender, EventArgs e)
        {
            progressBar1.PerformStep();
            Application.DoEvents();
        }

        public void SetFormText(string text)
        {
            label1.Text = text;
            this.Text = text;
        }

        
    }
}
