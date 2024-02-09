using System;
using Tekla.Structures.Geometry3d;

namespace Prism.Geometry
{
    public class Point3D
    {
        public double X;
        public double Y;
        public double Z;

        // Override Equals method
        public override bool Equals(object obj)
        {
            if (obj is Point3D other)
                return IsEqualTo(other);
            return false;
        }

        public bool IsEqualTo(Point3D other)
        {
            double tolerance = 0.5;
            return Math.Abs(X - other.X) < tolerance && Math.Abs(Y - other.Y) < tolerance && Math.Abs(Z - other.Z) < tolerance;
        }

        public Point3D(double X, double Y, double Z)
        {
            this.X = X;
            this.Y = Y;
            this.Z = Z;
        }

        public Point3D(Point p)
        {
            X = p.X;
            Y = p.Y;
            Z = p.Z;
        }

        public Point3D()
        {

        }

        public override string ToString()
        {
            double x = Math.Round(X, 3);
            double y = Math.Round(Y, 3);
            double z = Math.Round(Z, 3);
            return "(" + x + ", " + y + ", " + z + ")";
        }

        public Point3D Offset(Vector vector)
        {
            return new Point3D(this.X + vector.dX, this.Y + vector.dY, this.Z + vector.dZ);
        }

        public double DistancePerpToLine(Line1D L)
        {
            return Distances.Point2Line(this, L);
        }

        public double DistancePerpToPlane(Plane2D P)
        {
            return Distances.Point2Plane(this, P);
        }

        public double DistanceToOtherPoint(Point3D otherPoint)
        {
            return Distances.Point2Point(this, otherPoint);
        }

        public Point3D ProjectionOnLine(Line1D L)
        {
            Vector vL = L.Vector();
            Vector vP = new Vector(L.Start, this);
            double proj = vL.Dot(vP) / vL.Length();
            Vector uL = vL.UnitVector();
            uL.Scale(proj);
            return L.Start.Offset(uL);
        }

        public Point3D ProjectionOnPlane(Plane2D P)
        {
            Line1D L = new Line1D(this, P.Normal);
            return P.Point.ProjectionOnLine(L);
        }
    }
}