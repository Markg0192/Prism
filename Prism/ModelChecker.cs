using Prism.Geometry;
using System;
using System.Collections;
using System.Collections.Generic;
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
        public static List<ModelObject> IncorrectNameAndClass = new List<ModelObject>();
        public static List<ModelObject> MissingExecutionClass = new List<ModelObject>();
        public static List<ModelObject> HasNoFinish = new List<ModelObject>();
        public static List<ModelPart> StartNumbersDoNotMatch = new List<ModelPart>();
        public static List<ModelObject> StartNumbersDoNotMatchParts = new List<ModelObject>();
        public static List<ModelPart> PhasesDoNotMatch = new List<ModelPart>();
        public static List<ModelObject> PhasesDoNotMatchParts = new List<ModelObject>();
        public static List<ModelObject> NotOrderedParts = new List<ModelObject>();
        public static List<ModelObject> OrderedParts = new List<ModelObject>();
        public static List<ModelObject> PartsWithoutIntumescentLoading = new List<ModelObject>();
        public static List<ModelObject> IncorrectOrientation = new List<ModelObject>();

        public static void BoltThrough2Ply(SelectedObjects selectedObjects)
        {
            foreach (List<BoltGroup> ass in selectedObjects.AllBolts)
            {
                int executionClassData = 10;  //this is the number read from teklas UDA when no execution class is applied, we are defaulting to it not having one here
                                              // p.GetUserProperty(ModelUDA.ExcecutionClass(), ref executionClassData);

                if (executionClassData == 10)
                {
                    //MissingExecutionClass.Add(p);
                }
            }
        }

        public static bool RunStage4Checks(this SelectedObjects selectedObjects)
        {
            Factory location = PrismWarnings.FactoryLocation();
            if (location == Factory.Unknown)
            {
                PrismWarnings.IgnoreFittingCheck();
            }

            foreach (Assembly ass in selectedObjects.AssembliesList)
            {
                Part myMainPart = ass.GetMainPart() as Part;
                GetUnorderedParts(myMainPart);
                GetPartsWithoutAFinish(myMainPart);
                CheckForIntumescentLoading(myMainPart);

                ArrayList mySecondaries = ass.GetSecondaries();
                foreach (Part mySecondaryPart in mySecondaries)
                {
                    GetPartsThatStartNumbersDontMatch(myMainPart, mySecondaryPart);
                    GetPartsWherePhasesDontMatch(myMainPart, mySecondaryPart);
                    CheckFittings.GetIncorrectFittings(mySecondaryPart, location);
                }
            }
            return CheckForAndActionErrors();
        }

        public static void ClearOldLists()
        {
            CheckFittings.IncorrectGrade.Clear();
            CheckFittings.IncorrectLength.Clear();
            CheckFittings.IncorrectThickness.Clear();
            CheckFittings.AllIncorrectPlate.Clear();
            HasNoFinish.Clear();
            PartsWithoutIntumescentLoading.Clear();
            OrderedParts.Clear();
            NotOrderedParts.Clear();
            StartNumbersDoNotMatch.Clear();
            StartNumbersDoNotMatchParts.Clear();
            PhasesDoNotMatch.Clear();
            PhasesDoNotMatchParts.Clear();
        }

        public static bool MemberOrientationIsCorrect(SelectedObjects myObjects, out IgnoreType ignore)
        {
            IncorrectOrientation.Clear();
            MemberOrientation(myObjects);
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
            foreach (Assembly ass in selectedObjects.AssembliesList)
            {
                Part p = ass.GetMainPart() as Part;
                int executionClassData = 10;  //this is the number read from teklas UDA when no execution class is applied, we are defaulting to it not having one here
                p.GetUserProperty(ModelUDA.ExcecutionClass(), ref executionClassData);

                if (executionClassData == 10)
                {
                    MissingExecutionClass.Add(p);
                }
            }
        }

        public static void MemberOrientation(SelectedObjects selectedObjects)
        {
            int tolerance = 5;
            foreach (Assembly ass in selectedObjects.AssembliesList)
            {
                Beam b = ass.GetMainPart() as Beam;
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
                    IncorrectOrientation.Add(b);
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
                    IncorrectOrientation.Add(b);
                }
            }

        }

        private static void CheckRafterOrientation(Beam b)
        {
            //All rafters must be detailed with start point at the apex, so a simple check to make sure start point is higher than end point will do here
            if (b.StartPoint.Z < b.EndPoint.Z)
            {
                IncorrectOrientation.Add(b);
            }
        }

        private static void CheckColumnOrientation(Beam b)
        {
            //Column rotation must be "FRONT" or "BELOW", therefore "BACK" and "TOP" are wrong, start point must also be lower than end point.
            if (b.Position.Rotation == Position.RotationEnum.BACK || b.Position.Rotation == Position.RotationEnum.TOP || b.StartPoint.Z > b.EndPoint.Z)
            {
                IncorrectOrientation.Add(b);
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
                IncorrectOrientation.Add(b);
            }
        }

        public static void NameAndClassAligned(SelectedObjects selectedObjects)
        {
            foreach (Part p in selectedObjects.SelectedModelParts)
            {
                List<string> meantToBeClass = GdomValues.PartClass()[p.Name] as List<string>;
                if (meantToBeClass != null && !meantToBeClass.Contains(p.Class))
                {
                    IncorrectNameAndClass.Add(p);
                }
            }
        }

        public static bool ArePreviousStepsComplete(SelectedObjects selectedObjects, int stageNumber)
        {
            if (Environment.UserName != "mark.gibson")
            {
                foreach (Part p in selectedObjects.SelectedModelParts)
                {
                    string userProperty = "";
                    p.GetUserProperty(ModelUDA.PreviousStageName(stageNumber), ref userProperty);

                    if (userProperty == "")
                    {
                        PrismWarnings.PreviousStepIncomplete();
                        return false;
                    }
                }
            }

            return true;
        }

        private static bool CheckForAndActionErrors()
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

            return CheckFittings.DisplayFittingErrors();
        }

        public static IgnoreType PhaseMatchErrors()
        {
            if (PhasesDoNotMatch.Count != 0)
            {
                PrismWarnings.Warning = PhasesDoNotMatch.Count.ToString();

                PrismWarnings.PhasesDontMatch();

                ModelModifiers.SetPartsRed(PhasesDoNotMatchParts);

                return PrismWarnings.NewIgnoreWarning();
            }
            return IgnoreType.Unspecified;
        }

        public static IgnoreType StartNumbersErrors()
        {
            if (StartNumbersDoNotMatch.Count != 0)
            {
                PrismWarnings.Warning = StartNumbersDoNotMatch.Count.ToString();

                PrismWarnings.StartNumbersDontMatch();

                ModelModifiers.SetPartsRed(StartNumbersDoNotMatchParts);

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

                PrismWarnings.IntumescentLoadingMissing();

                ModelModifiers.SetPartsRed(PartsWithoutIntumescentLoading);

                return PrismWarnings.IgnoreIntumescentLoading();
            }
            return true;
        }

        public static bool FinishErrors()
        {
            if (HasNoFinish.Count != 0)
            {
                PrismWarnings.Warning = HasNoFinish.Count.ToString();

                PrismWarnings.HasNoFinish();

                ModelModifiers.SetPartsRed(HasNoFinish);

                return PrismWarnings.IgnoreWarning();
            }
            return true;
        }

        public static void GetPartsThatStartNumbersDontMatch(this Part mainPart, Part secondaryPart)
        {
            if (mainPart.AssemblyNumber.StartNumber != secondaryPart.PartNumber.StartNumber)
            {
                ModelPart newPart = new ModelPart(secondaryPart, mainPart.AssemblyNumber.StartNumber);
                StartNumbersDoNotMatchParts.Add(secondaryPart);
                StartNumbersDoNotMatch.Add(newPart);
            }
        }

        public static void GetPartsWherePhasesDontMatch(this Part mainPart, Part secondaryPart)
        {
            mainPart.GetPhase(out Phase mainPartPhase);
            secondaryPart.GetPhase(out Phase secondaryPhase);

            if (mainPartPhase.PhaseNumber != secondaryPhase.PhaseNumber)
            {
                ModelPart p = new ModelPart(secondaryPart, mainPartPhase);
                PhasesDoNotMatch.Add(p);
                PhasesDoNotMatchParts.Add(secondaryPart);
            }
        }

        public static void GetPartsWithoutAFinish(this Part mainPart)
        {
            if (mainPart.Finish.Length == 0)
            {
                HasNoFinish.Add(mainPart);
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

        private static void CheckForIntumescentLoading(this Part mainPart)
        {
            if (mainPart.Finish.StartsWith(GdomValues.IntumescentCode))
            {
                string dft = "";
                string wft = "";
                double dftNum = 0;
                double wftNum = 0;
                mainPart.GetUserProperty(ModelUDA.FireDFT(), ref dft);
                mainPart.GetUserProperty(ModelUDA.FireWFT(), ref wft);
                mainPart.GetUserProperty(ModelUDA.FireDFT(), ref dftNum);
                mainPart.GetUserProperty(ModelUDA.FireWFT(), ref wftNum);
                if ((dft == "" && dftNum == 0) || (wft == "" && wftNum == 0))
                {
                    PartsWithoutIntumescentLoading.Add(mainPart);
                }
            }
        }
    }
}