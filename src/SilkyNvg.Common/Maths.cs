using System;
using System.Numerics;

namespace SilkyNvg.Common
{
    internal static class Maths
    {

        public static float Mod(float a, float b)
        {
            return a % b;
        }

        public static int Clamp(int a, int min, int max)
        {
            return a < min ? min : (a > max ? max : a);
        }

        public static float Quantize(float a, float d)
        {
            return ((int)((a / d) + 0.5f)) * d;
        }

        public static float Sign(float a)
        {
            return a >= 0.0f ? 1.0f : -1.0f;
        }

        public static float Clamp(float a, float min, float max)
        {
            return a < min ? min : (a > max ? max : a);
        }

        public static float Cross(Vector2 d0, Vector2 d1)
        {
            return (d1.X * d0.Y) - (d0.X * d1.Y);
        }

        public static bool PtEquals(Vector2 p1, Vector2 p2, float tol)
        {
            Vector2 d = p2 - p1;
            return (d.X * d.X) + (d.Y * d.Y) < tol * tol;
        }

        public static float Triarea2(Vector2 a, Vector2 b, Vector2 c)
        {
            Vector2 ab = b - a;
            Vector2 ac = c - a;
            return (ac.X * ab.Y) - (ab.X * ac.Y);
        }

        public static float Normalize(ref Vector2 vector)
        {
            float d = MathF.Sqrt((vector.X * vector.X) + (vector.Y * vector.Y));
            if (d > 1e-6f)
            {
                float id = 1.0f / d;
                vector *= id;
            }
            return d;
        }

        public static float GetAverageScale(Matrix3x2 t)
        {
            float sx = MathF.Sqrt(t.M11 * t.M11 + t.M21 * t.M21);
            float sy = MathF.Sqrt(t.M12 * t.M12 + t.M22 * t.M22);
            return (sx + sy) * 0.5f;
        }

        public static uint CurveDivs(float r, float arc, float tol)
        {
            float da = MathF.Acos(r / (r + tol)) * 2.0f;
            return Math.Max(2, (uint)MathF.Ceiling(arc / da));
        }

        public static float DistPtSeg(Vector2 pos, Vector2 p, Vector2 q)
        {
            Vector2 pq = q - p;
            Vector2 d = pos - p;
            float delta = (pq.X * pq.X) + (pq.Y * pq.Y);
            float t = (pq.X * d.X) + (pq.Y * d.Y);

            if (delta > 0)
            {
                t /= delta;
            }


            t = Clamp(t, 0.0f, 1.0f);

            d = p + (t * pq) - pos;
            return (d.X * d.X) + (d.Y * d.Y);
        }

        public static bool IsTransformFlipped(Matrix3x2 t)
        {
            float det = (t.M11 * t.M22) - (t.M21 * t.M12);
            return det < 0.0f;
        }

    }
}
