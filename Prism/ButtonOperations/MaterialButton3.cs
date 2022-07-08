using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tekla.Structures.Model;

namespace Prism.ButtonOperations
{
    public static class MaterialButton3
    {
        public static void MaterialButton3op(this SelectedObjects myObjects, EmailWriter myEmailWriter, Model myModel, PrismProjectData projectData, string phaseNumber, string issueNumber, string orderType, int stageNumber, string stageType)
        {
            SevFolders myFolderManager = new SevFolders(myModel, phaseNumber, issueNumber);
            ReportManager myReportManager = new ReportManager(myModel, phaseNumber, issueNumber);
            BswxExporter myBswxExporter = new BswxExporter();

            PrelimMarker.AddPrelimMarks(myObjects, myModel);
            myFolderManager.CreateMatFolder();
            myReportManager.CreateMaterialReports(myObjects.SelectedModelParts, orderType);
            if (orderType != "Omit Material")
            {
                myBswxExporter.ExportBSWX(myObjects, myFolderManager.MatPath, projectData, phaseNumber, issueNumber, stageType);
            }
            else
            {
                myObjects.MoveAndRenameOmittedMembers(myModel) ;
                stageNumber = 8;
            }

            myObjects.ModifyAttributes(stageNumber, projectData);
            DialogResult finishBox = MessageBox.Show($"Thanks {projectData.First}, your material order is now complete, please forward the following email to the relevant purchasing team", "Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);

            if (finishBox == DialogResult.OK)
            {
                myEmailWriter.WriteMatEmail(myReportManager.MatReportPrefix, myObjects.SelectedModelParts.Count, issueNumber, phaseNumber, projectData.ProjNumber, projectData.ProjName, projectData.Full, orderType, myObjects.totalWeight);
            }
        }
    }
}