using Silk.NET.Maths;
using System.Runtime.InteropServices;

namespace SilkyNvg.OpenGL.Shaders
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct FragmentDataUniforms
    {

        private Matrix4X3<float> _scissorMatrix;
        private Matrix4X3<float> _paintMatrix;
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
        private int type;

    }
}
