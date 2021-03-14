using Silk.NET.Maths;

namespace SilkyNvg.Colouring
{

    /// <summary>
    /// A gradient colour / image.
    /// </summary>
    public class Paint
    {

        private Matrix3X2<float> _xform;
        private Vector2D<float> _extent;
        private float _radius;
        private float _feather;

        private Colour _innerColour;
        private Colour _outerColour;

        internal Matrix3X2<float> XForm
        {
            get => _xform;
            set => _xform = value;
        }

        internal Vector2D<float> Extent
        {
            get => _extent;
            set => _extent = value;
        }

        internal float Radius
        {
            get => _radius;
            set => _radius = value;
        }

        internal float Feather
        {
            get => _feather;
            set => _feather = value;
        }

        internal Colour InnerColour
        {
            get => _innerColour;
            set => _innerColour = value;
        }

        internal Colour OuterColour
        {
            get => _outerColour;
            set => _outerColour = value;
        }

        /// <summary>
        /// Create a paint from a <see cref="Colour"/>.
        /// </summary>
        /// <param name="colour">The colour the paint will have.</param>
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