using Silk.NET.Maths;

namespace SilkyNvg.Core
{
    public class Paint
    {

        public Matrix3X2<float> Transformation { get; private set; }
        public Vector2D<float> Extent { get; private set; }
        public float Radius { get; private set; }
        public float Feather { get; private set; }
        public Colour InnerColour { get; private set; }
        public Colour OuterColour { get; private set; }
        // TODO: Images

        public Paint(Matrix3X2<float> transformation, float radius, float feather, Colour inner, Colour outer)
        {
            Transformation = transformation;
            Radius = radius;
            Feather = feather;
            InnerColour = inner;
            OuterColour = outer;
        }

        private Paint(Matrix3X2<float> transformation, Vector2D<float> extent, float radius, float feather, Colour innerColour, Colour outerColour)
        {
            Transformation = transformation;
            Extent = extent;
            Radius = radius;
            Feather = feather;
            InnerColour = innerColour;
            OuterColour = outerColour;
        }

    }
}
