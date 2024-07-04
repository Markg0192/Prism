using Prism.Managers.ChangeManager;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Tekla.Structures.Drawing.UI;
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
            string packagingType = Logging.GetAdvancedSetting(projectData.ProjNumberAndGuid, AdvancedSettingType.FabPackType);

            if (packagingType.Contains("Lot"))
            {
                var groupedBeams = myObjects
                    .GetNonSeversafeParts()
                    .GroupBy(beam => beam.LotName)
                    .Select(group => group.ToList())
                    .ToList();

                foreach (var listOfMembers in groupedBeams)
                {


                }

                return true;
            }
            else
            {
                if (!runChangeManager)
                {
                    return CreateFirstIssue(projectData, phaseNumber, issueNumber, runSeversafe, myObjects, model, stageType, siteDate, teklaVersion, toolStrip, label);
                }

                string fileLocation = Constants.ModelDataLogLocation(projectData.ProjNumberAndGuid + "\\Fab XMLs");

                if (!ChangeHelper.RunChangeManagement(model, issueNumber, fileLocation, phaseNumber, projectData, myObjects, toolStrip, label, out List<SteelItemBase> revisedItems,
                    out List<SteelItemBase> omitItems, out List<SteelItemBase> addItems, out string messageForEmail)) return false;

                return issueNumber == "01"
                    ? CreateFirstIssue(projectData, phaseNumber, issueNumber, runSeversafe, myObjects, model, stageType, siteDate, teklaVersion, toolStrip, label)
                    : ProcessSubsequentIssues(projectData, phaseNumber, issueNumber, revisedItems, addItems, runSeversafe, myObjects, model, siteDate, stageType, messageForEmail, teklaVersion, toolStrip, label);
            }
        }

        private static bool CreateFirstIssue(PrismProjectData projectData, string phaseNumber, string issueNumber, bool runSeversafe, SelectedObjects myObjects, Model model, StageTypes stageType, string siteDate, string teklaVersion,
            ToolStrip toolStrip, ToolStripStatusLabel tssl)
        {
            if (!InitialisePackageAndCreateFolders(projectData, phaseNumber, issueNumber, runSeversafe, myObjects, out ReportManager reportManager, out CpuCounter cpuCounter)) return false;

            if (!ProcessAndPrintDrawings(cpuCounter, myObjects.GetNonSeversafeParts(), model, projectData, phaseNumber, issueNumber, reportManager, toolStrip, tssl, out DrawingManager drawingManager)) return false;

            if (!Constants.IsSpecialPerson()) { myObjects.ExportBSWX(reportManager.Folders.DspPath, projectData, phaseNumber, issueNumber, stageType); }

            reportManager.CreateFabReports(myObjects.GetNonSeversafeParts(), myObjects.PrismBoltGroups, teklaVersion, toolStrip, tssl);

            if (!myObjects.GetNonSeversafeParts().ModifyAttributes((int)stageType, projectData, toolStrip, tssl)) { return false; }

            reportManager.Folders.RemoveUnusedFolders();

            bool zipFileCanBeAttached = reportManager.Folders.ZipFolder(reportManager.Folders.FabPath);

            PrismWarnings.FabPackComplete(projectData, zipFileCanBeAttached);

            EmailWriter.WriteFabEmail(projectData, myObjects, reportManager.FabReportPrefix, issueNumber, phaseNumber, siteDate, reportManager.Folders.FabPath, zipFileCanBeAttached);

            Logging.UpdateFrozenDrawingCount(projectData.ProjNumberAndGuid, drawingManager.GetFrozenDrawings().Count, drawingManager.GetUnFrozenDrawings().Count);
            Logging.LogProgress(projectData.ProjNumberAndName, stageType.ToString(), 0, myObjects.GetMainParts().Count);

            return true;
        }

        private static bool ProcessSubsequentIssues(PrismProjectData projectData, string phaseNumber, string issueNumber, List<SteelItemBase> revisedItems, List<SteelItemBase> addItems,
            bool runSeversafe, SelectedObjects myObjects, Model model, string siteDate, StageTypes stageType, string messageForEmail, string teklaVersion, ToolStrip ts, ToolStripStatusLabel tssl)
        {
            if (revisedItems != null && addItems != null)
            {
                if (!InitialisePackageAndCreateFolders(projectData, phaseNumber, issueNumber, runSeversafe, myObjects, out ReportManager reportManager, out CpuCounter cpuCounter)) return false;

                List<Part> teklaParts = CombineAddAndOmitParts(model, revisedItems, addItems);

                List<PrismPart> combinedParts = new List<PrismPart>();
                foreach (Part p in teklaParts)
                {
                    combinedParts.Add(new PrismPart(p));
                }
                ModelModifiers.SelectParts(combinedParts);

                if (!ProcessAndPrintDrawings(cpuCounter, combinedParts, model, projectData, phaseNumber, issueNumber, reportManager, ts, tssl, out DrawingManager drawingManager)) return false;

                reportManager.CreateFabReports(combinedParts, myObjects.PrismBoltGroups, teklaVersion, ts, tssl);
                ModelModifiers.SelectParts(combinedParts);

                if (!Constants.IsSpecialPerson()) { myObjects.ExportBSWX(reportManager.Folders.DspPath, projectData, phaseNumber, issueNumber, stageType); }

                if (!combinedParts.ModifyAttributes((int)stageType, projectData, ts, tssl)) { return false; }

                reportManager.Folders.RemoveUnusedFolders();

                bool zipFileCanBeAttached = reportManager.Folders.ZipFolder(reportManager.Folders.FabPath);

                PrismWarnings.FabPackComplete(projectData, zipFileCanBeAttached);

                EmailWriter.WriteRevisedFabEmail(projectData, reportManager.FabReportPrefix, issueNumber, phaseNumber, siteDate, reportManager.Folders.FabPath, zipFileCanBeAttached, messageForEmail);

                Logging.UpdateFrozenDrawingCount(projectData.ProjNumberAndGuid, drawingManager.GetFrozenDrawings().Count, drawingManager.GetUnFrozenDrawings().Count);
                Logging.LogProgress(projectData.ProjNumberAndName, stageType.ToString(), 0, myObjects.GetMainParts().Count);
            }

            return true;
        }

        private static bool ProcessAndPrintDrawings(CpuCounter cpuCounter, List<PrismPart> partsToSelect, Model model, PrismProjectData projectData, string phaseNumber, string issueNumber, ReportManager reportManager, ToolStrip toolStrip, ToolStripStatusLabel statusLabel, out DrawingManager drawingManager)
        {
            CpuSpeedCheck(cpuCounter);
            ReportManager.SelectDrawingsInDocManager(partsToSelect);

            CpuSpeedCheck(cpuCounter);
            drawingManager = new DrawingManager(model, projectData, phaseNumber, issueNumber, reportManager.Folders.FabPath);

            if (!CheckForProblemsWithDrawings(drawingManager, projectData.ProjNumber, out bool createDrawings)) return false;

            if (createDrawings)
            {
                CpuSpeedCheck(cpuCounter);
                PrismMacroBuilder.IssueAndLockStampOff();
                List<int> drawingCount = NewCountDrawings(drawingManager);
                DrawingManager.NewPrintDrawings(reportManager, drawingManager, drawingCount, toolStrip, statusLabel);
            }

            return true;
        }

        private static bool CheckForProblemsWithDrawings(DrawingManager drawingManager, string projectNumber, out bool createDrawings)
        {
            createDrawings = true;

            if (drawingManager.GetOutOfDateDrawings().Count > 0) { PrismWarnings.DrawingsNotUpToDate(); return false; }
            if (drawingManager.GetDrawingFolder(DrawingFolder.Default).Count != 0)
            {
                PrismWarnings.IncorrectlyAssignedDrawings();
                Logging.UnAssignedDrawings(projectNumber, drawingManager);

                if (!PrismWarnings.CreatePackageWithoutDrawings()) return false;
                else createDrawings = false;
            }

            int drawingsWithoutRevisions = drawingManager.GetDrawingsWithoutRevision().Count;
            if (drawingsWithoutRevisions > 0)
            {
                PrismWarnings.DrawingsWithoutRevisions(drawingsWithoutRevisions);
                return false;
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

            if (myObjects.SeversafePresent) { myObjects.GetNonSeversafeParts().SelectParts(); }
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
            int fitEndPoint = dm.GetDrawingFolder(DrawingFolder.Default).Count + dm.GetDrawingFolder(DrawingFolder.FIT).Count;
            int notRequiredStartPoint = fitEndPoint;
            int notRequiredEndPoint = dm.GetDrawingFolder(DrawingFolder.NotRequired).Count;

            int pgcStartPoint = fitEndPoint + notRequiredEndPoint;
            int pgcEndPoint = dm.GetDrawingFolder(DrawingFolder.PGC).Count;

            int prtStartPoint = pgcStartPoint + pgcEndPoint;
            int prtEndPoint = dm.GetDrawingFolder(DrawingFolder.PRT).Count;

            int shaStartPoint = prtStartPoint + prtEndPoint;
            int shaEndPoint = dm.GetDrawingFolder(DrawingFolder.SHA).Count;

            int assStartPoint = 0;
            int assEndPoint = dm.GetDrawingFolder(DrawingFolder.ASS).Count;

            int assNotReqStartPoint = assEndPoint;
            int assNotReqEndPoint = dm.GetDrawingFolder(DrawingFolder.AssNotRequired).Count;

            int wldStartPoint = assNotReqStartPoint + assNotReqEndPoint;
            int wldEndPoint = dm.GetDrawingFolder(DrawingFolder.WLD).Count;

            return new List<int>() { fitStartPoint, fitEndPoint, notRequiredStartPoint, notRequiredEndPoint, pgcStartPoint, pgcEndPoint, prtStartPoint, prtEndPoint, shaStartPoint, shaEndPoint, assStartPoint, assEndPoint, assNotReqStartPoint, assNotReqEndPoint, wldStartPoint, wldEndPoint };
        }

        private static List<int> NewCountDrawings(DrawingManager dm)
        {
            int assStartPoint = 0;
            int assEndPoint = dm.GetDrawingFolder(DrawingFolder.ASS).Count;

            int fitStartPoint = assStartPoint + assEndPoint;
            int fitEndPoint = dm.GetDrawingFolder(DrawingFolder.FIT).Count;

            int notRequiredStartPoint = fitStartPoint + fitEndPoint;
            int notRequiredEndPoint = dm.GetDrawingFolder(DrawingFolder.NotRequired).Count + dm.GetDrawingFolder(DrawingFolder.AssNotRequired).Count;

            int pgcStartPoint = notRequiredStartPoint + notRequiredEndPoint;
            int pgcEndPoint = dm.GetDrawingFolder(DrawingFolder.PGC).Count;

            int prtStartPoint = pgcStartPoint + pgcEndPoint;
            int prtEndPoint = dm.GetDrawingFolder(DrawingFolder.PRT).Count;

            int shaStartPoint = prtStartPoint + prtEndPoint;
            int shaEndPoint = dm.GetDrawingFolder(DrawingFolder.SHA).Count;

            int wldStartPoint = shaStartPoint + shaEndPoint;
            int wldEndPoint = dm.GetDrawingFolder(DrawingFolder.WLD).Count;

            return new List<int>() { assStartPoint, assEndPoint, fitStartPoint, fitEndPoint, notRequiredStartPoint, notRequiredEndPoint, pgcStartPoint, pgcEndPoint, prtStartPoint, prtEndPoint, shaStartPoint, shaEndPoint, wldStartPoint, wldEndPoint };

        }
    }
}