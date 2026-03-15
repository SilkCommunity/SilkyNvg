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

        internal static void CutQuadraticBezier(Vector2 p0, Vector2 p1, Vector2 p2, float t, out Vector2 cpl, out Vector2 cpr, out Vector2 hp)
        {
            cpl = Vector2.Lerp(p0, p1, t);
            cpr = Vector2.Lerp(p1, p2, t);
            hp = Vector2.Lerp(cpl, cpr, t);
        }

        internal static void CutCubicBezier(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t, out Vector2 cpl1,
            out Vector2 cpl2, out Vector2 cpr1, out Vector2 cpr2, out Vector2 hp)
        {
            Vector2 l = Vector2.Lerp(p0, p1, t);
            Vector2 h = Vector2.Lerp(p1, p2, t);
            Vector2 r = Vector2.Lerp(p2, p3, t);

            cpl1 = l;
            cpl2 = Vector2.Lerp(l, h, t);

            cpr1 = Vector2.Lerp(h, r, t);
            cpr2 = r;

            hp = Vector2.Lerp(cpl2, cpr1, t);
        }

        // Assume a != 0
        internal static void SolveQuadratic(float a, float b, float c, out float t1, out float t2)
        {
            float discriminant = b * b - 4 * a * c;
            if (discriminant < 0)
            {
                t1 = t2 = float.NaN;
                return;
            }
            else if (discriminant == 0)
            {
                t1 = -b / (2 * a);
                t2 = float.NaN;
                return;
            }
            
            float sqrtOfDiscriminant = (float)Math.Sqrt(discriminant);
            if (b >= 0)
            {
                t1 = (float)(-b - sqrtOfDiscriminant) / (2 * a);
                t2 = (2 * c) / (float)(-b - sqrtOfDiscriminant);
            }
            else
            {
                t1 = (2 * c) / (float)(-b + sqrtOfDiscriminant);
                t2 = (float)(-b + sqrtOfDiscriminant) / (2 * a);
            }
        }
        
    }
}