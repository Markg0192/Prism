using System;
using System.Collections.Generic;

namespace Prism
{
    public static class Constants
    {
        public static string RefreshDrawingsMacro = "PrismRefreshDrawings.cs";
        public static string DrawingPrinterMacro = "PrismDrawingPrintss.cs";
        public static string DrawingOperation = "PrismDrawingOperation.cs";
        public static string IssueDrawings = "IssueStampDrawings.cs";
        public static string PrismDataLogLocation = @"\\sev-los-fs1\application data$\Prism\Model Data";

        public static string ModelDataLogLocation(string jobName)
        {
           return $@"\\sev-los-fs1\application data$\Prism\Model Data\{jobName}";
        }

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

                "mark.gibson",
                "Allister.Jackson"
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
