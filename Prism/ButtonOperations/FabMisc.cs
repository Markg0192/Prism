using Tekla.Structures.Model;

namespace Prism
{
    public static class FabMisc
    {
        public static void FabMiscOp(Model model, string siteDate, SelectedObjects selectedObjects, bool runSeversafe, ReportManager reportManager, int divisionNo, PrismProjectData projData)
        {
            if (selectedObjects.AllBolts[0].Count != 0 || selectedObjects.AllBolts[1].Count != 0)
            {
                CreateBoltOrder(reportManager, siteDate);
                ModelModifiers.StampBoltUDA(selectedObjects.AllBolts[0], reportManager.ProjectData.Full, reportManager.ProjectData.Date);
                ModelModifiers.StampBoltUDA(selectedObjects.AllBolts[1], reportManager.ProjectData.Full, reportManager.ProjectData.Date);
            }

            if (runSeversafe) { SeversafeOrder.CreateSeversafeOrder(model, selectedObjects.SeversafeParts, siteDate, reportManager, divisionNo, reportManager.EpoReportPrefix, projData); }

            ModelModifiers.StampPartFabUDA(selectedObjects.SelectedModelParts, reportManager.PhaseNum, reportManager.IssueNum);
            ViewManager.CreateFabView(reportManager.PhaseNum, reportManager.IssueNum, reportManager.ProjectData, selectedObjects);
        }

        public static void CreateBoltOrder(ReportManager reportManager, string siteDate)
        {
            reportManager.CreateBoltList(reportManager.FabReportPrefix, "Order Bolts");
            reportManager.Folders.ZipFolder(reportManager.Folders.BoltPath);
            EmailWriter.WriteBoltOrderEmail(reportManager.ProjectData, reportManager.FabReportPrefix, reportManager.IssueNum, reportManager.PhaseNum, siteDate, reportManager.Folders.BoltPath);
        }
    }
}