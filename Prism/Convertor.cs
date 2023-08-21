using Prism.Geometry;
using Tekla.Structures.Geometry3d;

namespace Prism
{
    public static class Convertor
    {
        public static Point3D PointToPoint3D(Point point)
        {
            return new Point3D(point.X, point.Y, point.Z);
        }
    }
}
