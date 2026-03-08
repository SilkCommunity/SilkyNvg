using System;
using System.Numerics;

namespace SilkyNvg.Utils
{
    internal static class FloatTests
    {

        internal static bool IsNumeric(this Vector2 v)
        {
            return !(float.IsInfinity(v.X) || float.IsInfinity(v.Y) || float.IsNaN(v.X) || float.IsNaN(v.Y));
        }

        internal static bool FpEquals(this float a, float b, float epsilon)
        {
            return Math.Abs(a - b) <= epsilon;
        }

        internal static bool FpEquals(this Vector2 a, Vector2 b, float epsilon)
        {
            return a.X.FpEquals(b.X, epsilon) && a.Y.FpEquals(b.Y, epsilon);
        }
    
    }
}
