using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism
{
    public static class Constants
    {
        public static string RefreshDrawingsMacro = "PrismRefreshDrawings.cs";

        public static bool IsSpecialPerson()
        { 
            if(Environment.UserName == "mar.gibson")
            {
                return true;
            }
            return false;
        }
    }
}
