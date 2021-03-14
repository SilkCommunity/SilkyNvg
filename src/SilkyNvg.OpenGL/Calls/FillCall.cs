using Silk.NET.OpenGL;
using SilkyNvg.OpenGL.Shaders;

namespace SilkyNvg.OpenGL.Calls
{
    internal class FillCall : Call
    {

        public FillCall(int triangleOffset, int triangleCount, Blend blendFunc, FragmentDataUniforms uniforms, Path[] paths)
            : base(triangleOffset, triangleCount, blendFunc, uniforms, paths) { }

        public override void Run(GLInterface glInterface, GL gl)
        {
            glInterface.BlendFuncSeperate(_blendFunc);

            gl.Enable(EnableCap.StencilTest);
            glInterface.StencilMask(0xff);
            glInterface.StencilFunc(StencilFunction.Always, 0xff, 0);
            gl.ColorMask(false, false, false, false);

            glInterface.SetUniforms(_uniforms, 0);
            glInterface.CheckError("fill simple");

            gl.StencilOpSeparate(StencilFaceDirection.Front, StencilOp.Keep, StencilOp.Keep, StencilOp.IncrWrap);
            gl.StencilOpSeparate(StencilFaceDirection.Back, StencilOp.Keep, StencilOp.Keep, StencilOp.DecrWrap);
            gl.Disable(EnableCap.CullFace);
            for (int i = 0; i < _paths.Length; i++)
            {
                gl.DrawArrays(PrimitiveType.TriangleFan, _paths[i].FillOffset, (uint)_paths[i].FillCount);
            }
            gl.Enable(EnableCap.CullFace);

            gl.ColorMask(true, true, true, true);

            glInterface.SetUniforms(_uniforms, 0);
            glInterface.CheckError("Fill fill");

            if (glInterface.LaunchParameters.Antialias)
            {
                // TODO: Antialias
            }

            glInterface.StencilFunc(StencilFunction.Notequal, 0xff, 0x0);
            gl.StencilOp(StencilOp.Zero, StencilOp.Zero, StencilOp.Zero);
            gl.DrawArrays(PrimitiveType.TriangleStrip, _triangleOffset, (uint)_triangleCount);

            gl.Disable(EnableCap.StencilTest);
        }
    }
}
