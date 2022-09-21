using System.Windows.Forms;
using static Prism.Enums;

namespace Prism.ButtonOperations
{
    public static class CreatePackageButton
    {
        public static string CreateFabPackage(this SelectedObjects myObjects, PrismProjectData projectData, string packageLocation, string phaseNumber, string issueNumber, StageTypes stageType, string siteDate)
        {
            ReportManager myReportManager = new ReportManager(projectData, phaseNumber, issueNumber);
            //MyDrawingManager = new DrawingManager(Model, phaseNumber.Text, issueNumber.Text, selectedObjects); Temporarily not in use

            switch (packageLocation)
            {
                case "SNI":
                    myObjects.ExportBSWX(myReportManager.Folders.DspPath, projectData, phaseNumber, issueNumber, stageType);
                    myReportManager.Folders.CreateFabFolders();
                    myReportManager.CreateFabReports(myObjects.SelectedModelParts, myObjects.AllBolts, packageLocation);
                    // MyDrawingManager.PrintDrawings(StatusLabel); Temporarily not in use
                    myObjects.SelectedModelParts.ModifyAttributes((int)stageType, projectData);
                    //MyFolderManager.RemoveUnusedFolders(); Temporarily not in use
                    myObjects.LockSelected();
                    DialogResult finishBox = PrismWarnings.FabPackComplete(projectData);

                    if (finishBox == DialogResult.OK)
                    {
                        EmailWriter.WriteFabEmail(projectData, myObjects, myReportManager.FabReportPrefix, issueNumber, phaseNumber, siteDate);
                    }
                    return "Complete";

                //None of the below cases are reachable now due to button restrictions
                case "SUK":
                    return "Cancelled";

                case "SDB":
                    return "Cancelled";

                case "Harry Peers":
                    return "Cancelled";

                case "DAM Structures":
                    return "Cancelled";
            }
            return "Cancelled";
        }
    }
}