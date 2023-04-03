using System;
using System.Windows.Forms;

namespace Prism.CustomDialogs
{
    public partial class BoltOrderType : Form
    {
        public int OrderBoltsFrom = 0;

        public BoltOrderType()
        {
            InitializeComponent();
            CenterToScreen();
            TopMost = true;
        }

        private void btn_FromBolts_Click(object sender, EventArgs e)
        {
            OrderBoltsFrom = 1;
            Close();
        }

        private void btn_FromAss_Click(object sender, EventArgs e)
        {
            OrderBoltsFrom = 2;
            Close();
        }
    }
}
