using Prism.Geometry;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;
using static Prism.Enums;

namespace Prism
{
    /// <summary>
    /// The pre run checks class is used to check the model for any issues in the model that may cause the program to crash.
    /// These are ran before the main program and warns the user of the problem and closes the app before it crashes.
    /// </summary>
    public static class ModelChecker
    {
        public static List<PrismPart> IncorrectNameAndClass = new List<PrismPart>();
        public static List<PrismPart> MissingExecutionClass = new List<PrismPart>();
        public static List<PrismPart> HasNoFinish = new List<PrismPart>();
        public static List<PrismPart> StartNumbersDoNotMatch = new List<PrismPart>();
        public static List<PrismPart> PhasesDoNotMatch = new List<PrismPart>();
        public static List<PrismPart> PhasesDoNotMatchParts = new List<PrismPart>();
        public static List<ModelObject> NotOrderedParts = new List<ModelObject>();
        public static List<ModelObject> OrderedParts = new List<ModelObject>();
        public static List<PrismPart> PartsWithoutIntumescentLoading = new List<PrismPart>();
        public static List<PrismPart> IncorrectOrientation = new List<PrismPart>();

        public static void BoltThrough2Ply(SelectedObjects selectedObjects)
        {
            foreach (PrismBoltGroup pbg in selectedObjects.PrismBoltGroups)
            {

                int executionClassData = 10;  //this is the number read from teklas UDA when no execution class is applied, we are defaulting to it not having one here
                                              // p.GetUserProperty(ModelUDA.ExcecutionClass(), ref executionClassData);

                if (executionClassData == 10)
                {
                    //MissingExecutionClass.Add(p);
                }
            }
        }

        public static bool RunStage4Checks(this SelectedObjects selectedObjects, string userName)
        {
            Factory location = PrismWarnings.FactoryLocation();
            if (location == Factory.Unknown)
            {
                PrismWarnings.IgnoreFittingCheck();
            }

            foreach (PrismPart myMainPart in selectedObjects.GetMainParts())
            {
                GetUnorderedParts(myMainPart.Part);
                GetPartsWithoutAFinish(myMainPart.Part);
                CheckForIntumescentLoading(myMainPart.Part);

                ArrayList mySecondaries = myMainPart.Part.GetAssembly().GetSecondaries();
                foreach (Part mySecondaryPart in mySecondaries)
                {
                    GetPartsThatStartNumbersDontMatch(myMainPart.Part, mySecondaryPart);
                    GetPartsWherePhasesDontMatch(myMainPart.Part, mySecondaryPart);
                    CheckFittings.GetIncorrectFittings(mySecondaryPart, location);
                }
            }
            return CheckForAndActionErrors(userName, selectedObjects.PrismBoltGroups);
        }

        private static List<BoltGroup> GetShearStudBoltGroups(List<BoltGroup> boltGroups)
        {
            return boltGroups
                .Where(boltGroup => boltGroup.BoltStandard == "SHEAR-STUD" && CheckBoltProperty(boltGroup))
                .ToList();
        }

        private static bool CheckBoltProperty(BoltGroup boltGroup)
        {
            string property = "";
            boltGroup.GetReportProperty(ModelUDA.BoltShearStudTag(), ref property);
            return string.IsNullOrEmpty(property); // Return true if the property is not null or empty
        }

        private static bool IsShearStud(List<BoltGroup> boltGroup)
        {
            string property = null;
            return boltGroup.Any(bolt =>
            {
                if (bolt.BoltStandard == "SHEAR-STUD")
                {
                    property = "";
                    bolt.GetReportProperty(ModelUDA.BoltShearStudTag(), ref property);
                    return property == "";
                }
                return false;
            });
        }

        public static void ClearOldLists()
        {
            CheckFittings.IncorrectGrade.Clear();
            CheckFittings.IncorrectLength.Clear();
            CheckFittings.IncorrectThickness.Clear();
            CheckFittings.UnOrderedObjects.Clear();
            CheckFittings.AllIncorrectPlate.Clear();
            HasNoFinish.Clear();
            PartsWithoutIntumescentLoading.Clear();
            OrderedParts.Clear();
            NotOrderedParts.Clear();
            StartNumbersDoNotMatch.Clear();
            PhasesDoNotMatch.Clear();
            PhasesDoNotMatchParts.Clear();
        }

