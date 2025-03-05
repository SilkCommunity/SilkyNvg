using SilkyNvg.Common.Geometry;
using System.Numerics;

namespace SilkyNvg.Rendering
{
    public struct Scissor
    {

        public Matrix3x2 Transform { get; }

        public SizeF Extent { get; }

        public Scissor(SizeF extent)
        {
            Extent = extent;
            Transform = default;
        }

        public Scissor(Matrix3x2 transform, SizeF extent)
        {
            Transform = transform;
            Extent = extent;
        }

    }
}
