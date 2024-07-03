using Prism.CustomDialogs;
using System.Collections.Generic;
using Tekla.Structures.Model;
using Tekla.Structures.Model.UI;
using static Prism.Enums;

namespace Prism
{
    public static class PrismWarnings
    {
        public static string Warning = "";

        public static Factory FactoryLocation()
        {
            var form = new FactoryLocation();
            form.ShowDialog();
            return form.myLocation;
        }

        public static IgnoreType NewIgnoreWarning()
        {
            var form = new IgnoreWarning();
            form.ShowDialog();
            return form.Ignore;
        }

        public static int ExecutionClassWarning()
        {
            var form = new ExecutionClass();
            form.ShowDialog();
            return form.executionClass;
        }

        public static int SpecialFittingOrder()
        {
            var form = new SpecialFittingOrders();
            form.ShowDialog();
            return form.orderAction;
        }

        public static int DivsionFrom()
        {
            var form = new Division();
            form.ShowDialog();
            return form.DivisionOut;
        }

        public static int BoltOrderType()
        {
            var form = new BoltOrderType();
            form.ShowDialog();
            return form.OrderBoltsFrom;
        }

        public static int FabsecCarcassAction()
        {
            var form = new FabsecCarcassOrder();
            form.ShowDialog();
            return form.OrderAction;
        }

        public static void LockedPartsSelected(List<PrismPart> failedParts)
        {
            const string notUpToDateMessage = "You have selected some parts that are locked, please de-select or unlock these to continue.";
            const string notUpToDateTitle = "Locked parts selected";
            CreateOkWithReportForm(notUpToDateTitle, notUpToDateMessage, failedParts, "Part locked");
        }

        public static void FirstTimeInTheModel()
        {
            string notUpToDateMessage = $"IMPORTANT: This is the first time Prism has detected a model with this name.\r\rIf this is correct please continue.\r\r " +
                $"If this model has been using Prism already then it's likely the Project Number or Name has recently changed, this will result in Prism data loss.\r" +
                $"To retrieve the old data information please contact the developers (Contacts can be found in the help/about tab).";
            const string notUpToDateTitle = "IMPORTANT! New model detected.";
            CreateOKForm(notUpToDateMessage, notUpToDateTitle);
        }

        public static void BigTimeUsage(int timesUsed)
        {
            string notUpToDateMessage = $"Congratulations, your latest run of Prism was the {timesUsed}th time it's been used. I hope you are happy with yourself, goodbye.";
            const string notUpToDateTitle = "Prism is great";
            CreateOKForm(notUpToDateMessage, notUpToDateTitle);
        }

        public static void NcDataCreationFailed()
        {
            string notUpToDateMessage = $"NC Data creation failed:\r\rNo NC has been created, if this was not the intention please create this manually and add to your package.";
            const string notUpToDateTitle = "No NC found";
            CreateOKForm(notUpToDateMessage, notUpToDateTitle);
        }

        public static void CantFindAdvancedSettings()
        {
            string notUpToDateMessage = $"Prism cannot find the advanced settings file for this project, contact help to fix this.";
            const string notUpToDateTitle = "No settings file found";
            CreateOKForm(notUpToDateMessage, notUpToDateTitle);
        }

        public static void IgnoreFittingCheck()
        {
            const string notUpToDateMessage = "Without selecting a location Prism cannot filter abnormal fittings.";
            const string notUpToDateTitle = "Are you sure?";
            CreateOKForm(notUpToDateMessage, notUpToDateTitle);
        }

        public static void PrelimStartReset(string resetNo)
        {
            string notUpToDateMessage = $"Prelim number start point manually set to {resetNo}";
            string notUpToDateTitle = "Are you sure?";
            CreateOKForm(notUpToDateMessage, notUpToDateTitle);
        }

        public static void AbnormalFittings(List<PrismPart> failedParts)
        {
            string notUpToDateMessage = $"You have selected some abnormal fittings that should either be bought out items or changed to something standard.\r\r{Warning}";
            const string notUpToDateTitle = "Abnormal Fittings";
            CreateOkWithReportForm(notUpToDateMessage, notUpToDateTitle, failedParts, "");
        }

        public static void PrelimNumberStartReset(int old, int newP)
        {
            string notUpToDateMessage = $"Prelim numbering start point has been modified from {old} to {newP}";
            string notUpToDateTitle = "Prelim start modified";
            CreateOKForm(notUpToDateMessage, notUpToDateTitle);
        }

