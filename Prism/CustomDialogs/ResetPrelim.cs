using System;
using System.Windows.Forms;
using Tekla.Structures.Model;

namespace Prism.CustomDialogs
{
    public partial class ResetPrelim : Form
    {
        PrismProjectData ProjectData;
        ExternalService.WebService1 WebService;

        public ResetPrelim(PrismProjectData projectData, ExternalService.WebService1 webService)
        {
            InitializeComponent();
            TopMost = true;
            CenterToScreen();
            ProjectData = projectData;
            WebService = webService;
            lbl_NextPrelim.Text = Logging.GetLastUsedPrelim(projectData.ProjNumberAndGuid, projectData.WebService).ToString();
        }

        private void btn_PrelimReset_Click(object sender, EventArgs e)
        {
            if (PrismWarnings.ResetPrelimMarking())
            {
                int lastUsedNoBeforeReset = Convert.ToInt32(Logging.GetLastUsedPrelim(ProjectData.ProjNumberAndGuid, ProjectData.WebService).ToString());
                SetNewValues(ProjectData);
                string newStartPoint = Logging.GetLastUsedPrelim(ProjectData.ProjNumberAndGuid, ProjectData.WebService).ToString();

                PrismWarnings.PrelimNumberStartReset(lastUsedNoBeforeReset, Convert.ToInt32(newStartPoint));               
                lbl_NextPrelim.Text = newStartPoint;
                Logging.LogProgress(ProjectData.ProjNumberAndName, "PRELIM RESET - before/after", Convert.ToInt32(newStartPoint), lastUsedNoBeforeReset, WebService);
            }
        }

        private void SetNewValues(PrismProjectData projectData)
        {
            Logging.SetLastUsedPrelim(projectData.ProjNumberAndGuid, Convert.ToInt32(txt_PrelimReset.Text), projectData.WebService);
        }

        private void btn_Refresh_Click(object sender, EventArgs e)
        {
            lbl_NextPrelim.Text = Logging.GetLastUsedPrelim(ProjectData.ProjNumberAndGuid, ProjectData.WebService).ToString();
        }

        private void btn_Close_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}