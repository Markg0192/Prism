using Prism.Geometry;
using System.Collections.Generic;

namespace Prism
{
    public class MyCut : SteelItemBase
    {
        public bool Checked { get; set; }
        public string BooleanPartType { get; set; }
        public MyPlane BooleanPlane { get;set; }
        public MyChamfer Chamfer { get; set; }      
        public double FirstBevelDimension { get; set; }
        public string FirstChamferEndType { get; set; }
        public Point3D FirstEnd { get; set; }
        public double SecondBevelDimension { get; set; }
        public string SecondChamferEndType { get; set; }
        public Point3D SecondEnd { get; set; }
        public List<Point3D> CutContourPoints { get; set; }
    }
}