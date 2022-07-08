using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism
{
    public static class GdomValues
    {
        public static Hashtable PartClass() // This table matches a part name to its correct class according to the GDOM
        {
            Hashtable partClassTable = new Hashtable();
            partClassTable.Add("BEAM", new List<string> { "3" });
            partClassTable.Add("COLUMN", new List<string> { "2", "5" });
            partClassTable.Add("BRACE", new List<string> { "4", "13" });
            partClassTable.Add("FABSEC", new List<string> { "7" });
            partClassTable.Add("RAFTER", new List<string> { "8" });
            partClassTable.Add("PORTAL RAFTER", new List<string> { "8" });
            return partClassTable;
        }
    }
}