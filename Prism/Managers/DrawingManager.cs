using Prism.Validation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tekla.Structures.Model;
using static Prism.Enums;
using Task = System.Threading.Tasks.Task;

namespace Prism
{
	/// <summary>
	/// The Drawing Manager class takes care of all drawing related tasks.
	/// This includes updating status label with current drawing print status and Printing drawings
	/// </summary>
	public class DrawingManager
	{
		private SelectedObjects _selectedObjects;
		private FolderManager _folders;
		private Model _model;

		public List<PrismDrawing> Drawings = new List<PrismDrawing>();
		public int NumberOfNcRequired { get; set; }

		public bool CreateReportAndGetDrawingInfo(string packagePath, List<PrismPart> prismParts, ToolStrip toolStrip, ToolStripStatusLabel tssl)
		{
			const int maxAttempts = 3;
			const string reportFileName = "PrismDrawing_List.xsr";
			const int fileLockTimeoutSeconds = 15;

			bool valid = false;

			for (int attempt = 1; attempt <= maxAttempts; attempt++)
			{
				UpdateLabel(toolStrip, tssl, attempt);

				// Clear previous drawings and generate a new report file
				Drawings.Clear();
				string reportPath = Path.Combine(packagePath, reportFileName);
				if (!GenerateReport(packagePath, reportPath, fileLockTimeoutSeconds))
				{
					return false;
				}

				// Read the report file and populate the Drawings list
				ReadReportAndPopulateDrawings(reportPath);

				// Validate that all prism parts have an associated drawing
				List<PrismPart> missingParts = ValidateDrawingSelection.ValidateSelection(Drawings, prismParts);
				if (missingParts.Count > 0)
				{
					// On the final attempt, show a warning and exit.
					if (attempt == maxAttempts)
					{
						PrismWarnings.DrawingAndPartSelectionMisMatch(missingParts);
						return false;
					}
					// Otherwise, the loop continues to retry.
				}
				else
				{
					valid = true;
					break;  // Everything is valid—exit the retry loop.
				}
			}

			// Calculate the number of NC data files required based on valid drawings.
			// We return he number of drawings for this only counting them if they are distinct, one of a kind, in the list.
			NumberOfNcRequired = Drawings
				.Where(d => d.DrawingFolder == DrawingFolder.ASS
						 || d.DrawingFolder == DrawingFolder.FIT
						 || d.DrawingFolder == DrawingFolder.PRT)
				.Select(d => d.DrawingNumber).Distinct().Count();

			return valid;
		}

		//public bool CreateReportAndGetDrawingInfo(string packagePath, List<PrismPart> prismParts, Action<int, string> progress = null)
		//{
		//	const int maxAttempts = 3;
		//	const string reportFileName = "PrismDrawing_List.xsr";
		//	const int fileLockTimeoutSeconds = 15;

		//	bool valid = false;

		//	for (int attempt = 1; attempt <= maxAttempts; attempt++)
		//	{
		//		int attemptStart = 5 + ((attempt - 1) * 25);

		//		progress?.Invoke(attemptStart, $"Checking drawing information - attempt {attempt} of {maxAttempts}...");

		//		Drawings.Clear();

		//		string reportPath = Path.Combine(packagePath, reportFileName);

		//		progress?.Invoke(attemptStart + 5, $"Generating drawing report - attempt {attempt} of {maxAttempts}...");

		//		if (!GenerateReport(packagePath, reportPath, fileLockTimeoutSeconds))
		//		{
		//			return false;
		//		}

		//		progress?.Invoke(attemptStart + 10, $"Reading drawing information - attempt {attempt} of {maxAttempts}...");

		//		ReadReportAndPopulateDrawings(reportPath);

		//		progress?.Invoke(attemptStart + 15, $"Validating drawings - attempt {attempt} of {maxAttempts}...");

		//		List<PrismPart> missingParts = ValidateDrawingSelection.ValidateSelection(Drawings, prismParts);

		//		if (missingParts.Count > 0)
		//		{
		//			if (attempt == maxAttempts)
		//			{
		//				progress?.Invoke(80, $"Drawing validation failed - {missingParts.Count} parts are missing drawings.");

		//				PrismWarnings.DrawingAndPartSelectionMisMatch(missingParts);

		//				return false;
		//			}

		//			progress?.Invoke(attemptStart + 20, $"{missingParts.Count} parts are missing drawings - retrying...");

		//			continue;
		//		}

		//		valid = true;

		//		progress?.Invoke(80, "Drawing information validated.");

		//		break;
		//	}

		//	progress?.Invoke(90, "Calculating required NC files...");

		//	NumberOfNcRequired = Drawings
		//		.Where(d => d.DrawingFolder == DrawingFolder.ASS || d.DrawingFolder == DrawingFolder.FIT || d.DrawingFolder == DrawingFolder.PRT)
		//		.Select(d => d.DrawingNumber)
		//		.Distinct()
		//		.Count();

		//	progress?.Invoke(100, $"Drawing information complete - {NumberOfNcRequired} NC files required.");

		//	return valid;
		//}

