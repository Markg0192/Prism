using System.IO;
using Tekla.Structures.Model;
using Tekla.Structures.Model.Operations;
using System.Collections.Generic;
using static Prism.Enums;
using System.Drawing.Printing;
using System.Drawing;
using System;
using System.Linq;

namespace Prism
{
    /// <summary>
    /// The Report Manager class is where all reports are created.
    /// NC files are also created from here.
    /// </summary>
    public class ReportManager
    {
        private string _title1;
        private string _title2;
        private string _title3;
        private string _reportBolts;

        #region group accepted reports
        //Material Procurement
        private const string _output1Pname = "-1-PrelimHotRolledMemList.xsr";
        private string _report1Pname = $"{_output1Pname.Replace("xsr", "rpt")}";
        private const string _output1PAname = "-1a-PrelimHotRolledMemList-ADD.xsr";
        private string _report1PAname = $"{_output1PAname.Replace("xsr", "rpt")}";
        private const string _output1POname = "-1o-PrelimHotRolledMemList-OMIT.xsr";
        private string _report1POname = $"{_output1POname.Replace("xsr", "rpt")}";

        private const string _output1PFname = "-1F-PrelimSpecialFitList.xsr";
        private string _report1PFname = $"{_output1PFname.Replace("xsr", "rpt")}";
        private const string _output1PFAname = "-1Fa-PrelimSpecialFitList-ADD.xsr";
        private string _report1PFAname = $"{_output1PFAname.Replace("xsr", "rpt")}";
        private const string _output1PFOname = "-1Fo-PrelimSpecialFitList-OMIT.xsr";
        private string _report1PFOname = $"{_output1PFOname.Replace("xsr", "rpt")}";
        public const string _g2ReportOutput = "-G2_Assy7.xsr";
        public string _g2ReportName = $"{_g2ReportOutput.Replace("xsr", "rpt")}";

        //Bolt ordering
        private const string _outputBolts = "-SEV-BOLTS-STRUMIS-SUMMARY_v3.xsr";
        private const string _reportBoltsName = "-SEV-BOLTS-STRUMIS-SUMMARY_v3.rpt";

        private const string _outputSelectedBolts = "-SEV-BOLTS-STRUMIS-ONLY-SELECTED_v1.xsr";
        private const string _reportSelectedBoltsName = "-SEV-BOLTS-STRUMIS-ONLY-SELECTED_v1.rpt";

        private const string _output5OName = "-5o-Bolt-OMIT.xsr";
        private string _report5OName = $"{_output5OName.Replace("xsr", "rpt")}";
        private const string _output5ONameSelected = "-5o-Bolt-OMIT-SelectedOnly.xsr";
        private string _report5ONameSelected = $"{_output5ONameSelected.Replace("xsr", "rpt")}";

        //Fab Packages
        private const string _output2Name = "-2-HotRolledMemList.xsr";
        private string _report2Name = $"{_output2Name.Replace("xsr", "rpt")}";
        private const string _output2AName = "-2a-HotRolledMemList-ADD.xsr";
        private string _report2AName = $"{_output2AName.Replace("xsr", "rpt")}";
        private const string _output2OName = "-2o-HotRolledMemList-OMIT.xsr";
        private string _report2OName = $"{_output2OName.Replace("xsr", "rpt")}";

        private const string _output3Name = "-3-HotRolledFitList.xsr";
        private string _report3Name = $"{_output3Name.Replace("xsr", "rpt")}";
        private const string _output3AName = "-3a-HotRolledFitList-ADD.xsr";
        private string _report3AName = $"{_output3AName.Replace("xsr", "rpt")}";
        private const string _output3OName = "-3o-HotRolledFitList-OMIT.xsr";
        private string _report3OName = $"{_output3OName.Replace("xsr", "rpt")}";

        private const string _output4Name = "-4-ShopBoltList.xsr";
        private string _report4Name = $"{_output4Name.Replace("xsr", "rpt")}";
        private const string _output4AName = "-4a-ShopBoltList-ADD.xsr";
        private string _report4AName = $"{_output4AName.Replace("xsr", "rpt")}";
        private const string _output4OName = "-4o-ShopBoltList-OMIT.xsr";
        private string _report4OName = $"{_output4OName.Replace("xsr", "rpt")}";
        private const string _output4LName = "-4l-ShopBoltLocationList.xsr";
        private string _report4LName = $"{_output4LName.Replace("xsr", "rpt")}";

