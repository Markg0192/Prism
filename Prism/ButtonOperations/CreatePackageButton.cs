using System.IO.Compression;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tekla.Structures.Model;
using static Prism.Enums;

namespace Prism.ButtonOperations
{
    public static class CreatePackageButton
    {
        public static async Task<bool> CreateFabPackage(this SelectedObjects myObjects, Model model, PrismProjectData projectData, string phaseNumber, string issueNumber, StageTypes stageType, string siteDate)
        {
            ReportManager myReportManager = new ReportManager(projectData, phaseNumber, issueNumber);

            if (!myReportManager.Folders.CreateFabFolders()) return false;
            if (!myReportManager.Folders.CreateBoltFolder()) return false;

            myObjects.ExportBSWX(myReportManager.Folders.DspPath, projectData, phaseNumber, issueNumber, stageType);

            ReportManager.SelectDrawingsInDocManager();
            myReportManager.CreateFabReports(myObjects.SelectedModelParts, myObjects.AllBolts);
            DrawingManager myDrawingManager = new DrawingManager(model, projectData, phaseNumber, issueNumber, myObjects, myReportManager.Folders.DspPath);
            ReportManager.IssueDrawings();
            if (!myDrawingManager.DrawingsAreUpToDate) { PrismWarnings.DrawingsNotUpToDate(); return false; }

            myDrawingManager.PrintDPM(myReportManager.Folders.FabPath);
            myObjects.SelectedModelParts.ModifyAttributes((int)stageType, projectData);

            myReportManager.Folders.RemoveUnusedFolders();

            myReportManager.Folders.ZipFolder(myReportManager.Folders.FabPath);

            DialogResult finishBox = PrismWarnings.FabPackComplete(projectData);

            if (finishBox == DialogResult.OK)
            {
                EmailWriter.WriteFabEmail(projectData, myObjects, myReportManager.FabReportPrefix, issueNumber, phaseNumber, siteDate, myReportManager.Folders.FabPath);
            }
            Logging.LogProgress(projectData.ProjName, "Fab Package", 0, myObjects.AssembliesList.Count);
            return true;
        }


    }
}