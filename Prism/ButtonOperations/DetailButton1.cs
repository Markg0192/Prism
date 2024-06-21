using System.Windows.Forms;

namespace Prism.ButtonOperations
{
    public static class DetailButton1
    {
        public static bool DetailButton1op(this SelectedObjects myObjects, PrismProjectData projectData, int stageNumber, ToolStrip toolStrip, ToolStripStatusLabel tssl)
        {
            if (!myObjects.RunStage4Checks(projectData.Full)) { return false; }
            if (!ModelChecker.NameAndClassAign(myObjects)) { return false; }
            if (!ModelChecker.PartsHaveExecutionClass(myObjects)) { return false; }

            if (!myObjects.PrismParts.ModifyAttributes(stageNumber, projectData, toolStrip, tssl)) return false; ;
            ModelModifiers.RedrawViews();
            int autoFixCount = ModelChecker.PhasesDoNotMatch.Count + ModelChecker.StartNumbersDoNotMatch.Count;
            Logging.LogProgress(projectData.ProjNumberAndName, "Detail 1", autoFixCount, myObjects.GetMainParts().Count);
            return true;
        }
    }
}