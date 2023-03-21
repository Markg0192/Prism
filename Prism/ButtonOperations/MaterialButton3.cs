using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Tekla.Structures.Model;
using Tekla.Structures.Model.Operations;
using static Prism.Enums;

namespace Prism.ButtonOperations
{
    public static class MaterialButton3
    {
        public static bool MaterialButton3op(this SelectedObjects myObjects, PrismProjectData projectData, ProjectInfo projectInfo,
            string phaseNumber, string issueNumber, string orderType, int stageNumber, StageTypes stageType, Model model)
        {
            ReportManager myReportManager = new ReportManager(projectData, phaseNumber, issueNumber);
            // HDBolts.StampConnectionCodeOnMainMember(myObjects);
            bool fabsecsPresent = FabsecProcessing.AddCarcassToSelection(model, myObjects, out List<Part> originalFabsecs);

            foreach (Part myPart in myObjects.SelectedModelParts)
            {
                myPart.GetUnorderedParts();
            }

            if (orderType == "Order HD Bolts") //Order HD bolts not an option therefore this statement is never true(for now)
            {
                HDBolts.OrderHDBolts(myObjects, myReportManager);
                Logging.LogProgress(projectData.ProjName, "Material 3 - HD Bolts", 0, myObjects.AssembliesList.Count);                
            }

            if (orderType == "Order Heavy Fittings")
            {
                Factory location = PrismWarnings.FactoryLocation();
                if (location == Factory.Unknown)
                {
                    PrismWarnings.IgnoreFittingCheck();
                }
                foreach (Assembly ass in myObjects.AssembliesList)
                {
                    ArrayList mySecondaries = ass.GetSecondaries();
                    foreach (Part mySecondaryPart in mySecondaries)
                    {
                        CheckFittings.GetIncorrectFittings(mySecondaryPart, location);
                    }
                }
                CheckFittings.AllIncorrectPlate.SelectParts();
                fabsecsPresent = false;
                // myReportManager.Folders.CreateMatFolder(false);
                //  myReportManager.CreateMaterialReports(myObjects, orderType, stageType);
            }

            if (orderType == "Omit Material")
            {
                if (ModelChecker.NotOrderedParts.Count != 0)
                {
                    PrismWarnings.HasNotBeenOrderedOMIT();
                    return false;
                }
            }

            if (orderType != "Omit Material")
            {
                if (ModelChecker.OrderedParts.Count != 0)
                {
                    PrismWarnings.HasAlreadyBeenOrdered();
                    return false;
                }
            }

            ModelModifiers.VariationCheck(phaseNumber, myObjects, projectData);
            myObjects.AddPrelimMarks(projectInfo);
            myReportManager.Folders.CreateMatFolder(fabsecsPresent);

            if (fabsecsPresent)
            {
                DialogResult fabsecWarning = PrismWarnings.FabsecsPresent();
                if (fabsecWarning == DialogResult.Yes)
                {
                    ReportManager.SelectDrawingsInDocManager();
                    Operation.CreateReportFromSelected(myReportManager.DrawingDpmReportRpt, Path.Combine(myReportManager.Folders.FabsecCarcassPath, myReportManager.DrawingDpmReportXsr), "", "", "");
                    DrawingManager dm = new DrawingManager(model, projectData, phaseNumber, issueNumber, myObjects, myReportManager.Folders.FabsecCarcassPath);
                    dm.PrintDPM(myReportManager.Folders.FabsecCarcassPath);
                    originalFabsecs.ModifyAttributes(stageNumber, projectData);
                    ModelModifiers.RemoveIDDessin(myReportManager.Folders.FabsecCarcassPath);
                    //MessageBox.Show("Sorry! I cannot get your carcass drawings, please create these manually.");
                }
                else { return false; }
            }
            myReportManager.CreateMaterialReports(myObjects, orderType, stageType);
            if (orderType == "Omit Material")
            {
                stageNumber = 8;
                myObjects.MoveAndRenameOmittedMembers();
            }

            myObjects.SelectedModelParts.ModifyAttributes(stageNumber, projectData);
            DialogResult finishBox = PrismWarnings.MaterialOrderComplete(projectData);
            if (finishBox == DialogResult.OK)
            {
                EmailWriter.WriteMatEmail(projectData, myObjects, myReportManager.MatReportPrefix, issueNumber, phaseNumber, orderType);
            }

            Logging.LogProgress(projectData.ProjName, "Material 3", 0, myObjects.AssembliesList.Count);
            return true;
        }
    }
}