namespace Prism.ButtonOperations
{
    public static class DetailButton2
    {
        public static bool DetailButton2op(this SelectedObjects myObjects, PrismProjectData projectData, int stageNumber, string columnOrientationType, string flangeThickness)
        {
            if (!myObjects.RunStage4Checks()) { return false; }

            ColumnOrientation.DetailColumnOrientationHoles(myObjects, columnOrientationType, flangeThickness);

            if(!myObjects.SelectedModelParts.ModifyAttributes(stageNumber, projectData)) { return false; }
            ModelModifiers.RedrawViews();
            int autoFixCount = ModelChecker.PhasesDoNotMatch.Count + ModelChecker.StartNumbersDoNotMatch.Count;
            Logging.LogProgress(projectData.ProjNumberAndName, "Detail 2", autoFixCount, myObjects.AssembliesList.Count, projectData.WebService);
            return true;
        }
    }
}