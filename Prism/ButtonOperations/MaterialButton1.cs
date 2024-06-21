using System.Windows.Forms;
using static Prism.Enums;

namespace Prism.ButtonOperations
{
    public static class MaterialButton1
    {
        public static bool MaterialButton1op(this SelectedObjects myObjects, PrismProjectData projectData, int stageNumber, ToolStrip toolStrip, ToolStripStatusLabel tssl)
        {
            if (!ModelChecker.NameAndClassAign(myObjects)) { return false; }
            if (!ModelChecker.PartsHaveExecutionClass(myObjects)) { return false; }

            myObjects.PrismParts.SelectParts();
            if (!myObjects.PrismParts.ModifyAttributes(stageNumber, projectData, toolStrip, tssl)) { return false; }
            Logging.LogProgress(projectData.ProjNumberAndName, "Material 1", ModelChecker.IncorrectNameAndClass.Count + ModelChecker.MissingExecutionClass.Count, myObjects.GetMainParts().Count);
            return true;

        }
    }
}