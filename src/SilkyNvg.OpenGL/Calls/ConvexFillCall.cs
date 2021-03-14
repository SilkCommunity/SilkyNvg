using Silk.NET.OpenGL;
using SilkyNvg.OpenGL.Shaders;

namespace SilkyNvg.OpenGL.Calls
{
    internal class ConvexFillCall : Call
    {

        public ConvexFillCall(int triangleOffset, int triangleCount, Blend blendFunc, FragmentDataUniforms uniforms, Path[] paths)
            : base(triangleOffset, triangleCount, blendFunc, uniforms, paths) { }


        public override void Run(GLInterface glInterface, GL gl)
        {
            glInterface.BlendFuncSeperate(_blendFunc);

            glInterface.SetUniforms(_uniforms, 0);
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
