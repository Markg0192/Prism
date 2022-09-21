using System.Collections.Generic;
using Tekla.Structures.Model;
using Tekla.Structures.Model.UI;
using static Prism.Enums;
using static Prism.IgnoreWarning;

namespace Prism.ButtonOperations
{
    public static class MaterialButton1
    {
        public static bool MaterialButton1op(this SelectedObjects myObjects, PrismProjectData projectData, int stageNumber)
        {
            if (!NameAndClassAign(myObjects)) { return false; }            
            if (PartsHaveExecutionClass(myObjects))
            {
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
            IgnoreType ignore = DisplayOrderErrors(ModelChecker.IncorrectNameAndClass, "NameAndClass");

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
            IgnoreType ignore = DisplayOrderErrors(ModelChecker.MissingExecutionClass, "Exc");

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

        public static IgnoreType DisplayOrderErrors(List<ModelObject> errorList, string warning)
        {
            if (errorList.Count != 0)
            {
                if (warning == "NameAndClass")
                {
                    PrismWarnings.NameAndClassDontMatch();
                }
                else
                {
                    PrismWarnings.ExecutionClassMissing();
                }
                ModelObjectVisualization.SetTransparencyForAll(TemporaryTransparency.SEMITRANSPARENT);
                ModelObjectVisualization.SetTemporaryStateForAll(new Color(0.5, 0.5, 0.5));

                ModelObjectVisualization.SetTemporaryState(errorList, new Color(1, 0, 0));
                return PrismWarnings.NewIgnoreWarning();
            }
            return IgnoreType.Unspecified;
        }
    }
}