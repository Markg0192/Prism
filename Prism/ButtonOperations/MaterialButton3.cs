using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Tekla.Structures.Model;
using Tekla.Structures.Model.Operations;
using static Prism.Enums;

namespace Prism.ButtonOperations
{
    public static class MaterialButton3
    {
        public static bool MaterialButton3op(this SelectedObjects myObjects, PrismProjectData projectData, ProjectInfo projectInfo,
            string phaseNumber, string issueNumber, string orderType, int stageNumber, StageTypes stageType, Model model, string siteDate)
        {
            ReportManager myReportManager = new ReportManager(projectData, phaseNumber, issueNumber);
            // HDBolts.StampConnectionCodeOnMainMember(myObjects);
            bool fabsecsPresent = FabsecProcessing.AddCarcassToSelection(model, myObjects, out List<Part> originalFabsecs);

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

            if (!OrderFabsecs(fabsecsPresent, myReportManager, model, projectData, phaseNumber, issueNumber, myObjects, stageNumber, originalFabsecs)) { return false; }

            ModelModifiers.VariationCheck(phaseNumber, myObjects, projectData);
            myObjects.AddPrelimMarks(projectInfo);

            myReportManager.CreateMaterialReports(myObjects, orderType, stageType);

            MoveOmitMaterial(orderType, stageNumber, myObjects);

            if (!FinishOrder(myObjects, stageNumber, projectData, myReportManager.MatReportPrefix, issueNumber, phaseNumber, orderType, myReportManager)) { return false; }

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
            string issueNumber, SelectedObjects myObjects, int stageNumber, List<Part> originalFabsecs)
        {
            if (fabsecsPresent)
            {
                DialogResult fabsecWarning = PrismWarnings.FabsecsPresent();
                if (fabsecWarning == DialogResult.Yes)
                {
                    ReportManager.SelectDrawingsInDocManager(null);
                    Operation.CreateReportFromSelected(ReportManager._drawingDpmReportRpt, Path.Combine(myReportManager.Folders.FabsecCarcassPath, ReportManager._drawingDpmReportXsr), "", "", "");
                    DrawingManager dm = new DrawingManager(model, projectData, phaseNumber, issueNumber, myObjects, myReportManager.Folders.FabsecCarcassPath);
                    dm.PrintDrawingToModelFolder(myReportManager.Folders.FabsecCarcassPath);
                    if (!originalFabsecs.ModifyAttributes(stageNumber, projectData)) { return false; }
                    ModelModifiers.RemoveIDDessin(myReportManager.Folders.FabsecCarcassPath);
                }
                else { return false; }
            }
            return true;
        }

        private static void MoveOmitMaterial(string orderType, int stageNumber, SelectedObjects myObjects)
        {
            if (orderType == "Omit Material")
            {
                stageNumber = 8;
                myObjects.MoveAndRenameOmittedMembers();
            }
        }

        public static bool FinishOrder(SelectedObjects myObjects, int stageNumber, PrismProjectData projectData, string matReportPrefix,
            string issueNumber, string phaseNumber, string orderType, ReportManager reportManager, bool isSpecialFittingOrder = false)
        {
            if(!myObjects.SelectedModelParts.ModifyAttributes(stageNumber, projectData, isSpecialFittingOrder)) { return false; }

            DialogResult finishBox = PrismWarnings.MaterialOrderComplete(projectData);
            if (finishBox == DialogResult.OK)
            {
                reportManager.Folders.ZipFolder(reportManager.Folders.MatPath);
                EmailWriter.WriteMatEmail(projectData, myObjects, matReportPrefix, issueNumber, phaseNumber, orderType, reportManager.Folders.MatPath);
            }
            Logging.LogProgress(projectData.ProjName, "Material 3", 0, myObjects.AssembliesList.Count);
            return true;
        }
    }
}