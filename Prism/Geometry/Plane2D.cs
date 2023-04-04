using System;

namespace Prism.Geometry
{
    public class Plane2D : IEquatable<Plane2D>
    {
        public Point3D Point;
        public Vector Normal;

        /// <summary>
        /// Constructor using any single point on the plane and a normal vector
        /// </summary>
        public Plane2D(Point3D P, Vector N)
        {
            Point = P;
            Normal = N.UnitVector();
        }

        /// <summary>
        /// Constructor for a 2D plane from 3 distinct points
        /// </summary>
        public Plane2D(Point3D p1, Point3D p2, Point3D p3)
        {
            Vector v1 = new Vector(p1, p2);
            Vector v2 = new Vector(p1, p3);
            Normal = v1.Cross(v2).UnitVector();
            Point = p1;
        }

        public override string ToString()
        {
            return Point.ToString() + "-" + Normal.ToString();
        }

        public double DistancePerpToPoint(Point3D P)
        {
            return Distances.Point2Plane(P, this);
        }

        public bool Equals(Plane2D otherPlane)
        {
            double tolerance = 1000;
            double tolerance2 = 0.01;
            if (Distances.Point2Plane(otherPlane.Point, this) > tolerance) return false;
            if (this.Normal.Cross(otherPlane.Normal).Length() < tolerance2) return true;
            return false;
        }
    }
}