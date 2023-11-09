using Microsoft.Office.Interop.Excel;
using System.Collections.Generic;
using Tekla.Structures.Model;
using Model = Tekla.Structures.Model.Model;

namespace Prism
{
    public static class Order
    {
        public static void ShearStuds()
        {

        }

        public static bool BoltsSeversafeAndHdBolts(string orderType, ReportManager myReportManager, string siteDate, Model model, SelectedObjects myObjects, PrismProjectData projectData, out bool orderRequired)
        {
            orderRequired = false;
            if (orderType.Contains("Bolts"))
            {
                orderRequired = true;
                return OrderBolts(orderType, myReportManager, siteDate);
            }

            if (orderType.Contains("Seversafe"))
            {
                orderRequired = true;
                return OrderSeversafe(model, myObjects, siteDate, myReportManager, projectData);
            }

            if (orderType == "Order HD Bolts") //Order HD bolts not an option therefore this statement is never true(for now) 
            {
                orderRequired = true;
                return OrderHoldingDownBolts(myObjects, myReportManager, projectData.ProjNumberAndName, projectData.WebService); //Doesn't do anything..
            }
            return true;
        }

        private static bool OrderSeversafe(Model model, SelectedObjects myObjects, string siteDate, ReportManager myReportManager, PrismProjectData projectData)
        {
            int divisionNo = PrismWarnings.DivsionFrom();
            if (divisionNo == 0) { PrismWarnings.Cancelled(); return false; }

            SeversafeOrder.CreateSeversafeOrder(model, myObjects.SeversafeParts, siteDate, myReportManager, divisionNo, myReportManager.EpoReportPrefix, projectData);
            return true;
        }

        private static bool OrderBolts(string orderType, ReportManager reportManager, string siteDate)
        {
            int typeOfOrder = PrismWarnings.BoltOrderType();

            if (!reportManager.Folders.CreateBoltFolder()) return false;

            if (typeOfOrder == 1) { reportManager.CreateSelectedBoltList(reportManager.BoltReportPrefix, orderType); }
            else reportManager.CreateBoltList(reportManager.BoltReportPrefix, orderType);

            reportManager.Folders.ZipFolder(reportManager.Folders.BoltPath);
            EmailWriter.WriteBoltOrderEmail(reportManager.ProjectData, reportManager.BoltReportPrefix, reportManager.IssueNum, reportManager.PhaseNum, siteDate, reportManager.Folders.BoltPath);
            return true;
        }

        public static bool Fabsecs(bool fabsecsPresent, ReportManager myReportManager, Model model, PrismProjectData projectData, string phaseNumber,
            string issueNumber, SelectedObjects myObjects, int stageNumber, List<Part> originalFabsecs, List<Part> fabsecCarcasses)
        {
            if (fabsecsPresent)
            {
                bool includeCarcassDrawings = PrismWarnings.RunFabsecDrawings();
                if (includeCarcassDrawings)
                {
                    ReportManager.SelectDrawingsInDocManager(null);
                    DrawingManager dm = new DrawingManager(model, projectData, phaseNumber, issueNumber);
                    if (dm.NotLabelledDrawings.Count != 0) { PrismWarnings.IncorrectlyAssignedDrawings(); return false; }

                    List<int> drawingCount = new List<int> { 0, dm.PgcDrawings.Count };
                    DrawingManager.PrintAndIssueDrawings(myReportManager.Folders.MatFolder, myReportManager.Folders.MatPath, drawingCount, "\\PGC", 0, 1, myReportManager, false);
                    PrismMacroBuilder.ClearPrintDialog();
                }
                fabsecCarcasses.SelectParts();
                myReportManager.CreateG2Assy();
                myObjects.SelectedModelParts.SelectParts();

                if (!originalFabsecs.ModifyAttributes(stageNumber, projectData)) { return false; }

                //  else { return false; }
            }
            return true;
        }

        private static bool OrderHoldingDownBolts(SelectedObjects selectedObjects, ReportManager reportManager, string projectName, ExternalService.WebService1 service)
        {
            List<Part> myHDBolts = HDBolts.GetHdBoltItems(selectedObjects, true);
            ModelModifiers.SelectParts(myHDBolts);
            reportManager.CreateHDBoltList(); // currently does nothing
            Logging.LogProgress(projectName, "Material 3 - HD Bolts", 0, selectedObjects.AssembliesList.Count, service);
            return true;
        }

        private static void HDBoltTopNutAndWasher(SelectedObjects selectedObjects, ReportManager reportManager)
        {
            List<Part> myHDBolts = HDBolts.GetHdBoltItems(selectedObjects, false);
            ModelModifiers.SelectParts(myHDBolts);
            reportManager.CreateHDBoltList(); // currently does nothing
        }
    }
}
