using System.Windows.Forms;

namespace Prism.ButtonOperations
{
    public static class DetailButton3
    {
        public static bool DetailButton3op(this SelectedObjects myObjects, PrismProjectData projectData, int stageNumber, ToolStrip toolStrip, ToolStripStatusLabel tssl)
        {
            if (!myObjects.RunStage4Checks(projectData.Full)) { return false; }
            myObjects.PrismParts.SelectParts();
            if (!ModelModifiers.PerformNumbering()) return false;
                   
            myObjects.PrismParts.CreateDrawings();    
            
            if(!myObjects.PrismParts.ModifyAttributes(stageNumber, projectData, toolStrip, tssl)) { return false; }
            ModelModifiers.RedrawViews();            
            int autoFixCount = ModelChecker.PhasesDontMatch + ModelChecker.StartNumbersDontMatch;
            Logging.LogProgress(projectData.ProjNumberAndName, "Detail 3", autoFixCount, myObjects.GetMainParts().Count);
            return true;
        }
    }
}