using Prism.ExternalService;
using System;
using System.Windows.Forms;

namespace Prism.CustomDialogs
{
    public partial class AdvancedSettings : Form
    {
        private PrismProjectData ProjectData;
        public bool CloseForm = false;

        public AdvancedSettings(PrismProjectData projectData)
        {
            InitializeComponent();
            TopMost = true;
            StatusLabel.Text = "Settings";
            CenterToScreen();
            ProjectData = projectData;
            string[] files = WebService.DirectoryGetFiles(1, "*.txt", projectData.ProjNumberAndGuid);
            bool advancedSettingPresent = false;

            foreach (string file in files)
            {
                if (file.Contains("Advanced Settings"))
                {
                    advancedSettingPresent = true;
                }
            }

            if (!advancedSettingPresent)
            {
                PrismWarnings.CantFindAdvancedSettings();
                CloseForm = true;
                this.Close();
                return;
            }

            txt_PrelimPrefix.Text = Logging.GetPrelimPrefix(projectData.ProjNumberAndGuid).ToString();
            cmb_FaPackType.Text = Logging.GetFabPackType(projectData.ProjNumberAndGuid).ToString();
        }

        private void btn_Apply_Click(object sender, EventArgs e)
        {
            WebService.WriteToSpecificLine(1, 1, "Prelim Mark Prefix: " + txt_PrelimPrefix.Text, Constants.ModelProjectAdvancedSettingLocation(ProjectData.ProjNumberAndGuid));
            WebService.WriteToSpecificLine(1, 2, "Fab Pack Type: " + cmb_FaPackType.Text, Constants.ModelProjectAdvancedSettingLocation(ProjectData.ProjNumberAndGuid));
            StatusLabel.Text = "Settings applied";
        }
    }
}