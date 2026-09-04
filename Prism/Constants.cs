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
        public static string NumberSelectedPartsMacro = "NumberSelected.cs";
        public static string PrismPackageFolderName = "Prism Packages";
        public static string SelectedDrawings = "SelectDrawings.cs";
        public static string RunPrismDrawingList = "RunPrismDrawingList.cs";

        public static int PrismDataLogLocation = 1;
        public static int PrismDebugLogLoction = 2;
        public static int PrismTotalUseLogLocation = 3;
        public static int PrismLoginLogLocation = 4;
        public static int PrismLogLocation = 5;
        public static int PrismUnassignedDrawingsLocation = 6;
        public static int PrismUserUserLogLocation = 7;
        public static int PrismModelData = 8;
        public static int PrismHelp = 9;
        public static int PrismNCPassed = 10;
        public static int PrismNCFailed = 11;
        public static int PrismExceptions = 12;
        public static int PrismLatestVersion = 13;
        public static int PrismLatestVersionTracking = 14;


		public static int NewPrismDataLogLocation = 15;
		public static int NewPrismDebugLogLoction = 16;
		public static int NewPrismTotalUseLogLocation = 17;
		public static int NewPrismLoginLogLocation = 18;
		public static int NewPrismLogLocation = 19;
		public static int NewPrismUnassignedDrawingsLocation = 20;
		public static int NewPrismUserUserLogLocation = 21;
		public static int NewPrismModelData = 22;
		public static int NewPrismHelp = 23;
		public static int NewPrismNCPassed = 24;
		public static int NewPrismNCFailed = 25;
		public static int NewPrismExceptions = 26;
		public static int NewPrismLatestVersion = 27;
		public static int NewPrismLatestVersionTracking = 28;

		public static string FabsecCarcassIndicator = "Fabsec Carcass";
        public static string FabsecModelShaftIndicator = "Carcass Created From Member";

        public static string OmitGraveyrdViewName = "Omit Graveyard";

        public static string ModelDataLogLocation(string jobName)
        {
           return $@"\\sev-los-fs1\application data$\Prism\Model Data\{jobName}";
        }

		public static string ModelProjectInforLocation(string jobName)
        {
            return $@"{jobName}\Project Info.txt";
          //  return $@"\\sev-los-fs1\application data$\Prism\Model Data\{jobName}\Project Info.txt";
        }

        public static string ModelProjectAdvancedSettingLocation(string jobName)
        {
            return $@"{jobName}\Advanced Settings.txt";
            //  return $@"\\sev-los-fs1\application data$\Prism\Model Data\{jobName}\Project Info.txt";
        }

        /// <summary>
        /// if a use is a special person then all logging will be ignored (new interface app only)
        /// </summary>
        /// <returns></returns>
        public static bool IsSpecialPerson()
        {
            if (Environment.UserName == "mark.gibson")
            {
                return true;
            }
            return false;
        }

        public static bool SpecialOperationUser()
        {
            List<string> specialOperationUsers = new List<string>
            {
				"mark.glibson"
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
