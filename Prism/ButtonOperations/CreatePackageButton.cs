using Prism.Managers.ChangeManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tekla.Structures;
using Tekla.Structures.Model;
using static Prism.Enums;
using Model = Tekla.Structures.Model.Model;

namespace Prism.ButtonOperations
{
	public static class CreatePackageButton
	{
		public static async Task<(bool success, int totalNcRequired)> CreateFabPackage(this SelectedObjects myObjects, Model model, PrismProjectData projectData, string phaseNumber, string issueNumber, StageTypes stageType, string siteDate, bool runSeversafe, bool runChangeManager,
			ToolStrip toolStrip, ToolStripStatusLabel label, string teklaVersion)
		{
			int totalNcRequired = 0;
			string packagingType = Logging.GetAdvancedSetting(projectData.ProjNumberAndGuid, AdvancedSettingType.FabPackType);

			if (packagingType.Contains("Lot"))
			{
				var groupedBeams = myObjects
					.GetNonSeversafeParts()
					.GroupBy(beam => beam.LotName)
					.Select(group => group.ToList())
					.ToList();

				foreach (var listOfMembers in groupedBeams)
				{
					// Perform operations on each listOfMembers as needed
				}

				return (true, totalNcRequired);
			}
			else
			{
				if (!runChangeManager)
				{
					var (success, ncRequired) = await CreateFirstIssue(projectData, phaseNumber, issueNumber, runSeversafe, myObjects, model, stageType, siteDate, teklaVersion, toolStrip, label);
					totalNcRequired = ncRequired;
					return (success, totalNcRequired);
				}

				string fileLocation = Constants.ModelDataLogLocation(projectData.ProjNumberAndGuid + "\\Fab XMLs");

				/*if (!ChangeHelper.RunChangeManagement(model, issueNumber, fileLocation, phaseNumber, projectData, myObjects, toolStrip, label, out List<SteelItemBase> revisedItems,
					out List<SteelItemBase> omitItems, out List<SteelItemBase> addItems, out string messageForEmail))
				{
					return (false, totalNcRequired);
				}*/

				// Call the asynchronous RunChangeManagementAsync method and destructure the tuple result
				var (changeSuccess, revisedItems, omitItems, addItems, messageForEmail) = await ChangeHelper.RunChangeManagementAsync(
					model, issueNumber, fileLocation, phaseNumber, projectData, myObjects, toolStrip, label);

				if (!changeSuccess)
				{
					return (false, totalNcRequired);
				}


				if (issueNumber == "01")
				{
					var (success, ncRequired) = await CreateFirstIssue(projectData, phaseNumber, issueNumber, runSeversafe, myObjects, model, stageType, siteDate, teklaVersion, toolStrip, label);
					totalNcRequired = ncRequired;
					return (success, totalNcRequired);
				}
				else
				{
					var (success, ncRequired) = await ProcessSubsequentIssues(projectData, phaseNumber, issueNumber, revisedItems, addItems, runSeversafe, myObjects, model, siteDate, stageType, messageForEmail, teklaVersion, toolStrip, label);
					totalNcRequired = ncRequired;
					return (success, totalNcRequired);
				}
			}
		}

