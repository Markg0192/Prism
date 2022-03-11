using System.Collections;
using System.IO;
using Tekla.Structures.Model;
using Tekla.Structures.Model.Operations;
using Tekla.Structures.Catalogs;

namespace Prism
{
    /// <summary>
    /// The Report Manager class is where all reports are created.
    /// NC files are also created from here.
    /// </summary>
    public class ReportManager
    {
        public SevFolders Folders;
        private SevModelData _modelData;
        public readonly string FabReportPrefix;
        public readonly string MatReportPrefix;
        private string _title1;
        private string _title2;
        private string _title3;
        private string _report2QS;
        private string _report3; 
        private string _report3PG;
        private string _report4;
        private string _report7;
        private string _report7s;
        private string _report8;
        private string _report8L;
        private string _report8s;
        private string _report9;
        private string _pdfPocListOutput;
        private string _niFirmFolderReportPath;
        private const string _report2QSname = "-2QS-SNI-AssemblyBreakdownList.rpt";
        private const string _report3name = "-3-SNI-HotRolledMemList.rpt";
        private const string _report3Pname = "-3P-SNI-PrelimHotRolledMemList.rpt";
        private const string _report3PAname = "-3PA-SNI-PrelimHotRolledMemList-ADD.rpt";
        private const string _report3POname = "-3PO-SNI-PrelimHotRolledMemList-OMIT.rpt";
        private const string _report3PGname = "-3_PG-SNI-HotRolledPlateGirderList.rpt";
        private const string _report4name = "-4-SNI-HotRolledFitList.rpt";
        private const string _report7name = "-7-SNI-ShopBoltList.rpt";
        private const string _report7sname = "-7S-SNI-ShopSTUDList.rpt";
        private const string _report8name = "-8-SNI-SiteBoltList.rpt";
        private const string _report8Lname = "-8L-SNI-SiteBoltLocationList.rpt";
        private const string _report8sname = "-8s-SNI-SiteSTUDList.rpt";
        private const string _report9name = "-9-SNI-SiteDeliveryBatchList.rpt";
        private const string _output2QS = "-2QS-SNI-AssemblyBreakdownList.xsr";
        private const string _output3 = "-3-SNI-HotRolledMemList.xsr";
        private const string _output3Pname = "-3P-SNI-PrelimHotRolledMemList.xsr";
        private const string _output3PAname = "-3PA-SNI-PrelimHotRolledMemList-ADD.xsr";
        private const string _output3POname = "-3PO-SNI-PrelimHotRolledMemList-OMIT.xsr";
        private const string _output3PG = "-3_PG-SNI-HotRolledPlateGirderList.xsr";
        private const string _output4 = "-4-SNI-HotRolledFitList.xsr";
        private const string _output7 = "-7-SNI-ShopBoltList.xsr";
        private const string _output7s = "-7S-SNI-ShopSTUDList.xsr";
        private const string _output8 = "-8-SNI-SiteBoltList.xsr";
        private const string _output8L = "-8L-SNI-SiteBoltLocationList.xsr";
        private const string _output8s = "-8s-SNI-SiteSTUDList.xsr";
        private const string _output9 = "-9-SNI-SiteDeliveryBatchList.xsr";
        private const string _pdfPocList = "-00-Prism-PDF-POC.pdf.rpt";
        private const string _pdfPocListName = "-00-Prism-PDF-POC.pdf";
        private readonly string _phaseNumber;
        private readonly string _issueNumber;

        public ReportManager(Model model, string phaseNum, string issueNum)
        {
            _phaseNumber = phaseNum;
            _issueNumber = issueNum;
            Folders = model.CreateSevFolders(_phaseNumber, _issueNumber);
            _modelData = model.CreateSevModelData();
            FabReportPrefix = ($"{_modelData.ProjNumber}-{_phaseNumber}-FAB-ISSUE{_issueNumber}");
            MatReportPrefix = ($"{_modelData.ProjNumber}-{_phaseNumber}-PRELIM-ISSUE{_issueNumber}");
            _title1 = _phaseNumber;
            _title2 = _modelData.Initials;
            _title3 = _issueNumber;
        }

        public void CreateMaterialReports(ArrayList partsList, string orderType)
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

