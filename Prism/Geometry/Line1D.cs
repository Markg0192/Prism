namespace Prism.Geometry
{
    public class Line1D
    {
        public Point3D Start;
        public Point3D End;

        public Line1D(Point3D start, Point3D end)
        {
            Start = start;
            End = end;
        }

        public Line1D(Point3D start, Vector V)
        {
            Start = start;
            End = Start.Offset(V);
        }

        public Vector Vector()
        {
            return new Vector(Start, End);
        }

        public double Length()
        {
            return Vector().Length();
        }

        public double DistancePerpToPoint(Point3D P)
        {
            return Distances.Point2Line(P, this);
        }
    }
}