using System.IO;
using Tekla.Structures.Model;
using Tekla.Structures.Model.Operations;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using static Prism.Enums;
using System;
using Tekla.Structures;

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
        private const string _output1Pname = "-1-PrelimHotRolledMemList.pdf";
        private string _report1Pname = $"{_output1Pname}.rpt";
        private const string _output1PAname = "-1a-PrelimHotRolledMemList-ADD.pdf";
        private string _report1PAname = $"{_output1PAname}.rpt";
        private const string _output1POname = "-1o-PrelimHotRolledMemList-OMIT.pdf";
        private string _report1POname = $"{_output1POname}.rpt";

        private const string _output1PFname = "-1F-SEV-PrelimHRFitList.pdf";
        private string _report1PFname = $"{_output1PFname}.rpt";
        private const string _output1PFAname = "-1FA-SEV-PrelimHRFitList-ADD.pdf";
        private string _report1PFAname = $"{_output1PFAname}.rpt";
        private const string _output1PFOname = "-1FO-SEV-PrelimHRFitList-OMIT.pdf";
        private string _report1PFOname = $"{_output1PFOname}.rpt";

        //Bolt ordering
        private const string _outputBolts = "-SEV-BOLTS-STRUMIS-SUMMARY_v3.xsr";
        private const string _reportBoltsName = "-SEV-BOLTS-STRUMIS-SUMMARY_v3.rpt";

        //Fab Packages
        private const string _output2Name = "-2-HotRolledMemList.pdf";
        private string _report2Name = $"{_output2Name}.rpt";
        private const string _output2AName = "-2a-HotRolledMemList-ADD.pdf";
        private string _report2AName = $"{_output2AName}.rpt";
        private const string _output2OName = "-2o-HotRolledMemList-OMIT.pdf";
        private string _report2OName = $"{_output2OName}.rpt";

        private const string _output3Name = "-3-HotRolledFitList.pdf";
        private string _report3Name = $"{_output3Name}.rpt";
        private const string _output3AName = "-3a-HotRolledFitList-ADD.pdf";
        private string _report3AName = $"{_output3AName}.rpt";
        private const string _output3OName = "-3o-HotRolledFitList-OMIT.pdf";
        private string _report3OName = $"{_output3OName}.rpt";

        private const string _output4Name = "-4-ShopBoltList.pdf";
        private string _report4Name = $"{_output4Name}.rpt";
        private const string _output4AName = "-4a-ShopBoltList-ADD.pdf";
        private string _report4AName = $"{_output4AName}.rpt";
        private const string _output4OName = "-4o-ShopBoltList-OMIT.pdf";
        private string _report4OName = $"{_output4OName}.rpt";
        private const string _output4LName = "-4l--ShopBoltLocationList.pdf";
        private string _report4LName = $"{_output4LName}.rpt";

        private const string _output5Name = "-5-SiteBoltList.pdf";
        private string _report5Name = $"{_output5Name}.rpt";
        private const string _output5AName = "-5a-SiteBoltList-ADD.pdf";
        private string _report5AName = $"{_output5AName}.rpt";
        private const string _output5OName = "-5o-SiteBoltList-OMIT.pdf";
        private string _report5OName = $"{_output5OName}.rpt";
        private const string _output5LName = "-5l-SiteBoltLocationList.pdf";
        private string _report5LName = $"{_output5LName}.rpt";

        private const string _output6Name = "-6-AssemblyList.pdf";
        private string _report6Name = $"{_output6Name}.rpt";

        //Extras
        private const string _reportQSname = "-QS-AssemblyBreakdownList.rpt";
        private const string _outputQSname = "-QS-AssemblyBreakdownList.xsr";

        public string DrawingDpmReportRpt = "ID_dessins_KP1.rpt";
        public string DrawingDpmReportXsr = "ID_dessins_KP1.xsr";
        #endregion

        private string _NCPlateSetting;
        private string _NCProfileSetting;
        private bool create3PGReport = false;
        private PrismProjectData _projectData;
        private string _phaseNum;
        private string _issueNum;

        public ReportManager(PrismProjectData projectData, string phaseNum, string issueNum)
        {
            _projectData = projectData;
            _phaseNum = phaseNum;
            _issueNum = issueNum;
            Folders = new FolderManager(projectData, phaseNum, issueNum);
            FabReportPrefix = ($"{projectData.ProjNumber}-{phaseNum}-FAB-ISSUE{issueNum}");
            MatReportPrefix = ($"{projectData.ProjNumber}-{phaseNum}-PRELIM-ISSUE{issueNum}");
            _title1 = phaseNum;
            _title2 = projectData.Initials;
            _title3 = issueNum;
        }

        public FolderManager Folders;
        public readonly string FabReportPrefix;
        public readonly string MatReportPrefix;

        public void CreateMaterialReports(SelectedObjects selectedObjects, string orderType, StageTypes stageType)
        {
            string materialReport = "";
            string outputName = "";
            if (orderType == "Order Material")
            {
                selectedObjects.ExportBSWX(Folders.MatPath, _projectData, _phaseNum, _issueNum, stageType);
                ModelModifiers.RemoveLog(Folders.MatPath);
                materialReport = Path.Combine(FirmFolderLoc.ReportTemplates(), _report1Pname);
                outputName = _output1Pname;
            }
            if (orderType == "Add Material")
            {
                selectedObjects.ExportBSWX(Folders.MatPath, _projectData, _phaseNum, _issueNum, stageType);
                materialReport = Path.Combine(FirmFolderLoc.ReportTemplates(), _report1PAname);
                outputName = _output1PAname;
            }
            if (orderType == "Omit Material")
            {
                materialReport = Path.Combine(FirmFolderLoc.ReportTemplates(), _report1POname);
                outputName = _output1POname;
            }
            if (orderType == "Order Heavy Fittings")
            {
                selectedObjects.ExportBSWX(Folders.MatPath, _projectData, _phaseNum, _issueNum, stageType);
                materialReport = Path.Combine(FirmFolderLoc.ReportTemplates(), _report1Pname);
                outputName = _output1PFname;
            }
            Operation.CreateReportFromSelected(materialReport, Path.Combine(Folders.MatPath, $"{MatReportPrefix}{outputName}"), _title1, _title2, _title3);
        }

        public void CreateBoltList()
        {
            _reportBolts = Path.Combine(FirmFolderLoc.ReportTemplates(), _reportBoltsName);
            Operation.CreateReportFromSelected(_reportBolts, Path.Combine(Folders.BoltPath, $"{FabReportPrefix}{_outputBolts}"), _title1, _title2, _title3);
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

            Operation.CreateReportFromSelected(DrawingDpmReportRpt, Path.Combine(Folders.DspPath, DrawingDpmReportXsr), "", "", "");
            Operation.CreateReportFromSelected(qsReport, Path.Combine(Folders.DspPath, $"{FabReportPrefix}{_outputQSname}"), _title1, _title2, _title3);
            Operation.CreateReportFromSelected(assemblyReport, Path.Combine(Folders.ReportPath, $"{FabReportPrefix}{_output6Name}"), _title1, _title2, _title3);

            if (create3Report)
            {
                Operation.CreateReportFromSelected(hrMemberReport, Path.Combine(Folders.ReportPath, $"{FabReportPrefix}{_output2Name}"), _title1, _title2, _title3);
            }
            /* if (create3PGReport)
             {
                 Operation.CreateReportFromSelected(report3PG, Path.Combine(Folders.ReportPath, $"{FabReportPrefix}{_output3PG}"), _title1, _title2, _title3);
             }*/
            if (create4Report)
            {
                Operation.CreateReportFromSelected(hrFittingReport, Path.Combine(Folders.ReportPath, $"{FabReportPrefix}{_output3Name}"), _title1, _title2, _title3);
            }
            if (shopBoltsPresent || siteBoltsPresent) //then create our strumis summary report
            {
                Operation.CreateReportFromSelected(_reportBolts, Path.Combine(Folders.DspPath, $"{FabReportPrefix}{_outputBolts}"), _title1, _title3, _title2);
                // Operation.CreateReportFromSelected(_report8L, Path.Combine(Folders.ReportPath, $"{FabReportPrefix}{_output8L}"), _title1, _title2, _title3);
            }
            if (shopBoltsPresent) //Then create a shop bolts summary
            {
                Operation.CreateReportFromSelected(shopBoltReport, Path.Combine(Folders.ReportPath, $"{FabReportPrefix}{_output4Name}"), _title1, _title3, _title2);
            }
            if (siteBoltsPresent) //Then create a site bolts summary
            {
                Operation.CreateReportFromSelected(siteBoltReport, Path.Combine(Folders.ReportPath, $"{FabReportPrefix}{_output5Name}"), _title1, _title3, _title2);
            }

            Operation.CreateNCFilesFromSelected(_NCProfileSetting, Path.Combine(Folders.NcPath, " "));
            Operation.CreateNCFilesFromSelected(_NCPlateSetting, Path.Combine(Folders.NcPath, " "));
        }

        public static async void SelectDrawingsInDocManager()
        {
            var macrodir = "";
            TeklaStructuresSettings.GetAdvancedOption("XS_MACRO_DIRECTORY", ref macrodir);
            var dir = macrodir.Split(';')[0];
            if (!File.Exists(dir + @"\modeling\SelectReqDrawingsInManager.cs"))
            {
                var writer = new StreamWriter(dir + @"\modeling\SelectReqDrawingsInManager.cs");
                var macro = "#pragma warning disable 1633 // Unrecognized #pragma directive" + Environment.NewLine +
                            "#pragma reference \"Tekla.Macros.Wpf.Runtime\"" + Environment.NewLine +
                            "#pragma reference \"Tekla.Macros.Runtime\"" + Environment.NewLine +
                            "#pragma warning restore 1633 // Unrecognized #pragma directive" + Environment.NewLine +
                            Environment.NewLine +
                            "namespace UserMacros" + Environment.NewLine +
                                "{" + Environment.NewLine +
                                   " public sealed class Macro" + Environment.NewLine +
                                   "{" + Environment.NewLine +
                                       " [Tekla.Macros.Runtime.MacroEntryPointAttribute()]" + Environment.NewLine +
                                       " public static void Run(Tekla.Macros.Runtime.IMacroRuntime runtime)" + Environment.NewLine +
                                       " {" + Environment.NewLine +
                                          "  Tekla.Macros.Wpf.Runtime.IWpfMacroHost wpf = runtime.Get<Tekla.Macros.Wpf.Runtime.IWpfMacroHost>();" + Environment.NewLine +
                                          "  wpf.InvokeCommand(\"CommandRepository\", \"Drawing.DrawingList\");" + Environment.NewLine +
                                          "  wpf.View(\"DocumentManager.MainWindow\").Find(\"AID_DOCMAN_CategoryList\").As.Selector.DoSelection.With(\"albl_All_documents\").Invoke();" + Environment.NewLine +
                                          "  wpf.View(\"DocumentManager.MainWindow\").Find(\"AID_DOCMAN_ShowAllDocuments\").As.Button.Invoke();" + Environment.NewLine +
                                          "  wpf.View(\"DocumentManager.MainWindow\").Find(\"AID_DOCMAN_ButtonSelectDrawings\").As.Button.Invoke();" + Environment.NewLine +
                                       " }" + Environment.NewLine +
                                   " }" + Environment.NewLine +
                               " }";

                writer.Write(macro);
                writer.Close();
            }
            Operation.RunMacro("SelectReqDrawingsInManager.cs");

            while (Operation.IsMacroRunning()) // Wait until macro for selecting drawings in the document manager is complete before moving on
            {
                await System.Threading.Tasks.Task.Delay(10);
            }
        }
    }
}