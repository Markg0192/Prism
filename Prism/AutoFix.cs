using System.Collections.Generic;
using System.Threading;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;

namespace Prism
{
    public static class AutoFix
    {
        public static void PartNameAndClass()
        {
            foreach (Part p in ModelChecker.IncorrectNameAndClass)
            {
                List<string> myClass = GdomValues.PartClass()[p.Name] as List<string>;
                p.Class = myClass[0];
                p.Modify();
            }
            PrismWarnings.ErrorsFixed(ModelChecker.IncorrectNameAndClass.Count);
        }

        public static void MemberOrientation()
        {
            foreach (Beam b in ModelChecker.IncorrectOrientation)
            {
                if (b.Name == GdomValues.BeamName || b.Name == GdomValues.RafterName || b.Name == GdomValues.PortalRafterName || b.Name == GdomValues.BraceName)
                {
                    SwapHandles(b);
                }
                if (b.Name == GdomValues.ColumnName)
                {
                    if (b.Position.Rotation == Position.RotationEnum.TOP)
                    {
                        b.Position.Rotation = Position.RotationEnum.BELOW;
                    }
                    if (b.Position.Rotation == Position.RotationEnum.BACK)
                    {
                        b.Position.Rotation = Position.RotationEnum.FRONT;
                    }
                    if (b.StartPoint.Z > b.EndPoint.Z)
                    {
                        SwapHandles(b);
                    }
                    b.Modify();
                }
            }
            PrismWarnings.ErrorsFixed(ModelChecker.IncorrectOrientation.Count);
        }

        private static void SwapHandles(Beam b)
        {
            Point start = b.StartPoint;
            Point endPoint = b.EndPoint;
            b.StartPoint = endPoint;
            b.EndPoint = start;
            b.Modify();
        }

        public static void ExecutionClass()
        {
            int myExcClass = PrismWarnings.ExecutionClassWarning();
            foreach (Part p in ModelChecker.MissingExecutionClass)
            {
                p.SetUserProperty(ModelUDA.ExcecutionClass(), myExcClass);
                p.Modify();
            }
            PrismWarnings.ErrorsFixed(ModelChecker.MissingExecutionClass.Count);
        }

        public static void AssemblyAndStartNumbers()
        {
            foreach (ModelPart p in ModelChecker.StartNumbersDoNotMatch)
            {
                p.Part.AssemblyNumber.StartNumber = p.StartNumber;
                p.Part.PartNumber.StartNumber = p.StartNumber;
                p.Part.Modify();
            }
            PrismWarnings.ErrorsFixed(ModelChecker.StartNumbersDoNotMatch.Count);
        }

        public static void PartPhasing()
        {
            foreach (ModelPart p in ModelChecker.PhasesDoNotMatch)
            {
                p.Part.SetPhase(p.Phase);
                p.Part.Modify();
            }
            PrismWarnings.ErrorsFixed(ModelChecker.PhasesDoNotMatch.Count);
        }
    }
}