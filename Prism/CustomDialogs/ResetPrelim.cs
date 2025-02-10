using System;
using System.Windows.Forms;
using Tekla.Structures.Model;

namespace Prism.CustomDialogs
{
	public partial class ResetPrelim : Form
	{
		PrismProjectData ProjectData;
		ExternalService.WebService1 WebService;

		public ResetPrelim(PrismProjectData projectData)
		{
			InitializeComponent();
			TopMost = true;
			CenterToScreen();
			ProjectData = projectData;
			WebService = projectData.WebService;
			SetLabelToCurrentLastNumber();
		}

		private void btn_PrelimReset_Click(object sender, EventArgs e)
		{
			if (PrismWarnings.ResetPrelimMarking())
			{
				if (Logging.GetLastUsedPrelim(ProjectData.ProjNumberAndGuid) is int lastUsedNoBeforeReset)
				{
					int newStartPoint = Convert.ToInt32(txt_PrelimReset.Text);
					if (Logging.SetLastUsedPrelim(ProjectData.ProjNumberAndGuid, newStartPoint))
					{
						PrismWarnings.PrelimNumberStartReset(lastUsedNoBeforeReset, newStartPoint);
						lbl_NextPrelim.Text = newStartPoint.ToString();
						Logging.LogProgress(ProjectData.ProjNumberAndName, "PRELIM RESET - before/after", newStartPoint, lastUsedNoBeforeReset);
					}
					PrismWarnings.GetLastUsedPrelimFailed();
				}
			}
		}

		private void btn_Refresh_Click(object sender, EventArgs e)
		{
			SetLabelToCurrentLastNumber();
		}

		private void btn_Close_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void SetLabelToCurrentLastNumber()
		{
			if (Logging.GetLastUsedPrelim(ProjectData.ProjNumberAndGuid) is int currentLastNumber)
			{
				lbl_NextPrelim.Text = currentLastNumber.ToString();
			}
			else
			{
				lbl_NextPrelim.Text = "Error";
			}
		}
	}
}