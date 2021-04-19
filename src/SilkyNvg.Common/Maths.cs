using Silk.NET.Maths;
using System;

namespace SilkyNvg.Common
{
    internal sealed class Maths
    {

        public static float Pi => MathF.PI;

        public static float Kappa => 0.5522847493f;

        public static Vector4D<float> IsectRects(float ax, float ay, float aw, float ah, float bx, float by, float bw, float bh)
        {
            float minx = MathF.Max(ax, bx);
            float miny = MathF.Max(ay, by);
            float maxx = MathF.Min(ax + aw, bx + bw);
            float maxy = MathF.Min(ay + ah, by + bh);
            return new Vector4D<float>(minx, miny, MathF.Max(0.0f, maxx - minx), MathF.Max(0.0f, maxy - miny));
        }

        public static bool IsTransformFlipped(Matrix3X2<float> transform)
        {
            float det = transform.M11 * transform.M22 - transform.M21 * transform.M12;
            return det < 0;
        }

        public static Matrix3X2<float> TransformMultiply(Matrix3X2<float> t, Matrix3X2<float> s)
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

        public static Matrix3X2<float> TransformPremultiply(Matrix3X2<float> t, Matrix3X2<float> s)
        {
            return TransformMultiply(s, t);
        }

        public static float GetAverageScale(Matrix3X2<float> t)
        {
            float sx = MathF.Sqrt(t.M11 * t.M11 + t.M21 * t.M21);
            float sy = MathF.Sqrt(t.M12 * t.M12 + t.M22 * t.M22);
            return (sx + sy) * 0.5f;
        }

        public static float Clamp(float value, float min, float max)
        {
            if (value < min)
                return min;
            else if (value > max)
                return max;
            else
                return value;
        }

        public static int Clamp(int value, int min, int max)
        {
            if (value < min)
                return min;
            else if (value > max)
                return max;
            else
                return value;
        }

        public static int CurveDivs(float r, float arc, float tol)
        {
            float da = MathF.Acos(r / (r + tol)) * 2.0f;
            return Math.Max(2, (int)MathF.Ceiling(arc / da));
        }

        public static Matrix3X4<float> XFormToMat3X4(Matrix3X2<float> t)
        {
            return new Matrix3X4<float>
            {
                M11 = t.M11,
                M12 = t.M12,
                M13 = 0.0f,
                M14 = 0.0f,
                M21 = t.M21,
                M22 = t.M22,
                M23 = 0.0f,
                M24 = 0.0f,
                M31 = t.M31,
                M32 = t.M32,
                M33 = 1.0f,
                M34 = 0.0f
            };
        }

        public static float Quantize(float a, float d)
        {
            return ((int)(a / d + 0.5f)) * d;
        }

        public static Matrix3X2<float> TransformInverse(Matrix3X2<float> t)
        {
            var inv = new Matrix3X2<float>();
            double det = (double)t.M11 * t.M22 - (double)t.M21 * t.M12;
            if (det > -1e-6 && det < 1e-6)
            {
                inv = Matrix3X2<float>.Identity;
            }
            double invdet = 1.0 / det;
            inv.M11 = (float)(t.M22 * invdet);
            inv.M21 = (float)(-t.M21 * invdet);
            inv.M31 = (float)(((double)t.M21 * t.M32 - (double)t.M22 * t.M31) * invdet);
            inv.M12 = (float)(-t.M12 * invdet);
            inv.M22 = (float)(t.M11 * invdet);
            inv.M32 = (float)(((double)t.M12 * t.M31 - (double)t.M11 * t.M32) * invdet);
            return inv;
        }

        public static Matrix3X2<float> TransformSkewX(float angle)
        {
            return new Matrix3X2<float>
            {
                M11 = 1.0f,
                M12 = 0.0f,
                M21 = MathF.Tan(angle),
                M22 = 1.0f,
                M31 = 0.0f,
                M32 = 0.0f,
            };
        }

        public static Matrix3X2<float> TransformSkewY(float angle)
        {
            return new Matrix3X2<float>
            {
                M11 = 1.0f,
                M12 = MathF.Tan(angle),
                M21 = 0.0f,
                M22 = 1.0f,
                M31 = 0.0f,
                M32 = 0.0f
            };
        }

        public static Matrix3X2<float> TransformTranslate(float x, float y)
        {
            return new Matrix3X2<float>()
            {
                M11 = 1.0f,
                M12 = 0.0f,
                M21 = 0.0f,
                M22 = 1.0f,
                M31 = x,
                M32 = y
            };
        }

        public static Matrix3X2<float> TransformScale(float x, float y)
        {
            return new Matrix3X2<float>()
            {
                M11 = x,
                M12 = 0.0f,
                M21 = 0.0f,
                M22 = y,
                M31 = 0.0f,
                M32 = 0.0f
            };
        }

        public static Matrix3X2<float> TransformRotate(float angle)
        {
            float cs = MathF.Cos(angle);
            float sn = MathF.Sin(angle);
            return new Matrix3X2<float>()
            {
                M11 = cs,
                M12 = sn,
                M21 = -sn,
                M22 = cs,
                M31 = 0.0f,
                M32 = 0.0f
            };
        }

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
            Vector2D<float> transformed = new()
            {
                X = s.X * t.M11 + s.Y * t.M21 + t.M31,
                Y = s.X * t.M12 + s.Y * t.M22 + t.M32
            };
            return transformed;
        }

        public static bool PtEquals(float x1, float y1, float x2, float y2, float tol)
        {
            float dx = x2 - x1;
            float dy = y2 - y1;
            return dx * dx + dy * dy < tol * tol;
        }

        public static float Cross(float dx0, float dy0, float dx1, float dy1)
        {
            return dx1 * dy0 - dx0 * dy1;
        }

        public static float distancePtSegment(float x, float y, float px, float py, float qx, float qy)
        {
            float pqx, pqy, dx, dy, d, t;
            pqx = qx - px;
            pqy = qy - py;
            dx = x - px;
            dy = y - py;
            d = pqx * pqx + pqy * pqy;
            t = pqx * dx + pqy * dy;
            if (d > 0) 
                t /= d;
            if (t < 0) 
                t = 0;
            else if (t > 1) 
                t = 1;
            dx = px + t * pqx - x;
            dy = py + t * pqy - y;
            return dx * dx + dy * dy;
        }

    }
}