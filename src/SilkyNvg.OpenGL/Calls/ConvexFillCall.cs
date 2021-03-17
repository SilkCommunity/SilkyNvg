using Silk.NET.OpenGL;
using SilkyNvg.OpenGL.Shaders;

namespace SilkyNvg.OpenGL.Calls
{
    internal class ConvexFillCall : Call
    {

        private readonly int _triangleOffset;
        private readonly int _triangleCount;

        public ConvexFillCall(int triangleOffset, int triangleCount, Blend blendFunc, FragmentDataUniforms uniforms, Path[] paths)
            : base(blendFunc, uniforms, paths)
        {
            _triangleOffset = triangleOffset;
            _triangleCount = triangleCount;
        }


        public override void Run(GLInterface glInterface, GL gl)
        {
            glInterface.BlendFuncSeperate(_blendFunc);

            glInterface.SetUniforms(_uniforms, 0); // TODO: Image
            glInterface.CheckError("convex fill");

            for (int i = 0; i < _paths.Length; i++)
            {
                gl.DrawArrays(PrimitiveType.TriangleFan, _paths[i].FillOffset, (uint)_paths[i].FillCount);
                if (_paths[i].StrokeCount > 0)
                {
                    gl.DrawArrays(PrimitiveType.TriangleStrip, _paths[i].StrokeOffset, (uint)_paths[i].StrokeCount);
                }
            }
        }

    }
}