		public async Task<bool> CreateReportAndGetDrawingInfo(string packagePath, List<PrismPart> prismParts, Action<int, string> progress = null, Action<string, double> recordTiming = null)
		{
			const int maxAttempts = 3;
			const string reportFileName = "PrismDrawing_List.xsr";
			const int fileLockTimeoutSeconds = 15;

			bool valid = false;

			for (int attempt = 1; attempt <= maxAttempts; attempt++)
			{
				int attemptStart = 5 + ((attempt - 1) * 25);

				progress?.Invoke(attemptStart, $"Checking drawing information - attempt {attempt} of {maxAttempts}...");

				await Task.Yield();

				Drawings.Clear();

				string reportPath = Path.Combine(packagePath, reportFileName);

				progress?.Invoke(attemptStart + 5, $"Opening Tekla Document Manager - attempt {attempt} of {maxAttempts}...");

				await Task.Yield();

				Stopwatch reportTimer = Stopwatch.StartNew();
				if (!await GenerateReport(packagePath, reportPath, fileLockTimeoutSeconds, progress, attemptStart + 5, attemptStart + 10))
				{
					reportTimer.Stop();
					recordTiming?.Invoke("DrawingReport", reportTimer.Elapsed.TotalMilliseconds);
					return false;
				}
				reportTimer.Stop();
				recordTiming?.Invoke("DrawingReport", reportTimer.Elapsed.TotalMilliseconds);

				progress?.Invoke(attemptStart + 10, $"Reading drawing information - attempt {attempt} of {maxAttempts}...");

				await Task.Yield();

				Stopwatch readTimer = Stopwatch.StartNew();
				ReadReportAndPopulateDrawings(reportPath);
				readTimer.Stop();
				recordTiming?.Invoke("DrawingReportRead", readTimer.Elapsed.TotalMilliseconds);

				progress?.Invoke(attemptStart + 15, $"Validating drawings - attempt {attempt} of {maxAttempts}...");

				await Task.Yield();

				Stopwatch validateTimer = Stopwatch.StartNew();
				List<PrismPart> missingParts = ValidateDrawingSelection.ValidateSelection(Drawings, prismParts);
				validateTimer.Stop();
				recordTiming?.Invoke("DrawingValidation", validateTimer.Elapsed.TotalMilliseconds);

				if (missingParts.Count > 0)
				{
					if (attempt == maxAttempts)
					{
						progress?.Invoke(80, $"Drawing validation failed - {missingParts.Count} parts are missing drawings.");

						await Task.Yield();

						PrismWarnings.DrawingAndPartSelectionMisMatch(missingParts);

						return false;
					}

					progress?.Invoke(attemptStart + 20, $"{missingParts.Count} parts are missing drawings - retrying...");

					await Task.Yield();

					continue;
				}

				valid = true;

				progress?.Invoke(80, "Drawing information validated.");

				await Task.Yield();

				break;
			}

			progress?.Invoke(90, "Calculating required NC files...");

			await Task.Yield();

			Stopwatch ncTimer = Stopwatch.StartNew();
			NumberOfNcRequired = Drawings
				.Where(d => d.DrawingFolder == DrawingFolder.ASS || d.DrawingFolder == DrawingFolder.FIT || d.DrawingFolder == DrawingFolder.PRT)
				.Select(d => d.DrawingNumber)
				.Distinct()
				.Count();
			ncTimer.Stop();
			recordTiming?.Invoke("DrawingNcCount", ncTimer.Elapsed.TotalMilliseconds);

			progress?.Invoke(100, $"Drawing information complete - {NumberOfNcRequired} NC files required.");

			return valid;
		}

		public async Task<bool> CreateReportAndGetDrawingInfoAsync(string packagePath, List<PrismPart> prismParts, Action<int, string> progress = null)
		{
			const int maxAttempts = 3;
			const string reportFileName = "PrismDrawing_List.xsr";
			const int fileLockTimeoutSeconds = 15;

			bool valid = false;

			for (int attempt = 1; attempt <= maxAttempts; attempt++)
			{
				progress?.Invoke(20 + ((attempt - 1) * 10), "Checking drawing information - attempt " + attempt + " of " + maxAttempts + "...");

				Drawings.Clear();

				string reportPath = Path.Combine(packagePath, reportFileName);

				bool reportCreated = await Task.Run(() => GenerateReport(packagePath, reportPath, fileLockTimeoutSeconds));

				if (!reportCreated)
				{
					return false;
				}

				progress?.Invoke(28 + ((attempt - 1) * 10), "Reading drawing information...");

				await Task.Run(() => ReadReportAndPopulateDrawings(reportPath));

				progress?.Invoke(30 + ((attempt - 1) * 10), "Validating drawings against selected parts...");

				List<PrismPart> missingParts = await Task.Run(() => ValidateDrawingSelection.ValidateSelection(Drawings, prismParts));

				if (missingParts.Count > 0)
				{
					if (attempt == maxAttempts)
					{
						progress?.Invoke(60, "Drawing validation failed.");

						PrismWarnings.DrawingAndPartSelectionMisMatch(missingParts);

						return false;
					}

					progress?.Invoke(30 + (attempt * 10), missingParts.Count + " parts are missing drawings - retrying...");

					continue;
				}

				valid = true;

				progress?.Invoke(60, "Drawing information validated.");

				break;
			}

			NumberOfNcRequired = Drawings.Where(d => d.DrawingFolder == DrawingFolder.ASS || d.DrawingFolder == DrawingFolder.FIT ||
					d.DrawingFolder == DrawingFolder.PRT).Select(d => d.DrawingNumber).Distinct().Count();

			progress?.Invoke(65, "Drawing information complete - " + NumberOfNcRequired + " NC files required.");

			return valid;
		}

		/// <summary>
		/// Runs the macros to generate the report and waits for the file to be ready.
		/// </summary>
		private bool GenerateReport(string packagePath, string reportPath, int fileLockTimeoutSeconds)
		{
			// Execute macros that select drawings and run the report.
			PrismMacroBuilder.SelectDrawings();
			PrismMacroBuilder.RunPrismDrawingReport(packagePath);

			// Wait for the report to be created.
			if (!CreateReportAndWait(reportPath))
			{
				return false;
			}

			// Wait until the file is unlocked, or exit if it times out.
			if (!IfLockedWait(reportPath, fileLockTimeoutSeconds))
			{
				return false;
			}

			return true;
		}

