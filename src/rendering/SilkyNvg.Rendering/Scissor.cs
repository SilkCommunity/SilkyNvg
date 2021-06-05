using Silk.NET.Maths;

namespace SilkyNvg.Rendering
{
    public struct Scissor
    {

        public Matrix3X2<float> Transform { get; }

        public Vector2D<float> Extent { get; }

        public Scissor(Vector2D<float> extent)
        {
            Extent = extent;
            Transform = default;
        }

    }
}
