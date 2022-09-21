namespace Prism
{
    public static class FabMisc
    {
        public static void FabMiscOp(string phaseNumber, string issueNumber, PrismProjectData projectData, string siteDate, SelectedObjects selectedObjects)
        {
            CreateBoltOrder(phaseNumber, issueNumber, projectData, siteDate);
            ModelModifiers.StampBoltUDA(selectedObjects.AllBolts[0], projectData.Full, projectData.Date);

            ModelModifiers.StampPartFabUDA(selectedObjects.SelectedModelParts, phaseNumber, issueNumber);
            ViewManager.CreateFabView(phaseNumber, issueNumber, projectData, selectedObjects);
        }

        public static void CreateBoltOrder(string phaseNumber, string issueNumber, PrismProjectData projectData, string siteDate)
        {
            ReportManager myReportManager = new ReportManager(projectData, phaseNumber, issueNumber);
            myReportManager.Folders.CreateBoltFolder();
            myReportManager.CreateBoltList();
            EmailWriter.WriteBoltOrderEmail(projectData, myReportManager.FabReportPrefix, issueNumber, phaseNumber, siteDate);
        }
    }
}