using System;
using System.Numerics;

namespace SilkyNvg.Utils
{
    internal static class Maths
    {
        
        internal const float FloatEpsilon = 1e-03F;
        
        internal static readonly Matrix4x4 M3 = new Matrix4x4(
              1,  0,  0, 0,
             -3,  3,  0, 0,
              3, -6,  3, 0,
             -1,  3, -3, 1
        );

        internal static readonly Matrix4x4 M3Inverse = new Matrix4x4(
            1,      0,      0, 0,
            1, 1f / 3,      0, 0,
            1, 2f / 3, 1f / 3, 0,
            1,      1,      1, 1
        );
        
        internal static bool FpEquals(this float a, float b)
        {
            return Math.Abs(a - b) <= FloatEpsilon;
        }
        
        internal static bool FpEquals(this Vector2 a, Vector2 b)
        {
            return (a - b).LengthSquared() <= FloatEpsilon * FloatEpsilon;
        }

        internal static bool IsInfinityOrNan(this float f)
        {
            return float.IsInfinity(f) || float.IsNaN(f);
        }

        internal static bool IsInfinityOrNan(this Vector2 v)
        {
            return v.X.IsInfinityOrNan() || v.Y.IsInfinityOrNan();
        }

        internal static bool PointsAreCollinear(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            float area = p1.X * (p2.Y - p3.Y) + p2.X * (p3.Y - p1.Y) + p3.X * (p1.Y - p2.Y);
            return area.FpEquals(0);
        }

        internal static Vector2 PointOnEllipse(Vector2 origin, float radiusX, float radiusY, Matrix3x2 rotation,
            float angle)
        {
            var planarPoint = new Vector2(
                radiusX * MathF.Cos(angle),
                radiusY * MathF.Sin(angle)
            );
            return Vector2.Transform(planarPoint, rotation) + origin;
        }

        internal static float NormaliseAngle(float angle)
        {
            float result = angle % MathF.Tau;
            return result >= 0 ? result : result + MathF.Tau;
        }

        // Quadrant numbering:
        //  0 - bottom right | 1 - bottom left | 2 - top left | 3 - top right
        internal static int Quadrant(float normalisedAngle)
        {
            return normalisedAngle switch
            {
                >= 0 and < MathF.PI / 2             => 0,
                >= MathF.PI / 2 and < MathF.PI      => 1,
                >= MathF.PI and < 3 * MathF.PI / 2  => 2,
                _                                   => 3
            };
        }
        
    }
}