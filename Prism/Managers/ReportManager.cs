using System.Collections;
using System.IO;
using Tekla.Structures.Model;
using Tekla.Structures.Model.Operations;
using Tekla.Structures.Catalogs;
using System.Collections.Generic;

namespace Prism
{
    /// <summary>
    /// The Report Manager class is where all reports are created.
    /// NC files are also created from here.
    /// </summary>
    public class ReportManager
    {
        public SevFolders Folders;
        private PrismProjectData _modelData;
        public readonly string FabReportPrefix;
        public readonly string MatReportPrefix;
        private string _title1;
        private string _title2;
        private string _title3;
        private string _reportBolts;
        private string _niFirmFolderReportPath;
        private const string _report2QSname = "-2QS-SNI-AssemblyBreakdownList.rpt";
        private const string _report3name = "Prism-3-SNI-HotRolledMemList.pdf.rpt";
        private const string _report3Pname = "Prism-3P-SNI-PrelimHotRolledMemList.pdf.rpt";
        private const string _report3PAname = "Prism-3PA-SNI-PrelimHotRolledMemList-ADD.pdf.rpt";
        private const string _report3POname = "Prism-3PO-SNI-PrelimHotRolledMemList-OMIT.pdf.rpt";
        private const string _report3PGname = "-3_PG-SNI-HotRolledPlateGirderList.rpt";
        private const string _report4name = "Prism-4-SNI-HotRolledFitList.pdf.rpt";
        private const string _report8Lname = "-8L-SNI-SiteBoltLocationList.rpt";
        private const string _reportBoltsName = "-SEV-BOLTS-STRUMIS-SUMMARY_v3.rpt";
        private const string _report9name = "Prism-9-SNI-SiteDeliveryBatchList.pdf.rpt";
        private const string _output2QS = "-2QS-SNI-AssemblyBreakdownList.xsr";
        private const string _output3 = "-3-SNI-HotRolledMemList.pdf";
        private const string _output3Pname = "-3P-SNI-PrelimHotRolledMemList.pdf";
        private const string _output3PAname = "-3PA-SNI-PrelimHotRolledMemList-ADD.pdf";
        private const string _output3POname = "-3PO-SNI-PrelimHotRolledMemList-OMIT.pdf";
        private const string _output3PG = "-3_PG-SNI-HotRolledPlateGirderList.xsr";
        private const string _output4 = "-4-SNI-HotRolledFitList.pdf";
        private const string _output8L = "-8L-SNI-SiteBoltLocationList.xsr";
        private const string _outputBolts = "-SEV-BOLTS-STRUMIS-SUMMARY_v3.xsr";
        private const string _output9 = "-9-SNI-SiteDeliveryBatchList.pdf";

        public ReportManager(Model model, string phaseNum, string issueNum)
        {
            Folders = new SevFolders(model, phaseNum, issueNum);
            _modelData = new PrismProjectData(model);
            FabReportPrefix = ($"{_modelData.ProjNumber}-{phaseNum}-FAB-ISSUE{issueNum}");
            MatReportPrefix = ($"{_modelData.ProjNumber}-{phaseNum}-PRELIM-ISSUE{issueNum}");
            _title1 = phaseNum;
            _title2 = _modelData.Initials;
            _title3 = issueNum;
        }

        public void CreateMaterialReports(List<Part> partsList, string orderType)
        {
            string materialReport = "";
            string outputName = "";
            _niFirmFolderReportPath = $"C:/Sev_Firm_2019i/Roles/SNI/Reports";
            if (orderType == "Order Material")
            {
                materialReport = Path.Combine(_niFirmFolderReportPath, _report3Pname);
                outputName = _output3Pname;
            }
            if (orderType == "Add Material")
            {
                materialReport = Path.Combine(_niFirmFolderReportPath, _report3PAname);
                outputName = _output3PAname;
            }
            if (orderType == "Omit Material")
            {
                materialReport = Path.Combine(_niFirmFolderReportPath, _report3POname);
                outputName = _output3POname;
            }

            Operation.CreateReportFromSelected(materialReport, Path.Combine(Folders.MatPath, $"{MatReportPrefix}{outputName}"), _title1, _title2, _title3);
        }

        public void CreateBoltList()
        {
            _niFirmFolderReportPath = $"C:/Sev_Firm_2019i/Roles/SNI/Reports";
            _reportBolts = Path.Combine(_niFirmFolderReportPath, _reportBoltsName);
            Operation.CreateReportFromSelected(_reportBolts, Path.Combine(Folders.ModelPath, $"Reports"), _title1, _title2, _title3);

        }

        public void CreateFabReports(List<Part> partsList, List<BoltGroup> boltList, string packageLocation)
        {
            _niFirmFolderReportPath = $"C:/Sev_Firm_2019i/Roles/{packageLocation}/Reports";
            string report2QS = Path.Combine(_niFirmFolderReportPath, _report2QSname);
            string report3 = Path.Combine(_niFirmFolderReportPath, _report3name);
            string report3PG = Path.Combine(_niFirmFolderReportPath, _report3PGname);
            string report4 = Path.Combine(_niFirmFolderReportPath, _report4name);
            string report8L = Path.Combine(_niFirmFolderReportPath, _report8Lname);
            _reportBolts = Path.Combine(_niFirmFolderReportPath, _reportBoltsName);
            string report9 = Path.Combine(_niFirmFolderReportPath, _report9name);
            bool create3Report = false;
            bool create3PGReport = false;
            bool create4Report = false;
            string sectionSize;
            LibraryProfileItem myProfileItem = new LibraryProfileItem();

            foreach (Part part in partsList)
            {
                myProfileItem.Select(part.Profile.ProfileString);
                sectionSize = new string(new char[] { myProfileItem.ProfileName.ToCharArray()[0], myProfileItem.ProfileName.ToCharArray()[1] });
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
            if(boltList.Count > 0)
            {
                Operation.CreateReportFromSelected(_reportBolts, Path.Combine(Folders.ReportPath, $"{FabReportPrefix}{_outputBolts}"), _title1, _title2, _title3);
             // Operation.CreateReportFromSelected(_report8L, Path.Combine(Folders.ReportPath, $"{FabReportPrefix}{_output8L}"), _title1, _title2, _title3);
            }

            Operation.CreateNCFilesFromSelected("-SNI-PROFILES", Path.Combine(Folders.NcPath, " "));
            Operation.CreateNCFilesFromSelected("-SNI-PLATES", Path.Combine(Folders.NcPath, " "));
        }
    }
}