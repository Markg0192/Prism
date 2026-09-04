using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Prism.Enums;
using Model = Tekla.Structures.Model.Model;

namespace Prism.ButtonOperations
{
	public static class MaterialButton3
	{
		public static bool MaterialButton3op(this SelectedObjects myObjects, PrismProjectData projectData, ReportManager reportManager, string teklaVersion,
			string orderType, int stageNumber, StageTypes stageType, Model model, string siteDate, ToolStrip toolStrip, ToolStripStatusLabel tssl)
		{

			// HDBolts.StampConnectionCodeOnMainMember(myObjects);

			//First check if the order is for Bolts, Seversafe or HD Bolts.
			bool boltSeversafeAndHdBoltsResult = Order.BoltsSeversafeAndHdBolts(orderType, reportManager, siteDate, model, myObjects, projectData, out bool orderRequired);
			if (orderRequired) return boltSeversafeAndHdBoltsResult;

			int typeOfOrder = 0;

			if (orderType.Contains("Fabsec Carcass"))
			{
				typeOfOrder = PrismWarnings.FabsecCarcassAction();
				//If there are fabsecs present the we need to add the carcass to the selection instead of those in the model space
				if (!FabsecOrderWorker(myObjects, orderType, reportManager, model, projectData, stageType, siteDate, typeOfOrder, teklaVersion, toolStrip, tssl)) return false;
			}

			else
			{
				foreach (PrismPart p in myObjects.PrismParts)
				{
					p.Part.GetUnorderedParts();
				}

				//Check if everything in the selection needs to be ordered/omitted
				if (!ShouldPartsBeOrdered(orderType)) { return false; }
				if (!reportManager.Folders.CreateMatFolder(false)) return false;

				//  if (!Order.Fabsecs(fabsecsPresent, myReportManager, model, projectData, phaseNumber, issueNumber, myObjects, stageNumber, originalFabsecs, fabsecCarcasses)) { return false; }

				if (!myObjects.AddPrelimMarks(projectData, toolStrip, tssl)) return false;

				reportManager.CreateMaterialReports(myObjects, orderType, stageType, toolStrip, tssl);

				myObjects.OmittedParts = MoveOmitMaterial(orderType, myObjects, myObjects.GetFabsecParts(), model);

				if (!FinishOrder(myObjects, stageType, projectData, reportManager.MatReportPrefix, orderType, reportManager, siteDate, toolStrip, tssl, false, myObjects.GetFabsecParts().Count > 0)) { return false; }
			}

			return true;
		}

		public static async Task<bool> MaterialButton3op(this SelectedObjects myObjects, PrismProjectData projectData, ReportManager reportManager, string teklaVersion,
			  string orderType, StageTypes stageType, Model model, string siteDate, Action<int, string> progress)
		{
			//First check if the order is for Bolts, Seversafe or HD Bolts.
			bool boltSeversafeAndHdBoltsResult = Order.BoltsSeversafeAndHdBolts(orderType, reportManager, siteDate, model, myObjects, projectData, out bool orderRequired);
			if (orderRequired) return boltSeversafeAndHdBoltsResult;

			int typeOfOrder = 0;

			if (orderType.Contains("Fabsec Carcass"))
			{
				//If there are fabsecs present the we need to add the carcass to the selection instead of those in the model space
				if (!await FabsecOrderWorker(myObjects, orderType, reportManager, model, projectData, stageType, siteDate, 2, teklaVersion, progress)) return false;
			}

			else
			{
				foreach (PrismPart p in myObjects.PrismParts)
				{
					p.Part.GetUnorderedParts();
				}

				//Check if everything in the selection needs to be ordered/omitted
				if (!ShouldPartsBeOrdered(orderType)) { return false; }
				if (!reportManager.Folders.CreateMatFolder(false)) return false;

				//  if (!Order.Fabsecs(fabsecsPresent, myReportManager, model, projectData, phaseNumber, issueNumber, myObjects, stageNumber, originalFabsecs, fabsecCarcasses)) { return false; }

				if (!myObjects.AddPrelimMarks(projectData)) return false;

				reportManager.CreateMaterialReports(myObjects, orderType, stageType);

				myObjects.OmittedParts = MoveOmitMaterial(orderType, myObjects, myObjects.GetFabsecParts(), model);

				if (!FinishOrder(myObjects, stageType, projectData, reportManager.MatReportPrefix, orderType, reportManager, siteDate, progress,
					false, myObjects.GetFabsecParts().Count > 0)) { return false; }
			}

			return true;
		}

		private static bool FabsecOrderWorker(SelectedObjects myObjects, string orderType, ReportManager myReportManager, Model model,
			PrismProjectData projectData, StageTypes stageType, string siteDate, int typeOfOrder, string teklaVersion,
			ToolStrip toolStrip, ToolStripStatusLabel tssl)
		{
			List<PrismPart> fabsecParts = myObjects.GetFabsecParts();
			bool fabsecsPresent = fabsecParts.Count > 0;
			if (orderType.Contains("Fabsec Carcass") && (!orderType.Contains("Add") || !orderType.Contains("Omit")))
			{
				if (!fabsecsPresent)
				{
					PrismWarnings.NoFabsecsSelected();
					return false;
				}

				if (!FabsecProcessing.CheckFabsecOrderStatusAgainstRequiredActions(fabsecParts, typeOfOrder)) return false;

				bool fabsecCarcassOrdering = Order.FabsecCarcasses(myReportManager, model, projectData, teklaVersion, myObjects, stageType, orderType, siteDate, typeOfOrder, toolStrip, tssl);
				return fabsecCarcassOrdering;
			}
			if (orderType.Contains("Fabsec Carcass") && (orderType.Contains("Add") || orderType.Contains("Omit")))
			{
				PrismWarnings.CannotProcessThisTypeOfOrder();
				return false;
			}
			return true;
		}

