using System;
using System.Windows.Forms;

namespace Prism.CustomDialogs
{
    public partial class Division : Form
    {
        public int DivisionOut;

        public Division()
        {
            InitializeComponent();
            CenterToScreen();
            TopMost = true;
        }

        private void btn_DivCI_Click(object sender, EventArgs e)
        {
            DivisionOut = 1;
            Close();
        }

        private void btn_DivNI_Click(object sender, EventArgs e)
        {
            DivisionOut = 2;
            Close();
        }

        private void btn_DivPP_Click(object sender, EventArgs e)
        {
            DivisionOut = 3;
            Close();
        }

        private void btn_DivOther_Click(object sender, EventArgs e)
        {
            DivisionOut = 4;
            Close();
        }

        private void btn_Cancel_Click(object sender, EventArgs e)
        {
            DivisionOut = 0;
            Close();
        }
    }
}
