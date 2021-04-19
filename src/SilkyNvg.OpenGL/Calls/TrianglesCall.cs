using Silk.NET.OpenGL;
using SilkyNvg.OpenGL.Shaders;

namespace SilkyNvg.OpenGL.Calls
{
    internal class TrianglesCall : Call
    {

        private readonly int _offset;
        private readonly int _length;

        public TrianglesCall(int image, Blend blend, int offset, int length, FragmentDataUniforms frag)
            : base(image, blend, frag, null)
        {
            _offset = offset;
            _length = length;
        }

        public override void Run(GLInterface glInterface, GL gl)
        {
            glInterface.BlendFuncSeperate(_blendFunc);
            glInterface.SetUniforms(_uniforms, _image);
            glInterface.CheckError("triangles fill");

            gl.DrawArrays(PrimitiveType.Triangles, _offset, (uint)_length);
        }

    }
}
