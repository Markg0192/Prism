using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tekla.Structures.Model;
using Tekla.Structures.Model.Operations;
using static Prism.Enums;

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

        /// <summary>
        /// /// Create the report and then read data
        /// </summary>
        public void CreateReportAndGetDrawingInfo(string packagePath)
        {            
            PrismMacroBuilder.SelectDrawings();
            PrismMacroBuilder.RunPrismDrawingReport(packagePath);
          //  string teklaReportLocation = "C:\\TeklaStructuresModels2023\\Sandbox\\PrismDrawing_List.rpt";

		//	string teklaReportLocation = "C:\\Sev_Firm_2021\\Reports\\Prism\\PrismDrawing_List.rpt";// Path.Combine(FirmFolderLoc.ReportTemplates(), "PrismDrawing_List.rpt");
            string newReportLocation = Path.Combine(packagePath,  "PrismDrawing_List.xsr");

            if (!CreateReportAndWait(/*teklaReportLocation, */newReportLocation)) return;

            // wait until Tekla Structures has unlocked the file, or timeout
            if (!IfLockedWait(newReportLocation, 15)) return;

            // read the report
            using (var reader = new StreamReader(newReportLocation))
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

            File.Delete(newReportLocation);

            NumberOfNcRequired = Drawings.Count(d => d.DrawingFolder == DrawingFolder.ASS) + Drawings.Count(d => d.DrawingFolder == DrawingFolder.FIT) + Drawings.Count(d => d.DrawingFolder == DrawingFolder.PRT);
        }

        public bool CreateReportAndWait(/*string teklaReportLocation, */string newReportLocation)
        {
           // Operation.CreateReportFromSelected(teklaReportLocation, newReportLocation, "", "", "");

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

        public DrawingManager(Model model, PrismProjectData projectData, string phaseNum, string issueNum, string packagePath)
        {
            _folders = new FolderManager(projectData, phaseNum, issueNum);
            Logging.DebugLog("folder manaager made", "");
            _model = model;

            CreateReportAndGetDrawingInfo(packagePath);

            Logging.DebugLog("drawingList made", "");
        }

        public static void NewPrintDrawings(ReportManager reportManager, DrawingManager drawingManager, List<int> drawingCount, ToolStrip toolStrip, ToolStripStatusLabel statusLabel)
        { 
            if (drawingManager.GetDrawingFolder(Enums.DrawingFolder.ASS).Count != 0) PrintAndIssueDrawings(reportManager.Folders.FabFolder, reportManager.Folders.FabPath, drawingCount, "\\ASS", 0, 1, reportManager, true, toolStrip, statusLabel);
            if (drawingManager.GetDrawingFolder(Enums.DrawingFolder.FIT).Count != 0) PrintAndIssueDrawings(reportManager.Folders.FabFolder, reportManager.Folders.FabPath, drawingCount, "\\FIT", 2, 3, reportManager, false, toolStrip, statusLabel);
            if (drawingManager.GetDrawingFolder(Enums.DrawingFolder.PGC).Count != 0) PrintAndIssueDrawings(reportManager.Folders.FabFolder, reportManager.Folders.FabPath, drawingCount, "\\PGC", 6, 7, reportManager, false, toolStrip, statusLabel);
            if (drawingManager.GetDrawingFolder(Enums.DrawingFolder.PRT).Count != 0) PrintAndIssueDrawings(reportManager.Folders.FabFolder, reportManager.Folders.FabPath, drawingCount, "\\PRT", 8, 9, reportManager, false, toolStrip, statusLabel);
            if (drawingManager.GetDrawingFolder(Enums.DrawingFolder.SHA).Count != 0) PrintAndIssueDrawings(reportManager.Folders.FabFolder, reportManager.Folders.FabPath, drawingCount, "\\SHA", 10, 11, reportManager, false, toolStrip, statusLabel);         
            if (drawingManager.GetDrawingFolder(Enums.DrawingFolder.WLD).Count != 0) PrintAndIssueDrawings(reportManager.Folders.FabFolder, reportManager.Folders.FabPath, drawingCount, "\\WLD", 12, 13, reportManager, true, toolStrip, statusLabel);

            PrismMacroBuilder.ClearPrintDialog();
        }

        public static void PrintAndIssueDrawings(string issueFolder, string issuePath, List<int> drawingCount, string folderPath, int countIndex1, int countIndex2, ReportManager reportManager, bool isAss, ToolStrip toolStrip, ToolStripStatusLabel statusLabel)
        {
           // Thread.Sleep(2000);
            PrismMacroBuilder.PrintSelectedDrawings(Constants.PrismPackageFolderName + "\\\\" + issueFolder, folderPath, drawingCount[countIndex1], drawingCount[countIndex2], isAss);
            WaitForPrinting(issuePath + folderPath, drawingCount[countIndex2], toolStrip, statusLabel, folderPath);
            PrismMacroBuilder.IssueAndLockStampOn();
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

        private static void WaitForPrinting(string printFolder, int desiredFileCount, ToolStrip toolStrip, ToolStripStatusLabel statusLabel, string drawingType)
        {
            string folderPath = printFolder;

            FileSystemWatcher watcher = new FileSystemWatcher(folderPath);
            watcher.EnableRaisingEvents = true;
            watcher.IncludeSubdirectories = false;

            int currentFileCount = Directory.GetFiles(folderPath).Length;
            int previousFileCount = currentFileCount;
            bool countIncreased = false;

            watcher.Created += (sender, e) =>
            {
                currentFileCount++;
                UpdateStatusLabel(toolStrip, statusLabel, currentFileCount, desiredFileCount, drawingType.Substring(1));

                if (currentFileCount >= desiredFileCount)
                {
                    watcher.EnableRaisingEvents = false; // Stop watching the folder
                }

                countIncreased = true; // File created, set flag to true
            };

            while (currentFileCount < desiredFileCount)
            {
                Thread.Sleep(8000); // Delay for 10 seconds before checking again

                if (!countIncreased)
                {
                    PrismWarnings.DrawingPrintFailed();
                    watcher.EnableRaisingEvents = false;
                    break;
                }

                int fileCountAfterCheck = Directory.GetFiles(folderPath).Length;

                if (fileCountAfterCheck > previousFileCount)
                {
                    previousFileCount = fileCountAfterCheck;
                    countIncreased = true; // Files increased, set flag to true
                }
                else
                {
                    countIncreased = false; // No new files found, set flag to false
                }

                currentFileCount = fileCountAfterCheck;
                UpdateStatusLabel(toolStrip, statusLabel, currentFileCount, desiredFileCount, drawingType.Substring(1));
            }

            if (currentFileCount >= desiredFileCount)
            {
                Console.WriteLine("Desired file count reached.");
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