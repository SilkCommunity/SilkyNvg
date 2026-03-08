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
        
    }
}