using System;
namespace RS.Win32API.Standard
{
    public static class FloatUtil
    {
        public static float FLT_EPSILON  =   1.192092896e-07F;
        public static float FLT_MAX_PRECISION = 0xffffff;
        public static float INVERSE_FLT_MAX_PRECISION = 1.0F / FLT_MAX_PRECISION;

        /// <summary>
        /// AreClose
        /// </summary>
        public static bool AreClose(float a, float b)
        {
            if(a == b) return true;
            // This computes (|a-b| / (|a| + |b| + 10.0f)) < FLT_EPSILON
            float eps = ((float)Math.Abs(a) + (float)Math.Abs(b) + 10.0f) * FLT_EPSILON;
            float delta = a - b;
            return(-eps < delta) && (eps > delta);
        }

        /// <summary>
        /// IsOne
        /// </summary>
        public static bool IsOne(float a)
        {
            return (float)Math.Abs(a-1.0f) < 10.0f * FLT_EPSILON;
        }

        /// <summary>
        /// IsZero
        /// </summary>
        public static bool IsZero(float a)
        {
            return (float)Math.Abs(a) < 10.0f * FLT_EPSILON;
        }

        /// <summary>
        /// IsCloseToDivideByZero
        /// </summary>
        public static bool IsCloseToDivideByZero(float numerator, float denominator)
        {
            // When updating this, please also update code in Arithmetic.h
            return Math.Abs(denominator) <= Math.Abs(numerator) * INVERSE_FLT_MAX_PRECISION;
        }

    }
}