        public static bool TagAbnormalFittings()
        {
            string notUpToDateMessage = $"Would you like to tag these abnormal fittings for ordering later?.\r\r {Warning}";
            const string notUpToDateTitle = "Abnormal Fittings Tag";
            return CreateYesNoForm(notUpToDateMessage, notUpToDateTitle);
        }

        public static void IntumescentLoadingMissing(List<PrismPart> failedParts)
        {
            const string notUpToDateMessage = "You have selected intumescent members that have no loading.";
            const string notUpToDateTitle = "Intumescent loadings";
            CreateOkWithReportForm(notUpToDateMessage, notUpToDateTitle, failedParts, "DFT/WFT loading missing");
        }

        public static bool IgnoreIntumescentLoading()
        {
            const string notUpToDateMessage = "Would you like to ignore this error and continue?";
            const string notUpToDateTitle = "Missing Intumescent Loadings";
            return CreateYesNoForm(notUpToDateMessage, notUpToDateTitle);
        }

        public static bool ResetPrelimMarking()
        {
            const string notUpToDateMessage = "WARNING! Selecting this button means your prelim marking will now start at the number given, Prism cannot undo this manual action. Are you sure you want to proceed?";
            const string notUpToDateTitle = "Be careful";
            return CreateYesNoForm(notUpToDateMessage, notUpToDateTitle);
        }
        
        public static bool RunChangeManagement()
        {
            const string notUpToDateMessage = "Do you want to use change management on this package?";
            const string notUpToDateTitle = "Access change management";
            return CreateYesNoForm(notUpToDateMessage, notUpToDateTitle);
        }

        public static bool KeepPartInModel()
        {
            const string notUpToDateMessage = "Would you like to keep the selected steel where it is?\r\rIf you say yes Prism will create a copy of each omitted member outside the model space and reset the attributes on the selected. \r\rIf you say no it will move the selected out of the model space and tag it as omitted.";
            const string notUpToDateTitle = "Keep selected in model";
            return CreateYesNoForm(notUpToDateMessage, notUpToDateTitle);
        }

        public static bool IsVariation()
        {
            const string notUpToDateMessage = "Is this a variation?";
            const string notUpToDateTitle = "Variation?";
            return CreateYesNoForm(notUpToDateMessage, notUpToDateTitle);
        }

        public static bool RunFabsecDrawings()
        {
            const string notUpToDateMessage = "There are FABSEC members present in your selection, would you like to add the carcass drawings to the order?\r\rNOTE: if you do not do this now you will have to do this yourself at a later date.";
            const string notUpToDateTitle = "FABSECS!";
            return CreateYesNoForm(notUpToDateMessage, notUpToDateTitle);
        }

        public static bool RunSpecialFittingDrawings()
        {
            const string notUpToDateMessage = "Have you created drawings for these special fittings? If so would you like to include these in the order?.";
            const string notUpToDateTitle = "Special fittings.";
            return CreateYesNoForm(notUpToDateMessage, notUpToDateTitle);
        }

        public static void NumberingIsNotUpToDate()
        {
            const string notUpToDateMessage = "Your member numbering is not up to date, please update and try again";
            const string notUpToDateTitle = "Numbers not up to date";
            CreateOKForm(notUpToDateMessage, notUpToDateTitle);
        }

        public static void NameAndClassDontMatch(List<PrismPart> failedParts)
        {
            const string notUpToDateMessage = "You have selected something thats name and class do not align with GDOM convention, please correct this to continue.";
            const string notUpToDateTitle = "Part name and class misalignment";
            CreateOkWithReportForm(notUpToDateMessage, notUpToDateTitle, failedParts, "Name and Class do not match");
        }

        public static void MemberOrientationIsWrong(List<PrismPart> failedParts)
        {
            const string notUpToDateMessage = "You have selected a member that has been input in the wrong orientation, please correct this to continue.";
            const string notUpToDateTitle = "Incorrect member orientation";
            CreateOkWithReportForm(notUpToDateMessage, notUpToDateTitle, failedParts, "Orientation is wrong");
        }

