using static Prism.Enums;

namespace Prism.ButtonOperations
{
    public static class MaterialButton1
    {
        public static bool MaterialButton1op(this SelectedObjects myObjects, PrismProjectData projectData, int stageNumber)
        {
            if (!NameAndClassAign(myObjects)) { return false; }
            if (PartsHaveExecutionClass(myObjects))
            {
                ModelModifiers.SelectParts(myObjects.SelectedModelParts);
                myObjects.SelectedModelParts.ModifyAttributes(stageNumber, projectData);
                Logging.LogProgress(projectData.ProjName, "Material 1", ModelChecker.IncorrectNameAndClass.Count + ModelChecker.MissingExecutionClass.Count, myObjects.AssembliesList.Count);
                return true;
            }
            return false;
        }

        private static bool NameAndClassAign(SelectedObjects myObjects)
        {
            ModelChecker.IncorrectNameAndClass.Clear();
            ModelChecker.NameAndClassAligned(myObjects);
            IgnoreType ignore = PrismWarnings.DisplayOrderErrors(ModelChecker.IncorrectNameAndClass, Error.NameAndClass);

            if (ignore == IgnoreType.AutoFix)
            {
                AutoFix.PartNameAndClass();
            }
            if (ignore == IgnoreType.Stop)
            {
                return false;
            }
            return true;
        }

        private static bool PartsHaveExecutionClass(SelectedObjects myObjects)
        {
            ModelChecker.MissingExecutionClass.Clear();
            ModelChecker.HasExecutionClass(myObjects);
            IgnoreType ignore = PrismWarnings.DisplayOrderErrors(ModelChecker.MissingExecutionClass, Error.Execution);

            if (ignore == IgnoreType.AutoFix)
            {
                AutoFix.ExecutionClass();
            }
            if (ignore == IgnoreType.Stop)
            {
                return false;
            }
            return true;
        }       
    }
}