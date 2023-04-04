using System;
using System.Windows.Forms;
using static Prism.Enums;

namespace Prism
{
    public partial class FactoryLocation : Form
    {
        public Factory myLocation = Factory.Unknown;

        public FactoryLocation()
        {
            InitializeComponent();
            CenterToScreen();
            TopMost = true;
        }

        private void txt_SNI_Click(object sender, EventArgs e)
        {
            myLocation = Factory.SNI;
            Close();
        }

        private void txt_SUK_Click(object sender, EventArgs e)
        {
            myLocation = Factory.SUK;
            Close();
        }

        private void txt_Unknown_Click(object sender, EventArgs e)
        {
            myLocation = Factory.Unknown;
            Close();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
