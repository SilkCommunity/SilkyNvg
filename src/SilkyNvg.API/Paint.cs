using Silk.NET.Maths;

namespace SilkyNvg
{
    public class Paint
    {

        private Matrix3X2<float> _xform;
        private Vector2D<float> _extent;
        private float _radius;
        private float _feather;

        private Colour _innerColour;
        private Colour _outerColour;

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

        public float Radius
        {
            get => _radius;
            set => _radius = value;
        }
        public float Feather
        {
            get => _feather;
            set => _feather = value;
        }

        public Colour InnerColour
        {
            get => _innerColour;
            set => _innerColour = value;
        }

        public Colour OuterColour
        {
            get => _outerColour;
            set => _outerColour = value;
        }

        public Paint(Colour colour)
        {
            _xform = Matrix3X2<float>.Identity;
            _radius = 0.0f;
            _feather = 1.0f;
            _innerColour = colour;
            _outerColour = colour;
        }

    }
}
