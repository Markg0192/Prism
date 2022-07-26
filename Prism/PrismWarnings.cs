using System.Windows.Forms;

namespace Prism
{
    public static class PrismWarnings
    {
        public static void NumberingIsNotUpToDate()
        {
            const string notUpToDateMessage = "Your member numbering is not up to date, please update and try again";
            const string notUpToDateTitle = "Numbers not up to date";
            MessageBox.Show(notUpToDateMessage, notUpToDateTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public static void NameAndClassDontMatch()
        {
            const string notUpToDateMessage = "You have selected something thats name and class do not align with GDOM convention, please correct this to continue.";
            const string notUpToDateTitle = "Part name and class misalignment";
            MessageBox.Show(notUpToDateMessage, notUpToDateTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public static void PreviousStepIncomplete()
        {
            const string notUpToDateMessage = "You have not completed all the required steps before this action, please correct this to continue.";
            const string notUpToDateTitle = "Incomplete stages";
            MessageBox.Show(notUpToDateMessage, notUpToDateTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public static void ExecutionClassMissing()
        {
            const string notUpToDateMessage = "You have selected items that do not have an execution class specified, please correct this to continue";
            const string notUpToDateTitle = "Execution class missing";
            MessageBox.Show(notUpToDateMessage, notUpToDateTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public static void StartNumbersDontMatch()
        {
            const string notUpToDateMessage = "You have primary members and secondary parts of the same assembly with mismatching start numbers, please correct this to continue.";
            const string notUpToDateTitle = "Mismatching numbers";
            MessageBox.Show(notUpToDateMessage, notUpToDateTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public static void PhasesDontMatch()
        {
            const string notUpToDateMessage = "You have primary members and secondary parts of the same assembly with mismatching phasing, please correct this to continue.";
            const string notUpToDateTitle = "Mismatching phasing";
            MessageBox.Show(notUpToDateMessage, notUpToDateTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public static void HasNoFinish()
        {
            const string notUpToDateMessage = "You have main parts without a finish, please correct this to continue.";
            const string notUpToDateTitle = "Missing finishes";
            MessageBox.Show(notUpToDateMessage, notUpToDateTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public static void HasNotBeenOrdered()
        {
            const string notUpToDateMessage = "You have main parts without a prelim number, this indicates it has not been ordered, please correct this to continue.";
            const string notUpToDateTitle = "Missing prelim marks";
            MessageBox.Show(notUpToDateMessage, notUpToDateTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public static bool IgnoreHasNotBeenOrdered()
        {
            const string notUpToDateMessage2 = "Would you like to ignore this error and continue?";
            const string notUpToDateTitle2 = "Missing prelims";
            DialogResult result = MessageBox.Show(notUpToDateMessage2, notUpToDateTitle2, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.No)
            {
                return false;
            }
            return true;
        }

        public static DialogResult MaterialOrderComplete(PrismProjectData projectData)
        {
            return MessageBox.Show($"Thanks {projectData.First}, your material order is now complete, " +
                 $"please forward the following email to the relevant purchasing team", "Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static DialogResult FabPackComplete(PrismProjectData projectData)
        {
           return MessageBox.Show($"Thanks {projectData.First}, your fab package is now complete, please attach your fab package, located in your model folder, " +
                       $"to the following email and send to the relevant team. PLEASE NOTE: This version of Prism does NOT print drawings, for now, you will have " +
                       $"to do this bit yourself.", "Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}