		private static async Task<(bool success, int totalNcRequired)> CreateFirstIssue(PrismProjectData projectData, string phaseNumber, string issueNumber, bool runSeversafe, SelectedObjects myObjects, Model model, StageTypes stageType, string siteDate, string teklaVersion,
			ToolStrip toolStrip, ToolStripStatusLabel tssl)
		{
			int totalNcRequired = 0;

			// Initialize package and create folders
			if (!InitialisePackageAndCreateFolders(projectData, phaseNumber, issueNumber, runSeversafe, myObjects, out ReportManager reportManager, out CpuCounter cpuCounter))
				return (false, totalNcRequired);

			// Await the ProcessAndPrintDrawings method and handle the result tuple
			var (success, drawingManager) = await ProcessAndPrintDrawings(myObjects, cpuCounter, myObjects.GetNonSeversafeParts(), model, projectData, phaseNumber, issueNumber, reportManager, toolStrip, tssl, myObjects.PrismDrawings);
			if (!success)
				return (false, totalNcRequired);

			// Use the DrawingManager to assign total NC required
			totalNcRequired = drawingManager.NumberOfNcRequired;

			myObjects.ExportBSWX(reportManager.Folders.DspPath, projectData, phaseNumber, issueNumber, stageType, toolStrip, tssl);

			// Create fabrication reports
			reportManager.CreateFabReports(myObjects.GetNonSeversafeParts(), myObjects.PrismBoltGroups, teklaVersion, toolStrip, tssl);

			// Modify attributes of non-seversafe parts
			if (!myObjects.GetNonSeversafeParts().ModifyAttributes((int)stageType, projectData, toolStrip, tssl))
				return (false, totalNcRequired);

			// Export IFC
			await IFCExporter.ExportIndividualIFC(myObjects, reportManager.Folders.IfcPath, toolStrip, tssl);

			toolStrip.Invoke(new Action(() =>
			{
				tssl.Text = $"Forming emails.";
			}));

			// Remove unused folders
			reportManager.Folders.RemoveUnusedFolders();

			// Zip folder for attachment
			bool zipFileCanBeAttached = reportManager.Folders.ZipFolder(reportManager.Folders.FabPath);

			// Complete fabrication package and send warning if applicable
			PrismWarnings.FabPackComplete(projectData, zipFileCanBeAttached);

			//EmailWriter.CreateFabEmailText(projectData, myObjects, reportManager.FabReportPrefix, issueNumber, phaseNumber, siteDate, reportManager.Folders.FabPath);

			// Send the fabrication email
			EmailWriter.WriteFabEmail(projectData, myObjects, reportManager.FabReportPrefix, issueNumber, phaseNumber, siteDate, reportManager.Folders.FabPath, zipFileCanBeAttached);

			// Update logs
			Logging.UpdateFrozenDrawingCount(projectData.ProjNumberAndGuid, drawingManager.GetFrozenDrawings().Count, drawingManager.GetUnFrozenDrawings().Count);
			Logging.LogProgress(projectData.ProjNumberAndName, stageType.ToString(), 0, myObjects.GetMainParts().Count);

			// Return the success status and the total number of NC required
			return (true, totalNcRequired);
		}

		private static async Task<(bool success, int totalNcRequired)> ProcessSubsequentIssues(PrismProjectData projectData, string phaseNumber, string issueNumber, List<SteelItemBase> revisedItems, List<SteelItemBase> addItems,
			bool runSeversafe, SelectedObjects myObjects, Model model, string siteDate, StageTypes stageType, string messageForEmail, string teklaVersion, ToolStrip ts, ToolStripStatusLabel tssl)
		{
			int totalNcRequired = 0;

			if (revisedItems != null && addItems != null)
			{
				if (!InitialisePackageAndCreateFolders(projectData, phaseNumber, issueNumber, runSeversafe, myObjects, out ReportManager reportManager, out CpuCounter cpuCounter))
					return (false, totalNcRequired);

				List<Part> teklaParts = CombineAddAndOmitParts(model, revisedItems, addItems);

				List<PrismPart> combinedParts = new List<PrismPart>();
				foreach (Part p in teklaParts)
				{
					combinedParts.Add(new PrismPart(p));
				}

				ModelModifiers.SelectParts(combinedParts);

				SelectedObjects objects = new SelectedObjects(projectData.ProjPath, stageType, phaseNumber, issueNumber, model, Constants.SpecialOperationUser(), ts, tssl);

				// Await the async method and handle the result tuple
				var (success, drawingManager) = await ProcessAndPrintDrawings(objects, cpuCounter, objects.PrismParts, model, projectData, phaseNumber, issueNumber, reportManager, ts, tssl, objects.PrismDrawings);
				if (!success)
					return (false, totalNcRequired);

				// Use the drawingManager to set totalNcRequired
				totalNcRequired = drawingManager.NumberOfNcRequired;

				objects.ExportBSWX(reportManager.Folders.DspPath, projectData, phaseNumber, issueNumber, stageType, ts, tssl);

				reportManager.CreateFabReports(objects.PrismParts, objects.PrismBoltGroups, teklaVersion, ts, tssl);

				if (!objects.PrismParts.ModifyAttributes((int)stageType, projectData, ts, tssl))
					return (false, totalNcRequired);

				await IFCExporter.ExportIndividualIFC(objects, reportManager.Folders.IfcPath, ts, tssl);

				reportManager.Folders.RemoveUnusedFolders();

				bool zipFileCanBeAttached = reportManager.Folders.ZipFolder(reportManager.Folders.FabPath);

				PrismWarnings.FabPackComplete(projectData, zipFileCanBeAttached);

				EmailWriter.WriteRevisedFabEmail(projectData, reportManager.FabReportPrefix, issueNumber, phaseNumber, siteDate, reportManager.Folders.FabPath, zipFileCanBeAttached, messageForEmail);

				Logging.UpdateFrozenDrawingCount(projectData.ProjNumberAndGuid, drawingManager.GetFrozenDrawings().Count, drawingManager.GetUnFrozenDrawings().Count);

				Logging.LogProgress(projectData.ProjNumberAndName, stageType.ToString(), 0, myObjects.GetMainParts().Count);
			}

			return (true, totalNcRequired);
		}

