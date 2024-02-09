using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;
using static Prism.Enum;
using Prism.Geometry;
using Vector = Prism.Geometry.Vector;
using Microsoft.Office.Interop.Excel;
using Model = Tekla.Structures.Model.Model;
using Point = Tekla.Structures.Geometry3d.Point;
using Tekla.Structures.RemotingHelper;
using System.Diagnostics.Eventing.Reader;

namespace Prism
{
    public class SteelItemBase : IModifiable
    {
        public List<string> ChangeMessages = new List<string>();
        public List<string> FittingChangeMessages = new List<string>();
        public string Guid { get; set; }
        public ModificationType Modification { get; set; }
        public string PartMark { get; set; }
        public string Name { get; set; }
        public double Length { get; set; }
        public string ProfileString { get; set; }
        public double Weight { get; set; }
        public string Material { get; set; }
        public string Finish { get; set; }
        //  public Point3D StartPoint { get; set; }
        // public Point3D EndPoint { get; set; }
        public List<MyBolts> Bolts = new List<MyBolts>();
        public List<MyWelds> Welds = new List<MyWelds>();
        public List<MyCut> Cuts = new List<MyCut>();
        public string FireDft { get; set; }
        public string FireWft { get; set; }
        public string ExecutionClass { get; set; }
        public string OnPlane { get; set; }
        public string Rotation { get; set; }
        public string Depth { get; set; }
        public double OnPlaneOffset { get; set; }
        public double RotationOffset { get; set; }
        public double DepthOffset { get; set; }
        public double StartDx { get; set; }
        public double StartDy { get; set; }
        public double StartZ { get; set; }
        public double EndDx { get; set; }
        public double EndDy { get; set; }
        public double EndZ { get; set; }
        public double StartWarp { get; set; }
        public double EndWarp { get; set; }
        public double Cambering { get; set; }
        public double Shortening { get; set; }
        public string CurvedPlane { get; set; }
        public double CurvedRadius { get; set; }
        public int CurvedSegments { get; set; }
        public string Comment { get; set; }
        public string Shorten { get; set; }
        public string Camber { get; set; }
        public string UserField1 { get; set; }
        public string UserField2 { get; set; }
        public string UserField3 { get; set; }
        public string UserField4 { get; set; }
        public string PaintNote1 { get; set; }
        public string PaintNote2 { get; set; }
        public string LaminationCheck { get; set; }
        public string FitForBearing { get; set; }
        public string FittingNote1 { get; set; }
        public string StraightnessTolerance { get; set; }
        public List<Point3D> ContourPoints { get; set; }
        public double BentPlateThickness { get; set; }
        public List<MyICurve> Curves { get; set; }
        public string FacePlateType { get; set; }
        public double SpiralBeamRotationAngle { get; set; }
        public Point3D SpiralBeamRotationAxisBasePoint { get; set; }
        public Vector SprialBeamRotationAxisDirection { get; set; }
        public Point3D SpiralBeamRotationAxisUpPoint { get; set; }
        public Point3D SpiralBeamRotationCenterPoint { get; set; }
        public double SpiralBeamTotalRise { get; set; }
        public double SpiralBeamTwistAngleStart { get; set; }
        public double SpiralBeamTwistAngleEnd { get; set; }
        public int NumberOfBolts { get; set; }

        protected void SetCommonProperties(Part part, Model model)
        {
            SetCommonUDAs(part);
            SetBasicProperties(part);

            Welds = GetMyWelds(part);
            //Cuts = GetMyCuts(part);
            SetPositionProperties(part);
            SetDeformingDataProperties(part);
            GetObjectSpecificProperties(part, false, null);
            Bolts = GetMyBolts(part, model);
            ModelModifiers.ResetWorkPlane(model);
        }

