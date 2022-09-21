using System.Collections;
using System.Collections.Generic;
using static Prism.Enums;

namespace Prism
{
    public static class GdomValues
    {
        public static string IntumescentCode = "IP";
        public static string AssemblyPartPrefix = "A";
        public static string AssemblyPrefix = "";
        public static string FabsecName = "FABSEC";
        public static string FabsecClass = "7";

        public static int MaxFittingLength(Factory factory, int plateThickness, bool isFlat)
        {
            if(factory == Factory.SUK)
            {
                return 1900;
            }
            if (factory == Factory.SNI)
            {
                if (!isFlat)
                {
                    if (plateThickness < 15.5)
                    {
                        return 1500;
                    }
                    return 2500;
                }
            }
            return 6000;
        }

        public static Hashtable PartClass() // This table matches a part name to its correct class according to the GDOM
        {
            Hashtable partClassTable = new Hashtable();
            partClassTable.Add("BEAM", new List<string> { "3" });
            partClassTable.Add("COLUMN", new List<string> { "2", "5" });
            partClassTable.Add("BRACE", new List<string> { "4", "13" });
            partClassTable.Add(FabsecName, new List<string> { FabsecClass });
            partClassTable.Add("RAFTER", new List<string> { "8" });
            partClassTable.Add("PORTAL-RAFTER", new List<string> { "8" });
            return partClassTable;
        }

        public static List<string> ApprovedFittingGrades(bool isFlat)
        {
            if(isFlat)
            {
                return new List<string>() { "S275JR" };
            }
            return new List<string>() { "S275JR", "S355JR", "S355J0", "S355J2", "S355J2+N" };
        }

        public static List<int> ApprovedFittingThickness(Factory factory)
        {
            if (factory == Factory.SUK)
            {
                return new List<int>() { 3, 6, 8, 10, 12, 15, 20, 25, 30, 35, 40, 45, 50 };
            }
            if (factory == Factory.SNI)
            {
                return new List<int>() { 3, 6, 8, 10, 12, 15, 20, 25, 30, 35, 40, 45, 50, 55, 60, 65, 70, 75, 80 };
            }
            return null;
        }

        public static List<string> FlatBarList()
        {
            List<string> flatBarList = new List<string>();
            flatBarList.Add("PLT6*25");
            flatBarList.Add("PLT6*40");
            flatBarList.Add("PLT6*50");
            flatBarList.Add("PLT6*60");
            flatBarList.Add("PLT6*65");
            flatBarList.Add("PLT6*80");
            flatBarList.Add("PLT6*100");
            flatBarList.Add("PLT6*150");

            flatBarList.Add("PLT8*80");
            flatBarList.Add("PLT8*100");
            flatBarList.Add("PLT8*150");

            flatBarList.Add("PLT10*40");
            flatBarList.Add("PLT10*50");
            flatBarList.Add("PLT10*60");
            flatBarList.Add("PLT10*65");
            flatBarList.Add("PLT10*70");
            flatBarList.Add("PLT10*75");
            flatBarList.Add("PLT10*80");
            flatBarList.Add("PLT10*90");
            flatBarList.Add("PLT10*100");
            flatBarList.Add("PLT10*120");
            flatBarList.Add("PLT10*130");
            flatBarList.Add("PLT10*150");
            flatBarList.Add("PLT10*180");
            flatBarList.Add("PLT10*200");
            flatBarList.Add("PLT10*250");
            flatBarList.Add("PLT10*300");
            flatBarList.Add("PLT10*350");

            flatBarList.Add("PLT12*50");
            flatBarList.Add("PLT12*65");
            flatBarList.Add("PLT12*80");
            flatBarList.Add("PLT12*100");
            flatBarList.Add("PLT12*120");
            flatBarList.Add("PLT12*130");
            flatBarList.Add("PLT12*150");
            flatBarList.Add("PLT12*180");
            flatBarList.Add("PLT12*200");
            flatBarList.Add("PLT12*250");
            flatBarList.Add("PLT12*300");
            flatBarList.Add("PLT12*350");
            flatBarList.Add("PLT12*450");

            flatBarList.Add("PLT15*100");
            flatBarList.Add("PLT15*120");
            flatBarList.Add("PLT15*130");
            flatBarList.Add("PLT15*150");
            flatBarList.Add("PLT15*180");
            flatBarList.Add("PLT15*200");
            flatBarList.Add("PLT15*250");
            flatBarList.Add("PLT15*300");
            flatBarList.Add("PLT15*350");
            flatBarList.Add("PLT15*400");
            flatBarList.Add("PLT15*450");

            flatBarList.Add("PLT20*100");
            flatBarList.Add("PLT20*120");
            flatBarList.Add("PLT20*130");
            flatBarList.Add("PLT20*150");
            flatBarList.Add("PLT20*180");
            flatBarList.Add("PLT20*200");
            flatBarList.Add("PLT20*250");
            flatBarList.Add("PLT20*300");
            flatBarList.Add("PLT20*350");
            flatBarList.Add("PLT20*400");
            flatBarList.Add("PLT20*450");

            flatBarList.Add("PLT25*100");
            flatBarList.Add("PLT25*150");
            flatBarList.Add("PLT25*180");
            flatBarList.Add("PLT25*200");
            flatBarList.Add("PLT25*250");
            flatBarList.Add("PLT25*300");
            flatBarList.Add("PLT25*350");
            flatBarList.Add("PLT25*400");
            flatBarList.Add("PLT25*450");

            return flatBarList;
        }
    }
}