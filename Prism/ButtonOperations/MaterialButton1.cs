namespace Prism.ButtonOperations
{
    public static class MaterialButton1
    {
        public static void MaterialButton1op(this SelectedObjects myObjects, PrismProjectData projectData, int stageNumber)
        {
            if (myObjects.HasExecutionClass() && myObjects.NameAndClassAligned())
            {
                myObjects.SelectedModelParts.ModifyAttributes(stageNumber, projectData);
            }
        }
    }
}