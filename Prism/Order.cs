using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Prism.Enums;
using Model = Tekla.Structures.Model.Model;

namespace Prism
{
	public static class Order 
	{ 
		public static void ShearStuds(string orderType, ReportManager myReportManager, string siteDate)
		{
			OrderBolts(orderType, myReportManager, siteDate, null);
		}

		public static bool BoltsSeversafeAndHdBolts(string orderType, ReportManager myReportManager, string siteDate, Model model, SelectedObjects myObjects, PrismProjectData projectData, out bool orderRequired)
		{
			orderRequired = false;
			if (orderType.Contains("Bolts"))
			{
				orderRequired = true;
				return OrderBolts(orderType, myReportManager, siteDate, myObjects);
			}

			if (orderType.Contains("Seversafe"))
			{
				orderRequired = true;
				return OrderSeversafe(model, myObjects, siteDate, myReportManager, projectData);
			}

			if (orderType == "Order HD Bolts") //Order HD bolts not an option therefore this statement is never true(for now) 
			{
				orderRequired = true;
				return OrderHoldingDownBolts(myObjects, myReportManager, projectData.ProjNumberAndName); //Doesn't do anything..
			}
			return true;
		}

		public static bool FabsecCarcasses(ReportManager myReportManager, Model model, PrismProjectData projectData, string teklaVersion,
			SelectedObjects myObjects, StageTypes stageType, string orderType, string orderDate, int typeOfOrder, ToolStrip toolStrip, ToolStripStatusLabel tssl)
		{
			if (typeOfOrder == 1)
			{
				return FabsecProcessing.CreateFabsecCarcasses(myObjects, projectData, model);
			}
			if (typeOfOrder == 2)
			{
				if (!FabsecProcessing.AddCarcassToSelection(model, myObjects, out List<PrismPart> originalFabsecs, out List<PrismPart> fabsecCarcasses)) return false;
				return OrderFabsecCarcasses(myReportManager, model, projectData, teklaVersion, myObjects, stageType, originalFabsecs, fabsecCarcasses, orderType, orderDate, toolStrip, tssl);
			}
			return false;
		}

		public static async Task<bool> FabsecCarcasses(ReportManager myReportManager, Model model, PrismProjectData projectData, string teklaVersion,
		SelectedObjects myObjects, StageTypes stageType, string orderType, string orderDate, int typeOfOrder, Action<int, string> progress )
		{
			if (typeOfOrder == 1)
			{
				return FabsecProcessing.CreateFabsecCarcasses(myObjects, projectData, model);
			}
			if (typeOfOrder == 2)
			{
				if (!FabsecProcessing.AddCarcassToSelection(model, myObjects, out List<PrismPart> originalFabsecs, out List<PrismPart> fabsecCarcasses)) return false;
				return await OrderFabsecCarcasses(myReportManager, model, projectData, teklaVersion, myObjects, stageType, originalFabsecs, fabsecCarcasses, orderType, orderDate, progress);
			}
			return false;
		}

		private static bool OrderSeversafe(Model model, SelectedObjects myObjects, string siteDate, ReportManager myReportManager, PrismProjectData projectData)
		{
			SeversafeOrder.CreateSeversafeOrder(model, myObjects.GetSeversafeParts(), siteDate, myReportManager, 1, myReportManager.EpoReportPrefix, projectData);
			return true;
		}

		private static bool OrderBolts(string orderType, ReportManager reportManager, string siteDate, SelectedObjects selectedObjects)
		{
			int typeOfOrder = PrismWarnings.BoltOrderType();

			if (!reportManager.Folders.CreateBoltFolder()) return false;

			if (typeOfOrder == 1) { reportManager.CreateSelectedBoltList(reportManager.BoltReportPrefix, orderType); }
			else reportManager.CreateBoltList(reportManager.BoltReportPrefix, orderType);

			List<PrismBoltGroup> boltGroups = selectedObjects != null && selectedObjects.PrismBoltGroups != null
				? selectedObjects.PrismBoltGroups.Where(bolt => bolt != null && !bolt.isShearStud).ToList()
				: new List<PrismBoltGroup>();

			int shopBoltCount = boltGroups.Where(bolt => bolt.isShop && bolt.BoltGroup != null).Sum(bolt => bolt.BoltGroup.BoltPositions.Count);
			int siteBoltCount = boltGroups.Where(bolt => !bolt.isShop && bolt.BoltGroup != null).Sum(bolt => bolt.BoltGroup.BoltPositions.Count);

			bool zipFileCanBeAttached = reportManager.Folders.ZipFolder(reportManager.Folders.BoltPath);
			EmailWriter.WriteBoltOrderEmail(reportManager.ProjectData, reportManager.BoltReportPrefix, reportManager.IssueNum, reportManager.PhaseNum, siteDate,
				reportManager.Folders.BoltPath, shopBoltCount, siteBoltCount, zipFileCanBeAttached);
			return true;
		}