		private async Task<bool> GenerateReport(string packagePath, string reportPath, int fileLockTimeoutSeconds, Action<int, string> progress, int startPercentage, int endPercentage)
		{
			progress?.Invoke(startPercentage, "Opening Tekla Document Manager...");

			await Task.Yield();

			PrismMacroBuilder.SelectDrawings();

			progress?.Invoke(startPercentage, "Tekla Document Manager opened. Running drawing report...");

			await Task.Yield();

			PrismMacroBuilder.RunPrismDrawingReport(packagePath);

			progress?.Invoke(endPercentage, "Waiting for Tekla drawing report to be created...");

			await Task.Yield();

			if (!CreateReportAndWait(reportPath))
			{
				return false;
			}

			progress?.Invoke(endPercentage, "Drawing report created. Waiting for file to become available...");

			await Task.Yield();

			if (!IfLockedWait(reportPath, fileLockTimeoutSeconds))
			{
				return false;
			}

			progress?.Invoke(endPercentage, "Tekla drawing report ready.");

			await Task.Yield();

			return true;
		}

		/// <summary>
		/// Reads the report file line by line and populates the Drawings list, then deletes the report file.
		/// </summary>
		private void ReadReportAndPopulateDrawings(string reportPath)
		{
			// Ensure we delete the file even if an exception occurs.
			try
			{
				using (var reader = new StreamReader(reportPath))
				{
					string line;
					while ((line = reader.ReadLine()) != null)
					{
						var items = line.Split(',');
						if (items.Length >= 8)
						{
							Drawings.Add(new PrismDrawing(items));
						}
					}
				}
			}
			finally
			{
				if (File.Exists(reportPath))
				{
					File.Delete(reportPath);
				}
			}
		}

		private void UpdateLabel(ToolStrip ts, ToolStripStatusLabel tssl, int attempt)
		{
			string labelText = "Gathering drawing information.";
			if (attempt > 1)
			{
				labelText = $"Attempt {attempt - 1} failed, trying again.";
			}
			ts.Invoke(new Action(() =>
			{
				tssl.Text = labelText;
			}));
		}

		public bool CreateReportAndWait(string newReportLocation)
		{
			int waitTime = 0;
			const int maxWaitTime = 10000; // 10 seconds

			while (waitTime < maxWaitTime)
			{
				if (File.Exists(newReportLocation))
				{
					return true;
				}

				Thread.Sleep(1000); // wait for 1 second
				waitTime += 1000;
			}

			return false;
		}

