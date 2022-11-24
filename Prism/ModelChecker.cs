using System.Collections;
using System.Collections.Generic;
using Tekla.Structures.Model;
using static Prism.Enums;
using static Prism.IgnoreWarning;

namespace Prism
{
    /// <summary>
    /// The pre run checks class is used to check the model for any issues in the model that may cause the program to crash.
    /// These are ran before the main program and warns the user of the problem and closes the app before it crashes.
    /// </summary>
    public static class ModelChecker
    {
        public static List<ModelObject> IncorrectNameAndClass = new List<ModelObject>();
        public static List<ModelObject> MissingExecutionClass = new List<ModelObject>();
        public static List<ModelObject> HasNoFinish = new List<ModelObject>();
        public static List<ModelPart> StartNumbersDoNotMatch = new List<ModelPart>();
        public static List<ModelObject> StartNumbersDoNotMatchParts = new List<ModelObject>();
        public static List<ModelPart> PhasesDoNotMatch = new List<ModelPart>();
        public static List<ModelObject> PhasesDoNotMatchParts = new List<ModelObject>();
        public static List<ModelObject> NotOrderedParts = new List<ModelObject>();
        public static List<ModelObject> OrderedParts = new List<ModelObject>();
        public static List<ModelObject> PartsWithoutIntumescentLoading = new List<ModelObject>();

        public static bool RunStage4Checks(this SelectedObjects selectedObjects)
        {
            CheckFittings.IncorrectGrade.Clear();
            CheckFittings.IncorrectLength.Clear();
            CheckFittings.IncorrectThickness.Clear();
            OrderedParts.Clear();
            NotOrderedParts.Clear();

            Factory location = PrismWarnings.FactoryLocation();
            if (location == Factory.Unknown)
            {
                PrismWarnings.IgnoreFittingCheck();
            }

            foreach (Assembly ass in selectedObjects.AssembliesList)
            {
                Part myMainPart = ass.GetMainPart() as Part;
                GetUnorderedParts(myMainPart);
                GetPartsWithoutAFinish(myMainPart);
                CheckForIntumescentLoading(myMainPart);

                ArrayList mySecondaries = ass.GetSecondaries();
                foreach (Part mySecondaryPart in mySecondaries)
                {
                    GetPartsThatStartNumbersDontMatch(myMainPart, mySecondaryPart);
                    GetPartsWherePhasesDontMatch(myMainPart, mySecondaryPart);
                    CheckFittings.GetIncorrectFittings(mySecondaryPart, location);
                }
            }
            return CheckForAndActionErrors();
        }

        public static void HasExecutionClass(SelectedObjects selectedObjects)
        {
            foreach (Assembly ass in selectedObjects.AssembliesList)
            {
                Part p = ass.GetMainPart() as Part;
                int executionClassData = 10;  //this is the number read from teklas UDA when no execution class is applied, we are defaulting to it not having one here
                p.GetUserProperty(ModelUDA.ExcecutionClass(), ref executionClassData);

                if (executionClassData == 10)
                {
                    MissingExecutionClass.Add(p);
                }
            }
        }

