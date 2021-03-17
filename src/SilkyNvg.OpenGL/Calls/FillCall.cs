using Silk.NET.OpenGL;
using SilkyNvg.OpenGL.Shaders;

namespace SilkyNvg.OpenGL.Calls
{
    internal class FillCall : Call
    {

        private readonly int _triangleOffset;
        private readonly int _triangleCount;

        public FillCall(int triangleOffset, int triangleCount, Blend blendFunc, FragmentDataUniforms uniforms, Path[] paths)
            : base(blendFunc, uniforms, paths)
        {
            _triangleOffset = triangleOffset;
            _triangleCount = triangleCount;
        }

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

            glInterface.SetUniforms(_uniforms, 0); // TODO: Image
            glInterface.CheckError("Fill fill");

            if (glInterface.LaunchParameters.Antialias)
            {
                glInterface.StencilFunc(StencilFunction.Equal, 0xff, 0);
                gl.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Keep);

                for (int i = 0; i < _paths.Length; i++)
                {
                    gl.DrawArrays(PrimitiveType.TriangleStrip, _paths[i].StrokeOffset, (uint)_paths[i].StrokeCount);
                }
            }

            glInterface.StencilFunc(StencilFunction.Notequal, 0xff, 0x0);
            gl.StencilOp(StencilOp.Zero, StencilOp.Zero, StencilOp.Zero);
            gl.DrawArrays(PrimitiveType.TriangleStrip, _triangleOffset, (uint)_triangleCount);

            gl.Disable(EnableCap.StencilTest);
        }
    }
}
