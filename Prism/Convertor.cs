using Prism.Geometry;
using System.Collections.Generic;
using System.Linq;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;

namespace Prism
{
    public static class Convertor
    {
        public static Point3D PointToPoint3D(Point point)
        {
            return new Point3D(point.X, point.Y, point.Z);
        }

        public static List<ModelObject> PrismPartsToModelObjects(List<PrismPart> prismParts)
        {
            return prismParts.Select(pp => pp.ModelObject).ToList();
        }
    }
}