        public static void PreviousStepIncomplete(List<PrismPart> failedParts)
        {
            const string notUpToDateMessage = "You have not completed all the required steps before this action, please correct this to continue.";
            const string notUpToDateTitle = "Incomplete stages";
            CreateOkWithReportForm(notUpToDateMessage, notUpToDateTitle, failedParts, "Previous step incomplete");
        }

        public static void HasNotBeenOrderedOMIT()
        {
            const string notUpToDateMessage = "You are trying to OMIT material that does not appear to have ever been ordered, please choose a different course of action.";
            const string notUpToDateTitle = "Can't Omit.";
            CreateOKForm(notUpToDateMessage, notUpToDateTitle);
        }

        public static bool FolderAlreadyExists(string existingFolder)
        {
            string notUpToDateMessage = $"{existingFolder}, do you want to delete and continue?";
            const string notUpToDateTitle = "Folder exists.";
            return CreateYesNoForm(notUpToDateMessage, notUpToDateTitle);
        }

        public static void ZipFolderAlreadyExists()
        {
            const string notUpToDateMessage = "The zip folder you are trying to create already exists in the model folder, it must be removed from the model folder before continuing";
            const string notUpToDateTitle = "Zip folder exists.";
            CreateOKForm(notUpToDateMessage, notUpToDateTitle);
        }

        public static void DirectoryCannotBeReached(string directory)
        {
            string notUpToDateMessage = $"The directory {directory} cannot be reached, package moving will be cancelled";
            string notUpToDateTitle = "Directory not found.";
            CreateOKForm(notUpToDateMessage, notUpToDateTitle);
        }

        public static void DirectoryCannotBeCopied(string message)
        {
            string notUpToDateMessage = $"The file cannot be moved, package moving will be cancelled, {message}";
            string notUpToDateTitle = "Copying failed.";
            CreateOKForm(notUpToDateMessage, notUpToDateTitle);
        }

        public static void KeepExistingPackage()
        {
            const string notUpToDateMessage = "You have decided to keep the existing package, action cancelled.";
            const string notUpToDateTitle = "Keep existing.";
            CreateOKForm(notUpToDateMessage, notUpToDateTitle);
        }

        public static bool AreYouHappyWithNumbering()
        {
            const string notUpToDateMessage = "Are you happy with your numbering?\r\rNOTE: If any numbering dialog is open please close this before continuing, Prism may fail if you move forward with numbering windows active.";
            const string notUpToDateTitle = "Numbering";
            return CreateYesNoForm(notUpToDateMessage, notUpToDateTitle);
        }

        public static bool FabsecsGreenAlreadyOn()
        {
            const string notUpToDateMessage = "Careful: This check processes fabsecs by setting unique marking and adding green to selected members.\n" + 
                "It looks like you have done this already on some selected parts. Are you sure you want to do this again? Clicking no will skip the fabsec processing" +
                " but will continue to do the other checks in this step.";
            const string notUpToDateTitle = "Fabsec Green";
            return CreateYesNoForm(notUpToDateMessage, notUpToDateTitle);

        }
        public static bool ShouldSeverSafeBeProcessed()
        {
            const string notUpToDateMessage = "You have Seversafe in your selection, would you like to create an order for this?";
            const string notUpToDateTitle = "Seversafe found";
            return CreateYesNoForm(notUpToDateMessage, notUpToDateTitle);
        }

        public static bool MoveToFabDirectory(string directory, string variationDirectory, bool isVariation)
        {
            string directoryString = directory == "" ? directory : $"You have specified location {directory} in advanced settings as an alternate location for this type of package.";
            string also = directory == "" ? "" : "also ";
            string s = variationDirectory == "" && directory == "" && isVariation ? "" : "s";
            string variationString = variationDirectory == "" ? variationDirectory : $"\r\rYou have {also}specified location {directory} in advanced settings as an alternate location for all variation packages";
            string notUpToDateMessage = $"{directoryString}{variationString}\r\rWould you like to move your package{s} here?";
                        
            string notUpToDateTitle = "Alternate Package directory";
            return CreateYesNoForm(notUpToDateMessage, notUpToDateTitle);
        }

        public static void HasAlreadyBeenOrdered()
        {
            const string notUpToDateMessage = "You are trying to order material that appears to have already been ordered.";
            const string notUpToDateTitle = "Can't Order again.";
            CreateOKForm(notUpToDateMessage, notUpToDateTitle);
        }

