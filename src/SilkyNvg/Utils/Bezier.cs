using System;
using System.Numerics;

namespace SilkyNvg.Utils;

internal static class Bezier
{
    
    internal static void SplitBezier(float t, Span<Vector2> points, Span<Vector2> left, Span<Vector2> right, int leftHead = 0, int rightHead = 0)
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

    internal static int CubicConvexHull(Span<Vector2> p, Span<int> hull)
    {
        // This is only for cubics!
        if (p.Length != 4 || hull.Length != 4)
        {
            return -1;
        }
            
        int left = 0;

        for (int i = 0; i < 4; i++)
        {
            if (p[i].X < p[left].X)
            {
                left = i;
            }
        }

        int current = left;
        int j = 0;
        int nhull = 0;

        do
        {
            hull[j] = current;
            nhull++;
            int bestGuessIndex = 0;

            for (int k = 0; k < 4; k++)
            {
                Vector2 best = p[bestGuessIndex] - p[current];
                Vector2 next = p[k] - p[current];
                    
                // if CP-k is to the right of current edge, it is a best guess.
                // (in case of colinearity, choose the point which is farthest from the current point)
                float cross = best.X * next.Y - best.Y * next.X;

                if (bestGuessIndex == current || cross < 0)
                {
                    bestGuessIndex = k;
                }
                else if (cross == 0)
                {
                    if (MathF.Abs(next.X) > MathF.Abs(best.X) || MathF.Abs(next.Y) > MathF.Abs(best.Y))
                    {
                        bestGuessIndex = k;
                    }
                }
            }

            j++;
            current = bestGuessIndex;
        } while (current != left && j < 4);

        return nhull;
    }
    
}