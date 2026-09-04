using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using static Prism.Enums;

namespace Prism.ButtonOperations
{
	public static class MaterialButton1
	{
		public static bool MaterialButton1op(this SelectedObjects myObjects, PrismProjectData projectData, StageTypes stageType, ToolStrip toolStrip, ToolStripStatusLabel tssl)
		{
			if (!ModelChecker.NameAndClassAlign(myObjects)) { return false; }
			if (!ModelChecker.PartsHaveExecutionClass(myObjects)) { return false; }

			myObjects.PrismParts.SelectParts();
			if (!myObjects.PrismParts.ModifyAttributes(stageType, projectData, null, toolStrip, tssl)) { return false; }
			Logging.LogProgress(projectData.ProjNumberAndName, "Material 1", ModelChecker.IncorrectNameAndClass.Count + ModelChecker.MissingExecutionClass.Count, 
				myObjects.GetMainParts().Count);
			return true;
		}

		public static bool MaterialCheckbutton(this SelectedObjects myObjects, PrismProjectData projectData, int startNumber, Action<int, string> progress)
		{
			int totalParts = myObjects.PrismParts.Count;

			for (int i = 0; i < totalParts; i++)
			{
				PrismPart prismPart = myObjects.PrismParts[i];

				int percentage = 25 + (int)(((i + 1) / (double)totalParts) * 35);

				progress?.Invoke(percentage, "Checking part " + (i + 1) + " of " + totalParts + " - name and class...");

				ModelChecker.NameAndClassAligned(prismPart);

				if (prismPart.IsMainPart)
				{
					progress?.Invoke(percentage, "Checking part " + (i + 1) + " of " + totalParts + " - execution class...");

					ModelChecker.HasExecutionClass(prismPart);

					progress?.Invoke(percentage, "Checking part " + (i + 1) + " of " + totalParts + " - orientation...");

					ModelChecker.MemberOrientation(prismPart);
				}
			}

			progress?.Invoke(62, "Adding start numbers...");

			myObjects.GetNonFabsecParts().AddStartNumbers(startNumber);

			FabsecProcessing.AddStartNumberToFabsecUda(myObjects.GetFabsecParts(), startNumber, progress);

			progress?.Invoke(72, "Updating material attributes...");

			int totalErrors = myObjects.PrismParts.Sum(part => part.PartErrors.Count);

			if (totalErrors == 0)
			{
				if (!myObjects.PrismParts.ModifyAttributes(StageTypes.Prelim1, projectData, progress, null))
				{
					return false;
				}
				if (!myObjects.PrismParts.ModifyAttributes(StageTypes.Prelim2, projectData, progress, null))
				{
					return false;
				}
			}

			progress?.Invoke(90, "Finshing...");

			return true;
		}
	}
}