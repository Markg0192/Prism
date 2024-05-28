using System;
using System.Collections.Generic;

namespace Prism
{
    public static class Constants
    {
        public static string ForceFabsecNumber = "ForceFabsecNumber.cs";
        public static string RefreshDrawingsMacro = "PrismRefreshDrawings.cs";
        public static string DrawingPrinterMacro = "PrismDrawingPrintss.cs";
        public static string DrawingOperation = "PrismDrawingOperation.cs";
        public static string IssueDrawings = "IssueStampDrawings.cs";
        public static string ClearPrintDialog = "ClearPrintDialog.cs";
        public static string PrismPackageFolderName = "Prism Packages";

        public static int PrismDataLogLocation = 1;
        public static int PrismDebugLogLoction = 2;
        public static int PrismTotalUseLogLocation = 3;
        public static int PrismLoginLogLocation = 4;
        public static int PrismLogLocation = 5;
        public static int PrismUnassignedDrawingsLocation = 6;
        public static int PrismUserUserLogLocation = 7;
        public static int PrismModelData = 8;
        public static int PrismNCFailed = 9;

        public static string FabsecCarcassIndicator = "Fabsec Carcass";
        public static string FabsecModelShaftIndicator = "Carcass Created From Member";

        public static string ModelDataLogLocation(string jobName)
        {
           return $@"\\sev-los-fs1\application data$\Prism\Model Data\{jobName}";
        }

        public static string ModelProjectInforLocation(string jobName)
        {
            return $@"{jobName}\Project Info.txt";
          //  return $@"\\sev-los-fs1\application data$\Prism\Model Data\{jobName}\Project Info.txt";
        }

        public static bool IsSpecialPerson()
        {
            if (Environment.UserName == "mar k.gibson")
            {
                return true;
            }
            return false;
        }

        public static bool SpecialOperationUser()
        {
            List<string> specialOperationUsers = new List<string>
            {
                "David.Hunter",
                "Ian.Partridge",
                "mark.gibson",
                "conan.mulholland",
                "Matthew.Poots",
            };

            foreach (string user in specialOperationUsers)
            {
                if (Environment.UserName == user)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
