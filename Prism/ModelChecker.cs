using System.Collections;
using System.Collections.Generic;
using Tekla.Structures.Model;

namespace Prism
{
    /// <summary>
    /// The pre run checks class is used to check the model for any issues in the model that may cause the program to crash.
    /// These are ran before the main program and warns the user of the problem and closes the app before it crashes.
    /// </summary>
    public static class ModelChecker
    {
        public static bool HasExecutionClass(this SelectedObjects selectedObjects)
        {
            foreach (Assembly ass in selectedObjects.AssembliesList)
            {
                Part p = ass.GetMainPart() as Part;
                int executionClassData = 10;  //this is the number read from teklas UDA when no execution class is applied, we are defaulting to it not having one here
                p.GetUserProperty(ModelUDA.ExcecutionClass(), ref executionClassData);

                if (executionClassData == 10)
                {
                    PrismWarnings.ExecutionClassMissing();
                    return false;
                }
            }
            return true;
        }

        public static bool NameAndClassAligned(this SelectedObjects selectedObjects)
        {
            foreach (Part p in selectedObjects.SelectedModelParts)
            {
                List<string> meantToBeClass = GdomValues.PartClass()[p.Name] as List<string>;
                if (meantToBeClass != null && !meantToBeClass.Contains(p.Class))
                {
                    PrismWarnings.NameAndClassDontMatch();
                    return false;
                }
            }
            return true;
        }

        public static bool ArePreviousStepsComplete(SelectedObjects selectedObjects, int stageNumber)
        {
            foreach (Part p in selectedObjects.SelectedModelParts)
            {
                string userProperty = "";
                p.GetUserProperty(ModelUDA.PreviousStageName(stageNumber), ref userProperty);
                if (userProperty == "")
                {
                    PrismWarnings.PreviousStepIncomplete();
                    return false;
                }
            }
            return true;
        }        

        public static bool RunStage4Checks(this SelectedObjects selectedObjects)
        {
            foreach (Assembly ass in selectedObjects.AssembliesList)
            {
                Part myMainPart = ass.GetMainPart() as Part;
                if (!myMainPart.HasBeenOrdered()) { return false; }
                if (!myMainPart.HasAFinish()) { return false; }
                ArrayList mySecondaries = ass.GetSecondaries();
                foreach (Part mySecondaryPart in mySecondaries)
                {
                    if (!myMainPart.StartNumbersMatch(mySecondaryPart)) { return false; }
                    if (!myMainPart.PhasesMatch(mySecondaryPart)) { return false; }
                }
            }
            return true;
        }

        public static bool StartNumbersMatch(this Part mainPart, Part secondaryPart)
        {
            if (mainPart.AssemblyNumber.StartNumber != secondaryPart.PartNumber.StartNumber)
            {
                PrismWarnings.StartNumbersDontMatch();
                return false;
            }
            return true;
        }

        public static bool PhasesMatch(this Part mainPart, Part secondaryPart)
        {
            mainPart.GetPhase(out Phase mainPartPhase);
            secondaryPart.GetPhase(out Phase secondaryPhase);

            if (mainPartPhase.PhaseNumber != secondaryPhase.PhaseNumber)
            {
                PrismWarnings.PhasesDontMatch();
                return false;
            }
            return true;
        }

        public static bool HasAFinish(this Part mainPart)
        {
            if (mainPart.Finish.Length == 0)
            {
                PrismWarnings.HasNoFinish();
                return false;
            }
            return true;
        }

        public static bool HasBeenOrdered(this Part mainPart)
        {
            string prelimMark = "";
            mainPart.GetUserProperty(ModelUDA.PrelimMark(), ref prelimMark);
            if (prelimMark.Length == 0)
            {
                PrismWarnings.HasNotBeenOrdered();
                return PrismWarnings.IgnoreHasNotBeenOrdered();
            }
            return true;
        }
    }
}