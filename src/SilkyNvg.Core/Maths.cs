using Silk.NET.Maths;
using SilkyNvg.Core.Geometry;
using System;

namespace SilkyNvg.Core
{
    public sealed class Maths
    {

        public static float Kappa
        {
            get
            {
                return 4.0f * (((float)Math.Sqrt(2.0) - 1.0f) / 3.0f);
            }
        }

        public static Matrix3X2<float> TransformIdentity => Matrix3X2<float>.Identity;

        public static float Normalize(Vector2D<float> src, ref Vector2D<float> dst)
        {
            float d = (float)Math.Sqrt(src.X * src.X + src.Y * src.Y);
            if (d > 1e-6f)
            {
                float id = 1.0f / d;
                dst.X *= id;
                dst.Y *= id;
            }
            return d;
        }

        public static Matrix3X4<float> XFormToMat3x4(Matrix3X2<float> t)
        {
            var m = new Matrix3X4<float>
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
            return m;
        }

        public static Vector2D<float> TransformPoint(Vector2D<float> p, Matrix3X2<float> t)
        {
            return Vector2D.Transform(p, t);
        }

        public static Matrix3X2<float> TransformInverse(Matrix3X2<float> t)
        {
            Matrix3X2<float> inv = new Matrix3X2<float>();
            double invdet, det = (double)t.M11 * t.M22 - (double)t.M21 - t.M12;
            if (det > -1e-6 && det < 1e-6)
            {
                inv = TransformIdentity;
            }
            invdet = 1.0 / det;
            inv.M11 = (float)(t.M22 * invdet);
            inv.M21 = (float)(-t.M21 * invdet);
            inv.M31 = (float)(((double)t.M21 * t.M32 - (double)t.M22 - t.M31) * invdet);
            inv.M12 = (float)(-t.M12 * invdet);
            inv.M22 = (float)(t.M11 * invdet);
            inv.M32 = (float)(((double)t.M12 * t.M31 - (double)t.M11 - t.M32) * invdet);
            return inv;
        }

        public static float Triarea2(Vector2D<float> a, Vector2D<float> b, Vector2D<float> c)
        {
            float abx = b.X - a.X;
            float aby = b.Y - a.Y;
            float acx = c.X - a.X;
            float acy = c.Y - a.Y;
            return acx * aby - abx * acy;
        }

        public static float PolyArea(Point[] points)
        {
            float area = 0;
            for (int i = 2; i < points.Length; i++)
            {
                var a = points[0];
                var b = points[i - 1];
                var c = points[i];
                area += Triarea2(a.Position, b.Position, c.Position);
            }
            return area * 0.5f;
        }

        public static Point[] PolyReverse(Point[] points)
        {
            int i = 0;
            int j = points.Length - 1;
            while (i < j)
            {
                var temp = points[i];
                points[i] = points[j];
                points[j] = temp;
                i++;
                j--;
            }
            return points;
        }

        public static bool PtEquals(Vector2D<float> a, Vector2D<float> b, float tol)
        {
            float dx = b.X - a.X;
            float dy = b.Y - a.Y;
            return dx * dx + dy * dy < tol * tol;
        }

    }
}
