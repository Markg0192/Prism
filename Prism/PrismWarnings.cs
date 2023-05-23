using Prism.CustomDialogs;
using System.Collections.Generic;
using System.Windows.Forms;
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

        public static int BoltOrderType()
        {
            var form = new BoltOrderType();
            form.ShowDialog();
            return form.OrderBoltsFrom;
        }

        public static void LockedPartsSelected()
        {
            const string notUpToDateMessage = "You have selected some parts that are locked, please de-select or unlock these to continue.";
            const string notUpToDateTitle = "Locked parts selected";
            CreateOKForm(notUpToDateMessage, notUpToDateTitle);

            //MessageBox.Show(notUpToDateMessage, notUpToDateTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public static void IgnoreFittingCheck()
        {
            const string notUpToDateMessage = "Without selecting a location Prism cannot filter abnormal fittings.";
            const string notUpToDateTitle = "Are you sure?";
            CreateOKForm(notUpToDateMessage, notUpToDateTitle);
            //MessageBox.Show(notUpToDateMessage, notUpToDateTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public static void PrelimStartReset(string resetNo)
        {
            string notUpToDateMessage = $"Prelim number start point manually set to {resetNo}";
            string notUpToDateTitle = "Are you sure?";
            CreateOKForm(notUpToDateMessage, notUpToDateTitle);
            // MessageBox.Show(notUpToDateMessage, notUpToDateTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public static void AbnormalFittings()
        {
            string notUpToDateMessage = $"You have selected some abnormal fittings that should either be bought out items or changed to something standard.\r {Warning}";
            const string notUpToDateTitle = "Abnormal Fittings";
            CreateOKForm(notUpToDateMessage, notUpToDateTitle);
            //MessageBox.Show(notUpToDateMessage, notUpToDateTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public static bool TagAbnormalFittings()
        {
            string notUpToDateMessage = $"Would you like to tag these abnormal fittings for ordering later?.\r {Warning}";
            const string notUpToDateTitle = "Abnormal Fittings Tag";
            return CreateYesNoForm(notUpToDateMessage, notUpToDateTitle);
           // return MessageBox.Show(notUpToDateMessage, notUpToDateTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        }

        public static void IntumescentLoadingMissing()
        {
            const string notUpToDateMessage = "You have selected intumescent members that have no loading.";
            const string notUpToDateTitle = "Intumescent loadings";
            CreateOKForm(notUpToDateMessage, notUpToDateTitle);
            // MessageBox.Show(notUpToDateMessage, notUpToDateTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public static bool IgnoreIntumescentLoading()
        {
            const string notUpToDateMessage = "Would you like to ignore this error and continue?";
            const string notUpToDateTitle = "Missing Intumescent Loadings";
            return CreateYesNoForm(notUpToDateMessage, notUpToDateTitle);
           /* DialogResult result = MessageBox.Show(notUpToDateMessage2, notUpToDateTitle2, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.No)
            {
                return false;
            }
            return true;*/
        }

        public static bool ResetPrelimMarking()
        {
            const string notUpToDateMessage = "WARNING! Selecting this button means your prelim marking will now start at the number given, Prism cannot undo this manual action. Are you sure you want to proceed?";
            const string notUpToDateTitle = "Be careful";
            return CreateYesNoForm(notUpToDateMessage, notUpToDateTitle);
           /* DialogResult result = MessageBox.Show(notUpToDateMessage, notUpToDateTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.No)
            {
                return false;
            }
            return true;*/
        }

        public static bool IsVariation()
        {
            const string notUpToDateMessage = "Is this a variation?";
            const string notUpToDateTitle = "Variation?";
            return CreateYesNoForm(notUpToDateMessage, notUpToDateTitle);
           /* DialogResult result = MessageBox.Show(notUpToDateMessage, notUpToDateTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.No)
            {
                return false;
            }
            return true;*/
        }

        public static bool FabsecsPresent()
        {
            const string notUpToDateMessage = "There are FABSEC members present in your selection, I will process these, have you tidied the carcass drawings?";
            const string notUpToDateTitle = "FABSECS!";
            return CreateYesNoForm(notUpToDateMessage, notUpToDateTitle);
            //return MessageBox.Show(notUpToDateMessage, notUpToDateTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        }

        public static void NumberingIsNotUpToDate()
        {
            const string notUpToDateMessage = "Your member numbering is not up to date, please update and try again";
            const string notUpToDateTitle = "Numbers not up to date";
            CreateOKForm(notUpToDateMessage, notUpToDateTitle);
            // MessageBox.Show(notUpToDateMessage, notUpToDateTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public static void NameAndClassDontMatch()
        {
            const string notUpToDateMessage = "You have selected something thats name and class do not align with GDOM convention, please correct this to continue.";
            const string notUpToDateTitle = "Part name and class misalignment";
            CreateOKForm(notUpToDateMessage, notUpToDateTitle);
            // MessageBox.Show(notUpToDateMessage, notUpToDateTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public static void MemberOrientationIsWrong()
        {
            const string notUpToDateMessage = "You have selected a member that has been input in the wrong orientation, please correct this to continue.";
            const string notUpToDateTitle = "Incorrect member orientation";
            CreateOKForm(notUpToDateMessage, notUpToDateTitle);
            //MessageBox.Show(notUpToDateMessage, notUpToDateTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public static void PreviousStepIncomplete()
        {
            const string notUpToDateMessage = "You have not completed all the required steps before this action, please correct this to continue.";
            const string notUpToDateTitle = "Incomplete stages";
            CreateOKForm(notUpToDateMessage, notUpToDateTitle);
            //MessageBox.Show(notUpToDateMessage, notUpToDateTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public static void HasNotBeenOrderedOMIT()
        {
            const string notUpToDateMessage = "You are trying to OMIT material that does not appear to have ever been ordered, please choose a different course of action.";
            const string notUpToDateTitle = "Can't Omit.";
            CreateOKForm(notUpToDateMessage, notUpToDateTitle);
            //MessageBox.Show(notUpToDateMessage, notUpToDateTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        /*public static DialogResult FabFolderAlreadyExists()
        {
            const string notUpToDateMessage = "The fab folder you are trying to create already exists in the model folder, can I replace it?";
            const string notUpToDateTitle = "Fab folder exists.";
            return MessageBox.Show(notUpToDateMessage, notUpToDateTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        }*/

        public static void FolderAlreadyExists(string existingFolder)
        {
            string notUpToDateMessage = $"{existingFolder} already exists in the model folder, it must be removed from the model folder before continuing";
            const string notUpToDateTitle = "Folder exists.";
            CreateOKForm(notUpToDateMessage, notUpToDateTitle);
            //MessageBox.Show(notUpToDateMessage, notUpToDateTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public static void ZipFolderAlreadyExists()
        {
            const string notUpToDateMessage = "The zip folder you are trying to create already exists in the model folder, it must be removed from the model folder before continuing";
            const string notUpToDateTitle = "Zip folder exists.";
            CreateOKForm(notUpToDateMessage, notUpToDateTitle);
            //MessageBox.Show(notUpToDateMessage, notUpToDateTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public static void KeepExistingPackage()
        {
            const string notUpToDateMessage = "You have decided to keep the existing package, action cancelled.";
            const string notUpToDateTitle = "Keep existing.";
            CreateOKForm(notUpToDateMessage, notUpToDateTitle);
            //MessageBox.Show(notUpToDateMessage, notUpToDateTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public static bool AreYouHappyWithNumbering()
        {
            const string notUpToDateMessage = "Are you happy with your numbering?";
            const string notUpToDateTitle = "Numbering";
            return CreateYesNoForm(notUpToDateMessage, notUpToDateTitle);
            //return MessageBox.Show(notUpToDateMessage, notUpToDateTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        }

        public static void HasAlreadyBeenOrdered()
        {
            const string notUpToDateMessage = "You are trying to order material that appears to have already been ordered.";
            const string notUpToDateTitle = "Can't Order again.";
            CreateOKForm(notUpToDateMessage, notUpToDateTitle);
            // MessageBox.Show(notUpToDateMessage, notUpToDateTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public static void ExecutionClassMissing()
        {
            const string notUpToDateMessage = "You have selected items that do not have an execution class specified, please correct this to continue";
            const string notUpToDateTitle = "Execution class missing";
            CreateOKForm(notUpToDateMessage, notUpToDateTitle);
            // MessageBox.Show(notUpToDateMessage, notUpToDateTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public static void StartNumbersDontMatch()
        {
            const string notUpToDateMessage = "You have primary members and secondary parts of the same assembly with mismatching start numbers, please correct this to continue.";
            const string notUpToDateTitle = "Mismatching numbers";
            CreateOKForm(notUpToDateMessage, notUpToDateTitle);
            // MessageBox.Show(notUpToDateMessage, notUpToDateTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public static void PhasesDontMatch()
        {
            const string notUpToDateMessage = "You have primary members and secondary parts of the same assembly with mismatching phasing, please correct this to continue.";
            const string notUpToDateTitle = "Mismatching phasing";
            CreateOKForm(notUpToDateMessage, notUpToDateTitle);
            // MessageBox.Show(notUpToDateMessage, notUpToDateTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public static void HasNoFinish()
        {
            const string notUpToDateMessage = "You have main parts without a finish, please correct this to continue.";
            const string notUpToDateTitle = "Missing finishes";
            CreateOKForm(notUpToDateMessage, notUpToDateTitle);
            // MessageBox.Show(notUpToDateMessage, notUpToDateTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public static void NoPartsSelected()
        {
            const string notUpToDateMessage = "You have not selected any members in the model, please make a selection and try again.";
            const string notUpToDateTitle = "Nothing selected";
            CreateOKForm(notUpToDateMessage, notUpToDateTitle);
            //MessageBox.Show(notUpToDateMessage, notUpToDateTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public static void HasNotBeenOrdered()
        {
            string notUpToDateMessage = $"Prism UDA 'Material Order Complete' (SEV-UDA-114) is empty on {Warning} selected parts, " +
                $"this indicates it has not been ordered, please correct this to continue.";
            const string notUpToDateTitle = "Missing prelim marks";
            CreateOKForm(notUpToDateMessage, notUpToDateTitle);
            //MessageBox.Show(notUpToDateMessage, notUpToDateTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public static void DrawingsNotUpToDate()
        {
            string notUpToDateMessage = "There are drawings in your selection that are not up to date. These must be updated before continuing.";
            const string notUpToDateTitle = "Update drawings";
            CreateOKForm(notUpToDateMessage, notUpToDateTitle);
            //MessageBox.Show(notUpToDateMessage, notUpToDateTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);

        }

        public static void NumbersNoLongerUpToDate()
        {
            string notUpToDateMessage = "Numbers that were up to date before running Prism are now modified, please review.";
            const string notUpToDateTitle = "Error";
            CreateOKForm(notUpToDateMessage, notUpToDateTitle);
            //MessageBox.Show(notUpToDateMessage, notUpToDateTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public static bool IgnoreWarning()
        {
            const string notUpToDateMessage = "Would you like to ignore this error and continue?";
            const string notUpToDateTitle = "Ignore?";
            return CreateYesNoForm(notUpToDateMessage, notUpToDateTitle);
          /*  DialogResult result = MessageBox.Show(notUpToDateMessage2, notUpToDateTitle2, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.No)
            {
                return false;
            }
            return true;*/
        }

        public static void MaterialOrderComplete(PrismProjectData projectData)
        {
            string notUpToDateMessage = $"Thanks {projectData.First}, your material order is now complete, " +
                 $"please forward the following email to the relevant purchasing team";
            string notUpToDateTitle = "Complete";
            CreateOKForm(notUpToDateMessage, notUpToDateTitle) ;

           /* return MessageBox.Show($"Thanks {projectData.First}, your material order is now complete, " +
                 $"please forward the following email to the relevant purchasing team", "Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);*/
        }

        public static void FabPackComplete(PrismProjectData projectData)
        {
            //return MessageBox.Show($"Thanks {projectData.First}, your fab package is now complete, please attach your fab package, located in your model folder, " +
            //            $"to the following email and send to the relevant team. PLEASE NOTE: This version of Prism does NOT print drawings, for now, you will have " +
            //            $"to do this bit yourself.", "Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
         //   return MessageBox.Show($"Thanks {projectData.First}, your fab package is now complete, please attach your fab package, located in your model folder, " +
                  //     $"to the following email and send to the relevant team.", "Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);


            string notUpToDateMessage = $"Thanks {projectData.First}, your fab package is now complete, please attach your fab package, located in your model folder, " +
                       $"to the following email and send to the relevant team.";
            string notUpToDateTitle = "Complete";
            CreateOKForm(notUpToDateMessage, notUpToDateTitle) ;

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
            // MessageBox.Show(notUpToDateMessage, notUpToDateTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public static IgnoreType DisplayOrderErrors(List<ModelObject> errorList, Error warning)
        {
            if (errorList.Count != 0)
            {
                if (warning == Error.NameAndClass)
                {
                    NameAndClassDontMatch();
                }
                if (warning == Error.Execution)
                {
                    ExecutionClassMissing();
                }
                if (warning == Error.Orientation)
                {
                    MemberOrientationIsWrong();
                }

                ModelObjectVisualization.SetTransparencyForAll(TemporaryTransparency.SEMITRANSPARENT);
                ModelObjectVisualization.SetTemporaryStateForAll(new Color(0.5, 0.5, 0.5));
                ModelObjectVisualization.SetTemporaryState(errorList, new Color(1, 0, 0));
                return NewIgnoreWarning();
            }
            return IgnoreType.Unspecified;
        }

        private static void CreateOKForm(string message, string title)
        {
            OkForm okForm = new OkForm(message, title);
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