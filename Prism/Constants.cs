using System;
using System.Collections.Generic;

namespace Prism
{
    public static class Constants
    {
        public static string RefreshDrawingsMacro = "PrismRefreshDrawings.cs";
        public static string DrawingOperation = "PrismDrawingOperation.cs";
        public static string IssueDrawings = "IssueStampDrawings.cs";

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
                "conan.mulholland",
                "Ian.Partridge",
                "Matthew.Poots",
                "mark.gibson"
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
