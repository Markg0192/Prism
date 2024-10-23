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

            txt_PrelimPrefix.Text = Logging.GetAdvancedSetting(projectData.ProjNumberAndGuid, Enums.AdvancedSettingType.PrelimPrefix);
            cmb_FaPackType.Text = Logging.GetAdvancedSetting(projectData.ProjNumberAndGuid, Enums.AdvancedSettingType.FabPackType);
            txt_DirectoryMaterial.Text = Logging.GetAdvancedSetting(projectData.ProjNumberAndGuid, Enums.AdvancedSettingType.DirectoryMaterial);
            txt_DirectoryCarcass.Text = Logging.GetAdvancedSetting(projectData.ProjNumberAndGuid, Enums.AdvancedSettingType.DirectoryCarcasses);
            txt_DirectoryBolts.Text = Logging.GetAdvancedSetting(projectData.ProjNumberAndGuid, Enums.AdvancedSettingType.DirectoryBolts);
            txt_DirectorySeversafe.Text = Logging.GetAdvancedSetting(projectData.ProjNumberAndGuid, Enums.AdvancedSettingType.DirectorySeversafe);
            txt_DirectoryFab.Text = Logging.GetAdvancedSetting(projectData.ProjNumberAndGuid, Enums.AdvancedSettingType.DirectoryFabPack);
            txt_DirectoryVariation.Text = Logging.GetAdvancedSetting(projectData.ProjNumberAndGuid, Enums.AdvancedSettingType.DirectoryVariation);  
            txt_FabsecGreen.Text = Logging.GetAdvancedSetting(projectData.ProjNumberAndGuid, Enums.AdvancedSettingType.FabsecGreen);
            if (txt_FabsecGreen.Text == "") txt_FabsecGreen.Text = "100";
		}

        private void btn_Apply_Click(object sender, EventArgs e)
        {
            StatusLabel.Text = "Applying settings";

            WriteSetting(Enums.AdvancedSettingType.PrelimPrefix, txt_PrelimPrefix.Text);
            WriteSetting(Enums.AdvancedSettingType.FabPackType, cmb_FaPackType.Text);
            WriteSetting(Enums.AdvancedSettingType.DirectoryMaterial, txt_DirectoryMaterial.Text);
            WriteSetting(Enums.AdvancedSettingType.DirectoryCarcasses, txt_DirectoryCarcass.Text);
            WriteSetting(Enums.AdvancedSettingType.DirectoryBolts, txt_DirectoryBolts.Text);
            WriteSetting(Enums.AdvancedSettingType.DirectorySeversafe, txt_DirectorySeversafe.Text);
            WriteSetting(Enums.AdvancedSettingType.DirectoryFabPack, txt_DirectoryFab.Text);
            WriteSetting(Enums.AdvancedSettingType.DirectoryVariation, txt_DirectoryVariation.Text);
			WriteSetting(Enums.AdvancedSettingType.FabsecGreen, txt_FabsecGreen.Text);

			StatusLabel.Text = "Settings applied";
        }

        private void WriteSetting(Enums.AdvancedSettingType setting, string textToWrite)
        {
            WebService.WriteToSpecificLine(Constants.PrismDataLogLocation, (int)setting, setting.ToString() + ":split: " + textToWrite, Constants.ModelProjectAdvancedSettingLocation(ProjectData.ProjNumberAndGuid));
        }

        private void btn_Close_Click(object sender, EventArgs e)
        {
            this.Close();
            return;
        }
    }
}