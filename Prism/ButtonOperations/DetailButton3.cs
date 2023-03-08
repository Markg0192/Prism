using System.Windows.Forms;

namespace Prism.ButtonOperations
{
    public static class DetailButton3
    {
        public static string DetailButton3op(this SelectedObjects myObjects, PrismProjectData projectData, int stageNumber)
        {
            if (!myObjects.RunStage4Checks()) { return "Cancelled"; }
            myObjects.SelectedModelParts.SelectParts();
            ModelModifiers.PerformNumbering();

            const string notUpToDateMessage = "Are you happy with your numbering?";
            const string notUpToDateTitle = "Numbering";
            DialogResult result = MessageBox.Show(notUpToDateMessage, notUpToDateTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                myObjects.CreateDrawings();                
            }
            else
            {
                return "Cancelled";
            }

            myObjects.SelectedModelParts.ModifyAttributes(stageNumber, projectData);
            ModelModifiers.RedrawViews();            
            int autoFixCount = ModelChecker.PhasesDoNotMatch.Count + ModelChecker.StartNumbersDoNotMatch.Count;
            Logging.LogProgress(projectData.ProjName, "Detail 3", autoFixCount, myObjects.AssembliesList.Count);
            return "Complete";
        }
    }
}