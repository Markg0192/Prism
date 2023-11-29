using System.Collections.Generic;
using Tekla.Structures.Model;
using static Prism.Enums;

namespace Prism.ButtonOperations
{
    public static class MaterialButton3
    {
        public static bool MaterialButton3op(this SelectedObjects myObjects, PrismProjectData projectData,
            string phaseNumber, string issueNumber, string orderType, int stageNumber, StageTypes stageType, Model model, string siteDate)
        {
            ReportManager myReportManager = new ReportManager(projectData, phaseNumber, issueNumber);
            // HDBolts.StampConnectionCodeOnMainMember(myObjects);

            //First check if the order is for Bolts, Seversafe or HD Bolts.
            bool boltSeversafeAndHdBoltsResult = Order.BoltsSeversafeAndHdBolts(orderType, myReportManager, siteDate, model, myObjects, projectData, out bool orderRequired);
            if(orderRequired) return boltSeversafeAndHdBoltsResult;

            //If there are fabsecs present the we need to add the carcass to the selection instead of those in the model space
            bool fabsecsPresent = FabsecProcessing.AddCarcassToSelection(model, myObjects, out List<Part> originalFabsecs, out List<Part> fabsecCarcasses);
            foreach (Part myPart in myObjects.SelectedModelParts)
            {
                myPart.GetUnorderedParts();
            }

            //Check if everything in the selection needs to be ordered/omitted
            if (!ShouldPartsBeOrdered(orderType)) { return false; }

            if (!myReportManager.Folders.CreateMatFolder(fabsecsPresent)) return false;

            if (!Order.Fabsecs(fabsecsPresent, myReportManager, model, projectData, phaseNumber, issueNumber, myObjects, stageNumber, originalFabsecs, fabsecCarcasses)) { return false; }

            ModelModifiers.VariationCheck(phaseNumber, myObjects, projectData);
            myObjects.AddPrelimMarks(projectData);

            myReportManager.CreateMaterialReports(myObjects, orderType, stageType);

            myObjects.OmittedParts = MoveOmitMaterial(orderType, myObjects, originalFabsecs);

            if (!FinishOrder(myObjects, stageNumber, projectData, myReportManager.MatReportPrefix, issueNumber, phaseNumber, orderType, myReportManager, siteDate, false, fabsecsPresent)) { return false; }

            return true;
        }

        private static bool ShouldPartsBeOrdered(string orderType)
        {
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
            return true;
        }

        private static List<Part> MoveOmitMaterial(string orderType, SelectedObjects myObjects, List<Part> originalFabsecs)
        {
            List<Part> movedParts = new List<Part>();
            if (orderType == "Omit Material")
            {
                bool keepPartInModel = PrismWarnings.KeepPartInModel();
                movedParts.AddRange(ModelModifiers.MoveAndRenameOmittedMembers(myObjects.SelectedModelParts, -100000, keepPartInModel));
                if (originalFabsecs != null)
                {
                    movedParts.AddRange(ModelModifiers.MoveAndRenameOmittedMembers(originalFabsecs, -100000, keepPartInModel));
                }
            }
            return movedParts;
        }

        public static bool FinishOrder(SelectedObjects myObjects, int stageNumber, PrismProjectData projectData, string matReportPrefix,
            string issueNumber, string phaseNumber, string orderType, ReportManager reportManager, string siteDate, bool isSpecialFittingOrder = false, bool fabsecsPresent = false)
        {
            if (orderType == "Omit Material")
            {
                if (!myObjects.OmittedParts.ModifyAttributes(8, projectData, isSpecialFittingOrder)) { return false; }
            }
            else
            {
                if (!myObjects.SelectedModelParts.ModifyAttributes(stageNumber, projectData, isSpecialFittingOrder)) { return false; }
            }

            PrismWarnings.MaterialOrderComplete(projectData);

            reportManager.Folders.ZipFolder(reportManager.Folders.MatPath);
            EmailWriter.WriteMatEmail(projectData, myObjects, matReportPrefix, issueNumber, phaseNumber, orderType, reportManager.Folders.MatPath, fabsecsPresent, siteDate);

            Logging.LogProgress(projectData.ProjNumberAndName, "Material 3", 0, myObjects.AssembliesList.Count);
            return true;
        }
    }
}