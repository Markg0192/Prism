using Tekla.Structures.Model;

namespace Prism.ButtonOperations
{
    public static class MaterialButton2
    {
        public static void MaterialButton2op(this SelectedObjects myObjects, string startNumber, int stageNumber, PrismProjectData projectData, Model model)
        {
            myObjects.ProcessFabsecs(model); 
            myObjects.AddStartNumbers(startNumber);
            myObjects.ModifyAttributes(stageNumber, projectData);
        }
    }
}