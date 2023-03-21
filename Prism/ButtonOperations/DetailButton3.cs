using System.Windows.Forms;

namespace Prism.ButtonOperations
{
    public static class DetailButton3
    {
        public static bool DetailButton3op(this SelectedObjects myObjects, PrismProjectData projectData, int stageNumber)
        {
            if (!myObjects.RunStage4Checks()) { return false; }
            myObjects.SelectedModelParts.SelectParts();
            ModelModifiers.PerformNumbering();

            DialogResult result = PrismWarnings.AreYouHappyWithNumbering();

            if (result == DialogResult.Yes)
            {
                myObjects.CreateDrawings();                
            }
            else
            {
                return false ;
            }

            myObjects.SelectedModelParts.ModifyAttributes(stageNumber, projectData);
            ModelModifiers.RedrawViews();            
            int autoFixCount = ModelChecker.PhasesDoNotMatch.Count + ModelChecker.StartNumbersDoNotMatch.Count;
            Logging.LogProgress(projectData.ProjName, "Detail 3", autoFixCount, myObjects.AssembliesList.Count);
            return true;
        }
    }
}