using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Tekla.Structures.Model;
using Tekla.Structures.Model.UI;
using static Prism.Enums;

namespace Prism
{
    public static class CheckFittings
    {
        public static List<ModelObject> IncorrectGrade = new List<ModelObject>();
        public static List<ModelObject> IncorrectThickness = new List<ModelObject>();
        public static List<ModelObject> IncorrectLength = new List<ModelObject>();
        public static List<Part> AllIncorrectPlate = new List<Part>();

        public static void GetIncorrectFittings(this Part part, Factory factory)
        {
            if (part.Profile.ProfileString.StartsWith("PLT") && !part.HasBeenOrdered(true))
            {
                GetIncorrectPlate(part, factory);
            }
        }

        private static void GetIncorrectPlate(Part myPart, Factory factory)
        {
            string profile = myPart.Profile.ProfileString;
            double plateThickness = 0;
            myPart.GetReportProperty("WEB_THICKNESS", ref plateThickness);

            double length = ModelModifiers.GetPartLength(myPart);
            bool isFlat = IsPartFlatBar(factory, profile, myPart.Material.MaterialString, length, plateThickness);

            GetFittingsWithIncorrectMaterial(myPart.Material.MaterialString, isFlat, myPart);
            GetFittingsWithIncorrectThickness(plateThickness, factory, myPart);
            GetFittingsWhichAreTooLong(myPart, factory, plateThickness, isFlat, length);
        }

        private static void GetFittingsWithIncorrectMaterial(string grade, bool isFlat, Part myPart)
        {
            List<string> approvedGrades = GdomValues.ApprovedFittingGrades(isFlat);
            string approvedGrade = approvedGrades.FirstOrDefault(x => grade == x);
            if (approvedGrade == null)
            {
                AllIncorrectPlate.Add(myPart);
                IncorrectGrade.Add(myPart);
            }
        }

        private static void GetFittingsWithIncorrectThickness(double plateThickness, Factory factory, Part myPart)
        {
            List<int> approvedThicknesses = GdomValues.ApprovedFittingThickness(factory);
            int thickness = approvedThicknesses.FirstOrDefault(x => plateThickness == x);
            if (thickness == 0)
            {
                AllIncorrectPlate.Add(myPart);
                IncorrectThickness.Add(myPart);
            }
        }

        private static void GetFittingsWhichAreTooLong(Part myPart, Factory factory, double plateThickness, bool isFlat, double length)
        {
            if (length > GdomValues.MaxFittingLength(factory, plateThickness, isFlat))
            {
                AllIncorrectPlate.Add(myPart);
                IncorrectLength.Add(myPart);
            }
        }

        private static bool IsPartFlatBar(Factory factory, string profile, string grade, double length, double plateThickness)
        {
            if (factory == Factory.SUK)
            {
                return false;
            }
            if (factory == Factory.SNI)
            {
                List<string> flatBars = GdomValues.FlatBarList();
                string flatBar = flatBars.FirstOrDefault(x => profile == x);
                if (flatBar == null) //Then the profile cannot be a flat bar
                {
                    return false;
                }

                if (flatBar != null && grade == "S275JR" && length <= 6000) //Is a potential flat bar profile, is the correct grade and under acceptable length.
                {
                    return true;
                }

                if (flatBar != null && grade != "S275JR" && length <= 6000)  //Is a potential flat bar profile but is the incorrect grade.
                {
                    if (plateThickness <= 15)
                    {
                        if (length > 1500) //if this is false then it can be made from sheet plate, if true we should special order flat
                        {
                            return true;
                        }
                        return false; //Because it can be made from sheet instead.
                    }
                    else
                    {
                        if (length > 2500) //if this is false then it can be made from sheet plate.
                        {
                            return true;
                        }
                        return false; //because it can be made from sheet instead.
                    }
                }
            }
            return true;
        }

        public static bool DisplayFittingErrors()
        {
            if (IncorrectThickness.Count() != 0 || IncorrectLength.Count() != 0 || IncorrectGrade.Count() != 0)
            {
                PrismWarnings.Warning = $"\rThere are {IncorrectGrade.Count()} parts selected with a non-standard grade (See red in the model).\r" +
                $"There are {IncorrectLength.Count()} parts selected with a non-standard length (See green in the model).\r" +
                $"There are {IncorrectThickness.Count()} parts selected with a non-standard thickness (See blue in the model).";

                PrismWarnings.AbnormalFittings();
                bool tagFittings = PrismWarnings.TagAbnormalFittings();
                if(tagFittings)
                {
                    ModelModifiers.ModifySpecialTag("Special", IncorrectLength);
                    ModelModifiers.ModifySpecialTag("Special", IncorrectThickness);
                    ModelModifiers.ModifySpecialTag("Special", IncorrectGrade);
                }
                ModelObjectVisualization.SetTransparencyForAll(TemporaryTransparency.SEMITRANSPARENT);
                ModelObjectVisualization.SetTemporaryStateForAll(new Color(0.5, 0.5, 0.5));

                ModelObjectVisualization.SetTemporaryState(IncorrectThickness, new Color(0, 0, 1));
                ModelObjectVisualization.SetTemporaryState(IncorrectLength, new Color(0, 1, 0));
                ModelObjectVisualization.SetTemporaryState(IncorrectGrade, new Color(1, 0, 0));
                return PrismWarnings.IgnoreWarning();
            }
            return true;
        }
    }
}