        public static void ExecutionClassMissing(List<PrismPart> failedParts)
        {
            const string notUpToDateMessage = "You have selected items that do not have an execution class specified, please correct this to continue";
            const string notUpToDateTitle = "Execution class missing";
            CreateOkWithReportForm(notUpToDateMessage, notUpToDateTitle, failedParts, "Execution class missing");
        }

        public static void StartNumbersDontMatch(List<PrismPart> failedParts)
        {
            const string notUpToDateMessage = "You have primary members and secondary parts of the same assembly with mismatching start numbers, please correct this to continue.";
            const string notUpToDateTitle = "Mismatching numbers";
            CreateOkWithReportForm(notUpToDateMessage, notUpToDateTitle, failedParts, "Fitting start number does not match main part start number");
        }

        public static void PhasesDontMatch(List<PrismPart> failedParts)
        {
            const string notUpToDateMessage = "You have primary members and secondary parts of the same assembly with mismatching phasing, please correct this to continue.";
            const string notUpToDateTitle = "Mismatching phasing";
            CreateOkWithReportForm(notUpToDateMessage, notUpToDateTitle, failedParts, "Fitting phase does not match main part phase");
        }

        public static void HasNoFinish(List<PrismPart> failedParts)
        {
            const string notUpToDateMessage = "You have main parts without a finish, please correct this to continue.";
            const string notUpToDateTitle = "Missing finishes";
            CreateOkWithReportForm(notUpToDateMessage, notUpToDateTitle, failedParts, "Part has no finish");
        }

        public static void NoPartsSelected()
        {
            const string notUpToDateMessage = "You have not selected any members in the model, please make a selection and try again.";
            const string notUpToDateTitle = "Nothing selected";
            CreateOKForm(notUpToDateMessage, notUpToDateTitle);
        }

        public static void HasNotBeenOrdered()
        {
            string notUpToDateMessage = $"Prism UDA 'Material Order Complete' (SEV-UDA-114) is empty on {Warning} selected parts, " +
                $"this indicates it has not been ordered, please correct this to continue.";
            const string notUpToDateTitle = "Missing prelim marks";
            CreateOKForm(notUpToDateMessage, notUpToDateTitle);
        }

        public static void HasNotBeenOrderedFabsec()
        {
            string notUpToDateMessage = $"Before creating Carcasses you must order the plate material.\n"
                + $"Prism UDA 'Material Order Complete' (SEV-UDA-114) is empty on {Warning} selected parts, " +
                $"this indicates it has not been ordered, please correct this to continue.";
            const string notUpToDateTitle = "Missing prelim marks";
            CreateOKForm(notUpToDateMessage, notUpToDateTitle);
        }

        public static void FabsecCarcassSelected()
        {
            string notUpToDateMessage = $"It looks like you have selected Fabsec Carcasses, you should not do this. Select the fabsec in the model space to continue.";
            const string notUpToDateTitle = "Select Fabsecs";
            CreateOKForm(notUpToDateMessage, notUpToDateTitle);
        }

        public static void FabsecCarcassAlreadyOrdered()
        {
            string notUpToDateMessage = $"It looks like you are trying to order Fabsec Carcasses that have already been ordered.\nPlease check the Carcass Ordered UDA, if this is not blank Prism will assume the order for this piece is already complete.";
            const string notUpToDateTitle = "Carcass already ordered";
            CreateOKForm(notUpToDateMessage, notUpToDateTitle);
        }

        public static void FabsecSelectedDoesNotHaveCarcass()
        {
            string notUpToDateMessage = $"Fabsec member(s) in your selection have not had Carcasses made, you must do this first.";
            const string notUpToDateTitle = "Create Carcasses";
            CreateOKForm(notUpToDateMessage, notUpToDateTitle);
        }

        public static void FabsecAlreadyHasCarcass()
        {
            string notUpToDateMessage = $"Fabsec member(s) in your selection already have Carcasses made, change your selection.";
            const string notUpToDateTitle = "Carcasses already existing";
            CreateOKForm(notUpToDateMessage, notUpToDateTitle);
        }

        public static void CannotProcessThisTypeOfOrder()
        {
            string notUpToDateMessage = $"Prism cannot process this type of order, please try another.";
            const string notUpToDateTitle = "Select Fabsecs";
            CreateOKForm(notUpToDateMessage, notUpToDateTitle);
        }

