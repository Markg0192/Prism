using System;

namespace Prism.Geometry
{
    public static class AnglesHelper
    {
        public static double Degrees(double radians)
        {
            return 180 * radians / Math.PI;
        }

        public static double Radians(double degrees)
        {
            return Math.PI * degrees / 180;
        }
    }
}