        private void SetCommonUDAs(Part part)
        {
            FireDft = TeklaHelper.GetStringAttribute(part, TeklaAttributes.FireDFT());
            FireWft = TeklaHelper.GetStringAttribute(part, TeklaAttributes.FireWFT());
            ExecutionClass = TeklaHelper.GetStringAttribute(part, TeklaAttributes.ExecutionClass());
            CurvedSegments = TeklaHelper.GetIntAttribute(part, TeklaAttributes.CurvedSegments());
            CurvedRadius = TeklaHelper.GetDoubleAttribute(part, TeklaAttributes.CurvedRadius());

            Comment = TeklaHelper.GetStringAttribute(part, TeklaAttributes.Comment());
            Shorten = TeklaHelper.GetStringAttribute(part, TeklaAttributes.Shorten());
            Camber = TeklaHelper.GetStringAttribute(part, TeklaAttributes.Camber());
            UserField1 = TeklaHelper.GetStringAttribute(part, TeklaAttributes.UserField1());
            UserField2 = TeklaHelper.GetStringAttribute(part, TeklaAttributes.UserField2());
            UserField3 = TeklaHelper.GetStringAttribute(part, TeklaAttributes.UserField3());
            UserField4 = TeklaHelper.GetStringAttribute(part, TeklaAttributes.UserField4());
            PaintNote1 = TeklaHelper.GetStringAttribute(part, TeklaAttributes.PaintNote1());
            PaintNote2 = TeklaHelper.GetStringAttribute(part, TeklaAttributes.PaintNote2());
            LaminationCheck = TeklaHelper.GetStringAttribute(part, TeklaAttributes.LaminationCheck());
            FitForBearing = TeklaHelper.GetStringAttribute(part, TeklaAttributes.FitForBearing());
            FittingNote1 = TeklaHelper.GetStringAttribute(part, TeklaAttributes.FittingNote1());
            StraightnessTolerance = TeklaHelper.GetStringAttribute(part, TeklaAttributes.StraightnessTolerance());
        }

        private void SetBasicProperties(Part part, bool isCut = false)
        {
            Modification = ModificationType.Unassigned;
            Name = part.Name;
            ProfileString = part.Profile.ProfileString;
            if (!isCut)
            {
                Guid = part.Identifier.GUID.ToString();
                Material = part.Material.MaterialString;
                Finish = part.Finish;
                PartMark = TeklaHelper.GetStringAttribute(part, TeklaAttributes.PartPos());
                Weight = Math.Round(TeklaHelper.GetDoubleAttribute(part, TeklaAttributes.Weight()), 2);
            }
        }

        private void GetObjectSpecificProperties(Part part, bool isCut, MyCut myCut)
        {
            switch (part)
            {
                case Beam beam:
                    SetBeamProperties(beam);
                    break;
                case BentPlate bp:
                    SetBentPlateProperties(bp);
                    break;
                case Brep brep:
                    SetBrepProperties(brep);
                    break;
                case ContourPlate cp:
                    SetContourPlateProperties(cp, isCut, myCut);
                    break;
                case LoftedPlate lp:
                    SetLoftedPlateProperties(lp);
                    break;
                case PolyBeam pb:
                    SetPolyBeamProperties(pb, isCut, myCut);
                    break;
                case SpiralBeam sp:
                    SetSpiralBeamProperties(sp);
                    break;
                default:
                    // Handle the case where part is none of the above types
                    throw new InvalidOperationException($"Unsupported part type: {part.GetType().Name}");
            }
        }

        private void SetBrepProperties(Brep brep)
        {
            Length = Math.Round(CalculateLength(brep), 2);
            //  StartPoint = ConvertToPoint3D(brep.StartPoint);
            // EndPoint = ConvertToPoint3D(brep.EndPoint);
            SetStartEndOffsets(brep);
        }

        private void SetBeamProperties(Beam beam)
        {
            Length = Math.Round(CalculateLength(beam), 2);
            //   StartPoint = ConvertToPoint3D(beam.StartPoint);
            //   EndPoint = ConvertToPoint3D(beam.EndPoint);
            SetStartEndOffsets(beam);
        }

        private void SetBentPlateProperties(BentPlate bp)
        {
            BentPlateThickness = bp.Thickness;
        }