        private const string _output5Name = "-5-SiteBoltList.xsr";
        private string _report5Name = $"{_output5Name.Replace("xsr", "rpt")}";
        private const string _output5AName = "-5a-SiteBoltList-ADD.xsr";
        private string _report5AName = $"{_output5AName.Replace("xsr", "rpt")}";

        private const string _output5LName = "-5l-SiteBoltLocationList.xsr";
        private string _report5LName = $"{_output5LName.Replace("xsr", "rpt")}";

        private const string _output6Name = "-6-AssemblyList.xsr";
        private string _report6Name = $"{_output6Name.Replace("xsr", "rpt")}";

        //Extras
        private const string _reportQSname = "-QSreport.csv.rpt";
        private const string _outputQSname = "-QSreport.csv";
        private const string _report7Name = "-7-FusionMap.csv.rpt";
        private const string _output7Name = "-FusionMap.csv";
        #endregion

        private string _NCPlateSetting;
        private string _NCProfileSetting;
        private bool create3PGReport = false;
        public PrismProjectData ProjectData;
        public string PhaseNum;
        public string IssueNum;

        public ReportManager(PrismProjectData projectData, string phaseNum, string issueNum)
        {
            ProjectData = projectData;
            PhaseNum = phaseNum;
            IssueNum = issueNum;
            Folders = new FolderManager(projectData, phaseNum, issueNum);
            FabReportPrefix = ($"{projectData.ProjNumber}-{phaseNum}-FAB-ISSUE{issueNum}");
            MatReportPrefix = ($"{projectData.ProjNumber}-{phaseNum}-PRELIM-ISSUE{issueNum}");
            EpoReportPrefix = ($"{projectData.ProjNumber}-{phaseNum}-EPO-ISSUE{issueNum}");
            BoltReportPrefix = $"{projectData.ProjNumber}-{phaseNum}-BOLT-ISSUE{issueNum}";
            _title1 = phaseNum;
            _title2 = projectData.Initials;
            _title3 = issueNum;
        }

        public FolderManager Folders;
        public readonly string FabReportPrefix;
        public readonly string MatReportPrefix;
        public readonly string EpoReportPrefix;
        public readonly string BoltReportPrefix;

        public void CreateMaterialReports(SelectedObjects selectedObjects, string orderType, StageTypes stageType, bool fabsecsPresent = false)
        {
            if (!orderType.Contains("Order Bolts"))
            {
                string materialReport = "";
                string outputName = "";
                string fabsecReport = "";
                if (orderType == "Order Material")
                {
                    selectedObjects.ExportBSWX(Folders.MatPath, ProjectData, PhaseNum, IssueNum, stageType);
                    ModelModifiers.RemoveLog(Folders.MatPath);
                    materialReport = Path.Combine(FirmFolderLoc.ReportTemplates(), _report1Pname);                   
                    outputName = _output1Pname;
                }
                if (orderType == "Add Material")
                {
                    selectedObjects.ExportBSWX(Folders.MatPath, ProjectData, PhaseNum, IssueNum, stageType);
                    ModelModifiers.RemoveLog(Folders.MatPath);
                    materialReport = Path.Combine(FirmFolderLoc.ReportTemplates(), _report1PAname);                   
                    outputName = _output1PAname;
                }
                if (orderType == "Omit Material")
                {
                    materialReport = Path.Combine(FirmFolderLoc.ReportTemplates(), _report1POname);
                    outputName = _output1POname;
                }
                if (orderType == "Order Special Fittings")
                {
                    selectedObjects.ExportBSWX(Folders.MatPath, ProjectData, PhaseNum, IssueNum, stageType);
                    ModelModifiers.RemoveLog(Folders.MatPath);
                    materialReport = Path.Combine(FirmFolderLoc.ReportTemplates(), _report1PFname);
                    outputName = _output1PFname;
                }
                if (orderType == "Add Special Fittings")
                {
                    selectedObjects.ExportBSWX(Folders.MatPath, ProjectData, PhaseNum, IssueNum, stageType);
                    ModelModifiers.RemoveLog(Folders.MatPath);
                    materialReport = Path.Combine(FirmFolderLoc.ReportTemplates(), _report1PFAname);
                    outputName = _output1PFAname;
                }
                if (orderType == "Omit Special Fittings")
                {
                    materialReport = Path.Combine(FirmFolderLoc.ReportTemplates(), _report1PFOname);
                    outputName = _output1PFOname;
                }
                Operation.CreateReportFromSelected(materialReport, Path.Combine(Folders.MatPath, $"{MatReportPrefix}{outputName}"), _title1, _title2, _title3);
                
                TextToPDF(Folders.MatPath);
            }
        }