        public static bool MemberOrientationIsCorrect(SelectedObjects selectedObjects, out IgnoreType ignore)
        {
            IncorrectOrientation.Clear();
            MemberOrientation(selectedObjects);
            //ModelChecker.IncorrectOrientation.Clear(); //if this line is active all orientation functionallity is disabled

            ignore = PrismWarnings.DisplayOrderErrors(IncorrectOrientation, Error.Orientation);

            if (ignore == IgnoreType.Stop)
            {
                return false;
            }
            return true;
        }

        public static void HasExecutionClass(SelectedObjects selectedObjects)
        {
            foreach (PrismPart p in selectedObjects.GetMainParts())
            {
                int executionClassData = 10;  //this is the number read from teklas UDA when no execution class is applied, we are defaulting to it not having one here
                p.Part.GetUserProperty(ModelUDA.ExcecutionClass(), ref executionClassData);

                if (executionClassData == 10)
                {
                    MissingExecutionClass.Add(p);
                }
            }
        }

        public static void MemberOrientation(SelectedObjects selectedObjects)
        {
            int tolerance = 5;
            foreach (PrismPart pPart in selectedObjects.GetMainParts())
            {
                Beam b = pPart.Part as Beam;
                if (b != null && (b.Profile.ProfileString.StartsWith("UB") || b.Profile.ProfileString.StartsWith("UKB") || b.Profile.ProfileString.StartsWith("UC") || b.Profile.ProfileString.StartsWith("UKC")))
                {
                    if (b.Name == GdomValues.BeamName && Math.Abs(b.StartPoint.Z - b.EndPoint.Z) < tolerance)
                    {
                        CheckBeamOrientation(b);
                    }

                    if (b.Name == GdomValues.ColumnName)
                    {
                        CheckColumnOrientation(b);
                    }

                    if (b.Name.Contains(GdomValues.RafterName))
                    {
                        CheckRafterOrientation(b);
                    }
                    if (b.Name == GdomValues.BraceName)
                    {
                        // CheckBraceOrientation(b);
                    }
                }
            }
        }

        private static void CheckBraceOrientation(Beam b)
        {
            bool check1 = false;
            bool check2 = false;

            bool isVertical = b.StartPoint.X == b.EndPoint.X && b.StartPoint.Y == b.EndPoint.Y ? true : false;

            Point3D p1 = new Point3D(b.StartPoint.X, b.StartPoint.Y, b.StartPoint.Z + 100);
            Point3D p2 = new Point3D(b.StartPoint);
            Point3D p3 = new Point3D(b.EndPoint);

            Geometry.Vector v = new Geometry.Vector(p2, p1);
            Geometry.Vector v2 = new Geometry.Vector(p2, p3);
            double radian = (double)v.AngleBetween(v2);
            double degree = AnglesHelper.Degrees(radian);

            Point startPoint = b.StartPoint;
            Point endPoint = b.EndPoint;
            if (isVertical)
            {
                if (b.StartPoint.Z > b.EndPoint.Z)
                {
                    IncorrectOrientation.Add(new PrismPart(b));
                }
            }
            else
            {
                double a = startPoint.X - endPoint.X;
                double o = startPoint.Y - endPoint.Y;
                double angle = Math.Atan2(o, a);
                double myAngle = 180 * angle / Math.PI;

                if (myAngle < 44.6 && myAngle > -135.4 || myAngle == 180)
                {
                    check1 = true;
                }

                if (myAngle == 90 || myAngle == -90)
                {
                    a = o;
                }

                double o1 = startPoint.Z - endPoint.Z;
                double angle2 = Math.Atan2(o1, a);
                double myAngle2 = 180 * angle2 / Math.PI;

                if (check1)
                {
                    if (myAngle2 <= 44.6 && myAngle2 >= -135.4)
                    {
                        check2 = true;
                    }
                }
                else
                {
                    if (myAngle2 < 44.6 && myAngle2 >= -135.4)
                    {
                        check2 = true;
                    }
                }

                if (check2)
                {
                    IncorrectOrientation.Add(new PrismPart(b));
                }
            }

        }