		private static async Task<(bool success, DrawingManager drawingManager)> ProcessAndPrintDrawings(SelectedObjects selectedObjects, CpuCounter cpuCounter, List<PrismPart> partsToSelect, Model model, PrismProjectData projectData, string phaseNumber, string issueNumber, ReportManager reportManager, ToolStrip toolStrip, ToolStripStatusLabel statusLabel, List<PrismDrawing> drawings)
		{
			CpuSpeedCheck(cpuCounter);
			ReportManager.SelectDrawingsInDocManager(partsToSelect);

			CpuSpeedCheck(cpuCounter);
			DrawingManager drawingManager = new DrawingManager(model, projectData, phaseNumber, issueNumber, reportManager.Folders.FabPath);

			if (!CheckForProblemsWithDrawings(drawingManager, projectData.ProjNumber, out bool createDrawings)) return (false, null);
			
			if (createDrawings)
			{
				string TeklaEnvi = string.Empty;
				TeklaStructuresSettings.GetAdvancedOption("XS_ROLE_INI", ref TeklaEnvi);

				if (TeklaEnvi.Contains("GENERAL") || TeklaEnvi.Contains("PORTAL"))
				{
					await QrCodeGenerator.ApplyQrCode(selectedObjects, projectData, reportManager.Folders.QrCodePath, drawingManager, toolStrip, statusLabel);
				}

				CpuSpeedCheck(cpuCounter);
				PrismMacroBuilder.IssueAndLockStampOff();
				List<int> drawingCount = NewCountDrawings(drawingManager);
				await DrawingManager.NewPrintDrawings(reportManager, drawingManager, drawingCount, toolStrip, statusLabel);
			}

			return (true, drawingManager);
		}

		private static bool CheckForProblemsWithDrawings(DrawingManager drawingManager, string projectNumber, out bool createDrawings)
		{
			createDrawings = true;

			if (drawingManager.GetOutOfDateDrawings().Count > 0) { PrismWarnings.DrawingsNotUpToDate(); return false; }
			if (drawingManager.GetDrawingFolder(DrawingFolder.Default).Count != 0)
			{
				PrismWarnings.IncorrectlyAssignedDrawings();
				Logging.UnAssignedDrawings(projectNumber, drawingManager);

				if (!PrismWarnings.CreatePackageWithoutDrawings()) return false;
				else createDrawings = false;
			}

			int drawingsWithoutRevisions = drawingManager.GetDrawingsWithoutRevision().Count;
			if (drawingsWithoutRevisions > 0)
			{
				PrismWarnings.DrawingsWithoutRevisions(drawingsWithoutRevisions);
				return false;
			}
			return true;
		}

		private static List<Part> CombineAddAndOmitParts(Model model, List<SteelItemBase> revisedItems, List<SteelItemBase> addItems)
		{
			List<Part> reviseItems = revisedItems
						   .Select(item => model.GetIdentifierByGUID(item.Guid))
						   .Select(id => model.SelectModelObject(id))
						   .OfType<Part>()
						   .ToList();
			List<Part> addedItems = addItems
						   .Select(item => model.GetIdentifierByGUID(item.Guid))
						   .Select(id => model.SelectModelObject(id))
						   .OfType<Part>()
						   .ToList();

			return addedItems.Concat(reviseItems).ToList();
		}

