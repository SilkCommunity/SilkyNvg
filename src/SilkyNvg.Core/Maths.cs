using Silk.NET.Maths;
using System;

namespace SilkyNvg.Core
{
    public sealed class Maths
    {

        public static float Normalize(Vector2D<float> v)
        {
            float d = MathF.Sqrt(v.X * v.X + v.Y * v.Y);
            if (d > 1e-6f)
            {
                float id = 1.0f / d;
                v.X *= id;
                v.Y *= id;
            }
            return d;
        }

        public static float Triarea2(Vector2D<float> a, Vector2D<float> b, Vector2D<float> c)
        {
            float abx = b.X - a.X;
            float aby = b.Y - a.Y;
            float acx = c.X - a.X;
            float acy = c.Y - a.Y;
            return acx * aby - abx * acy;
        }

        public static Vector2D<float> TransformPoint(Vector2D<float> s, Matrix3X2<float> t)
        {
            return Vector2D.Transform(s, t);
        }

        public static bool PtEquals(float x1, float y1, float x2, float y2, float tol)
        {
            float dx = x2 - x1;
            float dy = y2 - y1;
            return dx * dx + dy * dy < tol * tol;
        }

    }
}
