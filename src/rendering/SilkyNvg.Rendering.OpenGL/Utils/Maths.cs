using Silk.NET.Maths;

namespace SilkyNvg.Rendering.OpenGL.Utils
{
    internal static class Maths
    {

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

    }
}