		private static bool InitialisePackageAndCreateFolders(PrismProjectData projectData, string phaseNumber, string issueNumber, bool runSeversafe, SelectedObjects myObjects, out ReportManager reportManager, out CpuCounter cpuCounter)
		{
			cpuCounter = new CpuCounter();
			reportManager = new ReportManager(projectData, phaseNumber, issueNumber);

			if (!reportManager.Folders.CreateFabFolders()) return false;
			if (!reportManager.Folders.CreateBoltFolder()) return false;
			if (runSeversafe) { if (!reportManager.Folders.CreateEpoFolder()) return false; }

			if (myObjects.SeversafePresent) { myObjects.GetNonSeversafeParts().SelectParts(); }
			return true;
		}

		public static void CpuSpeedCheck(CpuCounter cpuCounter)
		{
			while (cpuCounter.CheckCPU() > 10)
			{
				System.Threading.Thread.Sleep(500);
			}
		}

		private static List<int> CountDrawings(DrawingManager dm)
		{
			int fitStartPoint = 0;
			int fitEndPoint = dm.GetDrawingFolder(DrawingFolder.Default).Count + dm.GetDrawingFolder(DrawingFolder.FIT).Count;
			int notRequiredStartPoint = fitEndPoint;
			int notRequiredEndPoint = dm.GetDrawingFolder(DrawingFolder.NotRequired).Count;

			int pgcStartPoint = fitEndPoint + notRequiredEndPoint;
			int pgcEndPoint = dm.GetDrawingFolder(DrawingFolder.PGC).Count;

			int prtStartPoint = pgcStartPoint + pgcEndPoint;
			int prtEndPoint = dm.GetDrawingFolder(DrawingFolder.PRT).Count;

			int shaStartPoint = prtStartPoint + prtEndPoint;
			int shaEndPoint = dm.GetDrawingFolder(DrawingFolder.SHA).Count;

			int assStartPoint = 0;
			int assEndPoint = dm.GetDrawingFolder(DrawingFolder.ASS).Count;

			int assNotReqStartPoint = assEndPoint;
			int assNotReqEndPoint = dm.GetDrawingFolder(DrawingFolder.AssNotRequired).Count;

			int wldStartPoint = assNotReqStartPoint + assNotReqEndPoint;
			int wldEndPoint = dm.GetDrawingFolder(DrawingFolder.WLD).Count;

			return new List<int>() { fitStartPoint, fitEndPoint, notRequiredStartPoint, notRequiredEndPoint, pgcStartPoint, pgcEndPoint, prtStartPoint, prtEndPoint, shaStartPoint, shaEndPoint, assStartPoint, assEndPoint, assNotReqStartPoint, assNotReqEndPoint, wldStartPoint, wldEndPoint };
		}

		private static List<int> NewCountDrawings(DrawingManager dm)
		{
			int assStartPoint = 0;
			int assEndPoint = dm.GetDrawingFolder(DrawingFolder.ASS).Count;

			int fitStartPoint = assStartPoint + assEndPoint;
			int fitEndPoint = dm.GetDrawingFolder(DrawingFolder.FIT).Count;

			int notRequiredStartPoint = fitStartPoint + fitEndPoint;
			int notRequiredEndPoint = dm.GetDrawingFolder(DrawingFolder.NotRequired).Count + dm.GetDrawingFolder(DrawingFolder.AssNotRequired).Count;

			int pgcStartPoint = notRequiredStartPoint + notRequiredEndPoint;
			int pgcEndPoint = dm.GetDrawingFolder(DrawingFolder.PGC).Count;

			int prtStartPoint = pgcStartPoint + pgcEndPoint;
			int prtEndPoint = dm.GetDrawingFolder(DrawingFolder.PRT).Count;

			int shaStartPoint = prtStartPoint + prtEndPoint;
			int shaEndPoint = dm.GetDrawingFolder(DrawingFolder.SHA).Count;

			int wldStartPoint = shaStartPoint + shaEndPoint;
			int wldEndPoint = dm.GetDrawingFolder(DrawingFolder.WLD).Count;

			return new List<int>() { assStartPoint, assEndPoint, fitStartPoint, fitEndPoint, notRequiredStartPoint, notRequiredEndPoint, pgcStartPoint, pgcEndPoint, prtStartPoint, prtEndPoint, shaStartPoint, shaEndPoint, wldStartPoint, wldEndPoint };

		}
	}
}