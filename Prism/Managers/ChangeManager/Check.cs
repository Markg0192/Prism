using System;
using System.Collections.Generic;
using System.Linq;

namespace Prism
{
    public static class Check
    {
        public static string HasOnPlaneChanged(SteelItemBase oldAssembly, SteelItemBase newAssembly)
        {
            if (oldAssembly.OnPlane != newAssembly.OnPlane)
            {
                return $"OnPlane has changed from {oldAssembly.OnPlane} to {newAssembly.OnPlane}.";
            }
            return null;
        }

        public static string HasRotationChanged(SteelItemBase oldAssembly, SteelItemBase newAssembly)
        {
            if (oldAssembly.Rotation != newAssembly.Rotation)
            {
                return $"Rotation has changed from {oldAssembly.Rotation} to {newAssembly.Rotation}.";
            }
            return null;
        }

        // Continue the pattern for other properties

        public static string HasDepthChanged(SteelItemBase oldAssembly, SteelItemBase newAssembly)
        {
            if (oldAssembly.Depth != newAssembly.Depth)
            {
                return $"Depth has changed from {oldAssembly.Depth} to {newAssembly.Depth}.";
            }
            return null;
        }

        public static string HasOnPlaneOffsetChanged(SteelItemBase oldAssembly, SteelItemBase newAssembly)
        {
            if (Math.Abs(oldAssembly.OnPlaneOffset - newAssembly.OnPlaneOffset) > 0.1)
            {
                return $"OnPlaneOffset has changed from {oldAssembly.OnPlaneOffset} to {newAssembly.OnPlaneOffset}.";
            }
            return null;
        }

        public static string HasRotationOffsetChanged(SteelItemBase oldAssembly, SteelItemBase newAssembly)
        {
            if (Math.Abs(oldAssembly.RotationOffset - newAssembly.RotationOffset) > 0.1)
            {
                return $"RotationOffset has changed from {oldAssembly.RotationOffset} to {newAssembly.RotationOffset}.";
            }
            return null;
        }

        public static string HasDepthOffsetChanged(SteelItemBase oldAssembly, SteelItemBase newAssembly)
        {
            if (Math.Abs(oldAssembly.DepthOffset - newAssembly.DepthOffset) > 0.1)
            {
                return $"DepthOffset has changed from {oldAssembly.DepthOffset} to {newAssembly.DepthOffset}.";
            }
            return null;
        }

        public static string HasStartCoordinatesChanged(SteelItemBase oldAssembly, SteelItemBase newAssembly)
        {
            if (Math.Abs(oldAssembly.StartDx - newAssembly.StartDx) > 0.1 ||
                Math.Abs(oldAssembly.StartDy - newAssembly.StartDy) > 0.1 ||
                Math.Abs(oldAssembly.StartZ - newAssembly.StartZ) > 0.1)
            {
                return $"Start coordinates have changed from Dx: {oldAssembly.StartDx}, Dy: {oldAssembly.StartDy}, Z: {oldAssembly.StartZ} to Dx: {newAssembly.StartDx}, Dy: {newAssembly.StartDy}, Z: {newAssembly.StartZ}.";
            }
            return null;
        }

        public static string HasEndCoordinatesChanged(SteelItemBase oldAssembly, SteelItemBase newAssembly)
        {
            if (Math.Abs(oldAssembly.EndDx - newAssembly.EndDx) > 0.1 ||
                Math.Abs(oldAssembly.EndDy - newAssembly.EndDy) > 0.1 ||
                Math.Abs(oldAssembly.EndZ - newAssembly.EndZ) > 0.1)
            {
                return $"End coordinates have changed from Dx: {oldAssembly.EndDx}, Dy: {oldAssembly.EndDy}, Z: {oldAssembly.EndZ} to Dx: {newAssembly.EndDx}, Dy: {newAssembly.EndDy}, Z: {newAssembly.EndZ}.";
            }
            return null;
        }


        public static string ForOmittedFittings(MyAssembly oldComponent, MyFitting matchingNewFitting, MyFitting oldFitting)
        {
            if (matchingNewFitting == null)
            {
                return $"Fitting number {oldFitting.PartMark} has been removed from assembly {oldComponent.PartMark}.";
            }
            return null;
        }

        public static IEnumerable<string> ForNewFittings(MyAssembly oldComponent, MyAssembly matchingNewComponent)
        {
            var messages = new List<string>();

            foreach (var newFitting in matchingNewComponent.Fittings)
            {
                if (!oldComponent.Fittings.Any(f => f.PartMark == newFitting.PartMark))
                {
                    messages.Add($"New fitting number {newFitting.PartMark} has been added to assembly {matchingNewComponent.PartMark}.");
                }
            }

            return messages;
        }

        public static string HasFireDFTChanged(SteelItemBase oldAssembly, SteelItemBase newAssembly)
        {
            if (oldAssembly.FireDft != newAssembly.FireDft)
            {
                return $"Part {oldAssembly.PartMark} has a different FireDFT value.";
            }
            return null;
        }

        public static string HasFireWFTChanged(SteelItemBase oldAssembly, SteelItemBase newAssembly)
        {
            if (oldAssembly.FireWft != newAssembly.FireWft)
            {
                return $"Part {oldAssembly.PartMark} has a different FireWFT value.";
            }
            return null;
        }

