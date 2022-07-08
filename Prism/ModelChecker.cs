using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using Tekla.Structures.Model;

namespace Prism
{
    /// <summary>
    /// The pre run checks class is used to check the model for any issues in the model that may cause the program to crash.
    /// These are ran before the main program and warns the user of the problem and closes the app before it crashes.
    /// </summary>
    public static class ModelChecker
    {      
        public static bool CheckExecutionField(this SelectedObjects modelEnum)
        {  
            foreach (Assembly ass in modelEnum.AssembliesList)
            {
                Part p = ass.GetMainPart() as Part;
                int executionClassData = 10;
                p.GetUserProperty("EN1090_EXC_PART", ref executionClassData);

                if (executionClassData == 10)
                {
                    const string notUpToDateMessage = "You have selected items that do not have an execution class specified, please correct this to continue";
                    const string notUpToDateTitle = "Execution class missing";
                    MessageBox.Show(notUpToDateMessage, notUpToDateTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
            }
            return true;
        }

        public static bool CheckNameAndClassAlignment(this SelectedObjects modelEnum)
        {
            foreach (Part p in modelEnum.SelectedModelParts)
            {
                List<string> meantToBeClass = GdomValues.PartClass()[p.Name] as List<string>;
                if (!meantToBeClass.Contains(p.Class)) 
                { 
                    ShowNameAndClassErrorMessage(); 
                    return false; 
                }
            }
            return true;
        }

        private static void ShowNameAndClassErrorMessage()
        {
            const string notUpToDateMessage = "You have selected something thats name and class do not align with GDOM convention, please correct this to continue.";
            const string notUpToDateTitle = "Part name and class misalignment";
            MessageBox.Show(notUpToDateMessage, notUpToDateTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public static bool ArePreviousStepsComplete(SelectedObjects modelEnum, int stageNumber)
        {
            foreach (Part p in modelEnum.SelectedModelParts)
            {
                string userProperty = "";
                p.GetUserProperty($"PRISM-{stageNumber - 1}-NAME", ref userProperty);
                if (userProperty == "")
                {
                    const string notUpToDateMessage = "You have not completed all the required steps before this action, please correct this to continue.";
                    const string notUpToDateTitle = "Incomplete stages";
                    MessageBox.Show(notUpToDateMessage, notUpToDateTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
            }
            return true;
        }

        public static bool CheckForInputs(int stageNumber, string input1, string input2)
        {
            if (stageNumber == 2 && input1 == "")
            {
                const string notUpToDateMessage = "You have not stated a start number, please complete this field to continue.";
                const string notUpToDateTitle = "Missing start number";
                MessageBox.Show(notUpToDateMessage, notUpToDateTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (stageNumber == 3 && input1 == "")
            {
                const string notUpToDateMessage = "You have stated a phase number, please complete this field to continue.";
                const string notUpToDateTitle = "Missing phase number";
                MessageBox.Show(notUpToDateMessage, notUpToDateTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (stageNumber == 3 && input2 == "")
            {
                const string notUpToDateMessage = "You have not stated an issue number, please complete this field to continue";
                const string notUpToDateTitle = "Missing issue number";
                MessageBox.Show(notUpToDateMessage, notUpToDateTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        public static bool RunStage4Checks(this SelectedObjects modelEnum)
        {
            bool process = true;
            foreach (Assembly ass in modelEnum.AssembliesList)
            {
                Part myMainPart = ass.GetMainPart() as Part;
                process = CheckMainPartHasBeenOrdered(myMainPart);
                if (!process) { return false; }
                process = CheckMainPartHasFinish(myMainPart);
                if (!process) { return false; }
                ArrayList mySecondaries = ass.GetSecondaries();
                foreach (Part mySecondaryPart in mySecondaries)
                {
                    process = CheckStartNumbersMatch(myMainPart, mySecondaryPart);
                    if (!process) { return false; }
                    process = CheckPhasesMatch(myMainPart, mySecondaryPart);
                    if (!process) { return false; }
                }
            }
            return true;
        }

        public static bool CheckStartNumbersMatch(Part mainPart, Part secondaryPart)
        {
            if (mainPart.AssemblyNumber.StartNumber != secondaryPart.PartNumber.StartNumber)
            {
                const string notUpToDateMessage = "You have primary members and secondary parts of the same assembly with mismatching start numbers, please correct this to continue.";
                const string notUpToDateTitle = "Mismatching numbers";
                MessageBox.Show(notUpToDateMessage, notUpToDateTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        public static bool CheckPhasesMatch(Part mainPart, Part secondaryPart)
        {
            mainPart.GetPhase(out Phase mainPartPhase);
            secondaryPart.GetPhase(out Phase secondaryPhase);
            int mainPartPhaseNumber = mainPartPhase.PhaseNumber;
            int secondaryPartPhaseNumber = secondaryPhase.PhaseNumber;

            if (mainPartPhaseNumber != secondaryPartPhaseNumber)
            {
                const string notUpToDateMessage = "You have primary members and secondary parts of the same assembly with mismatching phasing, please correct this to continue.";
                const string notUpToDateTitle = "Mismatching phasing";
                MessageBox.Show(notUpToDateMessage, notUpToDateTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        public static bool CheckMainPartHasFinish(Part mainPart)
        {
            if(mainPart.Finish.Length == 0)
            {
                const string notUpToDateMessage = "You have main parts without a finish, please correct this to continue.";
                const string notUpToDateTitle = "Missing finishes";
                MessageBox.Show(notUpToDateMessage, notUpToDateTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        public static bool CheckMainPartHasBeenOrdered(Part mainPart)
        {
            string prelimMark = "";
            mainPart.GetUserProperty("PRELIM_MARK", ref prelimMark);
            if(prelimMark.Length == 0)
            {
                const string notUpToDateMessage = "You have main parts without a prelim number, this indicates it has not been ordered, please correct this to continue.";
                const string notUpToDateTitle = "Missing prelim marks";
                MessageBox.Show(notUpToDateMessage, notUpToDateTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);

                const string notUpToDateMessage2 = "Would you like to ignore this error and continue?";
                const string notUpToDateTitle2 = "Missing prelims";
                DialogResult result = MessageBox.Show(notUpToDateMessage2, notUpToDateTitle2, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.No)
                {
                    return false;      
                }
            }
            return true;
        }
    }
}