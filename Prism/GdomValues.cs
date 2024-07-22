using System.Collections;
using System.Collections.Generic;
using static Prism.Enums;

namespace Prism
{
    public static class GdomValues
    {
        public static double MinimumFittingLength = 200; //the minimum length a UB/UC etc. fitting needs to be to be considered for ordered
        public static string IntumescentCode = "IP";
        public static string AssemblyPartPrefix = "A";
        public static string AssemblyPrefix = "";
        public static string FabsecName = "FABSEC";
        public static string FabsecClass = "7";
        public static string ColumnName = "COLUMN";
        public static string BeamName = "BEAM";
        public static string RafterName = "RAFTER";
        public static string PortalRafterName = "PORTAL-RAFTER";
        public static string BraceName = "BRACE";
        public static string SeversafePotProfile = "SHS50*50*4.0";

        public static string TemporaryObjectClass = "112";

        public static int MaxFittingLength(Factory factory, double plateThickness, bool isFlat)
        {
            if (factory == Factory.SUK)
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


        public static Dictionary<string, List<string>> PartClass()
        {
            Dictionary<string, List<string>> partClassDictionary = new Dictionary<string, List<string>>()
             {
                 { BeamName, new List<string> { "3" } },
                 { ColumnName, new List<string> { "2", "5" } },
                 { BraceName, new List<string> { "4", "13" } },
                 { FabsecName, new List<string> { "FabsecClass" } },
                 { RafterName, new List<string> { "8" } },
                 { PortalRafterName, new List<string> { "8" } },                 
             };
            return partClassDictionary;
        }

        public static List<string> ApprovedFittingGrades(bool isFlat)
        {
            if (isFlat)
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
            List<string> flatBarList = new List<string>
            {
                "PLT6*25",
                "PLT6*40",
                "PLT6*50",
                "PLT6*60",
                "PLT6*65",
                "PLT6*80",
                "PLT6*100",
                "PLT6*150",

                "PLT8*80",
                "PLT8*100",
                "PLT8*150",

                "PLT10*40",
                "PLT10*50",
                "PLT10*60",
                "PLT10*65",
                "PLT10*70",
                "PLT10*75",
                "PLT10*80",
                "PLT10*90",
                "PLT10*100",
                "PLT10*120",
                "PLT10*130",
                "PLT10*150",
                "PLT10*180",
                "PLT10*200",
                "PLT10*250",
                "PLT10*300",
                "PLT10*350",

                "PLT12*50",
                "PLT12*65",
                "PLT12*80",
                "PLT12*100",
                "PLT12*120",
                "PLT12*130",
                "PLT12*150",
                "PLT12*180",
                "PLT12*200",
                "PLT12*250",
                "PLT12*300",
                "PLT12*350",
                "PLT12*450",

                "PLT15*100",
                "PLT15*120",
                "PLT15*130",
                "PLT15*150",
                "PLT15*180",
                "PLT15*200",
                "PLT15*250",
                "PLT15*300",
                "PLT15*350",
                "PLT15*400",
                "PLT15*450",

                "PLT20*100",
                "PLT20*120",
                "PLT20*130",
                "PLT20*150",
                "PLT20*180",
                "PLT20*200",
                "PLT20*250",
                "PLT20*300",
                "PLT20*350",
                "PLT20*400",
                "PLT20*450",

                "PLT25*100",
                "PLT25*150",
                "PLT25*180",
                "PLT25*200",
                "PLT25*250",
                "PLT25*300",
                "PLT25*350",
                "PLT25*400",
                "PLT25*450"
            };

            return flatBarList;
        }

        public static Hashtable PageSizes() // The standard page length and widths and their page size
        {
            Hashtable pageSizeTable = new Hashtable
            {
                { "1152x821", "A0" },
                { "1500x821", "A0" },
                { "1800x821", "A0" },
                { "2100x821", "A0" },
                { "2500x821", "A0" },
                { "804x557", "A1" },
                { "584x410", "A2" },
                { "410x287", "A3" } };

            return pageSizeTable;
        }
    }
}