        public void CreateFabReports(ArrayList partsList, ArrayList boltList, string packageLocation)
        {
            _niFirmFolderReportPath = $"C:/Sev_Firm_2019i/Roles/{packageLocation}/Reports";
            _report2QS = Path.Combine(_niFirmFolderReportPath, _report2QSname);
            _report3 = Path.Combine(_niFirmFolderReportPath, _report3name);
            _report3PG = Path.Combine(_niFirmFolderReportPath, _report3PGname);
            _report4 = Path.Combine(_niFirmFolderReportPath, _report4name);
            _report7 = Path.Combine(_niFirmFolderReportPath, _report7name);
            _report7s = Path.Combine(_niFirmFolderReportPath, _report7sname);
            _report8 = Path.Combine(_niFirmFolderReportPath, _report8name);
            _report8L = Path.Combine(_niFirmFolderReportPath, _report8Lname);
            _report8s = Path.Combine(_niFirmFolderReportPath, _report8sname);
            _report9 = Path.Combine(_niFirmFolderReportPath, _report9name);
            _pdfPocListOutput = Path.Combine(Folders.ModelPath, _pdfPocList);

            bool create3Report = false;
            bool create3PGReport = false;
            bool create4Report = false;
            bool create7Report = false;
            bool create7sReport = false;
            bool create8Report = false;
            bool create8sReport = false;
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

            foreach (BoltGroup bolt in boltList)
            {
                bool isSiteBolt = bolt.BoltType != BoltGroup.BoltTypeEnum.BOLT_TYPE_WORKSHOP;
                bool isShopBolt = bolt.BoltType == BoltGroup.BoltTypeEnum.BOLT_TYPE_WORKSHOP;
                bool isShearStud = bolt.BoltStandard == "SHEAR-STUD";
                if (isShopBolt && !isShearStud) create7Report = true;
                if (isShopBolt && isShearStud) create7sReport = true;
                if (isSiteBolt && !isShearStud) create8Report = true;
                if (isSiteBolt && isShearStud) create8sReport = true;
            }
            Operation.CreateReportFromSelected(_report2QS, Path.Combine(Folders.DspPath, $"{FabReportPrefix}{_output2QS}"), _title1, _title2, _title3);
            Operation.CreateReportFromSelected(_report9, Path.Combine(Folders.ReportPath, $"{FabReportPrefix}{_output9}"), _title1, _title2, _title3);

            if (create3Report)
            {
                Operation.CreateReportFromSelected(_report3, Path.Combine(Folders.ReportPath, $"{FabReportPrefix}{_output3}"), _title1, _title2, _title3);
                Operation.CreateReportFromSelected(_pdfPocListOutput, Path.Combine(Folders.ReportPath, $"{FabReportPrefix}{_pdfPocListName}"), _title1, _title2, _title3);
            }
            if (create3PGReport)
            {
                Operation.CreateReportFromSelected(_report3PG, Path.Combine(Folders.ReportPath, $"{FabReportPrefix}{_output3PG}"), _title1, _title2, _title3);
            }
            if (create4Report)
            {
                Operation.CreateReportFromSelected(_report4, Path.Combine(Folders.ReportPath, $"{FabReportPrefix}{_output4}"), _title1, _title2, _title3);
            }
            if (create7Report)
            {
                Operation.CreateReportFromSelected(_report7, Path.Combine(Folders.ReportPath, $"{FabReportPrefix}{_output7}"), _title1, _title2, _title3);
            }
            if (create7sReport)
            {
                Operation.CreateReportFromSelected(_report7s, Path.Combine(Folders.ReportPath, $"{FabReportPrefix}{_output7s}"), _title1, _title2, _title3);
            }
            if (create8Report)
            {
                Operation.CreateReportFromSelected(_report8, Path.Combine(Folders.ReportPath, $"{FabReportPrefix}{_output8}"), _title1, _title2, _title3);
                Operation.CreateReportFromSelected(_report8L, Path.Combine(Folders.ReportPath, $"{FabReportPrefix}{_output8L}"), _title1, _title2, _title3);
            }
            if (create8sReport)
            {
                Operation.CreateReportFromSelected(_report8s, Path.Combine(Folders.ReportPath, $"{FabReportPrefix}{_output8s}"), _title1, _title2, _title3);
            }
            Operation.CreateNCFilesFromSelected("-SNI-PROFILES", Path.Combine(Folders.NcPath, " "));
            Operation.CreateNCFilesFromSelected("-SNI-PLATES", Path.Combine(Folders.NcPath, " "));
        }
    }
}