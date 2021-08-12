using System;
using System.Collections;
using System.IO;
using Tekla.Structures.Model;
using Tekla.Structures.Model.Operations;
using Tekla.Structures.Catalogs;

namespace Prism
{
    /// <summary>
    /// The Report Manager class is where all reports required for a fab package are created.
    /// NC files and bswx information is also created from here.
    /// </summary>
    public class ReportManager
    {
        public SevFolders folders;
        private SevModelData modelData;
        private SevModelEnumerator modelEnum;
        private readonly string reportPrefix;
        private string title1;
        private string title2;
        private string title3;
        private string report2QS;
        private string report3;
        private string report3PG;
        private string report4;
        private string report7;
        private string report7s;
        private string report8;
        private string report8L;
        private string report8s;
        private string report9;
        private string niFirmFolderReportPath;
        private const string report2QSname = "-2QS-SNI-AssemblyBreakdownList.rpt";
        private const string report3name = "-3-SNI-HotRolledMemList.rpt";
        private const string report3PGname = "-3_PG-SNI-HotRolledPlateGirderList.rpt";
        private const string report4name = "-4-SNI-HotRolledFitList.rpt";
        private const string report7name = "-7-SNI-ShopBoltList.rpt";
        private const string report7sname = "-7S-SNI-ShopSTUDList.rpt";
        private const string report8name = "-8-SNI-SiteBoltList.rpt";
        private const string report8Lname = "-8L-SNI-SiteBoltLocationList.rpt";
        private const string report8sname = "-8s-SNI-SiteSTUDList.rpt";
        private const string report9name = "-9-SNI-SiteDeliveryBatchList.rpt";
        private const string output2QS = "-2QS-SNI-AssemblyBreakdownList.xsr";
        private const string output3 = "-3-SNI-HotRolledMemList.xsr";
        private const string output3PG = "-3_PG-SNI-HotRolledPlateGirderList.xsr";
        private const string output4 = "-4-SNI-HotRolledFitList.xsr";
        private const string output7 = "-7-SNI-ShopBoltList.xsr";
        private const string output7s = "-7S-SNI-ShopSTUDList.xsr";
        private const string output8 = "-8-SNI-SiteBoltList.xsr";
        private const string output8L = "-8L-SNI-SiteBoltLocationList.xsr";
        private const string output8s = "-8s-SNI-SiteSTUDList.xsr";
        private const string output9 = "-9-SNI-SiteDeliveryBatchList.xsr";
        private readonly string phaseNumber;
        private readonly string issueNumber;

        public ReportManager(Model model, string phaseNum, string issueNum)
        {
            phaseNumber = phaseNum;
            issueNumber = issueNum;
            folders = model.SevFolders(phaseNumber, issueNumber);
            modelData = model.SevModelData();
            modelEnum = model.SevModelEnumerator();
            reportPrefix = ($"{modelData.ProjNumber}-{phaseNumber}-FAB-ISSUE{issueNumber}");
            title1 = phaseNumber.ToString();
            title2 = modelData.Initials;
            title3 = issueNumber.ToString();
        }

        public void CreateReports(ArrayList partsList, ArrayList boltList, string packageLocation)
        {
            niFirmFolderReportPath = $"C:/Sev_Firm_2019i/Roles/{packageLocation}/Reports";
            report2QS = Path.Combine(niFirmFolderReportPath, report2QSname);
            report3 = Path.Combine(niFirmFolderReportPath, report3name);
            report3PG = Path.Combine(niFirmFolderReportPath, report3PGname);
            report4 = Path.Combine(niFirmFolderReportPath, report4name);
            report7 = Path.Combine(niFirmFolderReportPath, report7name);
            report7s = Path.Combine(niFirmFolderReportPath, report7sname);
            report8 = Path.Combine(niFirmFolderReportPath, report8name);
            report8L = Path.Combine(niFirmFolderReportPath, report8Lname);
            report8s = Path.Combine(niFirmFolderReportPath, report8sname);
            report9 = Path.Combine(niFirmFolderReportPath, report9name);
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
                bool isSiteBolt = bolt.BoltType == BoltGroup.BoltTypeEnum.BOLT_TYPE_WORKSHOP;
                bool isShopBolt = bolt.BoltType != BoltGroup.BoltTypeEnum.BOLT_TYPE_WORKSHOP;
                bool isShearStud = bolt.BoltStandard == "SHEAR-STUD";
                if (isShopBolt && !isShearStud) create7Report = true;
                if (isShopBolt && isShearStud) create7sReport = true;
                if (isSiteBolt && !isShearStud) create8Report = true;
                if (isSiteBolt && isShearStud) create8sReport = true;
            }

            Operation.CreateReportFromSelected(report2QS, Path.Combine(folders.dspPath, $"{reportPrefix}{output2QS}"), title1, title2, title3);
            Operation.CreateReportFromSelected(report9, Path.Combine(folders.reportPath, $"{reportPrefix}{output9}"), title1, title2, title3);

            if (create3Report)
            {
                Console.WriteLine("3 List Produced");
                Operation.CreateReportFromSelected(report3, Path.Combine(folders.reportPath, $"{reportPrefix}{output3}"), title1, title2, title3);
            }
            if (create3PGReport)
            {
                Console.WriteLine("3PG List Produced");
                Operation.CreateReportFromSelected(report3PG, Path.Combine(folders.reportPath, $"{reportPrefix}{output3PG}"), title1, title2, title3);
            }
            if (create4Report)
            {
                Console.WriteLine("4 List Produced");
                Operation.CreateReportFromSelected(report4, Path.Combine(folders.reportPath, $"{reportPrefix}{output4}"), title1, title2, title3);
            }
            if (create7Report)
            {
                Console.WriteLine("7 List Produced");
                Operation.CreateReportFromSelected(report7, Path.Combine(folders.reportPath, $"{reportPrefix}{output7}"), title1, title2, title3);
            }
            if (create7sReport)
            {
                Console.WriteLine("7s List Produced");
                Operation.CreateReportFromSelected(report7s, Path.Combine(folders.reportPath, $"{reportPrefix}{output7s}"), title1, title2, title3);
            }
            if (create8Report)
            {
                Console.WriteLine("8 List Produced");
                Operation.CreateReportFromSelected(report8, Path.Combine(folders.reportPath, $"{reportPrefix}{output8}"), title1, title2, title3);
                Operation.CreateReportFromSelected(report8L, Path.Combine(folders.reportPath, $"{reportPrefix}{output8L}"), title1, title2, title3);
            }
            if (create8sReport)
            {
                Console.WriteLine("8s List Produced");
                Operation.CreateReportFromSelected(report8s, Path.Combine(folders.reportPath, $"{reportPrefix}{output8s}"), title1, title2, title3);
            }
            Operation.CreateNCFilesFromSelected("-SNI-PROFILES", Path.Combine(folders.ncPath, " "));
            Operation.CreateNCFilesFromSelected("-SNI-PLATES", Path.Combine(folders.ncPath, " "));
        }
    }
}