        private void SetLoftedPlateProperties(LoftedPlate lp)
        {
            Curves = new List<MyICurve>();
            foreach (ICurve c in lp.BaseCurves)
            {
                Curves.Add(new MyICurve { StartPoint = ConvertToPoint3D(c.StartPoint), EndPoint = ConvertToPoint3D(c.EndPoint), Length = c.Length });
            }
            FacePlateType = lp.FaceType.ToString();
        }

        private void SetPolyBeamProperties(PolyBeam pb, bool isCut, MyCut myCut)
        {
            ArrayList contourPoints = pb.Contour.ContourPoints;
            ContourPoints = new List<Point3D>();
            myCut.CutContourPoints = new List<Point3D>();

            foreach (Point p in contourPoints)
            {
                if (isCut) myCut.CutContourPoints.Add(ConvertToPoint3D(p));
                else ContourPoints.Add(ConvertToPoint3D(p));
            }
        }

        private void SetSpiralBeamProperties(SpiralBeam sp)
        {
            //StartPoint = ConvertToPoint3D(sp.StartPoint);
            // EndPoint = ConvertToPoint3D(sp.EndPoint);
            SpiralBeamRotationAngle = Math.Round(sp.RotationAngle, 2);
            SpiralBeamRotationAxisBasePoint = ConvertToPoint3D(sp.RotationAxisBasePoint);
            SprialBeamRotationAxisDirection = ConvertToMyVector(sp.RotationAxisDirection);
            SpiralBeamRotationAxisUpPoint = ConvertToPoint3D(sp.RotationAxisUpPoint);
            SpiralBeamRotationCenterPoint = ConvertToPoint3D(sp.RotationCenterPoint);
            SpiralBeamTotalRise = Math.Round(sp.TotalRise, 2);
            SpiralBeamTwistAngleStart = Math.Round(sp.TwistAngleStart, 2);
            SpiralBeamTwistAngleEnd = Math.Round(sp.TwistAngleEnd, 2);
        }

        private Vector ConvertToMyVector(Tekla.Structures.Geometry3d.Vector vector)
        {
            return new Vector(Math.Round(vector.X, 2), Math.Round(vector.Y, 2), Math.Round(vector.Z, 2));
        }

        private void SetContourPlateProperties(ContourPlate cp, bool isCut, MyCut myCut)
        {
            ArrayList contourPoints = cp.Contour.ContourPoints;
            ContourPoints = new List<Point3D>();
            if(isCut) myCut.CutContourPoints = new List<Point3D>();

            foreach (Point p in contourPoints)
            {
                if (isCut) myCut.CutContourPoints.Add(ConvertToPoint3D(p));
                else ContourPoints.Add(ConvertToPoint3D(p));
            }
        }

        private void SetStartEndOffsets(Brep item)
        {
            StartDx = Math.Round(item.StartPointOffset.Dx, 2);
            StartDy = Math.Round(item.StartPointOffset.Dy, 2);
            StartZ = Math.Round(item.StartPointOffset.Dz, 2);
            EndDx = Math.Round(item.EndPointOffset.Dx, 2);
            EndDy = Math.Round(item.EndPointOffset.Dy, 2);
            EndZ = Math.Round(item.EndPointOffset.Dz, 2);
        }

        private void SetStartEndOffsets(Beam item)
        {
            StartDx = Math.Round(item.StartPointOffset.Dx, 2);
            StartDy = Math.Round(item.StartPointOffset.Dy, 2);
            StartZ = Math.Round(item.StartPointOffset.Dz, 2);
            EndDx = Math.Round(item.EndPointOffset.Dx, 2);
            EndDy = Math.Round(item.EndPointOffset.Dy, 2);
            EndZ = Math.Round(item.EndPointOffset.Dz, 2);
        }

        private void SetPositionProperties(Part part)
        {
            OnPlane = part.Position.Plane.ToString();
            Rotation = part.Position.Rotation.ToString();
            Depth = part.Position.Depth.ToString();
            OnPlaneOffset = Math.Round(part.Position.PlaneOffset, 2);
            RotationOffset = Math.Round(part.Position.RotationOffset, 2);
            DepthOffset = Math.Round(part.Position.DepthOffset, 2);
        }

