using Silk.NET.Maths;

namespace SilkyNvg
{
    internal struct Scissor
    {

        private Matrix3X2<float> _xform;
        private Vector2D<float> _extent;

        public Matrix3X2<float> XForm
        {
            get => _xform;
            set => _xform = value;
        }

        public Vector2D<float> Extent
        {
            get => _extent;
            set => _extent = value;
        }

        public Scissor(Vector2D<float> extent)
        {
            _extent = extent;
            _xform = new Matrix3X2<float>();
        }

    }
}