using System.Windows.Forms;
using Tekla.Structures.Model;
using static Prism.Enums;

namespace Prism.ButtonOperations
{
    public static class MaterialButton3
    {
        public static void MaterialButton3op(this SelectedObjects myObjects, PrismProjectData projectData, ProjectInfo projectInfo, 
            string phaseNumber, string issueNumber, string orderType, int stageNumber, stageTypes stageType)
        {
            ReportManager myReportManager = new ReportManager(projectData, phaseNumber, issueNumber);

            myObjects.AddPrelimMarks(projectInfo);
            myReportManager.Folders.CreateMatFolder();
            myReportManager.CreateMaterialReports(myObjects.SelectedModelParts, orderType);
            if (orderType != "Omit Material")
            {
                myObjects.ExportBSWX(myReportManager.Folders.MatPath, projectData, phaseNumber, issueNumber, stageType);
            }
            else
            {
                myObjects.MoveAndRenameOmittedMembers();
                stageNumber = 8;
            }

            myObjects.ModifyAttributes(stageNumber, projectData);
            DialogResult finishBox = PrismWarnings.MaterialOrderComplete(projectData);

            if (finishBox == DialogResult.OK)
            {
                EmailWriter.WriteMatEmail(projectData, myObjects, myReportManager.MatReportPrefix, issueNumber, phaseNumber,  orderType);
            }
        }
    }
}