using System.Collections.Generic;
using Tekla.Structures.Model;

namespace Prism
{
    public static class AutoFix
    {
        public static void PartNameAndClass()
        {
            foreach(Part p in ModelChecker.IncorrectNameAndClass)
            {
                List<string> myClass = GdomValues.PartClass()[p.Name] as List<string>;
                p.Class = myClass[0];
                p.Modify();
            }
            PrismWarnings.ErrorsFixed(ModelChecker.IncorrectNameAndClass.Count);
        }

        public static void ExecutionClass()
        {
            int myExcClass = PrismWarnings.ExecutionClassWarning();
            foreach (Part p in ModelChecker.MissingExecutionClass)
            {
                p.SetUserProperty(ModelUDA.ExcecutionClass(), myExcClass);
                p.Modify();
            }
            PrismWarnings.ErrorsFixed(ModelChecker.MissingExecutionClass.Count);
        }

        public static void AssemblyAndStartNumbers()
        {
            foreach (ModelPart p in ModelChecker.StartNumbersDoNotMatch)
            {            
                p.Part.AssemblyNumber.StartNumber = p.StartNumber;
                p.Part.PartNumber.StartNumber = p.StartNumber;
                p.Part.Modify();
            }
            PrismWarnings.ErrorsFixed(ModelChecker.StartNumbersDoNotMatch.Count);
        }

        public static void PartPhasing()
        {
            foreach (ModelPart p in ModelChecker.PhasesDoNotMatch)
            {
                p.Part.SetPhase(p.Phase);
                p.Part.Modify();
            }
            PrismWarnings.ErrorsFixed(ModelChecker.PhasesDoNotMatch.Count);
        }
    }
}