using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tekla.Structures.Model;
using Tekla.Structures.Model.Operations;
using Tekla.Structures.Catalogs;

namespace Prism
{
    public class ReportManager
    {
        public SevFolders Folders;
        public SevModelData ModelData;
        public SevModelEnumerator ModelEnum;
        public LibraryProfileItem myProfileItem;
        public readonly string ReportPrefix;
        public string Title1;
        public string Title2;
        public string Title3;
        public string Report2QS;
        public string Report3;        
        public string Report3PG;
        public string Report4;
        public string Report7;
        public string Report7s;
        public string Report8;
        public string Report8L;
        public string Report8s;
        public string Report9;
        public readonly string niFirmFolderReportPath = "C:/Sev_Firm_2019i/Roles/SNI/Reports";
        public const string Report2QSname = "-2QS-SNI-AssemblyBreakdownList.rpt";
        public const string Report3name = "-3-SNI-HotRolledMemList.rpt";
        public const string Report3PGname = "-3_PG-SNI-HotRolledPlateGirderList.rpt";
        public const string Report4name = "-4-SNI-HotRolledFitList.rpt";
        public const string Report7name = "-7-SNI-ShopBoltList.rpt";
        public const string Report7sname = "-7S-SNI-ShopSTUDList.rpt";
        public const string Report8name = "-8-SNI-SiteBoltList.rpt";
        public const string Report8Lname = "-8L-SNI-SiteBoltLocationList.rpt";
        public const string Report8sname = "-8s-SNI-SiteSTUDList.rpt";
        public const string Report9name = "-9-SNI-SiteDeliveryBatchList.rpt";
        public const string Output2QS = "-2QS-SNI-AssemblyBreakdownList.xsr";
        public const string Output3 = "-3-SNI-HotRolledMemList.xsr";
        public const string Output3PG = "-3_PG-SNI-HotRolledPlateGirderList.xsr";
        public const string Output4 = "-4-SNI-HotRolledFitList.xsr";
        public const string Output7 = "-7-SNI-ShopBoltList.xsr";
        public const string Output7s = "-7S-SNI-ShopSTUDList.xsr";
        public const string Output8 = "-8-SNI-SiteBoltList.xsr";
        public const string Output8L = "-8L-SNI-SiteBoltLocationList.xsr";
        public const string Output8s = "-8s-SNI-SiteSTUDList.xsr";
        public const string Output9 = "-9-SNI-SiteDeliveryBatchList.xsr";
        public readonly string phaseNumber;
        public readonly string issueNumber;

