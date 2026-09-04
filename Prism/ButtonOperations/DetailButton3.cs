using iText.Commons.Actions.Data;
using Org.BouncyCastle.Utilities;
using System;
using System.Windows.Forms;
using static Prism.Enums;

namespace Prism.ButtonOperations
{
    public static class DetailButton3
    {
        public static bool DetailButton3op(this SelectedObjects myObjects, PrismProjectData projectData, StageTypes stageType, 
            ToolStrip toolStrip, ToolStripStatusLabel tssl)
        {
            if (!myObjects.RunStage4Checks(projectData.Full)) { return false; }
            myObjects.PrismParts.SelectParts();
            if (!ModelModifiers.PerformNumbering()) return false;
                   
            myObjects.PrismParts.CreateDrawings();    
            
            if(!myObjects.PrismParts.ModifyAttributes(stageType, projectData, null, toolStrip, tssl)) { return false; }
            ModelModifiers.RedrawViews();            
            int autoFixCount = ModelChecker.PhasesDontMatch + ModelChecker.StartNumbersDontMatch;
            Logging.LogProgress(projectData.ProjNumberAndName, "Detail 3", autoFixCount, myObjects.GetMainParts().Count);
            return true;
        }

        public static bool CreateMyDrawings(this SelectedObjects myObjects, StageTypes stageType, PrismProjectData projectData, Action<int, string> progress)
        {
			myObjects.GetNonSeversafeParts().SelectParts();
			if (!ModelModifiers.PerformNewNumbering()) return false;

			myObjects.PrismParts.CreateDrawings();

			if (!myObjects.PrismParts.ModifyAttributes(stageType, projectData, progress, null)) { return false; }

			return true;
		}
    }
}