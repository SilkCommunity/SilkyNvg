using Silk.NET.OpenGL;
using SilkyNvg.OpenGL.Shaders;

namespace SilkyNvg.OpenGL.Calls
{
    internal class StrokeCall : Call
    {

        public StrokeCall(Blend blendFunc, FragmentDataUniforms uniforms, Path[] paths)
            : base(blendFunc, uniforms, paths) { }

        public override void Run(GLInterface glInterface, GL gl)
        {
            if (glInterface.LaunchParameters.StencilStrokes)
            {
                // TODO: Implement Stencil Strokes
            }
            else
            {
                glInterface.SetUniforms(_uniforms, 0); // TODO: Image
                glInterface.CheckError("stroke fill");

                for (int i = 0; i < _paths.Length; i++)
                {
                    gl.DrawArrays(PrimitiveType.TriangleStrip, _paths[i].StrokeOffset, (uint)_paths[i].StrokeCount);
                }
            }
        }

    }
}
