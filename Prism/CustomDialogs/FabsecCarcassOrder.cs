using System;
using System.Windows.Forms;

namespace Prism.CustomDialogs
{
    public partial class FabsecCarcassOrder : Form
    {
        public int OrderAction = 0;

        public FabsecCarcassOrder()
        {
            InitializeComponent();
            CenterToScreen();
            TopMost = true;
        }

        private void btn_CreateCarcass_Click(object sender, EventArgs e)
        {
            OrderAction = 1;
            Close();
        }

        private void btn_OrderCarcass_Click(object sender, EventArgs e)
        {
            OrderAction = 2;
            Close();
        }
    }
}
