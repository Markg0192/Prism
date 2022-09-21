using Tekla.Structures.Model;

namespace Prism.ButtonOperations
{
    public static class MaterialButton2
    {
        public static void MaterialButton2op(this SelectedObjects myObjects, string startNumber, int stageNumber, PrismProjectData projectData, Model model)
        {
            myObjects.ProcessFabsecs(model, projectData); 
            myObjects.AddStartNumbers(startNumber);
            myObjects.SelectedModelParts.ModifyAttributes(stageNumber, projectData);
            Logging.LogProgress(projectData.ProjName, "Material 2", 0, myObjects.AssembliesList.Count);
        }
    }
}