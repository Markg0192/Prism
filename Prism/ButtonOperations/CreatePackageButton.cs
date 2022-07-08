using System.Windows.Forms;
using Tekla.Structures.Model;

namespace Prism.ButtonOperations
{
    public static class CreatePackageButton
    {
        private static string _unavailableLocation = "Sorry, this package location has not been added yet, please try another location.";

        public static void CreateBoltList(EmailWriter myEmailWriter, Model myModel, string phaseNumber, string issueNumber, PrismProjectData projectData)
        {
            ReportManager myReportManager = new ReportManager(myModel, phaseNumber, issueNumber);
            myReportManager.CreateBoltList();
            myEmailWriter.WriteBoltOrderEmail(myReportManager.FabReportPrefix, issueNumber, phaseNumber, projectData.ProjNumber, projectData.ProjName, projectData.Full, "Right Away!!");
}

        public static string CreateFabPackage(this SelectedObjects myObjects, EmailWriter myEmailWriter, PrismProjectData projectData, Model myModel, string packageLocation, string phaseNumber, string issueNumber, string stageType, int stageNumber)
        {

            ReportManager myReportManager = new ReportManager(myModel, phaseNumber, issueNumber);
            //MyDrawingManager = new DrawingManager(Model, phaseNumber.Text, issueNumber.Text, ModelEnum); Temporarily not in use
            SevFolders myFolderManager = new SevFolders(myModel, phaseNumber, issueNumber);
            BswxExporter myBswxExporter = new BswxExporter();

            bool packageSNI = packageLocation == "SNI";
            bool packageSUK = packageLocation == "SUK";
            bool packageSDB = packageLocation == "SDB";
            bool packageHarryPeers = packageLocation == "Harry Peers";
            bool packageDAMStructures = packageLocation == "DAM Structures";

            if (packageSNI)
            {
                myBswxExporter.ExportBSWX(myObjects, myFolderManager.DspPath, projectData, phaseNumber, issueNumber, stageType);
                myFolderManager.CreateFabFolders();
                myReportManager.CreateFabReports(myObjects.SelectedModelParts, myObjects.SelectedModelBolts, packageLocation);
                // MyDrawingManager.PrintDrawings(StatusLabel); Temporarily not in use
                myObjects.ModifyAttributes(stageNumber, projectData);
                //MyFolderManager.RemoveUnusedFolders(); Temporarily not in use
                myObjects.LockSelected();
                DialogResult finishBox = MessageBox.Show($"Thanks {projectData.First}, your fab package is now complete, please attach your fab package, located in your model folder, " +
                    $"to the following email and send to the relevant team. PLEASE NOTE: This version of Prism does NOT print drawings, you will have to do this bit yourself.", "Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);

                if (finishBox == DialogResult.OK)
                {
                    myEmailWriter.WriteFabEmail(myReportManager.FabReportPrefix, myObjects.AssembliesList.Count, myObjects.SelectedModelParts.Count,
                        issueNumber, phaseNumber, projectData.ProjNumber, projectData.ProjName, projectData.Full, myObjects.totalWeight);
                }
                return "Complete";
            }

            if (packageSUK)
            {
                MessageBox.Show(_unavailableLocation);
                return "Cancelled";
            }

            if (packageSDB)
            {
                MessageBox.Show(_unavailableLocation);
                return "Cancelled";
            }

            if (packageHarryPeers)
            {
                MessageBox.Show(_unavailableLocation);
                return "Cancelled";
            }

            if (packageDAMStructures)
            {
                MessageBox.Show(_unavailableLocation);
                return "Cancelled";
            }
            return "Cancelled";
        }
    }
}
