//using Microsoft.Office.Interop.Excel;
//using Org.BouncyCastle.Utilities;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Tekla.Structures.Drawing;
using Tekla.Structures.Model;
using static Prism.Enums;
using Model = Tekla.Structures.Model.Model;
using ModelObject = Tekla.Structures.Model.ModelObject;
using Part = Tekla.Structures.Model.Part;

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
            if (orderRequired) return boltSeversafeAndHdBoltsResult;

            int typeOfOrder = 0;

            if (orderType.Contains("Fabsec Carcass"))
            {
                typeOfOrder = PrismWarnings.FabsecCarcassAction();
                //If there are fabsecs present the we need to add the carcass to the selection instead of those in the model space
                if (!FabsecOrderWorker(myObjects, orderType, myReportManager, model, projectData, phaseNumber, issueNumber, stageNumber, siteDate, typeOfOrder)) return false;
            }

            else
            {
                foreach (Part p in myObjects.SelectedModelParts)
                {
                    p.GetUnorderedParts();
                }

                //Check if everything in the selection needs to be ordered/omitted
                if (!ShouldPartsBeOrdered(orderType)) { return false; }
                if (!myReportManager.Folders.CreateMatFolder(false)) return false;

                //  if (!Order.Fabsecs(fabsecsPresent, myReportManager, model, projectData, phaseNumber, issueNumber, myObjects, stageNumber, originalFabsecs, fabsecCarcasses)) { return false; }

                ModelModifiers.VariationCheck(phaseNumber, myObjects, projectData);
                myObjects.AddPrelimMarks(projectData);

                myReportManager.CreateMaterialReports(myObjects, orderType, stageType);

                myObjects.OmittedParts = MoveOmitMaterial(orderType, myObjects, myObjects.FabsecParts, model);
                
                if (!FinishOrder(myObjects, stageNumber, projectData, myReportManager.MatReportPrefix, issueNumber, phaseNumber, orderType, myReportManager, siteDate, false, myObjects.FabsecParts.Count > 0)) { return false; }
            }

            return true;
        }

        private static bool FabsecOrderWorker(SelectedObjects myObjects, string orderType, ReportManager myReportManager, Model model,
            PrismProjectData projectData, string phaseNumber, string issueNumber, int stageNumber, string siteDate, int typeOfOrder)
        {
            bool fabsecsPresent = myObjects.FabsecParts.Count > 0;
            if (orderType.Contains("Fabsec Carcass") && (!orderType.Contains("Add") || !orderType.Contains("Omit")))
            {
                if (!fabsecsPresent)
                {
                    PrismWarnings.NoFabsecsSelected();
                    return false;
                }

                if (!CheckFabsecOrderStatusAgainstRequiredActions(myObjects.FabsecParts, typeOfOrder)) return false;

                bool fabsecCarcassOrdering = Order.FabsecCarcasses(myReportManager, model, projectData, phaseNumber, issueNumber, myObjects, stageNumber, orderType, siteDate, typeOfOrder);
                return fabsecCarcassOrdering;
            }
            if (orderType.Contains("Fabsec Carcass") && (orderType.Contains("Add") || orderType.Contains("Omit")))
            {
                PrismWarnings.CannotProcessThisTypeOfOrder();
                return false;
            }
            return true;
        }

        private static bool CheckFabsecOrderStatusAgainstRequiredActions(List<ModelObject> partsList, int typeOfOrder)
        {
            foreach (ModelObject myModelObject in partsList)
            {
                Part myPart = myModelObject as Part;
                myPart.GetUnorderedParts();

                string prelimMark = "";
                myPart.GetUserProperty(ModelUDA.CurrentStageName(3), ref prelimMark); //Check prism uda material order complete for data    
                string carcassInfo = "";
                string carcassOrderInfo = "";
                myPart.GetUserProperty(ModelUDA.FabsecCarcassInfo(), ref carcassInfo); //Check prism uda material order complete for data    
                myPart.GetUserProperty(ModelUDA.FabsecCarcassOrdered(), ref carcassOrderInfo); //Check prism uda material order complete for data    
                bool isDrawingCreation = typeOfOrder == 1;

                if (isDrawingCreation)
                {
                    if (prelimMark.Length == 0)
                    {
                        PrismWarnings.HasNotBeenOrdered();
                        return false;
                        //then it is not ordered, this should be done before creating carcass
                        //return false;
                    }
                    if (carcassInfo.Length != 0)
                    {
                        PrismWarnings.FabsecAlreadyHasCarcass();
                        return false;
                        //then carcass has already been made, this is bad
                    }
                    if (carcassOrderInfo.Length != 0)
                    {
                        PrismWarnings.HasAlreadyBeenOrdered();
                        return false;
                        //then the carcass has already been ordered, this is bad
                    }
                }
                else
                {
                    if (prelimMark.Length == 0)
                    {
                        PrismWarnings.HasNotBeenOrdered();
                        return false;
                        //then material has not been ordered bad
                    }
                    if (carcassInfo.Length == 0)
                    {
                        PrismWarnings.FabsecSelectedDoesNotHaveCarcass();
                        return false;
                        //then carcass has not been created
                    }
                    if (carcassOrderInfo.Length != 0)
                    {
                        PrismWarnings.FabsecCarcassAlreadyOrdered();
                        return false;
                        //then the carcass has not been ordered.
                    }
                }
            }
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
            else if (ModelChecker.OrderedParts.Count != 0)
            {
                PrismWarnings.HasAlreadyBeenOrdered();
                return false;
            }

            return true;
        }

        private static List<Part> MoveOmitMaterial(string orderType, SelectedObjects myObjects, List<ModelObject> originalFabsecs, Model model)
        {
            List<Part> movedParts = new List<Part>();
            if (orderType == "Omit Material")
            {
                bool keepPartInModel = PrismWarnings.KeepPartInModel();
                movedParts.AddRange(ModelModifiers.MoveAndRenameOmittedMembers2(myObjects.NonFabsecParts, -100000, keepPartInModel, model, false));
                if (myObjects.FabsecParts.Count > 0)
                {
                    movedParts.AddRange(ModelModifiers.MoveAndRenameOmittedMembers2(myObjects.FabsecParts, -100000, keepPartInModel, model, true));
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

            if (fabsecsPresent && !orderType.Contains("Omit")) FabsecProcessing.RemoveGreenFromFabsecs(myObjects.FabsecParts);
            PrismWarnings.MaterialOrderComplete(projectData);

            reportManager.Folders.ZipFolder(reportManager.Folders.MatPath);
            EmailWriter.WriteMatEmail(projectData, myObjects, matReportPrefix, issueNumber, phaseNumber, orderType, reportManager.Folders.MatPath, fabsecsPresent, siteDate);

            Logging.LogProgress(projectData.ProjNumberAndName, "Material 3", 0, myObjects.AssembliesList.Count);

            return true;
        }
    }
}