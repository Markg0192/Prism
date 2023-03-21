using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tekla.Structures;
using Tekla.Structures.Drawing;
using Tekla.Structures.Model;
using static Prism.Enums;

namespace Prism.ButtonOperations
{
    public static class CreatePackageButton
    {
        public static async void CreateFabPackage(this SelectedObjects myObjects, Model model, PrismProjectData projectData, string phaseNumber, string issueNumber, StageTypes stageType, string siteDate)
        {
            ReportManager myReportManager = new ReportManager(projectData, phaseNumber, issueNumber);
            if (!myReportManager.Folders.CreateFabFolders()) return;
            myObjects.ExportBSWX(myReportManager.Folders.DspPath, projectData, phaseNumber, issueNumber, stageType);

            ReportManager.SelectDrawingsInDocManager();
            myReportManager.CreateFabReports(myObjects.SelectedModelParts, myObjects.AllBolts);
            DrawingManager myDrawingManager = new DrawingManager(model, projectData, phaseNumber, issueNumber, myObjects, myReportManager.Folders.DspPath);

            if (!myDrawingManager.DrawingsAreUpToDate) { PrismWarnings.DrawingsNotUpToDate(); return; }

            myDrawingManager.PrintDPM(myReportManager.Folders.FabPath);
            myObjects.SelectedModelParts.ModifyAttributes((int)stageType, projectData);

            myReportManager.Folders.RemoveUnusedFolders();
            DialogResult finishBox = PrismWarnings.FabPackComplete(projectData);

            if (finishBox == DialogResult.OK)
            {
                EmailWriter.WriteFabEmail(projectData, myObjects, myReportManager.FabReportPrefix, issueNumber, phaseNumber, siteDate);
            }
        }
    }
}