        public static void AddedLengthToFabsecFailed(int numberOfFailedMembers)
        {
            string notUpToDateMessage = $"Prism is unable to add green to {numberOfFailedMembers} of your selected members.\n" + 
                "This is usually caused by cuts on the end of the member, please remove the cuts to continue";
            const string notUpToDateTitle = "Fabsec green failed";
            CreateOKForm(notUpToDateMessage, notUpToDateTitle);
        }

        public static void NoFabsecsSelected()
        {
            string notUpToDateMessage = $"To work with Fabsec Carcasses you must first select some in the model.";
            const string notUpToDateTitle = "Select Fabsecs";
            CreateOKForm(notUpToDateMessage, notUpToDateTitle);
        }

        public static void DrawingsNotUpToDate()
        {
            string notUpToDateMessage = "There are drawings in your selection that are not up to date. These must be updated before continuing.";
            const string notUpToDateTitle = "Update drawings";
            CreateOKForm(notUpToDateMessage, notUpToDateTitle);
        }

        public static void DrawingPrintFailed()
        {
            string message = "Warning: The Tekla PDF printer has failed to print all drawings expected of the currrent type, please check drawing numbers at the end.";
            const string notUpToDateTitle = "Drawing print failure";
            CreateOKForm(message, notUpToDateTitle);

        }

        public static void IncorrectlyAssignedDrawings()
        {
            string notUpToDateMessage = "You have drawings that are not assigned to correctly, in the Title 1 field of each drawing the folder to print to must be asssigned. Title 1 must contain one of the following: \r\"ASS\", \"FIT\", \"PRT\", \"SHA\", \"PGC\", \"WLD\", \"Not Required\" \r\rIf this issue persists seek help from the development team.";
            const string notUpToDateTitle = "Update drawings";
            CreateOKForm(notUpToDateMessage, notUpToDateTitle);
        }

        public static bool NumbersNoLongerUpToDate()
        {
            string notUpToDateMessage = "Numbers that were up to date before running Prism are now modified, this should be reviewed, do you want to ignroe this warning?";
            const string notUpToDateTitle = "Error";
            return CreateYesNoForm(notUpToDateMessage, notUpToDateTitle);
        }

        public static bool ContinueAnyway()
        {
            string notUpToDateMessage = "Do you want to ignore this warning?";
            const string notUpToDateTitle = "Error";
            return CreateYesNoForm(notUpToDateMessage, notUpToDateTitle);
        }

        public static void SeversafeOrderCancelled()
        {
            string notUpToDateMessage = "No seversafe order will be created with this package.";
            const string notUpToDateTitle = "Cancelled";
            CreateOKForm(notUpToDateMessage, notUpToDateTitle);
        }

        public static void Cancelled()
        {
            string notUpToDateMessage = "Cancelled.";
            const string notUpToDateTitle = "Cancelled";
            CreateOKForm(notUpToDateMessage, notUpToDateTitle);
        }

        public static bool IgnoreWarning()
        {
            const string notUpToDateMessage = "Would you like to ignore this error and continue?";
            const string notUpToDateTitle = "Ignore?";
            return CreateYesNoForm(notUpToDateMessage, notUpToDateTitle);
        }

        public static void MaterialOrderComplete(PrismProjectData projectData)
        {
            string notUpToDateMessage = $"Thanks {projectData.First}, your order has been assembled, " +
                 $"please forward the following email to the relevant purchasing team";
            string notUpToDateTitle = "Complete";
            CreateOKForm(notUpToDateMessage, notUpToDateTitle) ;
        }

        public static void FabPackComplete(PrismProjectData projectData, bool fileCanBeAttached)
        {
            string notUpToDateMessage = $"Thanks {projectData.First},\r\rYour fab package has been assembled, your package can be found in your model folder." +
                       $"\r\r{Note(fileCanBeAttached)}";
            string notUpToDateTitle = "Complete";
            CreateOKForm(notUpToDateMessage, notUpToDateTitle) ;
        }

        private static string Note(bool fileCanBeAttached)
        {
            if(fileCanBeAttached)
            {
                return "It has also been attached to the following email, please forward this to the relevant team.";
            }
            else
            {
                return "Your package is too large to be attached to the following mail, please add your own link to the email and send to the relevant team.";
            }
        }
        