        private static void CheckRafterOrientation(Beam b)
        {
            //All rafters must be detailed with start point at the apex, so a simple check to make sure start point is higher than end point will do here
            if (b.StartPoint.Z < b.EndPoint.Z)
            {
                IncorrectOrientation.Add(new PrismPart(b));
            }
        }

        private static void CheckColumnOrientation(Beam b)
        {
            //Column rotation must be "FRONT" or "BELOW", therefore "BACK" and "TOP" are wrong, start point must also be lower than end point.
            if (b.Position.Rotation == Position.RotationEnum.BACK || b.Position.Rotation == Position.RotationEnum.TOP || b.StartPoint.Z > b.EndPoint.Z)
            {
                IncorrectOrientation.Add(new PrismPart(b));
            }
        }

        private static void CheckBeamOrientation(Beam b)
        {
            bool check1 = false;
            bool check2 = false;

            Point startPoint = b.StartPoint;
            Point endPoint = b.EndPoint;

            double a = startPoint.X - endPoint.X;
            double o = startPoint.Y - endPoint.Y;
            double angle = Math.Atan2(o, a);
            double myAngle = 180 * angle / Math.PI;

            if (myAngle > -44.6 && myAngle < 135.4)
            {
                check1 = true;
            }

            if (check1 || check2)
            {
                IncorrectOrientation.Add(new PrismPart(b));
            }
        }

        public static void NameAndClassAligned(SelectedObjects selectedObjects)
        {
            foreach (PrismPart p in selectedObjects.PrismParts)
            {
                List<string> meantToBeClass = GdomValues.PartClass()[p.Part.Name] as List<string>;
                if (meantToBeClass != null && !meantToBeClass.Contains(p.Part.Class))
                {
                    IncorrectNameAndClass.Add(p);
                }
            }
        }

        public static bool ArePreviousStepsComplete(SelectedObjects selectedObjects, int stageNumber)
        {
            List<PrismPart> incompleteParts = selectedObjects.GetNonSeversafeParts()
                .Where(p =>
                {
                    string userProperty = "";
                    p.Part.GetUserProperty(ModelUDA.PreviousStageName(stageNumber), ref userProperty);
                    return userProperty == "";
                })
                .Select(p => p)
                .ToList();

            if (incompleteParts.Any())
            {
                PrismWarnings.PreviousStepIncomplete(incompleteParts);
                return false;
            }

            return true;
        }

        private static bool CheckForAndActionErrors(string userName, List<PrismBoltGroup> prismBoltGroups)
        {
            if (!OrderErrors()) { return false; }
            if (!FinishErrors()) { return false; }

            IgnoreType startNumberError = StartNumbersErrors();
            if (startNumberError == IgnoreType.AutoFix)
            {
                AutoFix.AssemblyAndStartNumbers();
            }
            if (startNumberError == IgnoreType.Stop) { return false; }

            IgnoreType phaseMatchError = PhaseMatchErrors();
            if (phaseMatchError == IgnoreType.AutoFix)
            {
                AutoFix.PartPhasing();
            }
            if (phaseMatchError == IgnoreType.Stop) { return false; }

            if (!IntumesecentLoadingErrors()) { return false; }

            if (!ShearStudsNotOrdered(userName, prismBoltGroups)) { return false; }

            return CheckFittings.DisplayFittingErrors();
        }

        private static bool ShearStudsNotOrdered(string userName, List<PrismBoltGroup> prismBoltGroups)
        {
            if (prismBoltGroups.Any(pbg => !pbg.isOrdered && pbg.isShearStud)) // then there are shears studs that appear to not be ordered.
            {
                bool studsOrdered = PrismWarnings.UnorderedShearStuds();

                // If studs are not ordered and user decides not to ignore, return false
                if (!studsOrdered && !PrismWarnings.IgnoreAndContinue())
                {
                    return false;
                }

                // Modify attributes only if studs have been ordered
                if (studsOrdered)
                {
                    foreach (PrismBoltGroup boltGroup in prismBoltGroups.Where(pbg => !pbg.isOrdered && pbg.isShearStud))
                    {
                        string property = "";
                        boltGroup.BoltGroup.GetReportProperty(ModelUDA.BoltShearStudTag(), ref property);
                        if (property == "")
                        {
                            boltGroup.BoltGroup.SetUserProperty(ModelUDA.BoltOrderedBy(), userName);
                            boltGroup.BoltGroup.SetUserProperty(ModelUDA.BoltShearStudTag(), "Ordered");
                        }
                    }
                }
            }
            return true;
        }


