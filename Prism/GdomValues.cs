using System.Collections;
using System.Collections.Generic;

namespace Prism
{
    public static class GdomValues
    {

        public static string AssemblyPartPrefix = "A";
        public static string AssemblyPrefix = "";
        public static string FabsecName = "FABSEC";
        public static string FabsecClass = "7";

        public static Hashtable PartClass() // This table matches a part name to its correct class according to the GDOM
        {
            Hashtable partClassTable = new Hashtable();
            partClassTable.Add("BEAM", new List<string> { "3" });
            partClassTable.Add("COLUMN", new List<string> { "2", "5" });
            partClassTable.Add("BRACE", new List<string> { "4", "13" });
            partClassTable.Add(FabsecName, new List<string> { FabsecClass });
            partClassTable.Add("RAFTER", new List<string> { "8" });
            partClassTable.Add("PORTAL RAFTER", new List<string> { "8" });
            return partClassTable;
        }
    }
}