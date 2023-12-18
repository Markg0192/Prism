using System;
using System.Collections.Generic;
using System.ComponentModel;
//using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    }
}
