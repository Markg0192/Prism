using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tekla.Structures.Model;

namespace Prism.CustomDialogs
{
    public partial class ProjectControllers : Form
    {
        private ProjectInfo _pInfo;
        public ProjectControllers(ProjectInfo pInfo)
        {
            InitializeComponent();
            TopMost= true;
            CenterToScreen();
            _pInfo= pInfo;
            ReadExistingControllers(pInfo);
        }

        private void ReadExistingControllers(ProjectInfo pInfo)
        {
            string projectManager = "";
            pInfo.GetUserProperty("PrismPM", ref projectManager);
            string doManager = "";
            pInfo.GetUserProperty("PrismDOM", ref doManager);
            string documentControl = "";
            pInfo.GetUserProperty("PrismDOC", ref documentControl);
            string others = "";
            pInfo.GetUserProperty("PrismOTHERS", ref others);
            txt_DocumentControl.Text = documentControl;
            txt_Others.Text = others;
            txt_ProjectManagement.Text = projectManager;
            txt_DOManager.Text = doManager;
        }

        private void SetNewValues(ProjectInfo pInfo)
        {
            pInfo.SetUserProperty("PrismPM", txt_ProjectManagement.Text);
            pInfo.SetUserProperty("PrismDOM", txt_DOManager.Text);
            pInfo.SetUserProperty("PrismDOC", txt_DocumentControl.Text);
            pInfo.SetUserProperty("PrismOTHERS", txt_Others.Text);
        }

        private void btn_Apply_Click(object sender, EventArgs e)
        {
            SetNewValues(_pInfo);
            FormCCString();
        }

        private string FormCCString()
        {
            string[] pString = txt_ProjectManagement.Text.Split(' ');
            string[] doManagerString = txt_DOManager.Text.Split(' ');
            string[] docControlString = txt_DocumentControl.Text.Split(' ');
            string[] othersString = txt_Others.Text.Split(' ');

            AppendString(pString, "", out string first);
            AppendString(doManagerString, first, out string second);
            AppendString(docControlString, second, out string third);
            AppendString(othersString, third, out string ccString);

            return ccString;
        }

        private void AppendString(string[] input, string ccString, out string newCCstring)
        {
            foreach (string st in input)
            {
                if(st != "")
                {
                    ccString = ccString + st + "; ";
                }

             }
            newCCstring = ccString;
        }

        private void btn_Close_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
