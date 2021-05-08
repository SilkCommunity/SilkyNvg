using SilkyNvg.Core.Maths;
using System.Numerics;

namespace SilkyNvg.Transforms
{
    public static class NvgTransforms
    {

        public static Matrix3x2 Identity => TransformsImpl.Identity;

        public static Matrix3x2 TransformIdentity(this Nvg _)
        {
            return Identity;
        }

    }
}
