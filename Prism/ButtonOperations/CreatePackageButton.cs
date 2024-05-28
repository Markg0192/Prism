using Prism.Managers.ChangeManager;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Tekla.Structures.Model;
using static Prism.Enums;
using Model = Tekla.Structures.Model.Model;

namespace Prism.ButtonOperations
{
    public static class CreatePackageButton
    {
        public static bool CreateFabPackage(this SelectedObjects myObjects, Model model, PrismProjectData projectData, string phaseNumber, string issueNumber, StageTypes stageType, string siteDate, bool runSeversafe, bool runChangeManager,
           ToolStrip toolStrip, ToolStripStatusLabel label, string teklaVersion)
        {
            if (!runChangeManager)
            {
                return CreateFirstIssue(projectData, phaseNumber, issueNumber, runSeversafe, myObjects, model, stageType, siteDate, teklaVersion);
            }

            string fileLocation = Constants.ModelDataLogLocation(projectData.ProjNumberAndGuid + "\\Fab XMLs");

            if (!ChangeHelper.RunChangeManagement(model, issueNumber, fileLocation, phaseNumber, projectData, myObjects, toolStrip, label, out List<SteelItemBase> revisedItems,
                out List<SteelItemBase> omitItems, out List<SteelItemBase> addItems, out string messageForEmail)) return false;

            return issueNumber == "01"
                ? CreateFirstIssue(projectData, phaseNumber, issueNumber, runSeversafe, myObjects, model, stageType, siteDate, teklaVersion)
                : ProcessSubsequentIssues(projectData, phaseNumber, issueNumber, revisedItems, addItems, runSeversafe, myObjects, model, siteDate, stageType, messageForEmail, teklaVersion);
        }

        private static bool CreateFirstIssue(PrismProjectData projectData, string phaseNumber, string issueNumber, bool runSeversafe, SelectedObjects myObjects, Model model, StageTypes stageType, string siteDate,string teklaVersion)
        {
            if (!InitialisePackageAndCreateFolders(projectData, phaseNumber, issueNumber, runSeversafe, myObjects, out ReportManager reportManager, out CpuCounter cpuCounter)) return false;

            if (!ProcessAndPrintDrawings(cpuCounter, myObjects.NonSeversafeParts, model, projectData, phaseNumber, issueNumber, reportManager, out DrawingManager drawingManager)) return false;

            if (!Constants.IsSpecialPerson()) { myObjects.ExportBSWX(reportManager.Folders.DspPath, projectData, phaseNumber, issueNumber, stageType); }
  
            reportManager.CreateFabReports(myObjects.NonSeversafeParts, myObjects.PrismBoltGroups, teklaVersion);

            if (!myObjects.NonSeversafeParts.ModifyAttributes((int)stageType, projectData)) { return false; }

            reportManager.Folders.RemoveUnusedFolders();

            bool zipFileCanBeAttached = reportManager.Folders.ZipFolder(reportManager.Folders.FabPath);

            PrismWarnings.FabPackComplete(projectData, zipFileCanBeAttached);

            EmailWriter.WriteFabEmail(projectData, myObjects, reportManager.FabReportPrefix, issueNumber, phaseNumber, siteDate, reportManager.Folders.FabPath, zipFileCanBeAttached);

            Logging.UpdateFrozenDrawingCount(projectData.ProjNumberAndGuid, drawingManager.FrozenDrawings.Count, drawingManager.UnFrozenDrawings.Count);
            Logging.LogProgress(projectData.ProjNumberAndName, stageType.ToString(), 0, myObjects.AssembliesList.Count);

            return true;
        }

        private static bool ProcessSubsequentIssues(PrismProjectData projectData, string phaseNumber, string issueNumber, List<SteelItemBase> revisedItems, List<SteelItemBase> addItems,
            bool runSeversafe, SelectedObjects myObjects, Model model, string siteDate, StageTypes stageType, string messageForEmail, string teklaVersion)
        {
            if (revisedItems != null && addItems != null)
            {
                if (!InitialisePackageAndCreateFolders(projectData, phaseNumber, issueNumber, runSeversafe, myObjects, out ReportManager reportManager, out CpuCounter cpuCounter)) return false;

                List<Part> combinedParts = CombineAddAndOmitParts(model, revisedItems, addItems);

                ModelModifiers.SelectParts(combinedParts);

                if (!ProcessAndPrintDrawings(cpuCounter, combinedParts, model, projectData, phaseNumber, issueNumber, reportManager, out DrawingManager drawingManager)) return false;

                reportManager.CreateFabReports(combinedParts, myObjects.PrismBoltGroups, teklaVersion);
                ModelModifiers.SelectParts(combinedParts);

                if (!Constants.IsSpecialPerson()) { myObjects.ExportBSWX(reportManager.Folders.DspPath, projectData, phaseNumber, issueNumber, stageType); }

                if (!combinedParts.ModifyAttributes((int)stageType, projectData)) { return false; }

                reportManager.Folders.RemoveUnusedFolders();

                bool zipFileCanBeAttached = reportManager.Folders.ZipFolder(reportManager.Folders.FabPath);

                PrismWarnings.FabPackComplete(projectData, zipFileCanBeAttached);

                EmailWriter.WriteRevisedFabEmail(projectData, reportManager.FabReportPrefix, issueNumber, phaseNumber, siteDate, reportManager.Folders.FabPath, zipFileCanBeAttached, messageForEmail);

                Logging.UpdateFrozenDrawingCount(projectData.ProjNumberAndGuid, drawingManager.FrozenDrawings.Count, drawingManager.UnFrozenDrawings.Count);
                Logging.LogProgress(projectData.ProjNumberAndName, stageType.ToString(), 0, myObjects.AssembliesList.Count);
            }

            return true;
        }

