using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Tekla.Structures.Model;
using Tekla.Structures.Model.UI;
using static Prism.Enums;

namespace Prism
{
    public static class CheckFittings
    {
        public static List<PrismPart> IncorrectGrade = new List<PrismPart>();
        public static List<PrismPart> IncorrectThickness = new List<PrismPart>();
        public static List<PrismPart> IncorrectLength = new List<PrismPart>();
        public static List<PrismPart> UnOrderedObjects = new List<PrismPart>();
        public static List<Part> AllIncorrectPlate = new List<Part>();

        public static void GetIncorrectFittings(this Part part, Factory factory)
        {
            if (factory != Factory.Unknown && part.Profile.ProfileString.StartsWith("PLT") && !part.HasBeenOrdered(true))
            {
                GetIncorrectPlate(part, factory);
            }
            if (IsAPartThatShouldBeOrdered(part) && !part.HasBeenOrdered(true))
            {
                UnOrderedObjects.Add(new PrismPart(part) { ErrorString = "Part not Ordered"});
            }
        }

        private static bool IsAPartThatShouldBeOrdered(Part part)
        {
            if (ModelModifiers.GetPartLength(part) >= GdomValues.MinimumFittingLength)
            {
                string partProf = part.Profile.ProfileString;

                if (partProf == GdomValues.SeversafePotProfile) // ignore anything with the seversafe pot profile
                {
                    return false;
                }

                // Define a regular expression pattern to match the required prefixes.
                string pattern = "^(UB|UC|PFC|RSA|PG|WESTOK|UKC|UKB|JUMBO|SHS|CHS|RHS|CF-RHS|CF-CHS|CF-SHS)";

                // Perform the regex match on the partProf string.
                return Regex.IsMatch(partProf, pattern);
            }
            return false;
        }

        private static void GetIncorrectPlate(Part myPart, Factory factory)
        {
            string profile = myPart.Profile.ProfileString;
            double plateThickness = 0;
            myPart.GetReportProperty("WEB_THICKNESS", ref plateThickness);

            double length = ModelModifiers.GetPartLength(myPart);

            ContourPlate cp = myPart as ContourPlate;

            if (cp == null)
            {
                double width = ModelModifiers.GetPartWidth(myPart);
                profile = AddWidthToContour(profile, length, width);

                bool isFlat = IsPartFlatBar(factory, profile, myPart.Material.MaterialString, length, plateThickness);
                GetFittingsWhichAreTooLong(myPart, factory, plateThickness, isFlat, length);
                GetFittingsWithIncorrectMaterial(myPart.Material.MaterialString, isFlat, myPart);
            }
            else
            {
                GetFittingsWithIncorrectMaterial(myPart.Material.MaterialString, false, myPart);
            }

            GetFittingsWithIncorrectThickness(plateThickness, factory, myPart);
        }

        private static string AddWidthToContour(string profile, double length, double width)
        {
            string returnProfile = profile;
            if (profile.Contains("*"))
            {
                return profile;
            }
            else
            {
                return profile + $"*{width}";
            }
        }

        private static void GetFittingsWithIncorrectMaterial(string grade, bool isFlat, Part myPart)
        {
            List<string> approvedGrades = GdomValues.ApprovedFittingGrades(isFlat);
            string approvedGrade = approvedGrades.FirstOrDefault(x => grade == x);
            if (approvedGrade == null)
            {
                AllIncorrectPlate.Add(myPart);
                IncorrectGrade.Add(new PrismPart(myPart) { ErrorString = "Non-Standard Grade"});
            }
        }

        private static void GetFittingsWithIncorrectThickness(double plateThickness, Factory factory, Part myPart)
        {
            List<int> approvedThicknesses = GdomValues.ApprovedFittingThickness(factory);
            int thickness = approvedThicknesses.FirstOrDefault(x => plateThickness == x);
            if (thickness == 0)
            {
                AllIncorrectPlate.Add(myPart);
                IncorrectThickness.Add(new PrismPart(myPart) { ErrorString = "Non-Standard Thickness"});
            }
        }

        private static void GetFittingsWhichAreTooLong(Part myPart, Factory factory, double plateThickness, bool isFlat, double length)
        {
            if (length > GdomValues.MaxFittingLength(factory, plateThickness, isFlat))
            {
                AllIncorrectPlate.Add(myPart);
                IncorrectLength.Add(new PrismPart(myPart) { ErrorString = "Non-Standard Length" });
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
            if (IncorrectThickness.Count() != 0 || IncorrectLength.Count() != 0 || IncorrectGrade.Count() != 0 || UnOrderedObjects.Count() != 0)
            {
                PrismWarnings.Warning = FormErrorMessage();

                ModelModifiers.SetPartsRed(Convertor.PrismPartsToModelObjects(IncorrectGrade));
                ModelModifiers.SetPartsGreen(Convertor.PrismPartsToModelObjects(IncorrectLength), false);
                ModelModifiers.SetPartsBlue(Convertor.PrismPartsToModelObjects(IncorrectThickness), false);
                ModelModifiers.SetPartsYellow(Convertor.PrismPartsToModelObjects(UnOrderedObjects), false);

                // Combine lists into a single list
                List<PrismPart> combinedList = new List<PrismPart>();
                combinedList.AddRange(IncorrectThickness);
                combinedList.AddRange(IncorrectLength);
                combinedList.AddRange(IncorrectGrade);
                combinedList.AddRange(UnOrderedObjects);

                PrismWarnings.AbnormalFittings(combinedList);
                bool tagFittings = PrismWarnings.TagAbnormalFittings();
                if (tagFittings)
                {
                    ModelModifiers.ModifySpecialTag("Special", Convertor.PrismPartsToModelObjects(IncorrectLength));
                    ModelModifiers.ModifySpecialTag("Special", Convertor.PrismPartsToModelObjects(IncorrectThickness));
                    ModelModifiers.ModifySpecialTag("Special", Convertor.PrismPartsToModelObjects(IncorrectGrade));
                    ModelModifiers.ModifySpecialTag("Special", Convertor.PrismPartsToModelObjects(UnOrderedObjects));
                }
                return PrismWarnings.IgnoreWarning();
            }
            return true;
        }

        private static string FormErrorMessage()
        {
            string myMessage = "";
            if (IncorrectGrade.Count() != 0)
            {
                myMessage = myMessage + $"-There {AreSoManyParts(IncorrectGrade.Count())} selected with a non-standard grade (See red in the model).\r";
            }
            if (IncorrectLength.Count() != 0)
            {
                myMessage = myMessage + $"-There {AreSoManyParts(IncorrectLength.Count())} selected with a non-standard length (See green in the model).\r";
            }
            if (IncorrectThickness.Count() != 0)
            {
                myMessage = myMessage + $"-There {AreSoManyParts(IncorrectThickness.Count())} selected with a non-standard thickness (See blue in the model).\r";
            }
            if (UnOrderedObjects.Count() != 0)
            {
                myMessage = myMessage + $"-There {AreSoManyParts(UnOrderedObjects.Count())} selected with a heavy section size that should be ordered (See yellow in model).";
            }
            return myMessage;
        }

        private static string AreSoManyParts(int count)
        {
            return $"{AreIs(count)} {count} {PartParts(count)}";

        }
        private static string AreIs(int count)
        {
            if (count == 1)
            {
                return "is";
            }
            return "are";
        }
        private static string PartParts(int count)
        {
            if (count == 1)
            {
                return "part";
            }
            return "parts";
        }
    }
}