        public static string HasExecutionClassChanged(SteelItemBase oldAssembly, SteelItemBase newAssembly)
        {
            if (oldAssembly.ExecutionClass != newAssembly.ExecutionClass)
            {
                return $"Part {oldAssembly.PartMark} has a different Execution Class.";
            }
            return null;
        }

       /* public static string HasStartPointChanged(SteelItemBase oldAssembly, SteelItemBase matchingNewAssembly)
        {
            if (!oldAssembly.StartPoint.IsEqualTo(matchingNewAssembly.StartPoint))
            {
                return $"Part {oldAssembly.PartMark} has a different start point.";
            }
            return null;
        }

        public static string HasEndPointChanged(SteelItemBase oldAssembly, SteelItemBase matchingNewAssembly)
        {
            if (!oldAssembly.EndPoint.IsEqualTo(matchingNewAssembly.EndPoint))
            {
                return $"Part {oldAssembly.PartMark} has a different end point.";
            }
            return null;
        }
       */
        public static string HasNameChanged(SteelItemBase oldAssembly, SteelItemBase newAssembly)
        {
            if (newAssembly.Name != oldAssembly.Name)
            {
                return $"Part {oldAssembly.PartMark} name has changed from {oldAssembly.Name} to {newAssembly.Name}.";
            }
            return null;
        }

        public static string HasLengthChanged(SteelItemBase oldAssembly, SteelItemBase newAssembly)
        {
            if (newAssembly.Length != oldAssembly.Length)
            {
                return $"Part {oldAssembly.PartMark} length has changed from {oldAssembly.Length} to {newAssembly.Length}.";
            }
            return null;
        }

        public static string HasProfileStringChanged(SteelItemBase oldAssembly, SteelItemBase newAssembly)
        {
            if (oldAssembly.ProfileString != newAssembly.ProfileString)
            {
                return $"Part {oldAssembly.PartMark} profile string has changed from {oldAssembly.ProfileString} to {newAssembly.ProfileString}.";
            }
            return null;
        }

        public static string HasWeightChanged(SteelItemBase oldAssembly, SteelItemBase newAssembly)
        {
            if (oldAssembly.Weight != newAssembly.Weight)
            {
                return $"Part {oldAssembly.PartMark} weight has changed from {oldAssembly.Weight} to {newAssembly.Weight}.";
            }
            return null;
        }

        public static string HasMaterialChanged(SteelItemBase oldAssembly, SteelItemBase newAssembly)
        {
            if (oldAssembly.Material != newAssembly.Material)
            {
                return $"Part {oldAssembly.PartMark} material has changed from {oldAssembly.Material} to {newAssembly.Material}.";
            }
            return null;
        }

        public static string HasFinishChanged(SteelItemBase oldAssembly, SteelItemBase newAssembly)
        {
            if (oldAssembly.Finish != newAssembly.Finish)
            {
                return $"Part {oldAssembly.PartMark} finish has changed from {oldAssembly.Finish} to {newAssembly.Finish}.";
            }
            return null;
        }

        public static string HasStartWarpChanged(SteelItemBase oldAssembly, SteelItemBase newAssembly)
        {
            if (Math.Abs(oldAssembly.StartWarp - newAssembly.StartWarp) > 0.1)
            {
                return $"StartWarp has changed from {oldAssembly.StartWarp} to {newAssembly.StartWarp}.";
            }
            return null;
        }

        public static string HasEndWarpChanged(SteelItemBase oldAssembly, SteelItemBase newAssembly)
        {
            if (Math.Abs(oldAssembly.EndWarp - newAssembly.EndWarp) > 0.1)
            {
                return $"EndWarp has changed from {oldAssembly.EndWarp} to {newAssembly.EndWarp}.";
            }
            return null;
        }

        public static string HasCamberingChanged(SteelItemBase oldAssembly, SteelItemBase newAssembly)
        {
            if (Math.Abs(oldAssembly.Cambering - newAssembly.Cambering) > 0.1)
            {
                return $"Cambering has changed from {oldAssembly.Cambering} to {newAssembly.Cambering}.";
            }
            return null;
        }

        public static string HasShorteningChanged(SteelItemBase oldAssembly, SteelItemBase newAssembly)
        {
            if (Math.Abs(oldAssembly.Shortening - newAssembly.Shortening) > 0.1)
            {
                return $"Shortening has changed from {oldAssembly.Shortening} to {newAssembly.Shortening}.";
            }
            return null;
        }

        public static string HasCurvedPlaneChanged(SteelItemBase oldAssembly, SteelItemBase newAssembly)
        {
            if (oldAssembly.CurvedPlane != newAssembly.CurvedPlane)
            {
                return $"CurvedPlane has changed from {oldAssembly.CurvedPlane} to {newAssembly.CurvedPlane}.";
            }
            return null;
        }

        public static string HasCurvedRadiusChanged(SteelItemBase oldAssembly, SteelItemBase newAssembly)
        {
            if (Math.Abs(oldAssembly.CurvedRadius - newAssembly.CurvedRadius) > 0.1)
            {
                return $"CurvedRadius has changed from {oldAssembly.CurvedRadius} to {newAssembly.CurvedRadius}.";
            }
            return null;
        }

        public static string HasCurvedSegmentsChanged(SteelItemBase oldAssembly, SteelItemBase newAssembly)
        {
            if (oldAssembly.CurvedSegments != newAssembly.CurvedSegments)
            {
                return $"CurvedSegments has changed from {oldAssembly.CurvedSegments} to {newAssembly.CurvedSegments}.";
            }
            return null;
        }
    }
}
