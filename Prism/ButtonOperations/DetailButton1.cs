namespace Prism.ButtonOperations
{
    public static class DetailButton1
    {
        public static void DetailButton1op(this SelectedObjects myObjects, PrismProjectData projectData, int stageNumber)
        {            
           if(!myObjects.RunStage4Checks()) { return; }

            myObjects.SelectedModelParts.ModifyAttributes(stageNumber, projectData);
        }
    }
}