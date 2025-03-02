using AngleSharp.Css.Values;
using Silk.NET.Maths;

namespace SilkyNvg.Extensions.Svg.Parser.Utils
{
    internal static class MatrixValueExtensions
    {

        internal static Matrix3X3<float> ToMatrix3X3(this TransformMatrix matrix)
        {
            return new Matrix3X3<float>(
                (float)matrix.M11, (float)matrix.M21, (float)matrix.M31,
                (float)matrix.M12, (float)matrix.M22, (float)matrix.M32,
                (float)matrix.M13, (float)matrix.M23, (float)matrix.M33
            );
        }

    }
}
