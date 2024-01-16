using System.Windows.Forms;

namespace Prism.ButtonOperations
{
    public static class DetailButton3
    {
        public static bool DetailButton3op(this SelectedObjects myObjects, PrismProjectData projectData, int stageNumber)
        {
            if (!myObjects.RunStage4Checks(projectData.Full)) { return false; }
            myObjects.SelectedModelParts.SelectParts();
            ModelModifiers.PerformNumbering();

            bool result = PrismWarnings.AreYouHappyWithNumbering();

            if (result)
            {
                myObjects.SelectedModelParts.CreateDrawings();        
            }
            else
            {
                return false ;
            }

            if(!myObjects.SelectedModelParts.ModifyAttributes(stageNumber, projectData)) { return false; }
            ModelModifiers.RedrawViews();            
            int autoFixCount = ModelChecker.PhasesDoNotMatch.Count + ModelChecker.StartNumbersDoNotMatch.Count;
            Logging.LogProgress(projectData.ProjNumberAndName, "Detail 3", autoFixCount, myObjects.AssembliesList.Count);
            return true;
        }
    }
}