using Prism.Managers.ChangeManager;
using System.Collections.Generic;
using System.Linq;
using Tekla.Structures.Model;
using static Prism.Enums;
using Model = Tekla.Structures.Model.Model;

namespace Prism.ButtonOperations
{
    public static class CreatePackageButton
    {
        public static bool CreateFabPackage(this SelectedObjects myObjects, Model model, PrismProjectData projectData, string phaseNumber, string issueNumber, StageTypes stageType, string siteDate, bool runSeversafe, bool runChangeManager)
        {
            if (!runChangeManager)
            {
                if (!CreateFirstIssue(projectData, phaseNumber, issueNumber, runSeversafe, myObjects, model, stageType, siteDate)) return false;
                return true;
            }

            else
            {
                bool isIssue01 = issueNumber == "01";
                string fileLocation = Constants.ModelDataLogLocation(projectData.ProjNumberAndGuid + "\\Fab XMLs");

                if (!ChangeHelper.RunChangeManagement(model, issueNumber, fileLocation, phaseNumber, projectData,  out List<SteelItemBase> revisedItems,
                    out List<SteelItemBase> omitItems, out List<SteelItemBase> addItems, out string messageForEmail)) return false;

                if (isIssue01)
                {
                    if (!CreateFirstIssue(projectData, phaseNumber, issueNumber, runSeversafe, myObjects, model, stageType, siteDate)) return false;
                }
                else
                {
                    CpuCounter cpuCounter = new CpuCounter();
                    ReportManager reportManager = new ReportManager(projectData, phaseNumber, issueNumber);

                    if (revisedItems != null && addItems != null)
                    {
                        if (!reportManager.Folders.CreateFabFolders()) return false;
                        if (!reportManager.Folders.CreateBoltFolder()) return false;
                        if (runSeversafe) { if (!reportManager.Folders.CreateEpoFolder()) return false; }
                        if (myObjects.SeversafePresent) { myObjects.NonSeversafeParts.SelectParts(); }

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

                        List<Part> combinedParts = addedItems.Concat(reviseItems).ToList();

                        ModelModifiers.SelectParts(combinedParts);

                        CpuSpeedCheck(cpuCounter);
                        ReportManager.SelectDrawingsInDocManager(combinedParts);

                        CpuSpeedCheck(cpuCounter);
                        DrawingManager drawingManager = new DrawingManager(model, projectData, phaseNumber, issueNumber);

                        CpuSpeedCheck(cpuCounter);
                        PrismMacroBuilder.IssueAndLockStampOff();

                        if (!drawingManager.DrawingsAreUpToDate) { PrismWarnings.DrawingsNotUpToDate(); return false; }
                        if (drawingManager.NotLabelledDrawings.Count != 0)
                        {
                            PrismWarnings.IncorrectlyAssignedDrawings();
                            Logging.UnAssignedDrawings(projectData.ProjNumber, drawingManager.NotLabelledDrawings);
                            return false;
                        }

                        List<int> drawingCount = CountDrawings(drawingManager);
                        DrawingManager.PrintDrawings(reportManager, drawingManager, drawingCount);

                        reportManager.CreateFabReports(combinedParts, myObjects.PrismBoltGroups);
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
                }
            }
            return true;
        }

        private static bool CreateFirstIssue(PrismProjectData projectData, string phaseNumber, string issueNumber, bool runSeversafe, SelectedObjects myObjects, Model model, StageTypes stageType, string siteDate)
        {
            CpuCounter cpuCounter = new CpuCounter();
            ReportManager reportManager = new ReportManager(projectData, phaseNumber, issueNumber);

            if (!reportManager.Folders.CreateFabFolders()) return false;
            if (!reportManager.Folders.CreateBoltFolder()) return false;
            if (runSeversafe) { if (!reportManager.Folders.CreateEpoFolder()) return false; }

            if (myObjects.SeversafePresent) { myObjects.NonSeversafeParts.SelectParts(); }

            CpuSpeedCheck(cpuCounter);
            ReportManager.SelectDrawingsInDocManager(myObjects.NonSeversafeParts);

            CpuSpeedCheck(cpuCounter);
            DrawingManager drawingManager = new DrawingManager(model, projectData, phaseNumber, issueNumber);

            CpuSpeedCheck(cpuCounter);
            PrismMacroBuilder.IssueAndLockStampOff();

            if (!drawingManager.DrawingsAreUpToDate) { PrismWarnings.DrawingsNotUpToDate(); return false; }
            if (drawingManager.NotLabelledDrawings.Count != 0)
            {
                PrismWarnings.IncorrectlyAssignedDrawings();
                Logging.UnAssignedDrawings(projectData.ProjNumber, drawingManager.NotLabelledDrawings);
                return false;
            }

            List<int> drawingCount = CountDrawings(drawingManager);
            DrawingManager.PrintDrawings(reportManager, drawingManager, drawingCount);

            reportManager.CreateFabReports(myObjects.NonSeversafeParts, myObjects.PrismBoltGroups);
            if (!Constants.IsSpecialPerson()) { myObjects.ExportBSWX(reportManager.Folders.DspPath, projectData, phaseNumber, issueNumber, stageType); }

            if (!myObjects.NonSeversafeParts.ModifyAttributes((int)stageType, projectData)) { return false; }

            reportManager.Folders.RemoveUnusedFolders();

            bool zipFileCanBeAttached = reportManager.Folders.ZipFolder(reportManager.Folders.FabPath);

            PrismWarnings.FabPackComplete(projectData, zipFileCanBeAttached);

            EmailWriter.WriteFabEmail(projectData, myObjects, reportManager.FabReportPrefix, issueNumber, phaseNumber, siteDate, reportManager.Folders.FabPath, zipFileCanBeAttached);

            Logging.UpdateFrozenDrawingCount(projectData.ProjNumberAndGuid, drawingManager.FrozenDrawings.Count, drawingManager.UnFrozenDrawings.Count);
            Logging.LogProgress(projectData.ProjNumberAndName, stageType.ToString(), 0, myObjects.AssembliesList.Count);

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