using System;
using System.Windows.Forms;
using static Prism.Enums;

namespace Prism
{
    public partial class IgnoreWarning : Form
    {
        public IgnoreType Ignore = IgnoreType.Unspecified;
        
        public IgnoreWarning()
        {
            InitializeComponent();
            CenterToScreen();
            TopMost = true;
        }

        private void txt_AutoFix_Click(object sender, EventArgs e)
        {
            Ignore = IgnoreType.AutoFix;
            Close();
        }

        private void txt_Ignore_Click(object sender, EventArgs e)
        {
            Ignore = IgnoreType.Ignore;
            Close();
        }

        private void txt_Stop_Click(object sender, EventArgs e)
        {
            Ignore = IgnoreType.Stop;
            Close();
        }
    }
}