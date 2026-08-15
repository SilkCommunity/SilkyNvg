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

        internal static float Cross(Vector2 a, Vector2 b, Vector2 c)
        {
            Vector2 d1 = b - a;
            Vector2 d2 = c - a;
            return d1.X * d2.Y - d1.Y * d2.X;
        }

        internal static int ConvexHull(Span<Vector2> points, Span<int> hull)
        {
            // Smaller point set cannot have a convex hull
            if (points.Length < 3)
            {
                return 0;
            }

            int n = points.Length;

            // Find the leftmost point
            int left = 0;
            for (int i = 1; i < n; i++)
            {
                if (points[i].X < points[left].X || (points[i].X == points[left].X && points[i].Y < points[left].Y))
                {
                    left = i;
                }
            }

            int Orientation(Vector2 p, Vector2 q, Vector2 r)
            {
                float cross = (q.X - p.X) * (r.Y - p.Y) - (q.Y - p.Y) * (r.X - p.X);
                if (cross == 0)
                    return 0;
                return cross > 0 ? 1 : -1;
            }

            int nhull = 0;
            int p = left;
            do
            {
                hull[nhull++] = p;
                int q = (p + 1) % n; // initial candidate
                for (int i = 0; i < n; i++)
                {
                    if (i != p && (Orientation(points[p], points[i], points[q]) > 0 ||
                                   (Orientation(points[p], points[i], points[q]) == 0 &&
                                    (points[p] - points[i]).LengthSquared() > (points[p] - points[q]).LengthSquared())))
                    {
                        q = i;
                    }
                }
                p = q;
            } while (p != left);

            if (nhull > 1 && hull[0] == hull[nhull - 1])
            {
                nhull--;
            }

            return nhull;
        }
        
        internal static void SplitBezier(float t, Vector2[] points, Vector2[] left, Vector2[] right, int leftHead = 0, int rightHead = 0)
        {
            if (points.Length != left.Length - leftHead || points.Length != right.Length - rightHead)
            {
                throw new Exception("Splitting destination array missized");
            }

            if (points.Length == 1)
            {
                left[leftHead++] = points[0];
                right[right.Length - 1 - rightHead++] = points[0];
            }
            else
            {
                var newPoints = new Vector2[points.Length - 1];
                for (int i = 0; i < newPoints.Length; i++)
                {
                    if (i == 0)
                    {
                        left[leftHead++] = points[i];
                    }
                    if (i == newPoints.Length - 1)
                    {
                        right[right.Length - 1 - rightHead++] = points[i + 1];
                    }

                    newPoints[i] = (1 - t) * points[i] + t * points[i + 1];
                }
                SplitBezier(t, newPoints, left, right, leftHead, rightHead);
            }
        }
        
    }
}