        private void SetDeformingDataProperties(Part part)
        {
            StartWarp = Math.Round(part.DeformingData.Angle, 2);
            EndWarp = Math.Round(part.DeformingData.Angle2, 2);
            Cambering = Math.Round(part.DeformingData.Cambering, 2);
            Shortening = Math.Round(part.DeformingData.Shortening, 2);
        }

        private static double CalculateLength(Beam beam)
        {
            return Math.Round(Distances.Point2Point(beam.StartPoint, beam.EndPoint), 2);
        }

        private static double CalculateLength(Brep beam)
        {
            return Math.Round(Distances.Point2Point(beam.StartPoint, beam.EndPoint), 2);
        }

        private List<MyBolts> GetMyBolts(Part part, Model model)
        {
            List<MyBolts> myBoltsList = new List<MyBolts>();

            model.GetWorkPlaneHandler().SetCurrentTransformationPlane(new TransformationPlane(part.GetCoordinateSystem()));
            model.CommitChanges();

            ModelObjectEnumerator boltMoe = part.GetBolts();
            foreach (var setOfBolts in boltMoe)
            {
                BoltGroup bolt = setOfBolts as BoltGroup;
                if (bolt != null)
                {
                    //if (part.Identifier.GUID == bolt.PartToBeBolted.Identifier.GUID) //prevents getting the same bolt twice
                    { myBoltsList.Add(CreateMyBoltsFromBoltGroup(bolt)); }
                }
            }

            Cuts = GetMyCuts(part);
            return myBoltsList;
        }

        private List<MyWelds> GetMyWelds(Part part)
        {
            List<MyWelds> myWeldsList = new List<MyWelds>();

            ModelObjectEnumerator weldMoe = part.GetWelds();
            foreach (var weld in weldMoe)
            {
                Weld wld = weld as Weld;
                if (wld != null)
                {
                    if (part.Identifier.GUID == wld.MainObject.Identifier.GUID) //then the part we are checking is the mainobject of this weld (doing this stops us getting the same weld twice)
                    { myWeldsList.Add(CreateMyWeldFromTeklaWeld(wld)); }
                }
            }
            return myWeldsList;
        }

        private List<MyCut> GetMyCuts(Part part)
        {
            List<MyCut> myCutsList = new List<MyCut>();

            ModelObjectEnumerator cutMoe = part.GetBooleans();
            foreach (var cut in cutMoe)
            {
                myCutsList.Add(CreateNewCut(cut));
            }
            return myCutsList;
        }

        private MyCut CreateNewCut(object obj)
        {
            MyCut myCut = new MyCut();
            switch (obj)
            {
                case BooleanPart boolPart:
                    SetCommonUDAs(boolPart.OperativePart);
                    SetBasicProperties(boolPart.OperativePart, true);
                    SetPositionProperties(boolPart.OperativePart);
                    SetDeformingDataProperties(boolPart.OperativePart);
                    GetObjectSpecificProperties(boolPart.OperativePart, true, myCut);
                    myCut.BooleanPartType = boolPart.Type.ToString();
                    myCut.Guid = boolPart.Identifier.GUID.ToString();
                    break;

                case CutPlane cutPlane:
                    myCut.BooleanPlane = ConvertToMyPlane(cutPlane.Plane);
                    myCut.Guid = cutPlane.Identifier.GUID.ToString();
                    break;

                case EdgeChamfer chamfer:
                    myCut = ProcessChamfer(chamfer);
                    myCut.Guid = chamfer.Identifier.GUID.ToString();
                    break;

                case Fitting fitting:
                    myCut.BooleanPlane = ConvertToMyPlane(fitting.Plane);
                    myCut.Guid = fitting.Identifier.GUID.ToString();
                    break;
            }
            return myCut;
        }

        private MyPlane ConvertToMyPlane(Plane plane)
        {
            MyPlane myPlane = new MyPlane
            {
                AxisX = ConvertToMyVector(plane.AxisX),
                AxisY = ConvertToMyVector(plane.AxisY),
                Origin = ConvertToPoint3D(plane.Origin)
            };

            return myPlane;
        }

