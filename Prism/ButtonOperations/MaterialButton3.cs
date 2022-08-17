using System.Windows.Forms;
using Tekla.Structures.Model;
using static Prism.Enums;

namespace Prism.ButtonOperations
{
    public static class MaterialButton3
    {
        public static void MaterialButton3op(this SelectedObjects myObjects, PrismProjectData projectData, ProjectInfo projectInfo,
            string phaseNumber, string issueNumber, string orderType, int stageNumber, stageTypes stageType, Model model)
        {
            bool fabsecsPresent = FabsecProcessing.AddCarcassToSelection(model, myObjects);
            if (fabsecsPresent)
            {
                DialogResult fabsecWarning = PrismWarnings.FabsecsPresent();
                if (fabsecWarning == DialogResult.Yes)
                {
                    MessageBox.Show("Sorry! I cannot get your carcass drawings, please create these manually.");
                }
                else { return; }
            }

            ReportManager myReportManager = new ReportManager(projectData, phaseNumber, issueNumber);

            myReportManager.Folders.CreateMatFolder(fabsecsPresent);
            myObjects.AddPrelimMarks(projectInfo);
            myReportManager.CreateMaterialReports(myObjects, orderType, model, stageType);
            if (orderType == "Omit Material")
            {
                myObjects.MoveAndRenameOmittedMembers();
                stageNumber = 8;
            }
            myObjects.ModifyAttributes(stageNumber, projectData);
            DialogResult finishBox = PrismWarnings.MaterialOrderComplete(projectData);
            if (finishBox == DialogResult.OK)
            {
                EmailWriter.WriteMatEmail(projectData, myObjects, myReportManager.MatReportPrefix, issueNumber, phaseNumber, orderType);
            }
            if (fabsecsPresent)
            {
                MessageBox.Show("FABSECS! Please ensure to add fabsec carcass drawings to your package.");
            }
        }
    }
}