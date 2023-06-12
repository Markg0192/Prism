using System;
using System.Windows.Forms;

namespace Prism.CustomDialogs
{
    public partial class YesNoForm : Form
    {
        public bool Yes;

        public YesNoForm(string message, string title)
        {
            InitializeComponent();
            CenterToScreen();
            lbl_WarningText.AutoSize = true;
            lbl_WarningText.MaximumSize = new System.Drawing.Size(300, 1000);
            this.AutoSize = true;
            lbl_WarningText.Text = message;
            Text = title;
        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void btn_Yes_Click(object sender, EventArgs e)
        {
            Yes = true;
            this.Close();
        }

        private void btn_No_Click(object sender, EventArgs e)
        {
            Yes = false;
            this.Close();
        }
    }
}