        public ReportManager(Model model, string phaseNum, string issueNum)
        {
            phaseNumber = phaseNum;
            issueNumber = issueNum;
            Report2QS = Path.Combine(niFirmFolderReportPath, Report2QSname);
            Report3 = Path.Combine(niFirmFolderReportPath, Report3name);            
            Report3PG = Path.Combine(niFirmFolderReportPath, Report3PGname);
            Report4 = Path.Combine(niFirmFolderReportPath, Report4name);
            Report7 = Path.Combine(niFirmFolderReportPath, Report7name);
            Report7s = Path.Combine(niFirmFolderReportPath, Report7sname);
            Report8 = Path.Combine(niFirmFolderReportPath, Report8name);
            Report8L = Path.Combine(niFirmFolderReportPath, Report8Lname);
            Report8s = Path.Combine(niFirmFolderReportPath, Report8sname);
            Report9 = Path.Combine(niFirmFolderReportPath, Report9name);
            Folders = model.SevFolders(phaseNumber, issueNumber);
            ModelData = model.SevModelData();
            ModelEnum = model.SevModelEnumerator();
            ReportPrefix = ($"{ModelData.projNumber}-{phaseNumber}-FAB-ISSUE{issueNumber}");
            Title1 = phaseNumber.ToString();
            Title2 = ModelData.Initials;
            Title3 = issueNumber.ToString();
        }
        public void CreateReports(ArrayList partsList, ArrayList boltList)
        {
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
                bool isFitting = false;
                bool isPlateGirder;
                myProfileItem.Select(part.Profile.ProfileString);                
                sectionSize = new string(new char[] { myProfileItem.ProfileName.ToCharArray()[0], myProfileItem.ProfileName.ToCharArray()[1] });
                isPlateGirder = sectionSize == "PG";
                isFitting = sectionSize == "PL" || sectionSize == "RS" || sectionSize == "FL";
                if (!isFitting && !isPlateGirder) create3Report = true;
                if (isPlateGirder) create3PGReport = true;
                if (isFitting) create4Report = true;
            }
            foreach (BoltGroup bolt in boltList)
            {
                bool isSiteBolt;
                bool isShopBolt;
                bool isShearStud;
                isShopBolt = bolt.BoltType == BoltGroup.BoltTypeEnum.BOLT_TYPE_WORKSHOP;
                isSiteBolt = bolt.BoltType != BoltGroup.BoltTypeEnum.BOLT_TYPE_WORKSHOP;
                isShearStud = bolt.BoltStandard == "SHEAR-STUD";
                if (isShopBolt && !isShearStud) create7Report = true;
                if (isShopBolt && isShearStud) create7sReport = true;
                if (isSiteBolt && !isShearStud) create8Report = true;
                if (isSiteBolt && isShearStud) create8sReport = true;
            }
            Operation.CreateReportFromSelected(Report2QS, Path.Combine(Folders.DspPath, $"{ReportPrefix}{Output2QS}"), Title1, Title2, Title3);
            Operation.CreateReportFromSelected(Report9, Path.Combine(Folders.ReportPath, $"{ReportPrefix}{Output9}"), Title1, Title2, Title3);
            if (create3Report)
            {
                if(!File.Exists(Report3))
                {
                    Console.WriteLine("message here");
                }
                Console.WriteLine("3 List Produced");
                Operation.CreateReportFromSelected(Report3, Path.Combine(Folders.ReportPath, $"{ReportPrefix}{Output3}"), Title1, Title2, Title3);
            }
            if (create3PGReport)
            {
                Console.WriteLine("3PG List Produced");
                Operation.CreateReportFromSelected(Report3PG, Path.Combine(Folders.ReportPath, $"{ReportPrefix}{Output3PG}"), Title1, Title2, Title3);
            }
            if (create4Report)
            {
                Console.WriteLine("4 List Produced");
                Operation.CreateReportFromSelected(Report4, Path.Combine(Folders.ReportPath, $"{ReportPrefix}{Output4}"), Title1, Title2, Title3);
            }
            if (create7Report)
            {
                Console.WriteLine("7 List Produced");
                Operation.CreateReportFromSelected(Report7, Path.Combine(Folders.ReportPath, $"{ReportPrefix}{Output7}"), Title1, Title2, Title3);
            }
            if (create7sReport)
            {
                Console.WriteLine("7s List Produced");
                Operation.CreateReportFromSelected(Report7s, Path.Combine(Folders.ReportPath, $"{ReportPrefix}{Output7s}"), Title1, Title2, Title3);
            }
            if (create8Report)
            {
                Console.WriteLine("8 List Produced");
                Operation.CreateReportFromSelected(Report8, Path.Combine(Folders.ReportPath, $"{ReportPrefix}{Output8}"), Title1, Title2, Title3);
                Operation.CreateReportFromSelected(Report8L, Path.Combine(Folders.ReportPath, $"{ReportPrefix}{Output8L}"), Title1, Title2, Title3);
            }
            if (create8sReport)
            {
                Console.WriteLine("8s List Produced");
                Operation.CreateReportFromSelected(Report8s, Path.Combine(Folders.ReportPath, $"{ReportPrefix}{Output8s}"), Title1, Title2, Title3);
            }
            Operation.CreateNCFilesFromSelected("-SNI-PROFILES", Path.Combine(Folders.NCPath, " "));
            Operation.CreateNCFilesFromSelected("-SNI-PLATES", Path.Combine(Folders.NCPath, " "));            
        }
    }
}
