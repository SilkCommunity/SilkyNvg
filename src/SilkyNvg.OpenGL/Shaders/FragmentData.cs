using Silk.NET.Maths;
using SilkyNvg.Core;

namespace SilkyNvg.OpenGL.Shaders
{
    internal struct FragmentData
    {

        public Matrix3X4<float> ScissorMatrix { get; set; }
        public Matrix3X4<float> PaintMatrix;
        public Colour InnerColour;
        public Colour OuterColour;
        public Vector2D<float> ScissorExtent { get; set; }
        public Vector2D<float> ScissorScale { get; set; }
        public Vector2D<float> Extent;
        public float Radius { get; set; }
        public float Feather { get; set; }
        public float StrokeMultiplier { get; set; }
        public float StrokeThreshold { get; set; }
        public int TexType { get; set; }
        public ShaderType Type { get; set; }

        public void LoadToShader(NvgShader shader)
        {
            shader.LoadFragmentData(this);
        }

        public void Premult(Colour inner, Colour outer)
        {
            InnerColour = new Colour();
            OuterColour = new Colour();

            InnerColour.R = inner.R * inner.A;
            InnerColour.G = inner.G * inner.A;
            InnerColour.B = inner.B * inner.A;

            OuterColour.R = outer.R * outer.A;
            OuterColour.G = outer.G * outer.A;
            OuterColour.B = outer.B * outer.A;
        }

    }
}
