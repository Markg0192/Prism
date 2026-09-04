using iText.Commons.Actions.Data;
using Org.BouncyCastle.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Tekla.Structures.Model;
using static Prism.Enums;

namespace Prism.ButtonOperations
{
	public static class DetailButton1
	{
		public static bool DetailButton1op(this SelectedObjects myObjects, PrismProjectData projectData, StageTypes stageType, ToolStrip toolStrip, ToolStripStatusLabel tssl)
		{
			if (!myObjects.RunStage4Checks(projectData.Full)) { return false; }
			if (!ModelChecker.NameAndClassAlign(myObjects)) { return false; }
			if (!ModelChecker.PartsHaveExecutionClass(myObjects)) { return false; }

			if (!myObjects.PrismParts.ModifyAttributes(stageType, projectData, null, toolStrip, tssl)) return false; ;
			ModelModifiers.RedrawViews();
			int autoFixCount = ModelChecker.PhasesDontMatch + ModelChecker.StartNumbersDontMatch;
			Logging.LogProgress(projectData.ProjNumberAndName, "Detail 1", autoFixCount, myObjects.GetMainParts().Count);
			return true;
		}

		public static bool RunDrawingStageOneChecks(this SelectedObjects selectedObjects, Action<int, string> progress)
		{
			List<PrismPart> parts = selectedObjects.GetNonSeversafeParts().Where(part => part.IsMainPart).ToList();
			int totalParts = parts.Count;

			for (int i = 0; i < totalParts; i++)
			{
				PrismPart prismPart = parts[i];

				int percentage = 25 + (int)(((i + 1) / (double)totalParts) * 30);

				progress?.Invoke(
					percentage,
					"Checking part " + (i + 1) + " of " + totalParts + " - name and class...");

				ModelChecker.NameAndClassAligned(prismPart);

				if (prismPart.IsMainPart)
				{
					progress?.Invoke(
						percentage,
						"Checking part " + (i + 1) + " of " + totalParts + " - order status...");

					ModelChecker.CheckPartsAreOrdered(prismPart);

					progress?.Invoke(
						percentage,
						"Checking part " + (i + 1) + " of " + totalParts + " - execution class...");

					ModelChecker.HasExecutionClass(prismPart);
				}
			}

			int totalErrors = selectedObjects.PrismParts.Sum(part => part.PartErrors.Count);

			return totalErrors == 0;
		}

		public static bool RunDrawingStageTwoChecks(this SelectedObjects selectedObjects, Action<int, string> progress)
		{
			List<PrismPart> mainParts = selectedObjects.GetNonSeversafeParts().Where(part => part.IsMainPart).ToList();
			int totalParts = mainParts.Count;

			for (int i = 0; i < totalParts; i++)
			{
				PrismPart mainPart = mainParts[i];
				int percentage = 60 + (int)(((i + 1) / (double)totalParts) * 30);

				progress?.Invoke(
					percentage,
					"Checking main part " + (i + 1) + " of " + totalParts + " - finish...");

				ModelChecker.GetPartsWithoutAFinish(mainPart);

				progress?.Invoke(
					percentage,
					"Checking main part " + (i + 1) + " of " + totalParts + " - intumescent loading...");

				ModelChecker.CheckForIntumescentLoading(mainPart);

				ArrayList secondaries = mainPart.Part.GetAssembly().GetSecondaries();
				int totalSecondaries = secondaries.Count;

				for (int j = 0; j < totalSecondaries; j++)
				{
					Part secondaryPart = secondaries[j] as Part;

					if (secondaryPart == null)
					{
						continue;
					}

					progress?.Invoke(
						percentage,
						"Checking main part " + (i + 1) + " of " + totalParts +
						", secondary " + (j + 1) + " of " + totalSecondaries +
						" - numbering...");

					ModelChecker.GetPartsThatStartNumbersDontMatch(mainPart, secondaryPart, selectedObjects);

					progress?.Invoke(
						percentage,
						"Checking main part " + (i + 1) + " of " + totalParts +
						", secondary " + (j + 1) + " of " + totalSecondaries +
						" - phasing...");

					ModelChecker.GetPartsWherePhasesDontMatch(mainPart, secondaryPart, selectedObjects);
				}
			}

			int totalErrors = selectedObjects.PrismParts.Sum(part => part.PartErrors.Count);

			if (totalErrors > 0)
			{
				return false;
			}		

			return true;
		}
	}
}