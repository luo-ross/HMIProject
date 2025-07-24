using System;

namespace RS.Widgets.Commons
{
    public static class AngleHelper
    {
        /// <summary>
        ///     Method to convert from degrees to radians
        /// </summary>
        public static double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }

        /// <summary>
        ///     Method to convert from radians to degrees
        /// </summary>
        public static double RadiansToDegrees(double radians)
        {
            return radians * 180.0 / Math.PI;
        }
    }
}

