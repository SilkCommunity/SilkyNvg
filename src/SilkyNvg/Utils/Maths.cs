using System;
using System.Numerics;

namespace SilkyNvg.Utils
{
    internal static class Maths
    {
        
        internal const float FloatEpsilon = 1e-03F;

        internal static bool FpEquals(this float a, float b)
        {
            return Math.Abs(a - b) < FloatEpsilon;
        }
        
        internal static bool FpEquals(this Vector2 a, Vector2 b)
        {
            return (a - b).LengthSquared() < FloatEpsilon * FloatEpsilon;
        }

        internal static bool IsInfinityOrNan(this float f)
        {
            return float.IsInfinity(f) || float.IsNaN(f);
        }

        internal static bool IsInfinityOrNan(this Vector2 v)
        {
            return v.X.IsInfinityOrNan() || v.Y.IsInfinityOrNan();
        }
        
    }
}