		private static async Task<bool> FabsecOrderWorker(SelectedObjects myObjects, string orderType, ReportManager myReportManager, Model model,
			PrismProjectData projectData, StageTypes stageType, string siteDate, int typeOfOrder, string teklaVersion, Action<int, string> progress)
		{
			List<PrismPart> fabsecParts = myObjects.GetFabsecParts();
			bool fabsecsPresent = fabsecParts.Count > 0;
			if (orderType.Contains("Fabsec Carcass") && (!orderType.Contains("Add") || !orderType.Contains("Omit")))
			{
				if (!fabsecsPresent)
				{
					PrismWarnings.NoFabsecsSelected();
					return false;
				}

				if (!FabsecProcessing.CheckFabsecOrderStatusAgainstRequiredActions(fabsecParts, typeOfOrder)) return false;

				bool fabsecCarcassOrdering = await Order.FabsecCarcasses(myReportManager, model, projectData, teklaVersion, myObjects, stageType, orderType, siteDate, typeOfOrder, progress);
				return fabsecCarcassOrdering;
			}
			if (orderType.Contains("Fabsec Carcass") && (orderType.Contains("Add") || orderType.Contains("Omit")))
			{
				PrismWarnings.CannotProcessThisTypeOfOrder();
				return false;
			}
			return true;
		}

		private static bool ShouldPartsBeOrdered(string orderType)
		{
			if (orderType == "Omit Material")
			{
				if (ModelChecker.NotOrderedParts.Count != 0)
				{
					PrismWarnings.HasNotBeenOrderedOMIT();
					return false;
				}
			}
			else if (ModelChecker.OrderedParts.Count != 0)
			{
				PrismWarnings.HasAlreadyBeenOrdered();
				return false;
			}

			return true;
		}

		private static List<PrismPart> MoveOmitMaterial(string orderType, SelectedObjects myObjects, List<PrismPart> originalFabsecs, Model model)
		{
			ModelModifiers.ResetWorkPlane(model);
			List<PrismPart> movedParts = new List<PrismPart>();
			if (orderType == "Omit Material")
			{
				bool keepPartInModel = PrismWarnings.KeepPartInModel();
				movedParts.AddRange(ModelModifiers.MoveAndRenameOmittedMembers2(myObjects.GetNonFabsecParts(), -100000, keepPartInModel, model));
				if (originalFabsecs.Count > 0)
				{
					movedParts.AddRange(ModelModifiers.MoveAndRenameOmittedMembers2(myObjects.GetFabsecParts(), -100000, keepPartInModel, model));
				}
			}
			return movedParts;
		}

		public static bool FinishOrder(SelectedObjects myObjects, StageTypes stageType, PrismProjectData projectData, string matReportPrefix,
		  string orderType, ReportManager reportManager, string siteDate, ToolStrip toolStrip, ToolStripStatusLabel tssl, bool isSpecialFittingOrder = false, bool fabsecsPresent = false)
		{
			if (orderType == "Omit Material")
			{
				if (!myObjects.OmittedParts.ModifyAttributes(StageTypes.Unassigned, projectData, reportManager, toolStrip, tssl, isSpecialFittingOrder)) { return false; }
			}
			else
			{
				if (!myObjects.PrismParts.ModifyAttributes(stageType, projectData, reportManager, toolStrip, tssl, isSpecialFittingOrder)) { return false; }
			}

			if (fabsecsPresent && !orderType.Contains("Omit")) FabsecProcessing.RemoveGreenFromFabsecs(myObjects.GetFabsecParts(), projectData);
			PrismWarnings.MaterialOrderComplete(projectData);

			reportManager.Folders.ZipFolder(reportManager.Folders.MatPath);
			EmailWriter.WriteMatEmail(projectData, myObjects, matReportPrefix, reportManager.IssueNum, reportManager.PhaseNum, orderType, reportManager.Folders.MatPath, fabsecsPresent, siteDate);

			Logging.LogProgress(projectData.ProjNumberAndName, "Material 3", 0, myObjects.GetMainParts().Count);

			return true;
		}

		public static bool FinishOrder(SelectedObjects myObjects, StageTypes stageType, PrismProjectData projectData, string matReportPrefix,
	string orderType, ReportManager reportManager, string siteDate, Action<int, string> progress, bool isSpecialFittingOrder = false, bool fabsecsPresent = false)
		{
			if (orderType == "Omit Material")
			{
				if (!myObjects.OmittedParts.ModifyAttributes(StageTypes.Unassigned, projectData, progress, reportManager, isSpecialFittingOrder)) { return false; }
			}
			else
			{
				if (!myObjects.PrismParts.ModifyAttributes(stageType, projectData, progress, reportManager, isSpecialFittingOrder)) { return false; }
			}

			if (fabsecsPresent && !orderType.Contains("Omit")) FabsecProcessing.RemoveGreenFromFabsecs(myObjects.GetFabsecParts(), projectData);
			PrismWarnings.MaterialOrderComplete(projectData);

			reportManager.Folders.ZipFolder(reportManager.Folders.MatPath);
			EmailWriter.WriteMatEmail(projectData, myObjects, matReportPrefix, reportManager.IssueNum, reportManager.PhaseNum, orderType, reportManager.Folders.MatPath, fabsecsPresent, siteDate);

			return true;
		}
	}
}