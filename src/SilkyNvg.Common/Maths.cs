using Silk.NET.Maths;
using System;

namespace SilkyNvg.Common
{
    internal sealed class Maths
    {

        public static float Kappa => 0.5522847493f;

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

        public static float[] ToFloatArrayMatrix(Matrix3X4<float> t)
        {
            float[] m = new float[3 * 4];
            m[0] = t.M11;
            m[1] = t.M12;
            m[2] = t.M13;
            m[3] = t.M14;
            m[4] = t.M21;
            m[5] = t.M22;
            m[6] = t.M23;
            m[7] = t.M24;
            m[8] = t.M31;
            m[9] = t.M32;
            m[10] = t.M33;
            m[11] = t.M34;
            return m;
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