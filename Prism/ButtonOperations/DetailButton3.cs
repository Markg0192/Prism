using iText.Commons.Actions;
using iText.Commons.Actions.Data;
using Org.BouncyCastle.Utilities;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
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

			myObjects.AssignDrawingClassifications();

			if (!myObjects.PrismParts.ModifyAttributes(stageType, projectData, progress, null)) { return false; }

			return true;
		}

		private static void AssignDrawingClassifications(this SelectedObjects selectedObjects)
		{
			foreach (var part in selectedObjects.GetMainParts())
			{
				part.DrawingClassification = GetAssemblyClassification(part);
			}

			foreach (var part in selectedObjects.GetSecondaryParts())
			{
				part.DrawingClassification = GetFittingClassification(part);
			}
		}

		private static DrawingClassification GetAssemblyClassification(PrismPart part)
		{
			int fittingsCount = part.Assembly.GetSecondaries().Count;

			if (part.IsAbnormal)
				return DrawingClassification.ASS5;

			if (part.PartPrefix == "HR"
				|| part.AssemblyPrefix == "HR"
				|| part.AssemblyPrefix == "AS"
				|| part.Finish.EndsWith("M")
				|| fittingsCount > 12)
				return DrawingClassification.ASS4;

			if (part.Finish.StartsWith("G")
				|| (fittingsCount >= 4 && fittingsCount <= 12))
				return DrawingClassification.ASS3;

			if (fittingsCount > 0)
				return DrawingClassification.ASS2;

			return DrawingClassification.ASS1;
		}

		private static DrawingClassification GetFittingClassification(PrismPart part)
		{
			switch (part.PartPrefix)
			{
				case "H":
				case "DMP":
				case "MP":
					return DrawingClassification.FIT3;

				case "DP":
				case "BO":
				case "C":
				case "PP":
					return DrawingClassification.FIT2;

				default:
					return DrawingClassification.FIT1;
			}
		}
	}
}