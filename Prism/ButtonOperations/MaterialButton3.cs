using Microsoft.Office.Interop.Outlook;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Tekla.Structures.Model;
using Tekla.Structures.Model.Operations;
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
            bool fabsecsPresent = FabsecProcessing.AddCarcassToSelection(model, myObjects, out List<Part> originalFabsecs, out List<Part> fabsecCarcasses);

            if (orderType.Contains("Bolts"))
            {
                if (!CreateBoltOrder(orderType, myReportManager, phaseNumber, issueNumber, projectData, siteDate)) return false;
                return true;
            }

            foreach (Part myPart in myObjects.SelectedModelParts)
            {
                myPart.GetUnorderedParts();
            }

            OrderHDBolts(orderType, myObjects, myReportManager, projectData.ProjName); //Doesn't do anything..

            if (!ShouldPartsBeOrdered(orderType)) { return false; }

            if (!myReportManager.Folders.CreateMatFolder(fabsecsPresent)) return false;

            if (!OrderFabsecs(fabsecsPresent, myReportManager, model, projectData, phaseNumber, issueNumber, myObjects, stageNumber, originalFabsecs, fabsecCarcasses)) { return false; }

            ModelModifiers.VariationCheck(phaseNumber, myObjects, projectData);
            myObjects.AddPrelimMarks(projectData);

            myReportManager.CreateMaterialReports(myObjects, orderType, stageType);

            MoveOmitMaterial(orderType, myObjects, originalFabsecs);

            if (!FinishOrder(myObjects, stageNumber, projectData, myReportManager.MatReportPrefix, issueNumber, phaseNumber, orderType, myReportManager, false, fabsecsPresent)) { return false; }

            return true;
        }

        public static bool CreateBoltOrder(string orderType, ReportManager reportManager, string phaseNumber, string issueNumber, PrismProjectData projectData, string siteDate)
        {
            int typeOfOrder = PrismWarnings.BoltOrderType();

            reportManager = new ReportManager(projectData, phaseNumber, issueNumber);
            if (!reportManager.Folders.CreateBoltFolder()) return false;

            if (typeOfOrder == 1) { reportManager.CreateSelectedBoltList(reportManager.BoltReportPrefix, orderType); } else reportManager.CreateBoltList(reportManager.BoltReportPrefix, orderType);
            reportManager.Folders.ZipFolder(reportManager.Folders.BoltPath);
            EmailWriter.WriteBoltOrderEmail(projectData, reportManager.BoltReportPrefix, issueNumber, phaseNumber, siteDate, reportManager.Folders.BoltPath);
            return true;
        }

        private static void OrderHDBolts(string orderType, SelectedObjects myObjects, ReportManager myReportManager, string projectName)
        {
            if (orderType == "Order HD Bolts") //Order HD bolts not an option therefore this statement is never true(for now)
            {
                HDBolts.OrderHDBolts(myObjects, myReportManager);
                Logging.LogProgress(projectName, "Material 3 - HD Bolts", 0, myObjects.AssembliesList.Count);
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
                bool fabsecWarning = PrismWarnings.FabsecsPresent();
                if (fabsecWarning)
                {
                    ReportManager.SelectDrawingsInDocManager(null);
                    DrawingManager dm = new DrawingManager(model, projectData, phaseNumber, issueNumber, myObjects);
                    if(dm.NotLabelledDrawings.Count != 0) { PrismWarnings.IncorrectlyAssignedDrawings(); return false; }

                    List<int> drawingCount = new List<int> { 0, dm.PgcDrawings.Count };
                    DrawingManager.PrintAndIssueDrawings(myReportManager.Folders.MatFolder, myReportManager.Folders.MatPath, drawingCount, "\\PGC", 0, 1, myReportManager);
                    fabsecCarcasses.SelectParts();
                    myReportManager.CreateG2Assy();
                    myObjects.SelectedModelParts.SelectParts();                   

                    if (!originalFabsecs.ModifyAttributes(stageNumber, projectData)) { return false; }                  
                }
                else { return false; }
            }
            return true;
        }

        private static void MoveOmitMaterial(string orderType, SelectedObjects myObjects, List<Part> originalFabsecs)
        {
            if (orderType == "Omit Material")
            {
                ModelModifiers.MoveAndRenameOmittedMembers(myObjects.SelectedModelParts, -100000);
                if(originalFabsecs != null)
                {
                    ModelModifiers.MoveAndRenameOmittedMembers(originalFabsecs, -100000);
                }
            }
        }

        public static bool FinishOrder(SelectedObjects myObjects, int stageNumber, PrismProjectData projectData, string matReportPrefix,
            string issueNumber, string phaseNumber, string orderType, ReportManager reportManager, bool isSpecialFittingOrder = false, bool fabsecsPresent = false)
        {
            if (!myObjects.SelectedModelParts.ModifyAttributes(stageNumber, projectData, isSpecialFittingOrder)) { return false; }

            PrismWarnings.MaterialOrderComplete(projectData);

            reportManager.Folders.ZipFolder(reportManager.Folders.MatPath);
            EmailWriter.WriteMatEmail(projectData, myObjects, matReportPrefix, issueNumber, phaseNumber, orderType, reportManager.Folders.MatPath, fabsecsPresent);

            Logging.LogProgress(projectData.ProjName, "Material 3", 0, myObjects.AssembliesList.Count);
            return true;
        }
    }
}