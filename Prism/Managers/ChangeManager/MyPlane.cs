using Prism.Geometry;

namespace Prism
{
    public class MyPlane
    {
        public Vector AxisX { get; set; }
        public Vector AxisY { get; set; }
        public Point3D Origin { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is MyPlane other)
            {
                if (!(AxisX.dX == other.AxisX.dX && AxisX.dY == other.AxisX.dY && AxisX.dZ == other.AxisX.dZ)) return false;
                if(!(AxisY.dX == other.AxisY.dX && AxisY.dY == other.AxisY.dY && AxisY.dZ == other.AxisY.dZ)) return false;
                if(!(Origin.X == other.Origin.X && Origin.Y == other.Origin.Y && Origin.Z == other.Origin.Z)) return false;
                return true;//return AxisX.Equals(other.AxisX) && AxisY.Equals(other.AxisY) && Origin.IsEqualTo(other.Origin);
            }
            return false;
        }
    }
}