        public static void NameAndClassAligned(SelectedObjects selectedObjects)
        {
            foreach (Part p in selectedObjects.SelectedModelParts)
            {
                List<string> meantToBeClass = GdomValues.PartClass()[p.Name] as List<string>;
                if (meantToBeClass != null && !meantToBeClass.Contains(p.Class))
                {
                    IncorrectNameAndClass.Add(p);
                }
            }
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

        private static bool CheckForAndActionErrors()
        {
            if (!OrderErrors()) { return false; }
            if (!FinishErrors()) { return false; }

            IgnoreType startNumberError = StartNumbersErrors();
            if (startNumberError == IgnoreType.AutoFix)
            {
                AutoFix.AssemblyAndStartNumbers();
            }
            if (startNumberError == IgnoreType.Stop) { return false; }

            IgnoreType phaseMatchError = PhaseMatchErrors();
            if (phaseMatchError == IgnoreType.AutoFix)
            {
                AutoFix.PartPhasing();
            }
            if (phaseMatchError == IgnoreType.Stop) { return false; }

            if (!IntumesecentLoadingErrors()) { return false; }

            return CheckFittings.DisplayFittingErrors();
        }

        public static IgnoreType PhaseMatchErrors()
        {
            if (PhasesDoNotMatch.Count != 0)
            {
                PrismWarnings.Warning = PhasesDoNotMatch.Count.ToString();

                PrismWarnings.PhasesDontMatch();

                ModelModifiers.SetPartsRed(PhasesDoNotMatchParts);

                return PrismWarnings.NewIgnoreWarning();
            }
            return IgnoreType.Unspecified;
        }

        public static IgnoreType StartNumbersErrors()
        {
            if (StartNumbersDoNotMatch.Count != 0)
            {
                PrismWarnings.Warning = StartNumbersDoNotMatch.Count.ToString();

                PrismWarnings.StartNumbersDontMatch();

                ModelModifiers.SetPartsRed(StartNumbersDoNotMatchParts);

                return PrismWarnings.NewIgnoreWarning();
            }
            return IgnoreType.Unspecified;
        }

        public static bool OrderErrors()
        {
            if (NotOrderedParts.Count != 0)
            {
                PrismWarnings.Warning = NotOrderedParts.Count.ToString();

                PrismWarnings.HasNotBeenOrdered();

                ModelModifiers.SetPartsRed(NotOrderedParts);

                return PrismWarnings.IgnoreWarning();
            }
            return true;
        }

        public static bool IntumesecentLoadingErrors()
        {
            if (PartsWithoutIntumescentLoading.Count != 0)
            {
                PrismWarnings.Warning = PartsWithoutIntumescentLoading.Count.ToString();

                PrismWarnings.IntumescentLoadingMissing();

                ModelModifiers.SetPartsRed(PartsWithoutIntumescentLoading);

                return PrismWarnings.IgnoreIntumescentLoading();
            }
            return true;
        }

        public static bool FinishErrors()
        {
            if (HasNoFinish.Count != 0)
            {
                PrismWarnings.Warning = HasNoFinish.Count.ToString();

                PrismWarnings.HasNoFinish();

                ModelModifiers.SetPartsRed(HasNoFinish);

                return PrismWarnings.IgnoreWarning();
            }
            return true;
        }

        public static void GetPartsThatStartNumbersDontMatch(this Part mainPart, Part secondaryPart)
        {
            if (mainPart.AssemblyNumber.StartNumber != secondaryPart.PartNumber.StartNumber)
            {
                ModelPart newPart = new ModelPart(secondaryPart, mainPart.AssemblyNumber.StartNumber);
                StartNumbersDoNotMatchParts.Add(secondaryPart);
                StartNumbersDoNotMatch.Add(newPart);
            }
        }

        public static void GetPartsWherePhasesDontMatch(this Part mainPart, Part secondaryPart)
        {
            mainPart.GetPhase(out Phase mainPartPhase);
            secondaryPart.GetPhase(out Phase secondaryPhase);

            if (mainPartPhase.PhaseNumber != secondaryPhase.PhaseNumber)
            {
                ModelPart p = new ModelPart(secondaryPart, mainPartPhase);
                PhasesDoNotMatch.Add(p);
                PhasesDoNotMatchParts.Add(secondaryPart);
            }
        }

        public static void GetPartsWithoutAFinish(this Part mainPart)
        {
            if (mainPart.Finish.Length == 0)
            {
                HasNoFinish.Add(mainPart);
            }
        }

        public static bool HasBeenOrdered(this Part mainPart, bool skipMessages)
        {
            string prelimMark = "";
            mainPart.GetUserProperty(ModelUDA.CurrentStageName(3), ref prelimMark); //Check prism uda material order complete for data
            if (prelimMark.Length == 0)
            {
                if (skipMessages) { return false; }
            }
            return true;
        }

        public static void GetUnorderedParts(this Part mainPart)
        {
            OrderedParts.Clear();
            NotOrderedParts.Clear();
            string prelimMark = "";
            mainPart.GetUserProperty(ModelUDA.CurrentStageName(3), ref prelimMark); //Check prism uda material order complete for data
            if (prelimMark.Length == 0)
            {
                NotOrderedParts.Add(mainPart);
            }
            else
            {
                OrderedParts.Add(mainPart);
            }
        }

        private static void CheckForIntumescentLoading(this Part mainPart)
        {
            if (mainPart.Finish.StartsWith(GdomValues.IntumescentCode))
            {
                string dft = "";
                string wft = "";
                double dftNum = 0;
                double wftNum = 0;
                mainPart.GetUserProperty(ModelUDA.FireDFT(), ref dft);
                mainPart.GetUserProperty(ModelUDA.FireWFT(), ref wft);
                mainPart.GetUserProperty(ModelUDA.FireDFT(), ref dftNum);
                mainPart.GetUserProperty(ModelUDA.FireWFT(), ref wftNum);
                if ((dft == "" && dftNum == 0) || (wft == "" && wftNum == 0))
                {
                    PartsWithoutIntumescentLoading.Add(mainPart);
                }
            }
        }
    }
}