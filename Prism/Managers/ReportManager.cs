using System.IO;
using Tekla.Structures.Model;
using Tekla.Structures.Model.Operations;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using static Prism.Enums;

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
        private const string _output1Pname = "-1-SEV-PrelimHRList.pdf";  
        private string _report1Pname = $"Prism{_output1Pname}.rpt";
        private const string _output1PAname = "-1A-SEV-PrelimHRList-ADD.pdf";
        private string _report1PAname = $"Prism{_output1PAname}.rpt";
        private const string _output1POname = "-1O-SEV-PrelimHRList-OMIT.pdf";
        private string _report1POname = $"Prism{_output1POname}.rpt";
        private const string _outputBolts = "-SEV-BOLTS-STRUMIS-SUMMARY_v3.xsr";
        private const string _reportBoltsName = "-SEV-BOLTS-STRUMIS-SUMMARY_v3.rpt";      
        #endregion

        #region SNI Reports to be converted to group wide alternative
        private const string _report2QSname = "-2QS-SNI-AssemblyBreakdownList.rpt";
        private const string _report3name = "Prism-3-SNI-HotRolledMemList.pdf.rpt";
        private const string _report3PGname = "-3_PG-SNI-HotRolledPlateGirderList.rpt";
        private const string _report4name = "Prism-4-SNI-HotRolledFitList.pdf.rpt";
        private const string _report7name = "-7-SNI-ShopBoltList.rpt";
        private const string _report8Lname = "-8L-SNI-SiteBoltLocationList.rpt";
        private const string _report9name = "Prism-9-SNI-SiteDeliveryBatchList.pdf.rpt";
        private const string _output2QS = "-2QS-SNI-AssemblyBreakdownList.xsr";
        private const string _output3 = "-3-SNI-HotRolledMemList.pdf";
        private const string _output3PG = "-3_PG-SNI-HotRolledPlateGirderList.xsr";
        private const string _output4 = "-4-SNI-HotRolledFitList.pdf";
        private const string _output7 = "-7-SNI-ShopBoltList.xsr";
        private const string _output8L = "-8L-SNI-SiteBoltLocationList.xsr";
        private const string _output9 = "-9-SNI-SiteDeliveryBatchList.pdf";
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

        public void CreateMaterialReports(SelectedObjects selectedObjects, string orderType, Model model, stageTypes stageType)
        {
            string materialReport = "";
            string outputName = "";
            if (orderType == "Order Material")
            {
                selectedObjects.ExportBSWX(Folders.MatPath, _projectData, _phaseNum, _issueNum, stageType);
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

            Operation.CreateReportFromSelected(materialReport, Path.Combine(Folders.MatPath, $"{MatReportPrefix}{outputName}"), _title1, _title2, _title3);
        }

        public void CreateBoltList()
        {
            _reportBolts = Path.Combine(FirmFolderLoc.ReportTemplates(), _reportBoltsName);
            Operation.CreateReportFromSelected(_reportBolts, Path.Combine(Folders.BoltPath, $"{FabReportPrefix}{_outputBolts}"), _title1, _title2, _title3);
        }

        public void CreateFabReports(List<Part> partsList, List<List<BoltGroup>> boltList, string packageLocation)
        {
            _NCPlateSetting = "-SNI-PLATES";
            _NCProfileSetting = "-SNI-PROFILES";
            string report2QS = Path.Combine(FirmFolderLoc.ReportTemplates(), _report2QSname);
            string report3 = Path.Combine(FirmFolderLoc.ReportTemplates(), _report3name);
            string report3PG = Path.Combine(FirmFolderLoc.ReportTemplates(), _report3PGname);
            string report4 = Path.Combine(FirmFolderLoc.ReportTemplates(), _report4name);
            string report7 = Path.Combine(FirmFolderLoc.ReportTemplates(), _report7name);
            string report8L = Path.Combine(FirmFolderLoc.ReportTemplates(), _report8Lname);
            _reportBolts = Path.Combine(FirmFolderLoc.ReportTemplates(), _reportBoltsName);
            string report9 = Path.Combine(FirmFolderLoc.ReportTemplates(), _report9name);
            bool create3Report = false;
            bool create4Report = false;
            string sectionSize;

            foreach (Part part in partsList)
            {
                sectionSize = part.Profile.ProfileString.Substring(0, 2);
                bool isFitting = sectionSize == "PL" || sectionSize == "RS" || sectionSize == "FL";
                bool isPlateGirder = sectionSize == "PG";
                if (!isFitting && !isPlateGirder) create3Report = true;
                if (isPlateGirder) create3PGReport = true;
                if (isFitting) create4Report = true;
            }
            Operation.CreateReportFromSelected(report2QS, Path.Combine(Folders.DspPath, $"{FabReportPrefix}{_output2QS}"), _title1, _title2, _title3);
            Operation.CreateReportFromSelected(report9, Path.Combine(Folders.ReportPath, $"{FabReportPrefix}{_output9}"), _title1, _title2, _title3);

            if (create3Report)
            {
                Operation.CreateReportFromSelected(report3, Path.Combine(Folders.ReportPath, $"{FabReportPrefix}{_output3}"), _title1, _title2, _title3);
            }
            if (create3PGReport)
            {
                Operation.CreateReportFromSelected(report3PG, Path.Combine(Folders.ReportPath, $"{FabReportPrefix}{_output3PG}"), _title1, _title2, _title3);
            }
            if (create4Report)
            {
                Operation.CreateReportFromSelected(report4, Path.Combine(Folders.ReportPath, $"{FabReportPrefix}{_output4}"), _title1, _title2, _title3);
            }
            if (boltList[0].Count > 0 || boltList[1].Count > 0) //Then there are bolts present and we need the bolt report.
            {
                Operation.CreateReportFromSelected(_reportBolts, Path.Combine(Folders.ReportPath, $"{FabReportPrefix}{_outputBolts}"), _title1, _title3, _title2);
                // Operation.CreateReportFromSelected(_report8L, Path.Combine(Folders.ReportPath, $"{FabReportPrefix}{_output8L}"), _title1, _title2, _title3);
            }
            if (boltList[1].Count > 0) //Then there are shop bolts in the selection therefore make a shop bolt list.
            {
                Operation.CreateReportFromSelected(report7, Path.Combine(Folders.ReportPath, $"{FabReportPrefix}{_output7}"), _title1, _title3, _title2);
            }

            RemoveDPM(Folders.ReportPath);

            Operation.CreateNCFilesFromSelected(_NCProfileSetting, Path.Combine(Folders.NcPath, " "));
            Operation.CreateNCFilesFromSelected(_NCPlateSetting, Path.Combine(Folders.NcPath, " "));
        }

        //This method looks for unwanted "dpm" files that are created when tekla makes pdf reports.
        //The dpm is needed to create the pdf for some reason (a tekla thing), so we need to wait until the pdf reports are created then delete dpm
        //the loop below will deal with that.

        private void RemoveDPM(string folderPath)
        {
            List<string> fileTypes = new List<string>();
            if (Directory.Exists(folderPath))
            {
                foreach (string subFile in Directory.GetFiles(folderPath))
                {
                    fileTypes.Add(subFile.Substring(subFile.Length - 3));
                }
                if (fileTypes.Where(x => x.Contains("pdf")).Count() == fileTypes.Where(x => x.Contains("dpm")).Count())
                {
                    DeleteDPM(folderPath);
                }
                else
                {
                    Thread.Sleep(1000);
                    RemoveDPM(folderPath);
                }

                /* do { Thread.Sleep(1000); }
                 while (fileTypes.Where(x => x.Contains("pdf")).Count() != fileTypes.Where(x => x.Contains("dpm")).Count());

                 DeleteDPM(folderPath);*/
            }
        }

        private void DeleteDPM(string folderPath)
        {
            foreach (string subFile in Directory.GetFiles(folderPath))
            {
                if (subFile.Substring(subFile.Length - 3) == "dpm")
                {
                    File.Delete(subFile);
                }
            }
        }          
    }
}