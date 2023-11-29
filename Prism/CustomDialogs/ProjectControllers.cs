using System;
using System.Windows.Forms;
using Tekla.Structures.Model;

namespace Prism.CustomDialogs
{
    public partial class ProjectControllers : Form
    {
        private ProjectInfo _pInfo;
        private string fileLocation;
        private ExternalService.WebService1 service;

        public ProjectControllers(string jobName, ExternalService.WebService1 service, ProjectInfo pInfo)
        {
            InitializeComponent();
            TopMost = true;
            CenterToScreen();
            fileLocation = Constants.ModelProjectInforLocation(jobName);
            this.service = service;
            _pInfo = pInfo;
            ReadExistingControllers(pInfo);
        }

        private void ReadExistingControllers(ProjectInfo pInfo)
        {
            string projectManager = WebService.ReadSpecificLine(Constants.PrismModelData, 17, fileLocation);
            string doManager = WebService.ReadSpecificLine(Constants.PrismModelData, 18, fileLocation);
            string documentControl = WebService.ReadSpecificLine(Constants.PrismModelData, 19, fileLocation);
            string others = WebService.ReadSpecificLine(Constants.PrismModelData, 20, fileLocation);

            txt_DocumentControl.Text = documentControl;
            txt_Others.Text = others;
            txt_ProjectManagement.Text = projectManager;
            txt_DOManager.Text = doManager;
        }

        private void SetNewValues(ProjectInfo pInfo)
        {
            WebService.WriteToSpecificLine(Constants.PrismModelData, 17, txt_ProjectManagement.Text, fileLocation);
            WebService.WriteToSpecificLine(Constants.PrismModelData, 18, txt_DOManager.Text, fileLocation);
            WebService.WriteToSpecificLine(Constants.PrismModelData, 19, txt_DocumentControl.Text, fileLocation);
            WebService.WriteToSpecificLine(Constants.PrismModelData, 20, txt_Others.Text, fileLocation);
        }

        private async void btn_Apply_Click(object sender, EventArgs e)
        {
            lbl_ApplyStatus.Text = "Applying...";
            await System.Threading.Tasks.Task.Run(() =>
            {
                SetNewValues(_pInfo);
                FormCCString();
            });
            lbl_ApplyStatus.Text = "Code data applied.";
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
                if (st != "")
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