		/// <summary>
		/// Waits until a file is properly closed or returns false
		/// </summary>
		/// <param name="fileName"></param>
		/// /// <param name="seconds"></param>
		/// <returns></returns>
		public static bool IfLockedWait(string fileName, int seconds)
		{
			while (true)
			{
				try
				{
					using (var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
					{
						var readText = new byte[fileStream.Length];
						fileStream.Seek(0, SeekOrigin.Begin);
						var unused = fileStream.Read(readText, 0, (int)fileStream.Length);
					}
					return true;
				}

				catch (IOException)
				{
					// wait one second
					Thread.Sleep(1000);
					seconds--;
					if (seconds == 0)
						return false;
				}
			}
		}

		// Private constructor ensures that instances are only created through the factory method.
		private DrawingManager(Model model, PrismProjectData projectData, string phaseNum, string issueNum)
		{
			_folders = new FolderManager(projectData, phaseNum, issueNum);
			_model = model;
		}

		/// <summary>
		/// Factory method that creates a DrawingManager.
		/// If drawing validation fails, the user is asked if they want to continue anyway.
		/// Returns null if the user chooses not to continue.
		/// </summary>
		public static DrawingManager Create(Model model, PrismProjectData projectData, List<PrismPart> myParts, string phaseNum,
			string issueNum, string packagePath, ToolStrip ts, ToolStripStatusLabel tssl)
		{
			// Create the instance.
			var manager = new DrawingManager(model, projectData, phaseNum, issueNum);

			// Attempt to create the report and get drawing info.
			bool valid = manager.CreateReportAndGetDrawingInfo(packagePath, myParts, ts, tssl);

			// If validation fails, offer the user a chance to continue.
			if (!valid)
			{
				if (!PrismWarnings.MissingDrawingsFound())
				{
					// The user chose not to continue.
					return null;
				}
			}

			return manager;
		}



		public static async Task<DrawingManager> Create(Model model, PrismProjectData projectData, List<PrismPart> myParts, string phaseNum, string issueNum, string packagePath, Action<int, string> progress, Action<string, double> recordTiming = null)
		{
			progress?.Invoke(0, "Preparing drawing manager...");

			DrawingManager manager = new DrawingManager(model, projectData, phaseNum, issueNum);

			progress?.Invoke(5, "Checking drawing information...");

			await Task.Yield();

			Action<int, string> drawingInfoProgress = CreateProgressRange(progress, 5, 90);

			bool valid = await manager.CreateReportAndGetDrawingInfo(packagePath, myParts, drawingInfoProgress, recordTiming);

			if (!valid)
			{
				progress?.Invoke(95, "Drawing information requires confirmation...");

				await Task.Yield();

				Stopwatch userWait = Stopwatch.StartNew();
				bool continueWithMissingDrawings = PrismWarnings.MissingDrawingsFound();
				userWait.Stop();
				recordTiming?.Invoke("DrawingUserWait", userWait.Elapsed.TotalMilliseconds);

				if (!continueWithMissingDrawings)
				{
					return null;
				}
			}

			progress?.Invoke(100, "Drawing information ready.");

			return manager;
		}

		public static async Task<DrawingManager> CreateAsync(Model model, PrismProjectData projectData, List<PrismPart> myParts, string phaseNum,
			string issueNum, string packagePath, Action<int, string> progress)
		{
			DrawingManager manager = new DrawingManager(model, projectData, phaseNum, issueNum);

			bool valid = await manager.CreateReportAndGetDrawingInfoAsync(packagePath, myParts, progress);

			if (!valid)
			{
				if (!PrismWarnings.MissingDrawingsFound())
				{
					return null;
				}
			}

			return manager;
		}

		public static async Task NewPrintDrawings(ReportManager reportManager, DrawingManager drawingManager, List<int> drawingCount, string teklaVersion, ToolStrip toolStrip, ToolStripStatusLabel statusLabel)
		{
			if (drawingManager.GetDrawingFolder(Enums.DrawingFolder.ASS).Count != 0)
				await PrintAndIssueDrawings(reportManager.Folders.FabFolder, reportManager.Folders.FabPath, drawingCount, "\\ASS", 0, 1, reportManager, true, teklaVersion, toolStrip, statusLabel);

			if (drawingManager.GetDrawingFolder(Enums.DrawingFolder.FIT).Count != 0)
				await PrintAndIssueDrawings(reportManager.Folders.FabFolder, reportManager.Folders.FabPath, drawingCount, "\\FIT", 2, 3, reportManager, false, teklaVersion, toolStrip, statusLabel);

			if (drawingManager.GetDrawingFolder(Enums.DrawingFolder.PGC).Count != 0)
				await PrintAndIssueDrawings(reportManager.Folders.FabFolder, reportManager.Folders.FabPath, drawingCount, "\\PGC", 6, 7, reportManager, false, teklaVersion, toolStrip, statusLabel);

			if (drawingManager.GetDrawingFolder(Enums.DrawingFolder.PRT).Count != 0)
				await PrintAndIssueDrawings(reportManager.Folders.FabFolder, reportManager.Folders.FabPath, drawingCount, "\\PRT", 8, 9, reportManager, false, teklaVersion, toolStrip, statusLabel);

			if (drawingManager.GetDrawingFolder(Enums.DrawingFolder.SHA).Count != 0)
				await PrintAndIssueDrawings(reportManager.Folders.FabFolder, reportManager.Folders.FabPath, drawingCount, "\\SHA", 10, 11, reportManager, false, teklaVersion, toolStrip, statusLabel);

			if (drawingManager.GetDrawingFolder(Enums.DrawingFolder.WLD).Count != 0)
				await PrintAndIssueDrawings(reportManager.Folders.FabFolder, reportManager.Folders.FabPath, drawingCount, "\\WLD", 12, 13, reportManager, true, teklaVersion, toolStrip, statusLabel);

			RemoveSheetNumbersFromAllDrawings(reportManager.Folders.FabPath);

			PrismMacroBuilder.ClearPrintDialog();
		}

		public static async Task NewPrintDrawings(ReportManager reportManager, DrawingManager drawingManager, List<int> drawingCount, string teklaVersion)
		{
			if (drawingManager.GetDrawingFolder(Enums.DrawingFolder.ASS).Count != 0)
				await PrintAndIssueDrawings(reportManager.Folders.FabFolder, reportManager.Folders.FabPath, drawingCount, "\\ASS", 0, 1, reportManager, true, teklaVersion);

			if (drawingManager.GetDrawingFolder(Enums.DrawingFolder.FIT).Count != 0)
				await PrintAndIssueDrawings(reportManager.Folders.FabFolder, reportManager.Folders.FabPath, drawingCount, "\\FIT", 2, 3, reportManager, false, teklaVersion);

			if (drawingManager.GetDrawingFolder(Enums.DrawingFolder.PGC).Count != 0)
				await PrintAndIssueDrawings(reportManager.Folders.FabFolder, reportManager.Folders.FabPath, drawingCount, "\\PGC", 6, 7, reportManager, false, teklaVersion);

			if (drawingManager.GetDrawingFolder(Enums.DrawingFolder.PRT).Count != 0)
				await PrintAndIssueDrawings(reportManager.Folders.FabFolder, reportManager.Folders.FabPath, drawingCount, "\\PRT", 8, 9, reportManager, false, teklaVersion);

			if (drawingManager.GetDrawingFolder(Enums.DrawingFolder.SHA).Count != 0)
				await PrintAndIssueDrawings(reportManager.Folders.FabFolder, reportManager.Folders.FabPath, drawingCount, "\\SHA", 10, 11, reportManager, false, teklaVersion);

			if (drawingManager.GetDrawingFolder(Enums.DrawingFolder.WLD).Count != 0)
				await PrintAndIssueDrawings(reportManager.Folders.FabFolder, reportManager.Folders.FabPath, drawingCount, "\\WLD", 12, 13, reportManager, true, teklaVersion);

			RemoveSheetNumbersFromAllDrawings(reportManager.Folders.FabPath);

			PrismMacroBuilder.ClearPrintDialog();
		}

		public static async Task NewPrintDrawings(ReportManager reportManager, DrawingManager drawingManager, List<int> drawingCount, string teklaVersion, Action<int, string> progress)
		{
			int assCount = drawingManager.GetDrawingFolder(Enums.DrawingFolder.ASS).Count;
			int fitCount = drawingManager.GetDrawingFolder(Enums.DrawingFolder.FIT).Count;
			int pgcCount = drawingManager.GetDrawingFolder(Enums.DrawingFolder.PGC).Count;
			int prtCount = drawingManager.GetDrawingFolder(Enums.DrawingFolder.PRT).Count;
			int shaCount = drawingManager.GetDrawingFolder(Enums.DrawingFolder.SHA).Count;
			int wldCount = drawingManager.GetDrawingFolder(Enums.DrawingFolder.WLD).Count;

			int totalDrawingCount = assCount + fitCount + pgcCount + prtCount + shaCount + wldCount;
			int completedDrawingCount = 0;

			progress?.Invoke(0, totalDrawingCount == 1 ? "Preparing to print 1 fabrication drawing..." : $"Preparing to print {totalDrawingCount} fabrication drawings...");

			if (assCount > 0)
			{
				int startPercentage = GetDrawingProgressPercentage(completedDrawingCount, totalDrawingCount);
				int endPercentage = GetDrawingProgressPercentage(completedDrawingCount + assCount, totalDrawingCount);
				Action<int, string> drawingProgress = CreateProgressRange(progress, startPercentage, endPercentage);

				await PrintAndIssueDrawings(reportManager.Folders.FabFolder, reportManager.Folders.FabPath, drawingCount, "\\ASS", 0, 1, reportManager, true, teklaVersion, drawingProgress);

				completedDrawingCount += assCount;
			}

			if (fitCount > 0)
			{
				int startPercentage = GetDrawingProgressPercentage(completedDrawingCount, totalDrawingCount);
				int endPercentage = GetDrawingProgressPercentage(completedDrawingCount + fitCount, totalDrawingCount);
				Action<int, string> drawingProgress = CreateProgressRange(progress, startPercentage, endPercentage);

				await PrintAndIssueDrawings(reportManager.Folders.FabFolder, reportManager.Folders.FabPath, drawingCount, "\\FIT", 2, 3, reportManager, false, teklaVersion, drawingProgress);

				completedDrawingCount += fitCount;
			}

			if (pgcCount > 0)
			{
				int startPercentage = GetDrawingProgressPercentage(completedDrawingCount, totalDrawingCount);
				int endPercentage = GetDrawingProgressPercentage(completedDrawingCount + pgcCount, totalDrawingCount);
				Action<int, string> drawingProgress = CreateProgressRange(progress, startPercentage, endPercentage);

				await PrintAndIssueDrawings(reportManager.Folders.FabFolder, reportManager.Folders.FabPath, drawingCount, "\\PGC", 6, 7, reportManager, false, teklaVersion, drawingProgress);

				completedDrawingCount += pgcCount;
			}

			if (prtCount > 0)
			{
				int startPercentage = GetDrawingProgressPercentage(completedDrawingCount, totalDrawingCount);
				int endPercentage = GetDrawingProgressPercentage(completedDrawingCount + prtCount, totalDrawingCount);
				Action<int, string> drawingProgress = CreateProgressRange(progress, startPercentage, endPercentage);

				await PrintAndIssueDrawings(reportManager.Folders.FabFolder, reportManager.Folders.FabPath, drawingCount, "\\PRT", 8, 9, reportManager, false, teklaVersion, drawingProgress);

				completedDrawingCount += prtCount;
			}

			if (shaCount > 0)
			{
				int startPercentage = GetDrawingProgressPercentage(completedDrawingCount, totalDrawingCount);
				int endPercentage = GetDrawingProgressPercentage(completedDrawingCount + shaCount, totalDrawingCount);
				Action<int, string> drawingProgress = CreateProgressRange(progress, startPercentage, endPercentage);

				await PrintAndIssueDrawings(reportManager.Folders.FabFolder, reportManager.Folders.FabPath, drawingCount, "\\SHA", 10, 11, reportManager, false, teklaVersion, drawingProgress);

				completedDrawingCount += shaCount;
			}

			if (wldCount > 0)
			{
				int startPercentage = GetDrawingProgressPercentage(completedDrawingCount, totalDrawingCount);
				int endPercentage = GetDrawingProgressPercentage(completedDrawingCount + wldCount, totalDrawingCount);
				Action<int, string> drawingProgress = CreateProgressRange(progress, startPercentage, endPercentage);

				await PrintAndIssueDrawings(reportManager.Folders.FabFolder, reportManager.Folders.FabPath, drawingCount, "\\WLD", 12, 13, reportManager, true, teklaVersion, drawingProgress);

				completedDrawingCount += wldCount;
			}

			progress?.Invoke(96, "Finalising drawing PDF names...");

			RemoveSheetNumbersFromAllDrawings(reportManager.Folders.FabPath);

			progress?.Invoke(99, "Clearing Tekla print dialog...");

			PrismMacroBuilder.ClearPrintDialog();

			progress?.Invoke(100, totalDrawingCount == 1 ? "1 fabrication drawing printed." : $"{totalDrawingCount} fabrication drawings printed.");
		}

		private static int GetDrawingProgressPercentage(int completedDrawingCount, int totalDrawingCount)
		{
			if (totalDrawingCount <= 0) return 95;

			return (int)Math.Round((completedDrawingCount / (double)totalDrawingCount) * 95.0);
		}

		private static Action<int, string> CreateProgressRange(Action<int, string> progress, int startPercentage, int endPercentage)
		{
			if (progress == null) return null;

			return (percentage, message) =>
			{
				int clampedPercentage = Math.Max(0, Math.Min(100, percentage));
				int mappedPercentage = startPercentage + (int)Math.Round((endPercentage - startPercentage) * (clampedPercentage / 100.0));

				progress(mappedPercentage, message);
			};
		}

		/// <summary>
		/// This method turns Tekla drawings into PDF and puts an "Issue" stamp on it to signify it is issued
		/// It creates a tekla macro for sorting the document manager by file type and then selects drawings based on the relative to values of the drawing count
		/// these "drawingCounts" represent the start point for the current drawing type and the number of drawings of this type to print
		/// It then batches and processes these in groups of 50 as Teklas PDF Printer can sometimes struggle with larger chunks of drawings
		/// Eg. we have 110 drawings, 
		/// </summary>
		public static async Task PrintAndIssueDrawings(string issueFolder, string issuePath, List<int> drawingCount,
			string folderPath, int countIndex1, int countIndex2, ReportManager reportManager, bool isAss, string teklaVersion,
			ToolStrip toolStrip, ToolStripStatusLabel statusLabel)
		{
			int start = drawingCount[countIndex1]; // this is where in the total list of drawings this type starts
			int range = drawingCount[countIndex2]; // this is the count of drawings of the current type

			int finalEndPoint = start + range - 1;

			int batchSize = 50;

			int currentStart = start;

			while (currentStart <= finalEndPoint)
			{
				int remaining = finalEndPoint - currentStart + 1;
				int currentCount = Math.Min(batchSize, remaining);

				PrismMacroBuilder.PrintSelectedDrawings(Constants.PrismPackageFolderName + "\\\\" + issueFolder,
					folderPath, currentStart, currentCount, isAss, teklaVersion);

				string printFolder = issuePath + folderPath;

				int desiredFileCount = Directory.GetFiles(printFolder).Length + currentCount;

				string drawingType = folderPath;

				await WaitForPrintingAsync(printFolder, desiredFileCount, toolStrip, statusLabel, drawingType, range);

				toolStrip.Invoke(new Action(() =>
				{
					statusLabel.Text = "Adding issue stamp.";
				}));

				PrismMacroBuilder.IssueAndLockStampOn();

				currentStart += currentCount;
			}
		}

		public static async Task PrintAndIssueDrawings(string issueFolder, string issuePath, List<int> drawingCount,
		string folderPath, int countIndex1, int countIndex2, ReportManager reportManager, bool isAss, string teklaVersion)
		{
			int start = drawingCount[countIndex1]; // this is where in the total list of drawings this type starts
			int range = drawingCount[countIndex2]; // this is the count of drawings of the current type

			int finalEndPoint = start + range - 1;

			int batchSize = 50;

			int currentStart = start;

			while (currentStart <= finalEndPoint)
			{
				int remaining = finalEndPoint - currentStart + 1;
				int currentCount = Math.Min(batchSize, remaining);

				PrismMacroBuilder.PrintSelectedDrawings(Constants.PrismPackageFolderName + "\\\\" + issueFolder,
					folderPath, currentStart, currentCount, isAss, teklaVersion);

				string printFolder = issuePath + folderPath;

				int desiredFileCount = Directory.GetFiles(printFolder).Length + currentCount;

				string drawingType = folderPath;

				await WaitForPrintingAsync(printFolder, desiredFileCount, drawingType, range);

				PrismMacroBuilder.IssueAndLockStampOn();

				currentStart += currentCount;
			}
		}

		public static async Task PrintAndIssueDrawings(string issueFolder, string issuePath, List<int> drawingCount, string folderPath, int countIndex1, int countIndex2, ReportManager reportManager, bool isAss, string teklaVersion, Action<int, string> progress)
		{
			int start = drawingCount[countIndex1];
			int range = drawingCount[countIndex2];

			if (range <= 0)
			{
				progress?.Invoke(100, "No drawings to print.");
				return;
			}

			int finalEndPoint = start + range - 1;
			int batchSize = 50;
			int currentStart = start;
			int completedDrawingCount = 0;
			string drawingType = folderPath.TrimStart('\\');

			progress?.Invoke(0, range == 1 ? $"Preparing to print 1 {drawingType} drawing..." : $"Preparing to print {range} {drawingType} drawings...");

			while (currentStart <= finalEndPoint)
			{
				int remaining = finalEndPoint - currentStart + 1;
				int currentCount = Math.Min(batchSize, remaining);
				string printFolder = issuePath + folderPath;
				int initialFileCount = Directory.Exists(printFolder) ? Directory.GetFiles(printFolder).Length : 0;
				int desiredFileCount = initialFileCount + currentCount;

				int batchStartPercentage = GetPrintProgressPercentage(completedDrawingCount, range);
				int batchEndPercentage = GetPrintProgressPercentage(completedDrawingCount + currentCount, range);

				progress?.Invoke(batchStartPercentage, currentCount == 1 ? $"Printing {drawingType} drawing {completedDrawingCount + 1} of {range}..." : $"Printing {drawingType} drawings {completedDrawingCount + 1} to {completedDrawingCount + currentCount} of {range}...");

				PrismMacroBuilder.PrintSelectedDrawings(Constants.PrismPackageFolderName + "\\\\" + issueFolder, folderPath, currentStart, currentCount, isAss, teklaVersion);

				Action<int, string> batchProgress = CreateProgressRange(progress, batchStartPercentage, batchEndPercentage);

				await WaitForPrintingAsync(printFolder, desiredFileCount, initialFileCount, completedDrawingCount, currentCount, drawingType, range, batchProgress);

				completedDrawingCount += currentCount;

				int completedPercentage = GetPrintProgressPercentage(completedDrawingCount, range);

				progress?.Invoke(completedPercentage, $"Adding issue stamp to {drawingType} drawings...");

				PrismMacroBuilder.IssueAndLockStampOn();

				currentStart += currentCount;
			}

			progress?.Invoke(100, range == 1 ? $"{drawingType} drawing printed." : $"{range} {drawingType} drawings printed.");
		}

		private static int GetPrintProgressPercentage(int completedDrawingCount, int totalDrawingCount)
		{
			if (totalDrawingCount <= 0) return 95;

			return (int)Math.Round((completedDrawingCount / (double)totalDrawingCount) * 95.0);
		}

		public static void RemoveSheetNumbersFromAllDrawings(string mainDirectoryPath)
		{
			if (!Directory.Exists(mainDirectoryPath))
			{
				Console.WriteLine("Main directory does not exist.");
				return;
			}

			// Get all subdirectories in the main directory
			string[] subdirectories = Directory.GetDirectories(mainDirectoryPath);

			foreach (string subdirectory in subdirectories)
			{
				RenamePdfFilesInDirectory(subdirectory);
			}
		}

		private static void RenamePdfFilesInDirectory(string directoryPath)
		{
			// Get all PDFs in the folder
			string[] pdfFiles = Directory.GetFiles(directoryPath, "*.pdf");

			foreach (string filePath in pdfFiles)
			{
				string fileName = Path.GetFileNameWithoutExtension(filePath);

				// Try removing " - 1" given to multi-draings if present
				TryRenameIfContains(filePath, directoryPath, fileName, " - 1");

				// Try removing " - 2" given to multo-drawings if present
				TryRenameIfContains(filePath, directoryPath, fileName, " - 2");
			}
		}

		/// <summary>
		/// Checks if the fileName contains a given pattern. If so, it removes the pattern,
		/// constructs a new file path, and renames the file—provided no file with that
		/// name already exists.
		/// </summary>
		private static void TryRenameIfContains(string oldFilePath, string directoryPath, string fileName, string patternToRemove)
		{
			// If the filename doesn't contain the pattern, do nothing
			if (!fileName.Contains(patternToRemove))
				return;

			// Build the base name without the pattern (e.g., remove " - 1")
			string baseName = fileName.Replace(patternToRemove, string.Empty);
			string newFileName = baseName + ".pdf";
			string newFilePath = Path.Combine(directoryPath, newFileName);

			// If a file of the new name already exists, skip renaming
			if (File.Exists(newFilePath))
				return;

			// Rename
			File.Move(oldFilePath, newFilePath);
		}

		public List<PrismDrawing> GetFrozenDrawings()
		{
			return Drawings.Where(part => part.IsFrozen).ToList();
		}

		public List<PrismDrawing> GetUnFrozenDrawings()
		{
			return Drawings.Where(part => !part.IsFrozen).ToList();
		}

		public List<PrismDrawing> GetLockedDrawings()
		{
			return Drawings.Where(part => part.IsLocked).ToList();
		}

		public List<PrismDrawing> GetDrawingFolder(Enums.DrawingFolder drawingFolder)
		{
			return Drawings.Where(part => part.DrawingFolder == drawingFolder).ToList();
		}

		public List<PrismDrawing> GetOutOfDateDrawings()
		{
			return Drawings.Where(part => part.ChangeMessage != "").ToList();
		}

		public List<PrismDrawing> GetDrawingsWithoutRevision()
		{
			return Drawings.Where(part => part.DrawingRevision == "").ToList();
		}

		public List<PrismDrawing> GetDrawingByType(string type)
		{
			return Drawings.Where(part => part.DrawingType == type).ToList();
		}

		/// <summary>
		/// This method watches the file currently being printed to until the number of files in the folder matches the number of
		/// drawings currently being printed, as soon as it's hit the task ends. If no new files are creted in a certain timespan then
		/// it is assumed somthing is wrong with printing and a warning is sent to the user
		/// </summary>
		private static async Task WaitForPrintingAsync(string printFolder, int desiredFileCount, ToolStrip toolStrip,
			ToolStripStatusLabel statusLabel, string drawingType, int totalNumberOfFilesOfCurrentType)
		{
			string folderPath = printFolder;

			using (FileSystemWatcher watcher = new FileSystemWatcher(folderPath))
			{
				watcher.EnableRaisingEvents = true;
				watcher.IncludeSubdirectories = false;

				int currentFileCount = Directory.Exists(folderPath)
					? Directory.GetFiles(folderPath).Length
					: 0;

				int previousFileCount = currentFileCount;

				// Used to instantly exit when done
				var tcs = new TaskCompletionSource<bool>();

				watcher.Created += (sender, e) =>
				{
					// Recheck actual file count 
					currentFileCount = Directory.GetFiles(folderPath).Length;

					UpdateStatusLabel(
						toolStrip,
						statusLabel,
						currentFileCount,
						totalNumberOfFilesOfCurrentType,
						drawingType.Substring(1));

					if (currentFileCount >= desiredFileCount)
					{
						watcher.EnableRaisingEvents = false;
						tcs.TrySetResult(true); // signal completion immediately
					}
				};

				DateTime lastProgressTime = DateTime.Now;

				while (currentFileCount < desiredFileCount)
				{
					var delayTask = Task.Delay(30000);
					var completedTask = await Task.WhenAny(delayTask, tcs.Task);

					if (completedTask == tcs.Task)
						break;

					int fileCountAfterCheck = Directory.GetFiles(folderPath).Length;

					if (fileCountAfterCheck >= desiredFileCount)
						break;

					if (fileCountAfterCheck > currentFileCount)
					{
						// ✅ progress detected
						lastProgressTime = DateTime.Now;
					}

					currentFileCount = fileCountAfterCheck;

					// ❗ Only fail if no progress for a LONG time
					if ((DateTime.Now - lastProgressTime).TotalMinutes >= 2)
					{
						PrismWarnings.DrawingPrintFailed();
						watcher.EnableRaisingEvents = false;
						break;
					}
				}

				//while (currentFileCount < desiredFileCount)
				//{
				//	var delayTask = Task.Delay(30000);

				//	//  Wait for either: completion OR timeout
				//	var completedTask = await Task.WhenAny(delayTask, tcs.Task);

				//	if (completedTask == tcs.Task)
				//	{
				//		// Finished immediately
				//		break;
				//	}

				//	// Fallback check (we need this because sometimes the watcher can miss events)
				//	int fileCountAfterCheck = Directory.GetFiles(folderPath).Length;

				//	if (fileCountAfterCheck >= desiredFileCount)
				//	{
				//		break;
				//	}

				//	if (fileCountAfterCheck == previousFileCount)
				//	{
				//		PrismWarnings.DrawingPrintFailed();
				//		watcher.EnableRaisingEvents = false;
				//		break;
				//	}

				//	previousFileCount = fileCountAfterCheck;
				//	currentFileCount = fileCountAfterCheck;
				//}

				if (currentFileCount >= desiredFileCount)
				{
					Console.WriteLine("Desired file count reached.");
				}
			}
		}

		private static async Task WaitForPrintingAsync(string printFolder, int desiredFileCount, int initialFileCount, int completedDrawingCount, int currentBatchCount, string drawingType, int totalDrawingCount, Action<int, string> progress)
		{
			string folderPath = printFolder;
			int currentFileCount = Directory.Exists(folderPath) ? Directory.GetFiles(folderPath).Length : 0;
			int lastReportedDrawingCount = completedDrawingCount;

			using (FileSystemWatcher watcher = new FileSystemWatcher(folderPath))
			{
				watcher.IncludeSubdirectories = false;

				TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

				Action<int> reportProgress = fileCount =>
				{
					int filesCreatedThisBatch = Math.Max(0, fileCount - initialFileCount);
					int completedInBatch = Math.Min(filesCreatedThisBatch, currentBatchCount);
					int currentDrawingCount = Math.Min(completedDrawingCount + completedInBatch, totalDrawingCount);

					if (currentDrawingCount <= lastReportedDrawingCount) return;

					lastReportedDrawingCount = currentDrawingCount;

					int percentage = currentBatchCount > 0 ? (int)Math.Round((completedInBatch / (double)currentBatchCount) * 100.0) : 100;

					progress?.Invoke(percentage, $"Printing {drawingType} drawing {currentDrawingCount} of {totalDrawingCount}...");
				};

				watcher.Created += (sender, e) =>
				{
					try
					{
						currentFileCount = Directory.GetFiles(folderPath).Length;

						reportProgress(currentFileCount);

						if (currentFileCount >= desiredFileCount)
						{
							watcher.EnableRaisingEvents = false;
							tcs.TrySetResult(true);
						}
					}
					catch
					{
						// The fallback loop will recheck the folder if the watcher fires
						// while Tekla still has a file locked or the directory is changing.
					}
				};

				watcher.EnableRaisingEvents = true;

				currentFileCount = Directory.GetFiles(folderPath).Length;

				reportProgress(currentFileCount);

				if (currentFileCount >= desiredFileCount)
				{
					watcher.EnableRaisingEvents = false;
					tcs.TrySetResult(true);
				}

				DateTime lastProgressTime = DateTime.Now;
				int previousFileCount = currentFileCount;

				while (currentFileCount < desiredFileCount)
				{
					Task delayTask = Task.Delay(30000);
					Task completedTask = await Task.WhenAny(delayTask, tcs.Task);

					if (completedTask == tcs.Task)
					{
						break;
					}

					int fileCountAfterCheck = Directory.GetFiles(folderPath).Length;

					if (fileCountAfterCheck > previousFileCount)
					{
						lastProgressTime = DateTime.Now;
					}

					currentFileCount = fileCountAfterCheck;
					previousFileCount = fileCountAfterCheck;

					reportProgress(currentFileCount);

					if (currentFileCount >= desiredFileCount)
					{
						watcher.EnableRaisingEvents = false;
						tcs.TrySetResult(true);
						break;
					}

					if ((DateTime.Now - lastProgressTime).TotalMinutes >= 2)
					{
						watcher.EnableRaisingEvents = false;

						PrismWarnings.DrawingPrintFailed();

						break;
					}
				}

				currentFileCount = Directory.GetFiles(folderPath).Length;

				reportProgress(currentFileCount);

				if (currentFileCount >= desiredFileCount)
				{
					Console.WriteLine("Desired file count reached.");
				}
			}
		}


		private static async Task WaitForPrintingAsync(string printFolder, int desiredFileCount, string drawingType, int totalNumberOfFilesOfCurrentType)
		{
			string folderPath = printFolder;

			using (FileSystemWatcher watcher = new FileSystemWatcher(folderPath))
			{
				watcher.EnableRaisingEvents = true;
				watcher.IncludeSubdirectories = false;

				int currentFileCount = Directory.Exists(folderPath)
					? Directory.GetFiles(folderPath).Length
					: 0;

				int previousFileCount = currentFileCount;

				// Used to instantly exit when done
				var tcs = new TaskCompletionSource<bool>();

				watcher.Created += (sender, e) =>
				{
					// Recheck actual file count 
					currentFileCount = Directory.GetFiles(folderPath).Length;

					if (currentFileCount >= desiredFileCount)
					{
						watcher.EnableRaisingEvents = false;
						tcs.TrySetResult(true); // signal completion immediately
					}
				};

				DateTime lastProgressTime = DateTime.Now;

				while (currentFileCount < desiredFileCount)
				{
					var delayTask = Task.Delay(30000);
					var completedTask = await Task.WhenAny(delayTask, tcs.Task);

					if (completedTask == tcs.Task)
						break;

					int fileCountAfterCheck = Directory.GetFiles(folderPath).Length;

					if (fileCountAfterCheck >= desiredFileCount)
						break;

					if (fileCountAfterCheck > currentFileCount)
					{
						// ✅ progress detected
						lastProgressTime = DateTime.Now;
					}

					currentFileCount = fileCountAfterCheck;

					// ❗ Only fail if no progress for a LONG time
					if ((DateTime.Now - lastProgressTime).TotalMinutes >= 2)
					{
						PrismWarnings.DrawingPrintFailed();
						watcher.EnableRaisingEvents = false;
						break;
					}
				}

				if (currentFileCount >= desiredFileCount)
				{
					Console.WriteLine("Desired file count reached.");
				}
			}
		}

		private static void UpdateStatusLabel(ToolStrip toolStrip, ToolStripStatusLabel statusLabel, int currentFileCount, int desiredFileCount, string drawingType)
		{
			toolStrip.Invoke(new System.Action(() =>
			{
				statusLabel.Text = $"Printing {drawingType} drawings: {currentFileCount} of {desiredFileCount}";
			}));
		}
	}
}