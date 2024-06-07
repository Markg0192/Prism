using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tekla.Structures;
using Tekla.Structures.Drawing;
using Tekla.Structures.Model;

//This class is temporarily not in use
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

        public List<Drawing> drawingsBySelectedParts = new List<Drawing>();
        public List<Drawing> AssDrawings = new List<Drawing>();
        public List<Drawing> FitDrawings = new List<Drawing>();
        public List<Drawing> ShaDrawings = new List<Drawing>();
        public List<Drawing> PrtDrawings = new List<Drawing>();
        public List<Drawing> PgcDrawings = new List<Drawing>();
        public List<Drawing> WldDrawings = new List<Drawing>();
        public List<Drawing> NotRequiredDrawings = new List<Drawing>();
        public List<Drawing> NotReqAssDrawings = new List<Drawing>();
        public List<Drawing> NotLabelledDrawings = new List<Drawing>();
        public List<Drawing> GADrawings = new List<Drawing>();
        public List<Drawing> AllFittings = new List<Drawing>();
        public List<Drawing> FrozenDrawings = new List<Drawing>();
        public List<Drawing> UnFrozenDrawings = new List<Drawing>();

        public DrawingManager(Model model, PrismProjectData projectData, string phaseNum, string issueNum)
        {
            _folders = new FolderManager(projectData, phaseNum, issueNum);
            Logging.DebugLog("folder manaager made", "");
            this._model = model;

            DrawingsAreUpToDate = CreateDrawingList();
            Logging.DebugLog("drawingList made", "");
        }

        public static List<PrismDrawing> PrismDrawingList = new List<PrismDrawing>();
        public bool DrawingsAreUpToDate { get; set; }

        private void CreatePrintSettingXML(string modelPath)
        {
            if (!File.Exists(modelPath + "\\attributes\\" + "PrismPDFOption.xml"))
            {
                XMLWriter.PDFPrintSettings(modelPath);
            }
        }

        public static void PrintDrawings(ReportManager reportManager, DrawingManager drawingManager, List<int> drawingCount, ToolStrip toolStrip, ToolStripStatusLabel statusLabel)
        {
            if (drawingManager.FitDrawings.Count != 0) PrintAndIssueDrawings(reportManager.Folders.FabFolder, reportManager.Folders.FabPath, drawingCount, "\\FIT", 0, 1, reportManager, false, toolStrip, statusLabel);
            if (drawingManager.PgcDrawings.Count != 0) PrintAndIssueDrawings(reportManager.Folders.FabFolder, reportManager.Folders.FabPath, drawingCount, "\\PGC", 4, 5, reportManager, false, toolStrip, statusLabel);
            if (drawingManager.PrtDrawings.Count != 0) PrintAndIssueDrawings(reportManager.Folders.FabFolder, reportManager.Folders.FabPath, drawingCount, "\\PRT", 6, 7, reportManager, false, toolStrip, statusLabel);
            if (drawingManager.ShaDrawings.Count != 0) PrintAndIssueDrawings(reportManager.Folders.FabFolder, reportManager.Folders.FabPath, drawingCount, "\\SHA", 8, 9, reportManager, false, toolStrip, statusLabel);
            if (drawingManager.AssDrawings.Count != 0) PrintAndIssueDrawings(reportManager.Folders.FabFolder, reportManager.Folders.FabPath, drawingCount, "\\ASS", 10, 11, reportManager, true, toolStrip, statusLabel);
            if (drawingManager.WldDrawings.Count != 0) PrintAndIssueDrawings(reportManager.Folders.FabFolder, reportManager.Folders.FabPath, drawingCount, "\\WLD", 12, 13, reportManager, true, toolStrip, statusLabel);

            PrismMacroBuilder.ClearPrintDialog();
        }

        public static void PrintAndIssueDrawings(string issueFolder, string issuePath, List<int> drawingCount, string folderPath, int countIndex1, int countIndex2, ReportManager reportManager, bool isAss, ToolStrip toolStrip, ToolStripStatusLabel statusLabel)
        {
            Thread.Sleep(2000);
            PrismMacroBuilder.PrintSelectedDrawings(Constants.PrismPackageFolderName + "\\\\" + issueFolder, folderPath, drawingCount[countIndex1], drawingCount[countIndex2], isAss);
            WaitForPrinting(issuePath + folderPath, drawingCount[countIndex2], toolStrip, statusLabel, folderPath);
            PrismMacroBuilder.IssueAndLockStampOn();
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
                Thread.Sleep(10000); // Delay for 10 seconds before checking again

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

        public bool CreateDrawingList()
        {
            IEnumerable<int> drawingNos = Tekla.Structures.DrawingInternal.Operation.GetDrawingsBySelectedParts(true, true);
            int counter = 0;

            foreach (var item in drawingNos) counter++;

            if (counter == 0)
            { //the refresh drawings method / macro is used here as a work around, when the user first opens the model the document
                //manager must be opened at least once to initialise it, if this not done the GetDrawingsBySelectedParts method does not work
                //RefreshDrawings quickly opens the document manager if it has not been opened before to do this initialisation 
                RefreshDrawings();
                drawingNos = Tekla.Structures.DrawingInternal.Operation.GetDrawingsBySelectedParts(true, true);
            }

            foreach (var no in drawingNos)
            {
                var id = new Identifier(no);
                var drawing = Tekla.Structures.DrawingInternal.Operation.GetDrawing(id);

                if (!(drawing is GADrawing))
                {
                    drawing.Select();

                    if (drawing.IsFrozen) { FrozenDrawings.Add(drawing); }
                    else { UnFrozenDrawings.Add(drawing); }

                    string title1 = drawing.Title1;
                    if (drawing.UpToDateStatus != DrawingUpToDateStatus.DrawingIsUpToDate)
                    {
                        return false;
                    }

                    switch (title1)
                    {
                        case string t when t != "aGAdrawing" && drawing is GADrawing: // This will hopefully never be the case, we use this to ignore GA drawings in the selection
                            GADrawings.Add(drawing);
                            break;
                        case string t when t.Contains("ASS"):
                            AssDrawings.Add(drawing);
                            break;
                        case string t when t.Contains("FIT"):
                            AllFittings.Add(drawing);
                            FitDrawings.Add(drawing);
                            break;
                        case string t when t.Contains("SHA"):
                            AllFittings.Add(drawing);
                            ShaDrawings.Add(drawing);
                            break;
                        case string t when t.Contains("PRT"):
                            AllFittings.Add(drawing);
                            PrtDrawings.Add(drawing);
                            break;
                        case string t when t.Contains("PGC"):
                            AllFittings.Add(drawing);
                            PgcDrawings.Add(drawing);
                            break;
                        case string t when t.Contains("WLD"):
                            WldDrawings.Add(drawing);
                            break;
                        case string t when t.Contains("Not Required") && drawing is SinglePartDrawing:
                            AllFittings.Add(drawing);
                            NotRequiredDrawings.Add(drawing);
                            break;
                        case string t when t.Contains("Not Required") && drawing is AssemblyDrawing:
                            NotReqAssDrawings.Add(drawing);
                            break;
                        default:
                            NotLabelledDrawings.Add(drawing);
                            break;
                    }
                }
                drawingsBySelectedParts.Add(drawing);
            }
            return true;
        }

        public bool CheckDrawingsAgain()
        {
            foreach (var drawing in drawingsBySelectedParts)
            {
                if (drawing.UpToDateStatus != DrawingUpToDateStatus.DrawingIsUpToDate)
                {
                    return false;
                }
            }
            return true;
        }

        private void UpdateDrawing(Drawing drawing, DrawingHandler drawingHandler)
        {
            drawingHandler.SetActiveDrawing(drawing, false);
            //drawingHandler.UpdateDrawing(drawing);
            drawingHandler.SaveActiveDrawing();

        }

        private static List<List<string>> AddDpmNameToDrawings(string ID_dessinPath)
        {
            List<List<string>> dpmList = new List<List<string>>();
            string idPath = $"{ID_dessinPath}/ID_dessins_KP1";
            if (!File.Exists(idPath))
            {
                idPath = idPath + ".xsr";
            }
            using (StreamReader sr = new StreamReader(idPath))
            {
                //   Logging.DebugLog("Found idDessin", "");

                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    var list = line.Split(',');
                    List<string> newStringList = new List<string>();
                    //  Logging.DebugLog(list[0] + "-" + list[1], "");

                    foreach (var item in list)
                    {
                        string trimmedString = item.Trim();
                        newStringList.Add(trimmedString);

                    }
                    dpmList.Add(newStringList);
                }
            }
            return dpmList;
        }

        private static bool RefreshDrawings()
        {
            PrismMacroBuilder.RefreshDrawings(); //checks if the macro exists, if it doesnt it creates one to do the job.
            return Tekla.Structures.Model.Operations.Operation.RunMacro(Constants.RefreshDrawingsMacro);
        }

        public void PrintDrawingToModelFolder(string fabPath)
        {
            string printerExeFile = GetDPMPrinterExeFile();
            //   Logging.DebugLog("Got dpm printer exe" + printerExeFile, "");

            ParallelLoopResult result = Parallel.ForEach(PrismDrawingList, prismDrawing =>
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = printerExeFile;
                startInfo.Arguments = GetArguments(prismDrawing.DpmPrinterSetting, prismDrawing.DpmFileName, $@"{fabPath}\{prismDrawing.DpmPDFSaveName}");
                var process = Process.Start(startInfo);
                process.WaitForExit();
            });

            //  Logging.DebugLog("Drawing loop complete", "");
        }

        /* public void PrintDrawingsToVault(SelectedObjects myObjects, ReportManager rp, string contractNumber)
         {
             string printerExeFile = GetDPMPrinterExeFile();
           //  DrawingVaultInterface.Drawing dv = new DrawingVaultInterface.Drawing("cc89a98a-e0c4-480a-83f9-0b27ec66be2b");
             string serverFileLocation = dv.ServerFileLocation;
             contractNumber = ProcessContractNumber(contractNumber);

             IFCExporter.ExportIndividualIFC(myObjects, rp.Folders.IfcPath, contractNumber);
             Logging.DebugLog("IFC exports complete", contractNumber);

             //contractNumber = "102"; 
             rp.Folders.CreateDrawingVaultFolders($"{serverFileLocation}{contractNumber}");

             List<string> drawings = new List<string>();
             ParallelLoopResult result = Parallel.ForEach(PrismDrawingList, prismDrawing =>
             {
                 string drawingNumber = prismDrawing.PdfName.Split('-')[0];
                 string revision = prismDrawing.RevMark == "0" ? "" : prismDrawing.RevMark; // if rev is 0 we need to return blank here for the vault
                 string fileLocation = $@"{serverFileLocation}{contractNumber}\{prismDrawing.DrawingFolderName}\{prismDrawing.PdfName}";
                 DateTime fileModifiedDate = DateTime.Now;

                 drawings.Add($"{contractNumber}, {drawingNumber}, {revision}, {prismDrawing.DrawingSize}, {fileLocation}, {fileModifiedDate.ToString()}");

                 ProcessStartInfo startInfo = new ProcessStartInfo();
                 startInfo.FileName = printerExeFile;

                 startInfo.Arguments = GetArguments(prismDrawing.DpmPrinterSetting, prismDrawing.DpmFileName, $@"{serverFileLocation}{contractNumber}\{prismDrawing.DpmPDFSaveName}");
                 var process = System.Diagnostics.Process.Start(startInfo);
                 process.WaitForExit();
             });

             // dv.ReviewLog();
             dv.CommitChanges(drawings);
             dv.Dispose();
         }*/

        private static string GetDPMPrinterExeFile()
        {
            string binString = null;
            TeklaStructuresSettings.GetAdvancedOption("XSBIN", ref binString);

            //  Logging.DebugLog($"bin string = {binString}", "");

            string exeFile = @"applications\Tekla\Model\DPMPrinter\DPMPrinterCommand.exe";
            // Logging.DebugLog(Path.Combine(binString, exeFile), "");

            return Path.Combine(binString, exeFile);
        }

        private static string GetArguments(string printerSettings, string dpm, string pdf)
        {
            StringBuilder arg = new StringBuilder();
            arg.Append(" settingsFile:" + "\"" + printerSettings + "\"");
            arg.Append(" dpm:" + "\"" + dpm + "\"");
            arg.Append(" printActive:false ");
            arg.Append(" printer:pdf ");
            arg.Append(" out:" + "\"" + pdf + "\"");

            return arg.ToString();
        }

        private static string ProcessContractNumber(string contractNumber)
        {
            if (contractNumber.StartsWith("C")) { contractNumber = contractNumber.Substring(1); } //Remove C from the start of a contract number
            if (contractNumber.Contains("-")) contractNumber = contractNumber.Split('-')[0]; //If a dash is present then contract number looks 1234-01, we need to remove the "-01"
            if (contractNumber.Length > 4) contractNumber = $"0{contractNumber}"; //contracts need to be 4 characters long, add 0 if its less
            if (contractNumber.Length > 4) contractNumber = $"0{contractNumber}"; //go again to be sure

            return contractNumber;
        }
    }
}