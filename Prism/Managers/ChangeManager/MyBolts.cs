using Prism.Geometry;

namespace Prism
{
    public class MyBolts
    {
        public bool Checked { get; set; }
        public int Quantity { get; set; }
      //  public double BoltDiameter { get; set; }
        public double Diameter { get; set; }
        public string Guid { get; set; }
        public string Standard { get; set; }
        public string Type { get; set; }
        public double StartDx { get; set; }
        public double StartDy { get; set; }
        public double StartDz { get; set; }
        public double EndDx { get; set; }
        public double EndDy { get; set; }
        public double EndDz { get; set; }
        public string Shape { get; set; }
        public string DistX { get; set; }
        public string DistY { get; set; }
        public string ConnectAs { get; set; }
        public string ThreadInMaterial { get; set; }
        public double CutLength { get; set; }
        public double ExtraLength { get; set; }
        public bool BoltOn { get; set; }
        public bool Washer1 { get; set; }
        public bool Washer2 { get; set; }
        public bool Washer3 { get; set; }
        public bool Nut1 { get; set; }
        public bool Nut2 { get; set;}
        public double CircleNumberOfBolts { get; set; }
        public double CircleDiameter { get; set; }
        public double Tolerance { get; set; }
        public string PlainHoleType { get; set; }
        public double SlottedHoleX { get; set; }
        public double SlottedHoleY { get; set; }
        public string RotateSlots { get; set; }
        public double OnPlane { get; set; }
        public string Rotation { get; set; }
        public double RotationDegree { get; set; }
        public Point3D StartPoint { get; set; }
        public Point3D EndPoint { get; set; }
    }
}