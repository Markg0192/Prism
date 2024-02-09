//using Microsoft.Office.Interop.Excel;
using Microsoft.Office.Interop.Outlook;
//using Org.BouncyCastle.Utilities;
using System.Collections.Generic;
using System.Windows.Forms.VisualStyles;
using Tekla.Structures.Model;
using Model = Tekla.Structures.Model.Model;

namespace Prism
{
    public static class Order
    {
        public static void ShearStuds(string orderType, ReportManager myReportManager, string siteDate)
        {
            OrderBolts(orderType, myReportManager, siteDate);
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
                return OrderHoldingDownBolts(myObjects, myReportManager, projectData.ProjNumberAndName); //Doesn't do anything..
            }
            return true;
        }

        public static bool FabsecCarcasses(ReportManager myReportManager, Model model, PrismProjectData projectData, string phaseNumber, string issueNumber,
            SelectedObjects myObjects, int stageNumber, string orderType, string orderDate, int typeOfOrder)
        {
            if (typeOfOrder == 1)
            {
                return FabsecProcessing.CreateFabsecCarcasses(myObjects, model);
            }
            if (typeOfOrder == 2)
            {
                if (!FabsecProcessing.AddCarcassToSelection(model, myObjects, out List<Part> originalFabsecs, out List<Part> fabsecCarcasses)) return false;
                return OrderFabsecCarcasses(myReportManager, model, projectData, phaseNumber, issueNumber, myObjects, stageNumber, originalFabsecs, fabsecCarcasses, orderType, orderDate);
            }
            return false;
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

        public static bool OrderFabsecCarcasses(ReportManager myReportManager, Model model, PrismProjectData projectData, string phaseNumber,
            string issueNumber, SelectedObjects myObjects, int stageNumber, List<Part> originalFabsecs, List<Part> fabsecCarcasses, string orderType, string orderDate)
        {
            if (!myReportManager.Folders.CreateFabsecCarcassFolder()) return false;
            ReportManager.SelectDrawingsInDocManager(null);
            DrawingManager dm = new DrawingManager(model, projectData, phaseNumber, issueNumber);
            if (dm.NotLabelledDrawings.Count != 0) { PrismWarnings.IncorrectlyAssignedDrawings(); return false; }

            List<int> drawingCount = new List<int> { 0, dm.PgcDrawings.Count };
            DrawingManager.PrintAndIssueDrawings(myReportManager.Folders.FabsecCarcassFolder, myReportManager.Folders.CarcassOrderPath, drawingCount, "\\PGC", 0, 1, myReportManager, false);
            PrismMacroBuilder.ClearPrintDialog();

            fabsecCarcasses.SelectParts();
            myReportManager.CreateG2Assy();
            BswxExporter.ExportBSWX(myObjects, myReportManager.Folders.CarcassOrderPath, projectData, phaseNumber, issueNumber, Enums.StageTypes.Prelim3);
            ModelModifiers.RemoveLog(myReportManager.Folders.CarcassOrderPath);

            myObjects.SelectedModelParts.SelectParts();

            if (!originalFabsecs.ModifyAttributes(stageNumber, projectData)) { return false; }
            foreach(Part fabsec in originalFabsecs)
            {
                ModelModifiers.ModifyUDA(fabsec, ModelUDA.FabsecCarcassOrdered(), projectData.Date);
            }
            model.CommitChanges();

            myReportManager.Folders.ZipFolder(myReportManager.Folders.CarcassOrderPath);

            PrismWarnings.MaterialOrderComplete(projectData);

            EmailWriter.WriteFabsecCarcassEmail(myReportManager.ProjectData, myReportManager.CarcassReportPrefix, myReportManager.IssueNum, myReportManager.PhaseNum, orderDate, myReportManager.Folders.CarcassOrderPath);

            return true;

        }

        private static bool OrderHoldingDownBolts(SelectedObjects selectedObjects, ReportManager reportManager, string projectName)
        {
            List<Part> myHDBolts = HDBolts.GetHdBoltItems(selectedObjects, true);
            ModelModifiers.SelectParts(myHDBolts);
            reportManager.CreateHDBoltList(); // currently does nothing
            Logging.LogProgress(projectName, "Material 3 - HD Bolts", 0, selectedObjects.AssembliesList.Count);
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