        public void CreateG2Assy()
        {
            string g2ReportName = Path.Combine(FirmFolderLoc.ReportTemplates(), _g2ReportName);
            Operation.CreateReportFromSelected(g2ReportName, Path.Combine(Folders.MatPath, $"{MatReportPrefix}{_g2ReportOutput}"), _title1, _title2, _title3);
        }

        public void CreateBoltList(string reportPrefix, string orderType)
        {
            string boltReportName = _reportBoltsName;
            string boltListOutputName = _outputBolts;

            if (orderType.Contains("Omit")) { boltReportName = _report5OName; boltListOutputName = _output5OName; }

            _reportBolts = Path.Combine(FirmFolderLoc.ReportTemplates(), boltReportName);
            Operation.CreateReportFromSelected(_reportBolts, Path.Combine(Folders.BoltPath, $"{reportPrefix}{boltListOutputName}"), _title1, _title2, _title3);
          //  TextToPDF(Folders.BoltPath);
        }

        public void CreateSelectedBoltList(string reportPrefix, string orderType)
        {
            string boltReportName = _reportSelectedBoltsName;
            string boltListOutputName = _outputSelectedBolts;

            if (orderType.Contains("Omit")) { boltReportName = _report5ONameSelected; boltListOutputName = _output5ONameSelected; }

            string reportBolts = Path.Combine(FirmFolderLoc.ReportTemplates(), boltReportName);
            Operation.CreateReportFromSelected(reportBolts, Path.Combine(Folders.BoltPath, $"{reportPrefix}{boltListOutputName}"), _title1, _title2, _title3);
            TextToPDF(Folders.BoltPath);

        }


        public void CreateHDBoltList()
        {

        }

        public async void CreateFabReports(List<Part> partsList, List<List<BoltGroup>> boltList)
        {
            while (Operation.IsMacroRunning()) // Wait until macro for selecting drawings in the document manager is complete before moving on
            {
                await System.Threading.Tasks.Task.Delay(10);
            }
            _NCPlateSetting = "-SNI-PLATES";
            _NCProfileSetting = "-SNI-PROFILES";
            string qsReport = Path.Combine(FirmFolderLoc.ReportTemplates(), _reportQSname);
            string hrMemberReport = Path.Combine(FirmFolderLoc.ReportTemplates(), _report2Name);
            string hrFittingReport = Path.Combine(FirmFolderLoc.ReportTemplates(), _report3Name);
            string shopBoltReport = Path.Combine(FirmFolderLoc.ReportTemplates(), _report4Name);
            string siteBoltReport = Path.Combine(FirmFolderLoc.ReportTemplates(), _report5Name);
            string assemblyReport = Path.Combine(FirmFolderLoc.ReportTemplates(), _report6Name);
            string fusionMapReport = Path.Combine(FirmFolderLoc.ReportTemplates(), _report7Name);

            _reportBolts = Path.Combine(FirmFolderLoc.ReportTemplates(), _reportBoltsName);
            string report9 = Path.Combine(FirmFolderLoc.ReportTemplates(), _report6Name);
            bool create3Report = false;
            bool create4Report = false;
            string sectionSize;

            bool siteBoltsPresent = boltList[0].Count > 0 ? true : false;
            bool shopBoltsPresent = boltList[1].Count > 0 ? true : false;

            foreach (Part part in partsList)
            {
                sectionSize = part.Profile.ProfileString.Substring(0, 2);
                bool isFitting = sectionSize == "PL" || sectionSize == "RS" || sectionSize == "FL";
                bool isPlateGirder = sectionSize == "PG";
                if (!isFitting && !isPlateGirder) create3Report = true;
                if (isPlateGirder) create3PGReport = true;
                if (isFitting) create4Report = true;
            }

            Operation.CreateReportFromSelected(qsReport, Path.Combine(Folders.DspPath, $"{FabReportPrefix}{_outputQSname}"), _title1, _title2, _title3);
            Operation.CreateReportFromSelected(assemblyReport, Path.Combine(Folders.ReportPath, $"{FabReportPrefix}{_output6Name}"), _title1, _title2, _title3);
            Operation.CreateReportFromSelected(fusionMapReport, Path.Combine(Folders.DspPath, $"{FabReportPrefix}{_output7Name}"), _title1, _title2, _title3);

            if (create3Report)
            {
                Operation.CreateReportFromSelected(hrMemberReport, Path.Combine(Folders.ReportPath, $"{FabReportPrefix}{_output2Name}"), _title1, _title2, _title3);
            }
            if (create4Report)
            {
                Operation.CreateReportFromSelected(hrFittingReport, Path.Combine(Folders.ReportPath, $"{FabReportPrefix}{_output3Name}"), _title1, _title2, _title3);
            }
            if (shopBoltsPresent || siteBoltsPresent) //then create our strumis summary report
            {
                Operation.CreateReportFromSelected(_reportBolts, Path.Combine(Folders.DspPath, $"{FabReportPrefix}{_outputBolts}"), _title1, _title2, _title3);
            }
            if (shopBoltsPresent) //Then create a shop bolts summary
            {
                Operation.CreateReportFromSelected(shopBoltReport, Path.Combine(Folders.ReportPath, $"{FabReportPrefix}{_output4Name}"), _title1, _title2, _title3);
            }
            if (siteBoltsPresent) //Then create a site bolts summary
            {
                Operation.CreateReportFromSelected(siteBoltReport, Path.Combine(Folders.ReportPath, $"{FabReportPrefix}{_output5Name}"), _title1, _title2, _title3);
            }

            Operation.CreateNCFilesFromSelected(_NCProfileSetting, Path.Combine(Folders.NcPath, " "));
            Operation.CreateNCFilesFromSelected(_NCPlateSetting, Path.Combine(Folders.NcPath, " "));

            TextToPDF(Folders.ReportPath);
        }

