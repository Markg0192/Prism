using System.Collections.Generic;
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
            foreach (Part myPart in myObjects.SelectedModelParts)
            {                
                if (orderType == "Omit Material" && !myPart.HasBeenOrdered(false))
                {
                    PrismWarnings.HasNotBeenOrderedOMIT();
                    return;
                } 

                if (orderType != "Omit Material" && !myPart.HasBeenOrdered(true))
                {
                    continue;
                } 

                if (orderType != "Omit Material" && myPart.HasBeenOrdered(false))
                {
                    PrismWarnings.HasAlreadyBeenOrdered();
                    return;
                }
            }

            bool fabsecsPresent = FabsecProcessing.AddCarcassToSelection(model, myObjects, out List<Part> originalFabsecs);
            if (fabsecsPresent)
            {
                DialogResult fabsecWarning = PrismWarnings.FabsecsPresent();
                if (fabsecWarning == DialogResult.Yes)
                {
                    originalFabsecs.ModifyAttributes(stageNumber, projectData);
                    MessageBox.Show("Sorry! I cannot get your carcass drawings, please create these manually.");
                }
                else { return; }
            }

            ReportManager myReportManager = new ReportManager(projectData, phaseNumber, issueNumber);

            myReportManager.Folders.CreateMatFolder(fabsecsPresent);

            myReportManager.CreateMaterialReports(myObjects, orderType, model, stageType);
            if (orderType == "Omit Material")
            {              
                stageNumber = 8;
                myObjects.MoveAndRenameOmittedMembers();
            }

            myObjects.AddPrelimMarks(projectInfo);            
            myObjects.SelectedModelParts.ModifyAttributes(stageNumber, projectData);
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