        public static void ErrorsFixed(int numberOfErrors)
        {
            string messageEnd = "errors fixed.";
            if (numberOfErrors == 1)
            {
                messageEnd = "error fixed.";
            }
            string notUpToDateMessage = $"{numberOfErrors} {messageEnd}";
            const string notUpToDateTitle = "Errors Fixed";
            CreateOKForm(notUpToDateMessage, notUpToDateTitle);
        }

        public static IgnoreType DisplayOrderErrors(List<PrismPart> errorList, Error warning)
        {
            if (errorList.Count != 0)
            {
                if (warning == Error.NameAndClass)
                {
                    NameAndClassDontMatch(errorList);
                }
                if (warning == Error.Execution)
                {
                    ExecutionClassMissing(errorList);
                }
                if (warning == Error.Orientation)
                {
                    MemberOrientationIsWrong(errorList);
                }

                ModelObjectVisualization.SetTransparencyForAll(TemporaryTransparency.SEMITRANSPARENT);
                ModelObjectVisualization.SetTemporaryStateForAll(new Color(0.5, 0.5, 0.5));
                ModelObjectVisualization.SetTemporaryState(Convertor.PrismPartsToModelObjects(errorList), new Color(1, 0, 0));
                return NewIgnoreWarning();
            }
            return IgnoreType.Unspecified;
        }

        public static bool UnorderedShearStuds()
        {
            const string notUpToDateMessage = "Prism has detected shear studs in your selection, these should have been manually pre-ordered by now.\r\rClick yes to confirm you have pre-ordered these.";
            const string notUpToDateTitle = "Shear studs";
            if (CreateYesNoForm(notUpToDateMessage, notUpToDateTitle))
            {
                return true;
            }
            return false;
        }

        public static bool CreatePackageWithoutDrawings()
        {
            const string notUpToDateMessage = "Would you like to create your package anyway without drawings?";
            const string notUpToDateTitle = "Skip Drawings";
            if (CreateYesNoForm(notUpToDateMessage, notUpToDateTitle))
            {
                return true;
            }
            return false;
        }

        public static void TooManyPartsForRocket(string numberOfParts, string limit)
        {
            string notUpToDateMessage = $"Sorry, you have selected {numberOfParts} parts, the limit for the rocket button it {limit}, " +
                $"either reduce your selection or run the full Prism process on the selected.";
            string notUpToDateTitle = "Over the limit.";
            CreateOKForm(notUpToDateMessage, notUpToDateTitle);
        }

        public static bool IgnoreAndContinue()
        {
            const string notUpToDateMessage = "Would you like to ignore this error and continue?";
            const string notUpToDateTitle = "Ignore";
            return CreateYesNoForm(notUpToDateMessage, notUpToDateTitle);
        }

        public static bool RocketButtonCheck()
        {
            const string notUpToDateMessage = "This will give you a fab package without doing any of Prisms standard checks, are you sure you want to continue?";
            const string notUpToDateTitle = "Careful now";
            return CreateYesNoForm(notUpToDateMessage, notUpToDateTitle);
        }

        public static bool OrderShearStuds()
        {
            const string notUpToDateMessage = "Do you want to run an order for unordered studs in the selection now?";
            const string notUpToDateTitle = "Shear studs";
            return CreateYesNoForm(notUpToDateMessage, notUpToDateTitle);
        }

        private static void CreateOKForm(string message, string title)
        {
            OkForm okForm = new OkForm(message, title);
            okForm.TopMost = true;
            okForm.ShowDialog();
        }

        private static void CreateOkWithReportForm(string message, string title, List<PrismPart> failedParts, string standardPartMessage)
        {
            OkWithReportForm okForm = new OkWithReportForm(message, title, failedParts, standardPartMessage);
            okForm.TopMost = true;
            okForm.ShowDialog();
        }

        private static void CreateOkWithReportForm(string message, string title, List<ModelObject> failedParts, string standardPartMessage)
        {
            OkWithReportForm okForm = new OkWithReportForm(message, title, failedParts, standardPartMessage);
            okForm.TopMost = true;
            okForm.ShowDialog();
        }

        private static bool CreateYesNoForm(string message, string title)
        {
            YesNoForm yesNo = new YesNoForm(message, title);
            yesNo.TopMost = true;
            yesNo.ShowDialog();
            return yesNo.Yes;
        }
    }
}