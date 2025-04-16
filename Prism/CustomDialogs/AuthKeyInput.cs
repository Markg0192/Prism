using System;
using System.Windows.Forms;

namespace Prism.CustomDialogs
{
	public partial class AuthKeyInput : Form
    {
        public string AuthKeyInputString = "";

        public AuthKeyInput()
        {
            InitializeComponent();
        }

        private void btn_Save_Click(object sender, EventArgs e)
        {
            AuthKeyInputString = txt_AutheKeyInput.Text;
            Close();
        }

		private void btn_Close_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
