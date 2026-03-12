using System;
using System.Numerics;

namespace SilkyNvg.Utils
{
    internal static class Maths
    {

        internal static bool IsAlgebraic(this float x)
        {
            return !float.IsNaN(x) && !float.IsInfinity(x);
        }

        internal static bool IsAlgebraic(this Vector2 v)
        {
            return v.X.IsAlgebraic() && v.Y.IsAlgebraic();
        }

        internal static bool FpEquals(this float a, float b, float tol)
        {
            return Math.Abs(a - b) <= tol;
        }

        internal static bool FpEquals(this Vector2 a, Vector2 b, float tol)
        {
            return a.X.FpEquals(b.X, tol) && a.Y.FpEquals(b.Y, tol);
        }
        
    }
}