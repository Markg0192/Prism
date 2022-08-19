namespace Prism.ButtonOperations
{
    public static class DetailButton2
    {
        public static void DetailButton2op(this SelectedObjects myObjects, PrismProjectData projectData, int stageNumber)
        {
            if (!myObjects.RunStage4Checks()) { return; }

            myObjects.SelectedModelParts.ModifyAttributes(stageNumber, projectData);           

        }
    }
}