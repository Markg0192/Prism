using System;

namespace Prism.Geometry
{
    public class Vector 
    {
        public double dX;
        public double dY;
        public double dZ;

        public Vector (double dX, double dY, double dZ)
        {
            this.dX = dX;
            this.dY = dY;
            this.dZ = dZ;
        }

        public Vector()
        {

        }

        public Vector(Point3D start, Point3D end)
        {
            dX = end.X - start.X;
            dY = end.Y - start.Y;
            dZ = end.Z - start.Z;
        }

        public Vector (Vector original, double scaleFactor)
        {
            dX = original.dX * scaleFactor;
            dY = original.dY * scaleFactor;
            dZ = original.dZ * scaleFactor;
        }

        public override string ToString()
        {
            double x = Math.Round(dX, 3);
            double y = Math.Round(dY, 3);
            double z = Math.Round(dZ, 3);
            return "[" + x + ", " + y + ", " + z + "]";
        }

        public bool CompareTo(Vector obj)
        {
            return dX == obj.dX && dY == obj.dY && dZ == obj.dZ;
        }

        public double Length()
        {
            return Math.Sqrt(dX * dX + dY * dY + dZ * dZ);
        }

        public double Dot(Vector other)
        {
            return this.dX * other.dX + this.dY * other.dY + this.dZ * other.dZ;
        }

        /// <summary>
        /// Gets the angle between this and any other vector
        /// Returns the angle in Radians
        /// Returns null if one of the vectors has zero length
        /// </summary>
        public double? AngleBetween(Vector other)
        {
            double l1 = this.Length();
            double l2 = other.Length();
            double lProduct = l1 * l2;
            if (lProduct != 0)
            {
                return Math.Acos(this.Dot(other) / lProduct);
            }
            else return null;
        }

        public Vector UnitVector()
        {
            return new Vector(this, 1 / this.Length());
        }

        public void Scale(double scaleFactor)
        {
            dX = dX * scaleFactor;
            dY = dY * scaleFactor;
            dZ = dZ * scaleFactor;
        }

        public Vector Cross(Vector other)
        {
            double i = this.dY * other.dZ - this.dZ * other.dY;
            double j = this.dZ * other.dX - this.dX * other.dZ;
            double k = this.dX * other.dY - this.dY * other.dX;
            return new Vector(i, j, k);
        }

        public Vector Add(Vector other)
        {
            return new Vector(this.dX + other.dX, this.dY + other.dY, this.dZ + other.dZ);
        }

        public Vector Subtract(Vector other)
        {
            return new Vector(this.dX - other.dX, this.dY - other.dY, this.dZ - other.dZ);
        }
    }
}