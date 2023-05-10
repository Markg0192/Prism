using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
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

        List<Drawing> drawingsBySelectedParts = new List<Drawing>();

        public DrawingManager(Model model, PrismProjectData projectData, string phaseNum, string issueNum, SelectedObjects selectedObjects, string ID_DessinPath)
        {
            _selectedObjects = selectedObjects;
            _folders = new FolderManager(projectData, phaseNum, issueNum);

            this._model = model;
            DrawingsAreUpToDate = CreateDrawingList(ID_DessinPath);
            CreatePrintSettingXML(projectData.ProjPath);
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

        public bool CreateDrawingList(string ID_DessinPath)
        {
            // List<Drawing> drawingsBySelectedParts = new List<Drawing>();
            DrawingHandler dh = new DrawingHandler();
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
                drawing.Select();
                if (drawing.UpToDateStatus != DrawingUpToDateStatus.DrawingIsUpToDate)
                {
                    return false;
                }

                if (!Constants.IsSpecialPerson()) UpdateDrawing(drawing, _selectedObjects.MyDrawingHandler);
                drawingsBySelectedParts.Add(drawing);
            }

            List<List<string>> dpmList = new List<List<string>>();

            string drawingIDList = Path.Combine(FirmFolderLoc.ReportTemplates(), ReportManager._drawingDpmReportRpt);

            Tekla.Structures.Model.Operations.Operation.CreateReportFromSelected(drawingIDList, Path.Combine(ID_DessinPath, ReportManager._drawingDpmReportXsr), "", "", "");

            if (drawingsBySelectedParts.Count != 0)
            {
                dpmList = AddDpmNameToDrawings(ID_DessinPath);
            }
            foreach (Drawing drawing in drawingsBySelectedParts)
            {
                PrismDrawing prismDrawing = new PrismDrawing(drawing, _selectedObjects, _model, dpmList);
                if (prismDrawing.DrawingRequired)
                {
                    PrismDrawingList.Add(prismDrawing);
                }
            }

            return true;
        }

        public bool CheckDrawingsAgain()
        {
            foreach(var drawing in drawingsBySelectedParts)
            {
                if(drawing.UpToDateStatus != DrawingUpToDateStatus.DrawingIsUpToDate)
                {
                    return false;
                }
            }
            return true;
        }

        private void UpdateDrawing(Drawing drawing, DrawingHandler drawingHandler)
        {
            drawingHandler.SetActiveDrawing(drawing, false);
            drawingHandler.SaveActiveDrawing();
        }

        private static List<List<string>> AddDpmNameToDrawings(string ID_dessinPath)
        {
            List<List<string>> dpmList = new List<List<string>>();
            using (StreamReader sr = new StreamReader($"{ID_dessinPath}/ID_dessins_KP1.xsr"))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    var list = line.Split(',');
                    List<string> newStringList = new List<string>();

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

            ParallelLoopResult result = Parallel.ForEach(PrismDrawingList, prismDrawing =>
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = printerExeFile;
                startInfo.Arguments = GetArguments(prismDrawing.DpmPrinterSetting, prismDrawing.DpmFileName, $@"{fabPath}\{prismDrawing.DpmPDFSaveName}");
                var process = Process.Start(startInfo);
                process.WaitForExit();
            });
        }

        public void PrintDrawingsToVault(SelectedObjects myObjects, ReportManager rp, string contractNumber)
        {
            string printerExeFile = GetDPMPrinterExeFile();
            DrawingVaultInterface.Drawing dv = new DrawingVaultInterface.Drawing("cc89a98a-e0c4-480a-83f9-0b27ec66be2b");
            string serverFileLocation = dv.ServerFileLocation;
            contractNumber = ProcessContractNumber(contractNumber);

            IFCExporter.ExportIndividualIFC(myObjects, rp.Folders.IfcPath, contractNumber);

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
        }

        private static string GetDPMPrinterExeFile()
        {
            string binString = null;
            TeklaStructuresSettings.GetAdvancedOption("XSBIN", ref binString);

            string exeFile = @"applications\Tekla\Model\DPMPrinter\DPMPrinterCommand.exe";
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