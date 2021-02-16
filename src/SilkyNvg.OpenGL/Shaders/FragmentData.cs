using Silk.NET.Maths;

namespace SilkyNvg.OpenGL.Shaders
{
    internal struct FragmentData
    {

        public Matrix3X3<float> ScissorMatrix { get; set; }
        public Matrix3X3<float> PaintMatrix { get; set; }
        public Colour InnerColour { get; set; }
        public Colour OuterColour { get; set; }
        public Vector2D<float> ScissorExtent { get; set; }
        public Vector2D<float> ScissorScale { get; set; }
        public Vector2D<float> Extent { get; set; }
        public float Radius { get; set; }
        public float Feather { get; set; }
        public float StrokeMultiplier { get; set; }
        public float StrokeThreshold { get; set; }
        public int TexType { get; set; }
        public int Type { get; set; }

        public void LoadToShader(NvgShader shader)
        {
            // TODO: Implement
        }

    }
}
