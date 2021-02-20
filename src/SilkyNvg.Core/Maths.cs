using Silk.NET.Maths;
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

        public static Vector2D<float> TransformPoint(Vector2D<float> p, Matrix3X2<float> t)
        {
            return Vector2D.Transform(p, t);
        }

    }
}
