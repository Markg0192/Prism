using iText.Commons.Actions.Data;
using Microsoft.Office.Interop.Excel;
using System.Windows.Forms;
using Tekla.Structures.Model;
using static Prism.Enums;
using Model = Tekla.Structures.Model.Model;

namespace Prism.ButtonOperations
{
	public static class MaterialButton2
	{
		public static bool MaterialButton2op(this SelectedObjects myObjects, string startNumber, StageTypes stageType, PrismProjectData projectData, Model model, ToolStrip toolStrip, ToolStripStatusLabel tssl)
		{
			//HDBolts.StampConnectionCodeOnMainMember(myObjects);
			if (ModelChecker.MemberOrientationIsCorrect(myObjects, out IgnoreType ignore))
			{
				if (!FabsecProcessing.PrepFabsecCarcassesForMaterialOrder(myObjects, projectData, model, startNumber)) return false;
				// if (!myObjects.ProcessFabsecs(model, projectData)) { return false; }

				myObjects.GetNonFabsecParts().AddStartNumbers(startNumber);

				if (!myObjects.PrismParts.ModifyAttributes(stageType, projectData, null, toolStrip, tssl)) { return false; }

				if (ignore == IgnoreType.AutoFix)
				{
					AutoFix.MemberOrientation(myObjects);
				}

				model.CommitChanges();
				Logging.LogProgress(projectData.ProjNumberAndName, "Material 2", 0, myObjects.GetMainParts().Count);
				return true;
			}
			return false;
		}

		public static bool PrepareFabsecs(this SelectedObjects myObjects, int stageNumber, PrismProjectData projectData, Model model)
		{
			if (!FabsecProcessing.PrepFabsecCarcassesForMaterialOrder(myObjects, projectData, model)) return false;

			model.CommitChanges();

			return true;
		}

		public static bool CreateFabsecCarcasses(this SelectedObjects myObjects, PrismProjectData projectData, Model model)
		{
			return FabsecProcessing.CreateFabsecCarcasses(myObjects, projectData, model);
		}
	}
}