using System.Collections.Generic;
using static Prism.Enums;
using Model = Tekla.Structures.Model.Model;

namespace Prism.ButtonOperations
{
    public static class CreatePackageButton
    {
        public static bool CreateFabPackage(this SelectedObjects myObjects, Model model, PrismProjectData projectData, string phaseNumber, string issueNumber, StageTypes stageType, string siteDate)
        {
            CpuCounter cpuCounter = new CpuCounter();
            ReportManager reportManager = new ReportManager(projectData, phaseNumber, issueNumber);

            if (!reportManager.Folders.CreateFabFolders()) return false;
            if (!reportManager.Folders.CreateBoltFolder()) return false;

            if (!Constants.IsSpecialPerson()) { myObjects.ExportBSWX(reportManager.Folders.DspPath, projectData, phaseNumber, issueNumber, stageType); }

            CpuSpeedCheck(cpuCounter);
            ReportManager.SelectDrawingsInDocManager(myObjects.SelectedModelParts);

            reportManager.CreateFabReports(myObjects.SelectedModelParts, myObjects.AllBolts);

            CpuSpeedCheck(cpuCounter);
            DrawingManager drawingManager = new DrawingManager(model, projectData, phaseNumber, issueNumber, myObjects);

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

            if (!myObjects.SelectedModelParts.ModifyAttributes((int)stageType, projectData)) { return false; }

            reportManager.Folders.RemoveUnusedFolders();

            reportManager.Folders.ZipFolder(reportManager.Folders.FabPath);

            PrismWarnings.FabPackComplete(projectData);

            EmailWriter.WriteFabEmail(projectData, myObjects, reportManager.FabReportPrefix, issueNumber, phaseNumber, siteDate, reportManager.Folders.FabPath);

            Logging.UpdateFrozenDrawingCount(projectData.ProjNumberAndName, drawingManager.FrozenDrawings.Count, drawingManager.UnFrozenDrawings.Count);
            Logging.LogProgress(projectData.ProjName, "Fab Package", 0, myObjects.AssembliesList.Count);
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

            return new List<int>() { fitStartPoint, fitEndPoint, notRequiredStartPoint, notRequiredEndPoint, pgcStartPoint, pgcEndPoint, prtStartPoint, prtEndPoint, shaStartPoint, shaEndPoint };
        }
    }
}