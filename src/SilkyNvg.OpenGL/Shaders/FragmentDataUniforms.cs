
using Silk.NET.Maths;
using System.Runtime.InteropServices;

namespace SilkyNvg.OpenGL.Shaders
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct FragmentDataUniforms
    {

        private Matrix3X4<float> _scissorMatrix;
        private Matrix3X4<float> _paintMatrix;
        private Vector4D<float> _innerColour;
        private Vector4D<float> _outerColour;
        private Vector2D<float> _scissorExt;
        private Vector2D<float> _scissorScale;
        private Vector2D<float> _extent;
        private float _radius;
        private float _feather;
        private float _strokeMult;
        private float _strokeThr;
        // TODO: Texture
        private int _type;

        public Matrix3X4<float> ScissorMatrix
        {
            get => _scissorMatrix;
            set => _scissorMatrix = value;
        }

        public Matrix3X4<float> PaintMatrix
        {
            get => _paintMatrix;
            set => _paintMatrix = value;
        }

        public Vector4D<float> InnerColour
        {
            get => _innerColour;
            set => _innerColour = value;
        }

        public Vector4D<float> OuterColour
        {
            get => _outerColour;
            set => _outerColour = value;
        }

        public Vector2D<float> ScissorExt
        {
            get => _scissorExt;
            set => _scissorExt = value;
        }

        public Vector2D<float> ScissorScale
        {
            get => _scissorScale;
            set => _scissorScale = value;
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

        public float StrokeMult
        {
            get => _strokeMult;
            set => _strokeMult = value;
        }

        public float StrokeThr
        {
            get => _strokeThr;
            set => _strokeThr = value;
        }

        public int Type
        {
            get => _type;
            set => _type = value;
        }

    }
}