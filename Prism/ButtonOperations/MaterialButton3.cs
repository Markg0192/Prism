using System.Collections.Generic;
using Tekla.Structures.Model;
using static Prism.Enums;

namespace Prism.ButtonOperations
{
    public static class MaterialButton3
    {
        public static bool MaterialButton3op(this SelectedObjects myObjects, PrismProjectData projectData,
            string phaseNumber, string issueNumber, string orderType, int stageNumber, StageTypes stageType, Model model, string siteDate)
        {
            ReportManager myReportManager = new ReportManager(projectData, phaseNumber, issueNumber);
            // HDBolts.StampConnectionCodeOnMainMember(myObjects);

            if (orderType.Contains("Bolts"))
            {
                if (!CreateBoltOrder(orderType, myReportManager, siteDate)) return false;
                return true;
            }

            if (orderType.Contains("Seversafe"))
            {
                int divisionNo = PrismWarnings.DivsionFrom();
                if (divisionNo == 0) { PrismWarnings.Cancelled(); return false; }

                SeversafeOrder.CreateSeversafeOrder(model, myObjects.SeversafeParts, siteDate, myReportManager, divisionNo, myReportManager.EpoReportPrefix, projectData);
                return true;
            }

            bool fabsecsPresent = FabsecProcessing.AddCarcassToSelection(model, myObjects, out List<Part> originalFabsecs, out List<Part> fabsecCarcasses);
            foreach (Part myPart in myObjects.SelectedModelParts)
            {
                myPart.GetUnorderedParts();
            }

            OrderHDBolts(orderType, myObjects, myReportManager, projectData.ProjName, projectData.WebService); //Doesn't do anything..

            if (!ShouldPartsBeOrdered(orderType)) { return false; }

            if (!myReportManager.Folders.CreateMatFolder(fabsecsPresent)) return false;

            if (!OrderFabsecs(fabsecsPresent, myReportManager, model, projectData, phaseNumber, issueNumber, myObjects, stageNumber, originalFabsecs, fabsecCarcasses)) { return false; }

            ModelModifiers.VariationCheck(phaseNumber, myObjects, projectData);
            myObjects.AddPrelimMarks(projectData, projectData.WebService);

            myReportManager.CreateMaterialReports(myObjects, orderType, stageType);

            myObjects.OmittedParts = MoveOmitMaterial(orderType, myObjects, originalFabsecs);

            if (!FinishOrder(myObjects, stageNumber, projectData, myReportManager.MatReportPrefix, issueNumber, phaseNumber, orderType, myReportManager, siteDate, false, fabsecsPresent)) { return false; }

            return true;
        }

        public static bool CreateBoltOrder(string orderType, ReportManager reportManager, string siteDate)
        {
            int typeOfOrder = PrismWarnings.BoltOrderType();

            if (!reportManager.Folders.CreateBoltFolder()) return false;

            if (typeOfOrder == 1) { reportManager.CreateSelectedBoltList(reportManager.BoltReportPrefix, orderType); }
            else reportManager.CreateBoltList(reportManager.BoltReportPrefix, orderType);

            reportManager.Folders.ZipFolder(reportManager.Folders.BoltPath);
            EmailWriter.WriteBoltOrderEmail(reportManager.ProjectData, reportManager.BoltReportPrefix, reportManager.IssueNum, reportManager.PhaseNum, siteDate, reportManager.Folders.BoltPath);
            return true;
        }

        private static void OrderHDBolts(string orderType, SelectedObjects myObjects, ReportManager myReportManager, string projectName, ExternalService.WebService1 service)
        {
            if (orderType == "Order HD Bolts") //Order HD bolts not an option therefore this statement is never true(for now)
            {
                HDBolts.OrderHDBolts(myObjects, myReportManager);
                Logging.LogProgress(projectName, "Material 3 - HD Bolts", 0, myObjects.AssembliesList.Count, service);
            }
        }

        private static bool ShouldPartsBeOrdered(string orderType)
        {
            if (orderType == "Omit Material")
            {
                if (ModelChecker.NotOrderedParts.Count != 0)
                {
                    PrismWarnings.HasNotBeenOrderedOMIT();
                    return false;
                }
            }

            if (orderType != "Omit Material")
            {
                if (ModelChecker.OrderedParts.Count != 0)
                {
                    PrismWarnings.HasAlreadyBeenOrdered();
                    return false;
                }
            }
            return true;
        }

        private static bool OrderFabsecs(bool fabsecsPresent, ReportManager myReportManager, Model model, PrismProjectData projectData, string phaseNumber,
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

        private static List<Part> MoveOmitMaterial(string orderType, SelectedObjects myObjects, List<Part> originalFabsecs)
        {
            List<Part> movedParts = new List<Part>();
            if (orderType == "Omit Material")
            {
                bool keepPartInModel = PrismWarnings.KeepPartInModel();
                movedParts.AddRange(ModelModifiers.MoveAndRenameOmittedMembers(myObjects.SelectedModelParts, -100000, keepPartInModel));
                if (originalFabsecs != null)
                {
                    movedParts.AddRange(ModelModifiers.MoveAndRenameOmittedMembers(originalFabsecs, -100000, keepPartInModel));
                }
            }
            return movedParts;
        }

        public static bool FinishOrder(SelectedObjects myObjects, int stageNumber, PrismProjectData projectData, string matReportPrefix,
            string issueNumber, string phaseNumber, string orderType, ReportManager reportManager, string siteDate, bool isSpecialFittingOrder = false, bool fabsecsPresent = false)
        {
            if (orderType == "Omit Material")
            {
                if (!myObjects.OmittedParts.ModifyAttributes(8, projectData, isSpecialFittingOrder)) { return false; }
            }
            else
            {
                if (!myObjects.SelectedModelParts.ModifyAttributes(stageNumber, projectData, isSpecialFittingOrder)) { return false; }
            }

            PrismWarnings.MaterialOrderComplete(projectData);

            reportManager.Folders.ZipFolder(reportManager.Folders.MatPath);
            EmailWriter.WriteMatEmail(projectData, myObjects, matReportPrefix, issueNumber, phaseNumber, orderType, reportManager.Folders.MatPath, fabsecsPresent, siteDate);

            Logging.LogProgress(projectData.ProjName, "Material 3", 0, myObjects.AssembliesList.Count, projectData.WebService);
            return true;
        }
    }
}