﻿using Silk.NET.Maths;
using System;

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

        public static float Cross(Vector2D<float> d0, Vector2D<float> d1)
        {
            return (d1.X * d0.Y) - (d0.X * d1.Y);
        }

        public static bool PtEquals(Vector2D<float> p1, Vector2D<float> p2, float tol)
        {
            Vector2D<float> d = p2 - p1;
            return (d.X * d.X) + (d.Y * d.Y) < tol * tol;
        }

        public static float Triarea2(Vector2D<float> a, Vector2D<float> b, Vector2D<float> c)
        {
            Vector2D<float> ab = b - a;
            Vector2D<float> ac = c - a;
            return (ac.X * ab.Y) - (ab.X * ac.Y);
        }

        public static float Normalize(ref Vector2D<float> vector)
        {
            float d = MathF.Sqrt((vector.X * vector.X) + (vector.Y * vector.Y));
            if (d > 1e-6f)
            {
                float id = 1.0f / d;
                vector *= id;
            }
            return d;
        }

        public static Matrix3X2<float> Multiply(Matrix3X2<float> t, Matrix3X2<float> s)
        {
            float t0 = t.M11 * s.M11 + t.M12 * s.M21;
            float t2 = t.M21 * s.M11 + t.M22 * s.M21;
            float t4 = t.M31 * s.M11 + t.M32 * s.M21 + s.M31;
            t.M12 = t.M11 * s.M12 + t.M12 * s.M22;
            t.M22 = t.M21 * s.M12 + t.M22 * s.M22;
            t.M32 = t.M31 * s.M12 + t.M32 * s.M22 + s.M32;
            t.M11 = t0;
            t.M21 = t2;
            t.M31 = t4;
            return t;
        }

        public static float GetAverageScale(Matrix3X2<float> t)
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

        public static float DistPtSeg(Vector2D<float> pos, Vector2D<float> p, Vector2D<float> q)
        {
            Vector2D<float> pq = q - p;
            Vector2D<float> d = pos - p;
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

        public static bool IsTransformFlipped(Matrix3X2<float> t)
        {
            float det = (t.M11 * t.M22) - (t.M21 * t.M12);
            return det < 0.0f;
        }

        internal static float VecAngle(Vector2D<float> u, Vector2D<float> v)
        {
            float r = Vector2D.Dot(u, v) / (u.Length * v.Length);
            r = MathF.Max(r, -1.0f);
            r = MathF.Min(r, 1.0f);
            return ((u.X * v.Y < u.Y * v.X) ? (-1.0f) : 1.0f) * MathF.Acos(r);
        }

        internal static float Square(float x)
        {
            return x * x;
        }

    }
}