        private MyCut ProcessChamfer(EdgeChamfer chamfer)
        {
            MyCut myCut = new MyCut
            {
                Chamfer = new MyChamfer
                {
                    Dz1 = Math.Round(chamfer.Chamfer.DZ1, 2),
                    Dz2 = Math.Round(chamfer.Chamfer.DZ2, 2),
                    X = Math.Round(chamfer.Chamfer.X, 2),
                    Y = Math.Round(chamfer.Chamfer.Y, 2),
                    Type = chamfer.Chamfer.Type.ToString()
                },
                FirstChamferEndType = chamfer.FirstChamferEndType.ToString(),
                FirstBevelDimension = Math.Round(chamfer.FirstBevelDimension, 2),
                FirstEnd = ConvertToPoint3D(chamfer.FirstEnd),
                Name = chamfer.Name,
                SecondBevelDimension = Math.Round(chamfer.SecondBevelDimension, 2),
                SecondChamferEndType = chamfer.SecondChamferEndType.ToString(),
                SecondEnd = ConvertToPoint3D(chamfer.SecondEnd)
            };

            return myCut;
        }

        private static MyWelds CreateMyWeldFromTeklaWeld(Weld weld)
        {
            return new MyWelds
            {
                Guid = weld.Identifier.GUID.ToString(),
                IsAroundWeld = weld.AroundWeld,
                IsShopWeld = weld.ShopWeld,
                Position = weld.Position.ToString(),
                Shape = weld.IntermittentType.ToString(),
                ConnectAs = weld.ConnectAssemblies.ToString(),
                Placement = weld.Placement.ToString(),
                Preperation = weld.Preparation.ToString(),
                AboveLinePrefix = weld.PrefixAboveLine,
                AboveLineType = weld.TypeAbove.ToString(),
                AboveLineSize = weld.SizeAbove,
                AboveLineAngle = weld.AngleAbove,
                AboveLineContour = weld.ContourAbove.ToString(),
                AboveLineFinish = weld.FinishAbove.ToString(),
                AboveLineRootFace = weld.RootFaceAbove.ToString(),
                AboveLineEffectiveThroat = weld.EffectiveThroatAbove,
                AboveLineRootOpening = weld.RootOpeningAbove,
                AboveLineNoOfInc = weld.IncrementAmountAbove,
                AboveLineLength = weld.LengthAbove,
                AboveLinePitch = weld.PitchAbove,

                BelowLinePrefix = weld.PrefixBelowLine,
                BelowLineType = weld.TypeBelow.ToString(),
                BelowLineSize = weld.SizeBelow,
                BelowLineAngle = weld.AngleBelow,
                BelowLineContour = weld.ContourBelow.ToString(),
                BelowLineFinish = weld.FinishBelow.ToString(),
                BelowLineRootFace = weld.RootFaceBelow.ToString(),
                BelowLineEffectiveThroat = weld.EffectiveThroatBelow,
                BelowLineRootOpening = weld.RootOpeningBelow,
                BelowLineNoOfInc = weld.IncrementAmountBelow,
                BelowLineLength = weld.LengthBelow,
                BelowLinePitch = weld.PitchBelow,

                NdtInspection = weld.NDTInspection.ToString(),
                ElectrodeInspection = weld.ElectrodeClassification.ToString(),
                ElectrodeStrength = weld.ElectrodeStrength.ToString(),
                ElectrodeCoefficient = weld.ElectrodeCoefficient.ToString(),
                ProcessType = weld.ProcessType.ToString(),
                ReferenceText = weld.ReferenceText
            };
        }

