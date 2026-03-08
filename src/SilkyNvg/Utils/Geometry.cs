using System;
using System.Numerics;

namespace SilkyNvg.Utils
{
    public static class Geometry
    {

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

        internal static bool QuadraticRoots(float a, float b, float c, float fpTol, out float x1, out float x2)
        {
            x1 = x2 = float.NaN;

            float discriminand = b * b - 4 * a * c;
            if (discriminand <= -fpTol)
            {
                return false;
            }
            else if (discriminand.FpEquals(0, fpTol))
            {

                x1 = x2 = -b / (2.0f * a);
            }
            else
            {
                if (b >= 0)
                {
                    x1 = (-b - (float)Math.Sqrt(discriminand)) / (2.0f * a);
                    x2 = (2.0f * c) / (-b - (float)Math.Sqrt(discriminand));
                }
                else
                {
                    x1 = (2.0f * c) / (-b + (float)Math.Sqrt(discriminand));
                    x2 = (-b + (float)Math.Sqrt(discriminand)) / (2.0f * a);
                }
            }

            return true;
        }
        
    }
}