        public static async void SelectDrawingsInDocManager(List<Part> selectedParts)
        {
            if (PrismMacroBuilder.DrawingOperations())
            {
                Logging.DebugLog("Macro Built", "");
            }
            else { Logging.DebugLog("Macro not built", ""); }

            if (selectedParts != null) selectedParts.SelectParts();
            Logging.DebugLog("Selected parts", "");

            Operation.RunMacro(Constants.DrawingOperation);
            Logging.DebugLog("Drawing operation complete", "");

            while (Operation.IsMacroRunning()) // Wait until macro for selecting drawings in the document manager is complete before moving on
            {
                await System.Threading.Tasks.Task.Delay(10);
                Logging.DebugLog("Drawing operation wait", "");
            }
        }

        private static void TextToPDF(string folderPath)
        {
            foreach (string subFile in Directory.GetFiles(folderPath))
            {
                if (subFile.EndsWith(".xsr") && !subFile.Contains("G2"))
                {
                    VirtualPrinter(subFile);
                    File.Delete(subFile);
                }
            }
        }

        private static void VirtualPrinter(string filePath)
        {
            Font font = new Font("Lucida Console", 10, FontStyle.Regular);

            string printerName = "Microsoft Print to PDF"; // name of the printer

            PrintDocument printDocument = new PrintDocument();
            printDocument.PrinterSettings.PrinterName = printerName;

            printDocument.PrinterSettings.PrintToFile = true;
            printDocument.PrinterSettings.PrintFileName = Path.ChangeExtension(filePath, "pdf");
            printDocument.DefaultPageSettings.PaperSize = new PaperSize("A4", 2100, 2970);
            printDocument.DefaultPageSettings.Margins = new Margins(40, 40, 40, 40); // 0.5 inch margins
            printDocument.DefaultPageSettings.Landscape = false; // portrait ori0entation
            printDocument.DefaultPageSettings.Color = false; // black and white output

            printDocument.DocumentName = Path.GetFileNameWithoutExtension(filePath);

            // Variables to keep track of current position in file and number of lines printed
            int linesPerPage = 74;
            int lineNumber = 0;
            int position = 0;

            printDocument.PrintPage += (sender, e) =>
            {
                // Read a portion of the file starting from the current position
                using (StreamReader reader = new StreamReader(filePath))
                {
                    string text = reader.ReadToEnd();
                    string[] lines = text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                    int linesToPrint = Math.Min(linesPerPage, lines.Length - lineNumber);
                    string portion = string.Join(Environment.NewLine, lines.Skip(lineNumber).Take(linesToPrint));
                    e.Graphics.DrawString(portion, font, Brushes.Black, e.MarginBounds);

                    // Update variables for next page
                    lineNumber += linesToPrint;
                    position += portion.Length;
                    if (lineNumber >= lines.Length)
                    {
                        e.HasMorePages = false;
                    }
                    else
                    {
                        e.HasMorePages = true;
                    }
                }
            };

            printDocument.Print();
        }
    }
}