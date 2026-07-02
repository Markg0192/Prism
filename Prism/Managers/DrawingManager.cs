using Prism.Validation;
using System;
using System.Collections.Generic;
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
			if(attempt > 1)
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
		public static DrawingManager Create(Model model, PrismProjectData projectData, List<PrismPart> myParts,	string phaseNum,
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

		/// <summary>
		/// This method turns Tekla drawings into PDF and puts an "Issue" stamp on it to signify it is issued
		/// It creates a tekla macro for sorting the document manager by file type and then selects drawings based on the relative to values of the drawing count
		/// these "drawingCounts" represent the start point for the current drawing type and the number of drawings of this type to print
		/// It then batches and processes these in groups of 50 as Teklas PDF Printer can sometimes struggle with larger chunks of drawings
		/// Eg. we have 110 drawings, 
		/// </summary>
		public static async Task PrintAndIssueDrawings(string issueFolder,string issuePath,	List<int> drawingCount,
			string folderPath,int countIndex1,	int countIndex2,ReportManager reportManager,bool isAss,	string teklaVersion,
			ToolStrip toolStrip,ToolStripStatusLabel statusLabel)
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
					folderPath,	currentStart,currentCount,isAss,teklaVersion);

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
		private static void TryRenameIfContains(string oldFilePath, string directoryPath, string fileName,	string patternToRemove)
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

		private static void UpdateStatusLabel(ToolStrip toolStrip, ToolStripStatusLabel statusLabel, int currentFileCount, int desiredFileCount, string drawingType)
		{
			toolStrip.Invoke(new System.Action(() =>
			{
				statusLabel.Text = $"Printing {drawingType} drawings: {currentFileCount} of {desiredFileCount}";
			}));
		}
	}
}