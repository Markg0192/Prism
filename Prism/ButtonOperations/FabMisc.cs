using System.Collections.Generic;
using Tekla.Structures.Model;
using System.Linq;
using System.Windows.Forms;

namespace Prism
{
    public static class FabMisc
    {
        public static void FabMiscOp(Model model, string siteDate, SelectedObjects selectedObjects, bool runSeversafe, ReportManager reportManager, int divisionNo, PrismProjectData projData, bool runChangeManager, ToolStrip toolStrip, ToolStripStatusLabel statusLabel, out bool boltOrderAdded)
        {
            boltOrderAdded = false;
            if (selectedObjects.PrismBoltGroups.Any(b => b.isOrdered == false))
            {
                List<BoltGroup> unorderedBoltGroups = selectedObjects.PrismBoltGroups
                                        .Where(pbg => !pbg.isOrdered) // Filter PrismBoltGroup objects where IsOrdered is false
                                        .Select(pbg => pbg.BoltGroup) // Select the BoltGroup property from those filtered PrismBoltGroup objects
                                        .ToList();

                CreateBoltOrder(reportManager, siteDate, unorderedBoltGroups);
                ModelModifiers.StampBoltUDA(unorderedBoltGroups, reportManager.ProjectData.Full, reportManager.ProjectData.Date, reportManager.PhaseNum, reportManager.IssueNum);
                boltOrderAdded = true;
            }

            if (runSeversafe) { SeversafeOrder.CreateSeversafeOrder(model, selectedObjects.GetSeversafeParts(), siteDate, reportManager, divisionNo, reportManager.EpoReportPrefix, projData); }

            ModelModifiers.StampPartFabUDA(selectedObjects.PrismParts, reportManager.PhaseNum, reportManager.IssueNum);
            ViewManager.CreateFabView(reportManager.PhaseNum, reportManager.IssueNum, reportManager.ProjectData, selectedObjects);         
        }

		public static void FabMiscOp(Model model, string siteDate, SelectedObjects selectedObjects, ReportManager reportManager, 
            bool runSeversafe, PrismProjectData projData, bool runChangeManager, out bool boltOrderAdded)
		{
			boltOrderAdded = false;
			if (selectedObjects.PrismBoltGroups.Any(b => b.isOrdered == false))
			{
				List<BoltGroup> unorderedBoltGroups = selectedObjects.PrismBoltGroups
										.Where(pbg => !pbg.isOrdered) // Filter PrismBoltGroup objects where IsOrdered is false
										.Select(pbg => pbg.BoltGroup) // Select the BoltGroup property from those filtered PrismBoltGroup objects
										.ToList();

				CreateBoltOrder(reportManager, siteDate, unorderedBoltGroups);
				ModelModifiers.StampBoltUDA(unorderedBoltGroups, reportManager.ProjectData.Full, reportManager.ProjectData.Date,
                    reportManager.PhaseNum, reportManager.IssueNum);
				boltOrderAdded = true;
			}

			if (runSeversafe) { SeversafeOrder.CreateSeversafeOrder(model, selectedObjects.GetSeversafeParts(), siteDate, reportManager, 1, 
                reportManager.EpoReportPrefix, projData); }

			ModelModifiers.StampPartFabUDA(selectedObjects.PrismParts, reportManager.PhaseNum, reportManager.IssueNum);
			ViewManager.CreateFabView(reportManager.PhaseNum, reportManager.IssueNum, reportManager.ProjectData, selectedObjects);
		}

		public static void CreateBoltOrder(ReportManager reportManager, string siteDate, List<BoltGroup> unorderedBoltGroups)
        {
            ModelModifiers.SelectBolts(unorderedBoltGroups);
            reportManager.CreateSelectedBoltList(reportManager.FabReportPrefix, "Order Bolts");
            reportManager.Folders.ZipFolder(reportManager.Folders.BoltPath);
            EmailWriter.WriteBoltOrderEmail(reportManager.ProjectData, reportManager.FabReportPrefix, reportManager.IssueNum, reportManager.PhaseNum, siteDate, reportManager.Folders.BoltPath);
        }
    }
}