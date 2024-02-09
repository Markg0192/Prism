namespace Prism.ButtonOperations
{
    public static class DetailButton1
    {
        public static bool DetailButton1op(this SelectedObjects myObjects, PrismProjectData projectData, int stageNumber)
        {
            if (!myObjects.RunStage4Checks(projectData.Full)) { return false; }
            if (!ModelChecker.NameAndClassAign(myObjects)) { return false; }
            if (!ModelChecker.PartsHaveExecutionClass(myObjects)) { return false; }

            if (!myObjects.SelectedModelParts.ModifyAttributes(stageNumber, projectData)) return false; ;
            ModelModifiers.RedrawViews();
            int autoFixCount = ModelChecker.PhasesDoNotMatch.Count + ModelChecker.StartNumbersDoNotMatch.Count;
            Logging.LogProgress(projectData.ProjNumberAndName, "Detail 1", autoFixCount, myObjects.AssembliesList.Count);
            return true;
        }
    }
}