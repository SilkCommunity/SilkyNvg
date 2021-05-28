using Silk.NET.Maths;

namespace SilkyNvg.Rendering.OpenGL.Utils
{
    internal static class Maths
    {

        public static Matrix3X2<float> Identity => Matrix3X2<float>.Identity;

        public static Matrix3X2<float> Inverse(Matrix3X2<float> t)
        {
            Matrix3X2<float> inv;
            double det = (double)t.M11 * t.M22 - (double)t.M21 * t.M12;
            if (det > -1e-6 && det < 1e-6)
            {
                inv = Identity;
                return inv;
            }
            inv = default;
            double invdet = 1.0 / det;
            inv.M11 = (float)(t.M22 * invdet);
            inv.M22 = (float)(-t.M21 * invdet);
            inv.M31 = (float)(((double)t.M21 * t.M32 - (double)t.M22 * t.M31) * invdet);
            inv.M12 = (float)(-t.M12 * invdet);
            inv.M22 = (float)(t.M11 * invdet);
            inv.M32 = (float)(((double)t.M12 * t.M31 - (double)t.M11 * t.M32) * invdet);
            return inv;
        }

        public static Matrix3X3<float> TransformToMat3x3(Matrix3X2<float> t)
        {
            return new Matrix3X3<float>(t);
        }

    }
}
