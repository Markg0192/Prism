using System;
using System.Windows.Forms;

namespace Prism
{
    public partial class ExecutionClass : Form
    {
        public int executionClass = 0;

        public ExecutionClass()
        {
            InitializeComponent();
            CenterToScreen();
        }

        private void txt_Go_Click(object sender, EventArgs e)
        {
            if(cmb_ExecutionClass.Text == "EXC1")
            {
                executionClass = 0;
            }
            if (cmb_ExecutionClass.Text == "EXC2")
            {
                executionClass = 1;
            }
            if (cmb_ExecutionClass.Text == "EXC3")
            {
                executionClass = 2;
            }
            if (cmb_ExecutionClass.Text == "EXC4")
            {
                executionClass = 3;
            }
            Close();
        }
    }
}