        private static bool ProcessAndPrintDrawings(CpuCounter cpuCounter, List<Part> partsToSelect, Model model, PrismProjectData projectData, string phaseNumber, string issueNumber, ReportManager reportManager, out DrawingManager drawingManager)
        {
            CpuSpeedCheck(cpuCounter);
            ReportManager.SelectDrawingsInDocManager(partsToSelect);

            CpuSpeedCheck(cpuCounter);
            drawingManager = new DrawingManager(model, projectData, phaseNumber, issueNumber);

            CpuSpeedCheck(cpuCounter);
            PrismMacroBuilder.IssueAndLockStampOff();

            bool createDrawings = true;

            if (!drawingManager.DrawingsAreUpToDate) { PrismWarnings.DrawingsNotUpToDate(); return false; }
            if (drawingManager.NotLabelledDrawings.Count != 0)
            {
                PrismWarnings.IncorrectlyAssignedDrawings();
                Logging.UnAssignedDrawings(projectData.ProjNumber, drawingManager);

                if (!PrismWarnings.CreatePackageWithoutDrawings()) return false;
                else createDrawings = false;
            }

            if (createDrawings)
            {
                List<int> drawingCount = CountDrawings(drawingManager);
                DrawingManager.PrintDrawings(reportManager, drawingManager, drawingCount);
            }

            return true;
        }

        private static List<Part> CombineAddAndOmitParts(Model model, List<SteelItemBase> revisedItems, List<SteelItemBase> addItems)
        {
            List<Part> reviseItems = revisedItems
                    .Select(item => model.GetIdentifierByGUID(item.Guid))
                    .Select(id => model.SelectModelObject(id))
                    .OfType<Part>()
                    .ToList();
            List<Part> addedItems = addItems
                    .Select(item => model.GetIdentifierByGUID(item.Guid))
                    .Select(id => model.SelectModelObject(id))
                    .OfType<Part>()
                    .ToList();

            return addedItems.Concat(reviseItems).ToList();
        }

        private static bool InitialisePackageAndCreateFolders(PrismProjectData projectData, string phaseNumber, string issueNumber, bool runSeversafe, SelectedObjects myObjects, out ReportManager reportManager, out CpuCounter cpuCounter)
        {
            cpuCounter = new CpuCounter();
            reportManager = new ReportManager(projectData, phaseNumber, issueNumber);

            if (!reportManager.Folders.CreateFabFolders()) return false;
            if (!reportManager.Folders.CreateBoltFolder()) return false;
            if (runSeversafe) { if (!reportManager.Folders.CreateEpoFolder()) return false; }

            if (myObjects.SeversafePresent) { myObjects.NonSeversafeParts.SelectParts(); }
            return true;
        }

        public static void CpuSpeedCheck(CpuCounter cpuCounter)
        {
            while (cpuCounter.CheckCPU() > 10)
            {
                System.Threading.Thread.Sleep(500);
            }
        }

        private static List<int> CountDrawings(DrawingManager dm)
        {
            int fitStartPoint = 0;
            int fitEndPoint = dm.NotLabelledDrawings.Count + dm.FitDrawings.Count;
            int notRequiredStartPoint = fitEndPoint;
            int notRequiredEndPoint = dm.NotRequiredDrawings.Count;

            int pgcStartPoint = fitEndPoint + notRequiredEndPoint;
            int pgcEndPoint = dm.PgcDrawings.Count;

            int prtStartPoint = pgcStartPoint + pgcEndPoint;
            int prtEndPoint = dm.PrtDrawings.Count;
            int shaStartPoint = prtStartPoint + prtEndPoint;
            int shaEndPoint = dm.ShaDrawings.Count;

            int assStartPoint = 0;
            int assEndPoint = dm.AssDrawings.Count;

            int wldStartPoint = assEndPoint;
            int wldEndPoint = dm.WldDrawings.Count;

            return new List<int>() { fitStartPoint, fitEndPoint, notRequiredStartPoint, notRequiredEndPoint, pgcStartPoint, pgcEndPoint, prtStartPoint, prtEndPoint, shaStartPoint, shaEndPoint, assStartPoint, assEndPoint, wldStartPoint, wldEndPoint };
        }
    }
}