/*using System;

namespace Prism
{
    public static class BoltCheck
    {
        public static string HasQuantityChanged(MyBolts oldBolt, MyBolts newBolt)
        {
            if (oldBolt.Quantity != newBolt.Quantity)
            {
                return $"Bolt ID {oldBolt.Guid} has changed quantity from {oldBolt.Quantity} to {newBolt.Quantity}.";
            }
            return null;
        }

        public static string HasStandardChanged(MyBolts oldBolt, MyBolts newBolt)
        {
            if (oldBolt.Standard != newBolt.Standard)
            {
                return $"Bolt ID {oldBolt.Guid} has changed standards from {oldBolt.Standard} to {newBolt.Standard}.";
            }
            return null;
        }

        public static string HasTypeChanged(MyBolts oldBolt, MyBolts newBolt)
        {
            if (oldBolt.Type != newBolt.Type)
            {
                return $"Bolt ID {oldBolt.Guid} has changed types from {oldBolt.Type} to {newBolt.Type}.";
            }
            return null;
        }

        public static string HasStartDxChanged(MyBolts oldBolt, MyBolts newBolt)
        {
            if (oldBolt.StartDx != newBolt.StartDx)
            {
                return $"Bolt ID {oldBolt.Guid} has changed StartDx from {oldBolt.StartDx} to {newBolt.StartDx}.";
            }
            return null;
        }

        public static string HasStartDyChanged(MyBolts oldBolt, MyBolts newBolt)
        {
            if (oldBolt.StartDy != newBolt.StartDy)
            {
                return $"Bolt ID {oldBolt.Guid} has changed StartDy from {oldBolt.StartDy} to {newBolt.StartDy}.";
            }
            return null;
        }

        public static string HasStartDzChanged(MyBolts oldBolt, MyBolts newBolt)
        {
            if (oldBolt.StartDz != newBolt.StartDz)
            {
                return $"Bolt ID {oldBolt.Guid} has changed StartDz from {oldBolt.StartDz} to {newBolt.StartDz}.";
            }
            return null;
        }

        public static string HasEndDxChanged(MyBolts oldBolt, MyBolts newBolt)
        {
            if (oldBolt.EndDx != newBolt.EndDx)
            {
                return $"Bolt ID {oldBolt.Guid} has changed EndDx from {oldBolt.EndDx} to {newBolt.EndDx}.";
            }
            return null;
        }

        public static string HasEndDyChanged(MyBolts oldBolt, MyBolts newBolt)
        {
            if (oldBolt.EndDy != newBolt.EndDy)
            {
                return $"Bolt ID {oldBolt.Guid} has changed EndDy from {oldBolt.EndDy} to {newBolt.EndDy}.";
            }
            return null;
        }

        public static string HasEndDzChanged(MyBolts oldBolt, MyBolts newBolt)
        {
            if (oldBolt.EndDz != newBolt.EndDz)
            {
                return $"Bolt ID {oldBolt.Guid} has changed EndDz from {oldBolt.EndDz} to {newBolt.EndDz}.";
            }
            return null;
        }

        public static string HasShapeChanged(MyBolts oldBolt, MyBolts newBolt)
        {
            if (oldBolt.Shape != newBolt.Shape)
            {
                return $"Bolt ID {oldBolt.Guid} has changed shape from {oldBolt.Shape} to {newBolt.Shape}.";
            }
            return null;
        }

        public static string HasDistXChanged(MyBolts oldBolt, MyBolts newBolt)
        {
            if (oldBolt.DistX != newBolt.DistX)
            {
                return $"Bolt ID {oldBolt.Guid} has changed DistX from {oldBolt.DistX} to {newBolt.DistX}.";
            }
            return null;
        }

        public static string HasDistYChanged(MyBolts oldBolt, MyBolts newBolt)
        {
            if (oldBolt.DistY != newBolt.DistY)
            {
                return $"Bolt ID {oldBolt.Guid} has changed DistY from {oldBolt.DistY} to {newBolt.DistY}.";
            }
            return null;
        }

        public static string HasDiameterChanged(MyBolts oldBolt, MyBolts newBolt)
        {
            if (Math.Abs(oldBolt.BoltDiameter - newBolt.BoltDiameter) > 0.1)
            {
                return $"Bolt ID {oldBolt.Guid} has changed diameter from {oldBolt.BoltDiameter} to {newBolt.BoltDiameter}.";
            }
            return null;
        }

        public static string HasConnectAsChanged(MyBolts oldBolt, MyBolts newBolt)
        {
            if (oldBolt.ConnectAs != newBolt.ConnectAs)
            {
                return $"Bolt ID {oldBolt.Guid} has changed connect as type from {oldBolt.ConnectAs} to {newBolt.ConnectAs}.";
            }
            return null;
        }

        public static string HasThreadInMaterialChanged(MyBolts oldBolt, MyBolts newBolt)
        {
            if (oldBolt.ThreadInMaterial != newBolt.ThreadInMaterial)
            {
                return $"Bolt ID {oldBolt.Guid} has changed thread in material from {oldBolt.ThreadInMaterial} to {newBolt.ThreadInMaterial}.";
            }
            return null;
        }

        public static string HasCutLength(MyBolts oldBolt, MyBolts newBolt)
        {
            if (Math.Abs(oldBolt.CutLength - newBolt.CutLength) > 0.1)
            {
                return $"Bolt ID {oldBolt.Guid} has changed cut length from {oldBolt.CutLength} to {newBolt.CutLength}.";
            }
            return null;
        }

        public static string HasExtraLength(MyBolts oldBolt, MyBolts newBolt)
        {
            if (Math.Abs(oldBolt.ExtraLength - newBolt.ExtraLength) > 0.1)
            {
                return $"Bolt ID {oldBolt.Guid} has changed extra length from {oldBolt.ExtraLength} to {newBolt.ExtraLength}.";
            }
            return null;
        }

        public static string HasBoltOnChanged(MyBolts oldBolt, MyBolts newBolt)
        {
            if (oldBolt.BoltOn != newBolt.BoltOn)
            {
                return $"Bolt ID {oldBolt.Guid} has changed bolt on from {oldBolt.BoltOn} to {newBolt.BoltOn}.";
            }
            return null;
        }

        public static string HasWasher1Changed(MyBolts oldBolt, MyBolts newBolt)
        {
            if (oldBolt.Washer1 != newBolt.Washer1)
            {
                return $"Bolt ID {oldBolt.Guid} has changed washer 1 from {oldBolt.Washer1} to {newBolt.Washer1}.";
            }
            return null;
        }

        public static string HasWasher2Changed(MyBolts oldBolt, MyBolts newBolt)
        {
            if (Math.Abs(oldBolt.BoltDiameter - newBolt.BoltDiameter) > 0.1)
            {
                return $"Bolt ID {oldBolt.Guid} has changed washer 2 from {oldBolt.Washer2} to {newBolt.Washer2}.";
            }
            return null;
        }

        public static string HasWasher3Changed(MyBolts oldBolt, MyBolts newBolt)
        {
            if (Math.Abs(oldBolt.BoltDiameter - newBolt.BoltDiameter) > 0.1)
            {
                return $"Bolt ID {oldBolt.Guid} has changed washer 3 from {oldBolt.Washer3} to {newBolt.Washer3}.";
            }
            return null;
        }

        public static string HasNut1Changed(MyBolts oldBolt, MyBolts newBolt)
        {
            if (Math.Abs(oldBolt.BoltDiameter - newBolt.BoltDiameter) > 0.1)
            {
                return $"Bolt ID {oldBolt.Guid} has changed nut 1 from {oldBolt.Nut1} to {newBolt.Nut1}.";
            }
            return null;
        }

        public static string HasNut2Changed(MyBolts oldBolt, MyBolts newBolt)
        {
            if (Math.Abs(oldBolt.BoltDiameter - newBolt.BoltDiameter) > 0.1)
            {
                return $"Bolt ID {oldBolt.Guid} has changed nut 2 from {oldBolt.Nut2} to {newBolt.Nut2}.";
            }
            return null;
        }

        public static string HasCircleNumberOfBoltsChanged(MyBolts oldBolt, MyBolts newBolt)
        {
            if (oldBolt.CircleNumberOfBolts != newBolt.CircleNumberOfBolts)
            {
                return $"Bolt ID {oldBolt.Guid} circle number of bolts has changed from {oldBolt.CircleNumberOfBolts} to {newBolt.CircleNumberOfBolts}.";
            }
            return null;
        }

        public static string HasCircleDiamterChanged(MyBolts oldBolt, MyBolts newBolt)
        {
            if (Math.Abs(oldBolt.CircleDiameter - newBolt.CircleDiameter) > 0.1)
            {
                return $"Bolt ID {oldBolt.Guid} has changed circle diameter from {oldBolt.CircleDiameter} to {newBolt.CircleDiameter}.";
            }
            return null;
        }

        public static string HasToleranceChanged(MyBolts oldBolt, MyBolts newBolt)
        {
            if (Math.Abs(oldBolt.Tolerance - newBolt.Tolerance) > 0.1)
            {
                return $"Bolt ID {oldBolt.Guid} has changed tolerance from {oldBolt.Tolerance} to {newBolt.Tolerance}.";
            }
            return null;
        }

        public static string HasPlainHoleTypeChanged(MyBolts oldBolt, MyBolts newBolt)
        {
            if (oldBolt.PlainHoleType != newBolt.PlainHoleType)
            {
                return $"Bolt ID {oldBolt.Guid} has changed plain hole type from {oldBolt.PlainHoleType} to {newBolt.PlainHoleType}.";
            }
            return null;
        }

        public static string HasSlottedHoleXChanged(MyBolts oldBolt, MyBolts newBolt)
        {
            if (Math.Abs(oldBolt.SlottedHoleX - newBolt.SlottedHoleX) > 0.1)
            {
                return $"Bolt ID {oldBolt.Guid} has changed slooted hole x from {oldBolt.SlottedHoleX} to {newBolt.SlottedHoleX}.";
            }
            return null;
        }

        public static string HasSlottedHoleYChanged(MyBolts oldBolt, MyBolts newBolt)
        {
            if (Math.Abs(oldBolt.SlottedHoleY - newBolt.SlottedHoleY) > 0.1)
            {
                return $"Bolt ID {oldBolt.Guid} has changed slotted hole y from {oldBolt.SlottedHoleY} to {newBolt.SlottedHoleY}.";
            }
            return null;
        }

        public static string HasRotateSlotesChanged(MyBolts oldBolt, MyBolts newBolt)
        {
            if (oldBolt.RotateSlots != newBolt.RotateSlots)
            {
                return $"Bolt ID {oldBolt.Guid} has changed rotate slots from {oldBolt.RotateSlots} to {newBolt.RotateSlots}.";
            }
            return null;
        }

        public static string HasOnPlaneChanged(MyBolts oldBolt, MyBolts newBolt)
        {
            if (Math.Abs(oldBolt.OnPlane - newBolt.OnPlane) > 0.1)
            {
                return $"Bolt ID {oldBolt.Guid} has changed on plane from {oldBolt.OnPlane} to {newBolt.OnPlane}.";
            }
            return null;
        }

        public static string HasRotationChanged(MyBolts oldBolt, MyBolts newBolt)
        {
            if (oldBolt.Rotation != newBolt.Rotation)
            {
                return $"Bolt ID {oldBolt.Guid} has changed rotation from {oldBolt.Rotation} to {newBolt.Rotation}.";
            }
            return null;
        }

        public static string HasRotationDegreeChanged(MyBolts oldBolt, MyBolts newBolt)
        {
            if (Math.Abs(oldBolt.RotationDegree - newBolt.RotationDegree) > 0.1)
            {
                return $"Bolt ID {oldBolt.Guid} has changed rotation degree from {oldBolt.RotationDegree} to {newBolt.RotationDegree}.";
            }
            return null;
        }

        public static string HasStartPointChanged(MyBolts oldBolt, MyBolts newBolt)
        {
            if (oldBolt.StartPoint.X != newBolt.StartPoint.X && oldBolt.StartPoint.Y != newBolt.StartPoint.Y 
                && oldBolt.StartPoint.Z != newBolt.StartPoint.Z)
            {
                return $"Bolt ID {oldBolt.Guid} has changed start point.";
            }
            return null;
        }

        public static string HasEndPointChanged(MyBolts oldBolt, MyBolts newBolt)
        {
            if (oldBolt.EndPoint.X != newBolt.EndPoint.X && oldBolt.EndPoint.Y != newBolt.EndPoint.Y
                && oldBolt.EndPoint.Z != newBolt.EndPoint.Z)
            {
                return $"Bolt ID {oldBolt.Guid} has changed end point.";
            }
            return null;
        }
    }
}*/