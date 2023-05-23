using System;
using System.Windows.Forms;

namespace Prism.CustomDialogs
{
    public partial class OkForm : Form
    {
        public OkForm(string message, string title)
        {
            InitializeComponent();
            CenterToScreen();
            lbl_WarningText.AutoSize = true;
            lbl_WarningText.MaximumSize = new System.Drawing.Size(300, 100);
            this.AutoSize= true;
            lbl_WarningText.Text = message;
            Text = title;
        }

        private void button1_Click(object sender, EventArgs e)
        {

            this.Close();
        }
    }
}
