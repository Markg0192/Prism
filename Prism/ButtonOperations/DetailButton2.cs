using System.Windows.Forms;

namespace Prism.ButtonOperations
{
    public static class DetailButton2
    {
        public static bool DetailButton2op(this SelectedObjects myObjects, PrismProjectData projectData, int stageNumber, string columnOrientationType, string flangeThickness,
            ToolStrip toolStrip, ToolStripStatusLabel tssl)
        {
            if (!myObjects.RunStage4Checks(projectData.Full)) { return false; }

            ColumnOrientation.DetailColumnOrientationHoles(myObjects, columnOrientationType, flangeThickness);

            if(!myObjects.PrismParts.ModifyAttributes(stageNumber, projectData, toolStrip, tssl)) { return false; }
            ModelModifiers.RedrawViews();
            int autoFixCount = ModelChecker.PhasesDontMatch + ModelChecker.StartNumbersDontMatch;
            Logging.LogProgress(projectData.ProjNumberAndName, "Detail 2", autoFixCount, myObjects.GetMainParts().Count);
            return true;
        }
    }
}