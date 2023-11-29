using static Prism.Enums;

namespace Prism.ButtonOperations
{
    public static class MaterialButton1
    {
        public static bool MaterialButton1op(this SelectedObjects myObjects, PrismProjectData projectData, int stageNumber)
        {
            if (!ModelChecker.NameAndClassAign(myObjects)) { return false; }
            if (!ModelChecker.PartsHaveExecutionClass(myObjects)) { return false; }

            ModelModifiers.SelectParts(myObjects.SelectedModelParts);
            if (!myObjects.SelectedModelParts.ModifyAttributes(stageNumber, projectData)) { return false; }
            Logging.LogProgress(projectData.ProjNumberAndName, "Material 1", ModelChecker.IncorrectNameAndClass.Count + ModelChecker.MissingExecutionClass.Count, myObjects.AssembliesList.Count);
            return true;

        }
    }
}