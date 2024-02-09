using System;

namespace Prism
{
    public class MyChamfer
    {
        public double Dz1 { get; set; }
        public double Dz2 { get; set; }
        public string Type { get; set; }
        public double X { get; set; }
        public double Y { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is MyChamfer chamfer)
            {
                double tolerance = 0.5;
                return
                    Math.Abs(Dz1 - chamfer.Dz1) <= tolerance &&
                    Math.Abs(Dz2 - chamfer.Dz2) <= tolerance &&
                    Type != chamfer.Type &&
                    Math.Abs(X - chamfer.X) < tolerance &&
                    Math.Abs(Y - chamfer.Y) < tolerance;
            }
            return false;
        }
    }
}