        private static MyBolts CreateMyBoltsFromBoltGroup(BoltGroup bolt)
        {
            BoltCircle circle = bolt as BoltCircle;
            return new MyBolts
            {
                // BoltDiameter = bolt.BoltSize,
                Diameter = bolt.BoltSize + bolt.Tolerance,
                Guid = bolt.Identifier.GUID.ToString(),
                Standard = bolt.BoltStandard,
                Type = bolt.BoltType.ToString(),
                StartDx = Math.Round(bolt.StartPointOffset.Dx, 2),
                StartDy = Math.Round(bolt.StartPointOffset.Dy, 2),
                StartDz = Math.Round(bolt.StartPointOffset.Dz, 2),
                EndDx = Math.Round(bolt.EndPointOffset.Dx, 2),
                EndDy = Math.Round(bolt.EndPointOffset.Dy, 2),
                EndDz = Math.Round(bolt.EndPointOffset.Dz, 2),
                Shape = DetermineBoltGroupType(bolt, out string distX, out string distY),
                DistX = distX,
                DistY = distY,
                Quantity = CalculateBoltQuantity(bolt),

                ConnectAs = bolt.ConnectAssemblies.ToString(),
                ThreadInMaterial = bolt.ThreadInMaterial.ToString(),
                CutLength = bolt.CutLength,
                ExtraLength = bolt.ExtraLength,
                BoltOn = bolt.Bolt,
                Washer1 = bolt.Washer1,
                Washer2 = bolt.Washer2,
                Washer3 = bolt.Washer3,
                Nut1 = bolt.Nut1,
                Nut2 = bolt.Nut2,
                CircleNumberOfBolts = circle != null ? circle.BoltPositions.Count : 0,
                CircleDiameter = circle != null ? circle.Diameter : 0,
                Tolerance = bolt.Tolerance,
                PlainHoleType = bolt.PlainHoleType.ToString(),
                SlottedHoleX = bolt.SlottedHoleX,
                SlottedHoleY = bolt.SlottedHoleY,
                RotateSlots = bolt.RotateSlots.ToString(),
                OnPlane = bolt.Position.PlaneOffset,
                Rotation = bolt.Position.Rotation.ToString(),
                RotationDegree = bolt.Position.RotationOffset,
                StartPoint = new Point3D(Math.Round(bolt.FirstPosition.X, 2), Math.Round(bolt.FirstPosition.Y, 2), Math.Round(bolt.FirstPosition.Z, 2)),
                EndPoint = new Point3D(Math.Round(bolt.SecondPosition.X, 2), Math.Round(bolt.SecondPosition.Y, 2), Math.Round(bolt.SecondPosition.Z, 2))
            };
        }

        private static int CalculateBoltQuantity(BoltGroup boltArray)
        {
            BoltArray bolt = boltArray as BoltArray;
            return bolt == null ? 0 : (bolt.GetBoltDistXCount() + 1) * (bolt.GetBoltDistYCount() + 1);
        }

        private static string DetermineBoltGroupType(BoltGroup boltGroup, out string distX, out string distY)
        {
            if (boltGroup is BoltArray boltArray)
            {
                distX = GetDistancesX(boltArray);
                distY = GetDistancesY(boltArray);
                return "Bolt Array";
            }

            if (boltGroup is BoltCircle)
            {
                distX = "";
                distY = "";
                return "Bolt Circle";
            }

            if (boltGroup is BoltXYList)
            {
                distX = "";
                distY = "";
                return "BoltXyList";
            }

            distX = "";
            distY = "";
            return "";
        }

        private static string GetDistancesX(BoltArray boltArray)
        {
            var distances = new StringBuilder();
            for (int i = 0; i < boltArray.GetBoltDistXCount(); i++)
            {
                string prefix = "";
                if (i == 0) prefix = ""; else prefix = "*";
                distances.Append(prefix + boltArray.GetBoltDistX(i).ToString());
            }
            return distances.ToString();
        }

        private static string GetDistancesY(BoltArray boltArray)
        {
            var distances = new StringBuilder();
            for (int i = 0; i < boltArray.GetBoltDistYCount(); i++)
            {
                string prefix = "";
                if (i == 0) prefix = ""; else prefix = "*";
                distances.Append(prefix + boltArray.GetBoltDistY(i).ToString());
            }
            return distances.ToString();
        }

        private static Point3D ConvertToPoint3D(Point source)
        {
            return new Point3D(Math.Round(source.X, 2), Math.Round(source.Y, 2), Math.Round(source.Z, 2));
        }
    }
}