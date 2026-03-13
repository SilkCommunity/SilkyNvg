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

        internal static void CutQuadraticBezierInHalf(Vector2 p0, Vector2 p1, Vector2 p2, out Vector2 cpl, out Vector2 cpr, out Vector2 hp)
        {
            cpl = Vector2.Lerp(p0, p1, 0.5f);
            cpr = Vector2.Lerp(p1, p2, 0.5f);
            hp = Vector2.Lerp(cpl, cpr, 0.5f);
        }

        internal static void CutCubicBezierInHalf(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, out Vector2 cpl1,
            out Vector2 cpl2, out Vector2 cpr1, out Vector2 cpr2, out Vector2 hp)
        {
            Vector2 l = Vector2.Lerp(p0, p1, 0.5f);
            Vector2 h = Vector2.Lerp(p1, p2, 0.5f);
            Vector2 r = Vector2.Lerp(p2, p3, 0.5f);

            cpl1 = l;
            cpl2 = Vector2.Lerp(l, h, 0.5f);

            cpr1 = Vector2.Lerp(h, r, 0.5f);
            cpr2 = r;

            hp = Vector2.Lerp(cpl2, cpr2, 0.5f);
        }
        
    }
}