		public static bool OrderFabsecCarcasses(ReportManager myReportManager, Model model, PrismProjectData projectData, string teklaVersion,
		 SelectedObjects myObjects, StageTypes stageType, List<PrismPart> originalFabsecs, List<PrismPart> fabsecCarcasses, string orderType, string orderDate, ToolStrip toolStrip, ToolStripStatusLabel tssl)
		{
			if (!myReportManager.Folders.CreateFabsecCarcassFolder()) return false;

			DrawingManager dm = DrawingManager.Create(model, projectData, fabsecCarcasses, myReportManager.PhaseNum, myReportManager.IssueNum, myReportManager.Folders.CarcassOrderPath, toolStrip, tssl);
			if (dm == null) return false;

			if (dm.GetDrawingFolder(Enums.DrawingFolder.Default).Count != 0) { PrismWarnings.IncorrectlyAssignedDrawings(); return false; }

			List<int> drawingCount = new List<int> { 0, dm.GetDrawingFolder(Enums.DrawingFolder.PGC).Count };
			DrawingManager.PrintAndIssueDrawings(myReportManager.Folders.FabsecCarcassFolder, myReportManager.Folders.CarcassOrderPath, drawingCount, "\\PGC", 0, 1, myReportManager, false, teklaVersion, toolStrip, tssl);
			PrismMacroBuilder.ClearPrintDialog();

			fabsecCarcasses.SelectParts();
			myReportManager.CreateG2Assy();
			BswxExporter.ExportBSWX(myObjects, myReportManager.Folders.CarcassOrderPath, projectData, myReportManager.PhaseNum, myReportManager.IssueNum, Enums.StageTypes.Prelim3, toolStrip, tssl);
			ModelModifiers.RemoveLog(myReportManager.Folders.CarcassOrderPath);

			myObjects.PrismParts.SelectParts();

			if (!originalFabsecs.ModifyAttributes(stageType, projectData, myReportManager, toolStrip, tssl)) { return false; }
			foreach (PrismPart fabsec in originalFabsecs)
			{
				ModelModifiers.ModifyUDA(fabsec.Part, ModelUDA.FabsecCarcassOrdered(), projectData.Date);
			}
			model.CommitChanges();

			bool zipFileCanBeAttached = myReportManager.Folders.ZipFolder(myReportManager.Folders.CarcassOrderPath);

			PrismWarnings.MaterialOrderComplete(projectData);

			EmailWriter.WriteFabsecCarcassEmail(myReportManager.ProjectData, originalFabsecs, myReportManager.CarcassReportPrefix, myReportManager.IssueNum, myReportManager.PhaseNum, orderDate, myReportManager.Folders.CarcassOrderPath, zipFileCanBeAttached);

			return true;
		}

		public static async Task<bool> OrderFabsecCarcasses(ReportManager myReportManager, Model model, PrismProjectData projectData, string teklaVersion,
 SelectedObjects myObjects, StageTypes stageType, List<PrismPart> originalFabsecs, List<PrismPart> fabsecCarcasses, string orderType, string orderDate, Action<int, string> progress)
		{
			if (!myReportManager.Folders.CreateFabsecCarcassFolder()) return false;

			DrawingManager dm = await DrawingManager.Create(model, projectData, fabsecCarcasses, myReportManager.PhaseNum, myReportManager.IssueNum, myReportManager.Folders.CarcassOrderPath, progress);
			if (dm == null) return false;

			if (dm.GetOutOfDateDrawings().Count > 0) { PrismWarnings.DrawingsNotUpToDate(); return false; }
			if (dm.GetDrawingFolder(Enums.DrawingFolder.Default).Count != 0) { PrismWarnings.IncorrectlyAssignedDrawings(); return false; }

			int drawingsWithoutRevisions = dm.GetDrawingsWithoutRevision().Count;
			if (drawingsWithoutRevisions > 0) { PrismWarnings.DrawingsWithoutRevisions(drawingsWithoutRevisions); return false; }

			List<int> drawingCount = new List<int> { 0, dm.GetDrawingFolder(Enums.DrawingFolder.PGC).Count };
			DrawingManager.PrintAndIssueDrawings(myReportManager.Folders.FabsecCarcassFolder, myReportManager.Folders.CarcassOrderPath, drawingCount, "\\PGC", 0, 1, myReportManager, false, teklaVersion);
			PrismMacroBuilder.ClearPrintDialog();

			fabsecCarcasses.SelectParts();
			myReportManager.CreateG2Assy();
			BswxExporter.ExportBSWX(myObjects, myReportManager.Folders.CarcassOrderPath, projectData, myReportManager.PhaseNum, myReportManager.IssueNum, Enums.StageTypes.Prelim3);
			ModelModifiers.RemoveLog(myReportManager.Folders.CarcassOrderPath);

			myObjects.PrismParts.SelectParts();

			if (!originalFabsecs.ModifyAttributes(stageType, projectData, progress, myReportManager)) { return false; }
			foreach (PrismPart fabsec in originalFabsecs)
			{
				ModelModifiers.ModifyUDA(fabsec.Part, ModelUDA.FabsecCarcassOrdered(), projectData.Date);
			}
			model.CommitChanges();

			myReportManager.Folders.ZipFolder(myReportManager.Folders.CarcassOrderPath);

			PrismWarnings.MaterialOrderComplete(projectData);

			EmailWriter.WriteFabsecCarcassEmail(myReportManager.ProjectData, originalFabsecs, myReportManager.CarcassReportPrefix, myReportManager.IssueNum, myReportManager.PhaseNum, orderDate, myReportManager.Folders.CarcassOrderPath, zipFileCanBeAttached);

			return true;
		}

		private static bool OrderHoldingDownBolts(SelectedObjects selectedObjects, ReportManager reportManager, string projectName)
		{
			List<PrismPart> myHDBolts = HDBolts.GetHdBoltItems(selectedObjects, true);
			ModelModifiers.SelectParts(myHDBolts);
			reportManager.CreateHDBoltList(); // currently does nothing
			Logging.LogProgress(projectName, "Material 3 - HD Bolts", 0, selectedObjects.GetMainParts().Count);
			return true;
		}

		private static void HDBoltTopNutAndWasher(SelectedObjects selectedObjects, ReportManager reportManager)
		{
			List<PrismPart> myHDBolts = HDBolts.GetHdBoltItems(selectedObjects, false);
			ModelModifiers.SelectParts(myHDBolts);
			reportManager.CreateHDBoltList(); // currently does nothing
		}
	}
}