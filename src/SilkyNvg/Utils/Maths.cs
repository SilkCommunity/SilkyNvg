using System;
using System.Numerics;

namespace SilkyNvg.Utils
{
    internal static class Maths
    {

        internal static bool IsNumeric(this float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }

        internal static bool IsNumeric(this Vector2 value)
        {
            return value.X.IsNumeric() && value.Y.IsNumeric();
        }

        internal static bool FpEquals(this float a, float b, float tol)
        {
            return Math.Abs(a - b) < tol;
        }

        internal static bool FpEquals(this Vector2 a, Vector2 b, float tol)
        {
            return a.X.FpEquals(b.X, tol) && a.Y.FpEquals(b.Y, tol);
        }
        
        internal static float InterpolateQuadraticBezier(float p0, float p1, float p2, float t)
        {
            float q1 = (1 - t) * p0 + t * p1;
            float q2 = (1 - t) * p1 + t * p2;

            return (1 - t) * q1 + t * q2;
        }

        internal static float InterpolateCubicBezier(float p0, float p1, float p2, float p3, float t)
        {
            float q1 = (1 - t) * p0 + t * p1;
            float q2 = (1 - t) * p1 + t * p2;
            float q3 = (1 - t) * p2 + t * p3;

            float r1 = (1 - t) * q1 + t * q2;
            float r2 = (1 - t) * q2 + t * q3;
            
            return (1 - t) * r1 + t * r2;
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
                t1 = (-b - sqrtOfDiscriminant) / (2 * a);
                t2 = (2 * c) / (-b - sqrtOfDiscriminant);
            }
            else
            {
                t1 = (2 * c) / (-b + sqrtOfDiscriminant);
                t2 = (-b + sqrtOfDiscriminant) / (2 * a);
            }
        }
        
    }
}