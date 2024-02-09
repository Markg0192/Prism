using System;
using Tekla.Structures.Geometry3d;

namespace Prism.Geometry
{
    /// <summary>
    /// Class to give distances between geometric objects
    /// the answers are scalar numbers (they dont have a direction)
    /// i.e. always positive numbers
    /// </summary>
    public static class Distances
    {
        public static double Point2Point(Point3D p1, Point3D p2)
        {
            return new Vector(p1, p2).Length();
        }

        /// <summary>
        /// returns the distance between a point and the infinite projection of a line
        /// this distance will always be the 3D perpendicular distance
        /// </summary>
        public static double Point2Line(Point3D P, Line1D L)
        {
            Vector vL = L.Vector();
            Vector vP = new Vector(L.Start, P);
            return Math.Abs(vL.Cross(vP).Length() / vL.Length());
        }

        /// <summary>
        /// Perpendicular distance between a point and its projection on the infinite plane
        /// </summary>
        public static double Point2Plane(Point3D P, Plane2D plane)
        {
            Vector vP = new Vector(plane.Point, P);
            Vector N = plane.Normal;
            return Math.Abs(vP.Dot(N) / N.Length());
        }

        public static double Point2Point(Point p13d, Point p23d)
        {
            Point3D p1 = new Point3D(p13d.X, p13d.Y, p13d.Z);
            Point3D p2 = new Point3D(p23d.X, p23d.Y, p23d.Z);
            return new Vector(p1, p2).Length();
        }
    }
}