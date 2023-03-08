namespace Prism.ButtonOperations
{
    public static class DetailButton2
    {
        public static void DetailButton2op(this SelectedObjects myObjects, PrismProjectData projectData, int stageNumber, string columnOrientationType, string flangeThickness)
        {
            if (!myObjects.RunStage4Checks()) { return; }

            ColumnOrientation.DetailColumnOrientationHoles(myObjects, columnOrientationType, flangeThickness);

            myObjects.SelectedModelParts.ModifyAttributes(stageNumber, projectData);
            ModelModifiers.RedrawViews();
            int autoFixCount = ModelChecker.PhasesDoNotMatch.Count + ModelChecker.StartNumbersDoNotMatch.Count;
            Logging.LogProgress(projectData.ProjName, "Detail 2", autoFixCount, myObjects.AssembliesList.Count);
        }
    }
}