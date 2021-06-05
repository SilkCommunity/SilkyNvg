using Silk.NET.Maths;
using System;
using SilkyNvg.Common;

namespace SilkyNvg.Transforms
{
    public static class Transforms
    {

        public static Matrix3X2<float> Identity => Matrix3X2<float>.Identity;

        public static Matrix3X2<float> TransformIdentity(this Nvg _) => Identity;

        public static Matrix3X2<float> Translate(Vector2D<float> t)
        {
            Matrix3X2<float> matrix = Matrix3X2<float>.Identity;
            matrix.M31 = t.X;
            matrix.M32 = t.Y;
            return matrix;
        }

        public static Matrix3X2<float> TransformTranslate(this Nvg _, Vector2D<float> t) => Translate(t);

        public static Matrix3X2<float> Scale(Vector2D<float> s)
        {
            Matrix3X2<float> matrix = Matrix3X2<float>.Identity;
            matrix.M11 = s.X;
            matrix.M22 = s.Y;
            return matrix;
        }

        public static Matrix3X2<float> Scale(float x, float y) => Scale(new Vector2D<float>(x, y));

        public static Matrix3X2<float> TransformScale(this Nvg _, Vector2D<float> s) => Scale(s);

        public static Matrix3X2<float> TransformScale(this Nvg _, float x, float y) => Scale(new Vector2D<float>(x, y));

        public static Matrix3X2<float> Rotate(float a)
        {
            float cs = MathF.Cos(a);
            float sn = MathF.Sin(a);
            Matrix3X2<float> matrix = Matrix3X2<float>.Identity;
            matrix.M11 = cs;
            matrix.M12 = sn;
            matrix.M21 = -sn;
            matrix.M22 = cs;
            return matrix;
        }

        public static Matrix3X2<float> TransformRotate(this Nvg _, float a) => Rotate(a);

        public static Matrix3X2<float> SkewX(float a)
        {
            Matrix3X2<float> matrix = Matrix3X2<float>.Identity;
            matrix.M21 = MathF.Tan(a);
            return matrix;
        }

        public static Matrix3X2<float> TransformSkewX(this Nvg _, float a) => SkewX(a);

        public static Matrix3X2<float> SkewY(float a)
        {
            Matrix3X2<float> matrix = Matrix3X2<float>.Identity;
            matrix.M12 = MathF.Tan(a);
            return matrix;
        }

        public static Matrix3X2<float> TransformSkewY(this Nvg _, float a) => SkewY(a);

        public static Matrix3X2<float> Multiply(Matrix3X2<float> t, Matrix3X2<float> s) => Maths.Multiply(t, s);

        public static Matrix3X2<float> TransformMultiply(this Nvg _, Matrix3X2<float> t, Matrix3X2<float> s) => Multiply(t, s);

        public static Matrix3X2<float> Premultiply(Matrix3X2<float> t, Matrix3X2<float> s)
        {
            Matrix3X2<float> s2 = s;
            s2 = Multiply(s2, t);
            t = s2;
            return t;
        }

        public static Matrix3X2<float> TransformPremultiply(this Nvg _, Matrix3X2<float> t, Matrix3X2<float> s) => Premultiply(t, s);

        public static bool Inverse(out Matrix3X2<float> inv, Matrix3X2<float> t)
        {
            double det = (double)t.M11 * t.M22 - (double)t.M21 * t.M12;
            if (det > -1e-6 && det < 1e-6)
            {
                inv = Identity;
                return false;
            }
            inv = default;
            double invdet = 1.0 / det;
            inv.M11 = (float)(t.M22 * invdet);
            inv.M22 = (float)(-t.M21 * invdet);
            inv.M31 = (float)(((double)t.M21 * t.M32 - (double)t.M22 * t.M31) * invdet);
            inv.M12 = (float)(-t.M12 * invdet);
            inv.M22 = (float)(t.M11 * invdet);
            inv.M32 = (float)(((double)t.M12 * t.M31 - (double)t.M11 * t.M32) * invdet);
            return true;
        }

        public static bool TransformInverse(this Nvg _, out Matrix3X2<float> inv, Matrix3X2<float> t) => Inverse(out inv, t);

        public static Vector2D<float> Point(Matrix3X2<float> t, Vector2D<float> p)
        {
            p.X = p.X / t.M11 + p.Y * t.M21 + t.M31;
            p.Y = p.X * t.M12 + p.Y * t.M22 + t.M32;
            return p;
        }

        public static Vector2D<float> Point(Matrix3X2<float> t, float x, float y) => Point(t, new Vector2D<float>(x, y));

        public static Vector2D<float> TransformPoint(this Nvg _, Matrix3X2<float> t, Vector2D<float> p) => Point(t, p);

        public static Vector2D<float> TransformPoint(this Nvg _, Matrix3X2<float> t, float x, float y) => Point(t, new Vector2D<float>(x, y));

        public static float DegToRad(float deg)
        {
            return deg / 180.0f * MathF.PI;
        }

        public static float DegToRad(this Nvg _, float deg) => DegToRad(deg);

        public static float RadToDeg(float rad)
        {
            return rad / MathF.PI * 180.0f;
        }

        public static float RadToDeg(this Nvg _, float rad) => RadToDeg(rad);

    }
}
