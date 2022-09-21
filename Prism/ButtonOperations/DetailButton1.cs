namespace Prism.ButtonOperations
{
    public static class DetailButton1
    {
        public static void DetailButton1op(this SelectedObjects myObjects, PrismProjectData projectData, int stageNumber)
        {
            if (!myObjects.RunStage4Checks()) { return; }

            myObjects.SelectedModelParts.ModifyAttributes(stageNumber, projectData);
            ModelModifiers.RedrawViews();
            int autoFixCount = ModelChecker.PhasesDoNotMatch.Count + ModelChecker.StartNumbersDoNotMatch.Count;
            Logging.LogProgress(projectData.ProjName, "Detail 1", autoFixCount, myObjects.AssembliesList.Count);
        }
    }
}