        public static IgnoreType PhaseMatchErrors()
        {
            if (PhasesDoNotMatch.Count != 0)
            {
                PrismWarnings.Warning = PhasesDoNotMatch.Count.ToString();

                PrismWarnings.PhasesDontMatch(PhasesDoNotMatch);

                ModelModifiers.SetPartsRed(Convertor.PrismPartsToModelObjects(PhasesDoNotMatchParts));

                return PrismWarnings.NewIgnoreWarning();
            }
            return IgnoreType.Unspecified;
        }

        public static IgnoreType StartNumbersErrors()
        {
            if (StartNumbersDoNotMatch.Count != 0)
            {
                PrismWarnings.Warning = StartNumbersDoNotMatch.Count.ToString();

                PrismWarnings.StartNumbersDontMatch(StartNumbersDoNotMatch);

                ModelModifiers.SetPartsRed(Convertor.PrismPartsToModelObjects(StartNumbersDoNotMatch));

                return PrismWarnings.NewIgnoreWarning();
            }
            return IgnoreType.Unspecified;
        }

        public static bool OrderErrors()
        {
            if (NotOrderedParts.Count != 0)
            {
                PrismWarnings.Warning = NotOrderedParts.Count.ToString();

                PrismWarnings.HasNotBeenOrdered();

                ModelModifiers.SetPartsRed(NotOrderedParts);

                return PrismWarnings.IgnoreWarning();
            }
            return true;
        }

        public static bool IntumesecentLoadingErrors()
        {
            if (PartsWithoutIntumescentLoading.Count != 0)
            {
                PrismWarnings.Warning = PartsWithoutIntumescentLoading.Count.ToString();

                PrismWarnings.IntumescentLoadingMissing(PartsWithoutIntumescentLoading);

                ModelModifiers.SetPartsRed(Convertor.PrismPartsToModelObjects(PartsWithoutIntumescentLoading));

                return PrismWarnings.IgnoreIntumescentLoading();
            }
            return true;
        }

        public static bool FinishErrors()
        {
            if (HasNoFinish.Count != 0)
            {
                PrismWarnings.Warning = HasNoFinish.Count.ToString();

                PrismWarnings.HasNoFinish(HasNoFinish);

                ModelModifiers.SetPartsRed(Convertor.PrismPartsToModelObjects(HasNoFinish));

                return PrismWarnings.IgnoreWarning();
            }
            return true;
        }

        public static void GetPartsThatStartNumbersDontMatch(this Part mainPart, Part secondaryPart)
        {
            if (mainPart.AssemblyNumber.StartNumber != secondaryPart.PartNumber.StartNumber)
            {
                //ModelPart newPart = new ModelPart(secondaryPart, mainPart.AssemblyNumber.StartNumber);
                PrismPart newPart = new PrismPart(secondaryPart) { StartNumber = mainPart.AssemblyNumber.StartNumber };
                StartNumbersDoNotMatch.Add(newPart);
            }
        }

        public static void GetPartsWherePhasesDontMatch(this Part mainPart, Part secondaryPart)
        {
            mainPart.GetPhase(out Phase mainPartPhase);
            secondaryPart.GetPhase(out Phase secondaryPhase);

            if (mainPartPhase.PhaseNumber != secondaryPhase.PhaseNumber)
            {
                PrismPart p = new PrismPart(secondaryPart) { Phase = mainPartPhase };
                PhasesDoNotMatch.Add(p);
                PhasesDoNotMatchParts.Add(new PrismPart(secondaryPart));
            }
        }

