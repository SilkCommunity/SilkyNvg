using Silk.NET.Maths;

namespace SilkyNvg.Core
{
    public class Scissor
    {

        private Matrix3X2<float> _xForm;
        private Vector2D<float> _extent;

        public Matrix3X2<float> XForm
        {
            get => _xForm;
            set
            {
                _xForm = value;
            }
        }

        public Vector2D<float> Extent
        {
            get => _extent;
            set
            {
                _extent = value;
            }
        }

        public Scissor(Vector2D<float> extent)
        {
            _extent = extent;
        }

    }
}
