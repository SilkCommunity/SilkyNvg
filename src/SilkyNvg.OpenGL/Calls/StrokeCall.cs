using Silk.NET.OpenGL;
using SilkyNvg.OpenGL.Shaders;

namespace SilkyNvg.OpenGL.Calls
{
    internal class StrokeCall : Call
    {

        private readonly FragmentDataUniforms _secondUniforms;

        public StrokeCall(int image, Blend blendFunc, FragmentDataUniforms uniforms, Path[] paths, FragmentDataUniforms secondUniforms)
            : base(image, blendFunc, uniforms, paths)
        {
            _secondUniforms = secondUniforms;
        }

        public override void Run(GLInterface glInterface, GL gl)
        {
            if (glInterface.LaunchParameters.StencilStrokes)
            {
                gl.Enable(EnableCap.StencilTest);
                glInterface.StencilMask(0xff);

                glInterface.StencilFunc(StencilFunction.Equal, 0xff, 0);
                gl.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Incr);
                glInterface.SetUniforms(_secondUniforms, 0);
                glInterface.CheckError("stroke fill");
                for (int i = 0; i < _paths.Length; i++)
                {
                    gl.DrawArrays(PrimitiveType.TriangleStrip, _paths[i].StrokeOffset, (uint)_paths[i].StrokeCount);
                }

                glInterface.SetUniforms(_uniforms, 0); // TODO: Images
                glInterface.StencilFunc(StencilFunction.Equal, 0xff, 0);
                gl.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Keep);
                for (int i = 0; i < _paths.Length; i++)
                {
                    gl.DrawArrays(PrimitiveType.TriangleStrip, _paths[i].StrokeOffset, (uint)_paths[i].StrokeCount);
                }

                gl.ColorMask(false, false, false, false);
                glInterface.StencilFunc(StencilFunction.Always, 0xff, 0);
                gl.StencilOp(StencilOp.Zero, StencilOp.Zero, StencilOp.Zero);
                glInterface.CheckError("stroke fill 2");
                for (int i = 0; i < _paths.Length; i++)
                {
                    gl.DrawArrays(PrimitiveType.TriangleStrip, _paths[i].StrokeOffset, (uint)_paths[i].StrokeCount);
                }
                gl.ColorMask(true, true, true, true);

                gl.Disable(EnableCap.StencilTest);
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