        public static void GetPartsWithoutAFinish(this Part mainPart)
        {
            if (mainPart.Finish.Length == 0)
            {
                HasNoFinish.Add(new PrismPart(mainPart));
            }
        }

        public static bool HasBeenOrdered(this Part mainPart, bool skipMessages)
        {
            string prelimMark = "";
            mainPart.GetUserProperty(ModelUDA.CurrentStageName(3), ref prelimMark); //Check prism uda material order complete for data
            if (prelimMark.Length == 0)
            {
                if (skipMessages) { return false; }
            }
            return true;
        }

        public static void GetUnorderedParts(this Part mainPart)
        {
            if (!mainPart.Profile.ProfileString.Contains("PLT") && !mainPart.Profile.ProfileString.Contains("FLT"))
            {
                string prelimMark = "";
                mainPart.GetUserProperty(ModelUDA.CurrentStageName(3), ref prelimMark); //Check prism uda material order complete for data    
                if (prelimMark.Length == 0)
                {
                    NotOrderedParts.Add(mainPart);
                }
                else
                {
                    OrderedParts.Add(mainPart);
                }
            }
        }

        private static void CheckForIntumescentLoading(this Part mainPart)
        {
            if (mainPart.Finish.StartsWith(GdomValues.IntumescentCode))
            {
                RetrieveDftAndWft(mainPart, ModelUDA.FireDFT(), ModelUDA.FireWFT(), out string dft, out string wft, out double dftNum, out double wftNum);
                if (IsMissingProperties(dft, dftNum, wft, wftNum))
                {
                    RetrieveDftAndWft(mainPart, ModelUDA.HempelFireDFT(), ModelUDA.HempelFireWFT(), out string hempDft, out string hempWft, out double hempDftNum, out double hempWftNum);
                    if (IsMissingProperties(hempDft, hempDftNum, hempWft, hempWftNum))
                    {
                        RetrieveDftAndWft(mainPart, ModelUDA.HempelOldFireDFT(), ModelUDA.HempelOldFireWFT(), out string oldHempDft, out string oldHempWft, out double oldHempDftNum, out double oldHempWftNum);
                        if (IsMissingProperties(oldHempDft, oldHempDftNum, oldHempWft, oldHempWftNum))
                        {
                            PartsWithoutIntumescentLoading.Add(new PrismPart(mainPart));
                        }
                    }
                }
            }
        }

        private static void RetrieveDftAndWft(Part part, string dftUda, string wftUda, out string dftString, out string wftString, out double dftDouble, out double wftDouble)
        {
            dftString = "";
            wftString = "";
            dftDouble = 0;
            wftDouble = 0;
            part.GetUserProperty(dftUda, ref dftString);
            part.GetUserProperty(dftUda, ref dftDouble);
            part.GetUserProperty(wftUda, ref wftString);
            part.GetUserProperty(wftUda, ref wftDouble);
        }

        private static bool IsMissingProperties(string dft, double dftNum, string wft, double wftNum)
        {
            return string.IsNullOrEmpty(dft) && dftNum == 0 && string.IsNullOrEmpty(wft) && wftNum == 0;
        }

        public static bool NameAndClassAign(SelectedObjects myObjects)
        {
            IncorrectNameAndClass.Clear();
            NameAndClassAligned(myObjects);
            IgnoreType ignore = PrismWarnings.DisplayOrderErrors(IncorrectNameAndClass, Error.NameAndClass);

            if (ignore == IgnoreType.AutoFix)
            {
                AutoFix.PartNameAndClass();
            }
            if (ignore == IgnoreType.Stop)
            {
                return false;
            }
            return true;
        }

        public static bool PartsHaveExecutionClass(SelectedObjects myObjects)
        {
            MissingExecutionClass.Clear();
            HasExecutionClass(myObjects);
            IgnoreType ignore = PrismWarnings.DisplayOrderErrors(MissingExecutionClass, Error.Execution);

            if (ignore == IgnoreType.AutoFix)
            {
                AutoFix.ExecutionClass();
            }
            if (ignore == IgnoreType.Stop)
            {
                return false;
            }
            return true;
        }
    }
}