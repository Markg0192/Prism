using System.Windows.Forms;
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
            DrawingManager drawingManager = new DrawingManager(model, projectData, phaseNumber, issueNumber, myObjects, reportManager.Folders.DspPath);

            CpuSpeedCheck(cpuCounter);
            PrismMacroBuilder.IssueDrawings();

            if (!drawingManager.DrawingsAreUpToDate) { PrismWarnings.DrawingsNotUpToDate(); return false; }

            if (Constants.IsSpecialPerson()) { drawingManager.PrintDrawingsToVault(myObjects, reportManager, projectData.ProjNumber); }

            else { drawingManager.PrintDrawingToModelFolder(reportManager.Folders.FabPath); }

            if(!myObjects.SelectedModelParts.ModifyAttributes((int)stageType, projectData)) { return false; }
  
            reportManager.Folders.RemoveUnusedFolders();

            reportManager.Folders.ZipFolder(reportManager.Folders.FabPath);

            DialogResult finishBox = PrismWarnings.FabPackComplete(projectData);

            if (finishBox == DialogResult.OK)
            {
                EmailWriter.WriteFabEmail(projectData, myObjects, reportManager.FabReportPrefix, issueNumber, phaseNumber, siteDate, reportManager.Folders.FabPath);
            }
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
    }
}