using Silk.NET.OpenGL;

namespace SilkyNvg.OpenGL.Calls
{
    internal class FillCall : Call
    {

        public FillCall(int pathOffset, int pathCount, int triangleOffset, int triangleCount, int uniformOffset, Blend blendFunc)
            : base(pathOffset, pathCount, triangleOffset, triangleCount, uniformOffset, blendFunc) { }

        public override void Run(GLInterface glInterface, GL gl)
        {
            gl.Enable(EnableCap.StencilTest);
            glInterface.StencilMask(0xff);
            glInterface.StencilFunc(StencilFunction.Always, 0xff, 0);
            gl.ColorMask(false, false, false, false);

            glInterface.SetUniforms(_uniformOffset, 0);
            glInterface.CheckError("fill simple");

            gl.StencilOpSeparate(StencilFaceDirection.Front, StencilOp.Keep, StencilOp.Keep, StencilOp.IncrWrap);
            gl.StencilOpSeparate(StencilFaceDirection.Back, StencilOp.Keep, StencilOp.Keep, StencilOp.DecrWrap);
            gl.Disable(EnableCap.CullFace);
            for (int i = 0; i < _pathCount; i++)
            {
                gl.DrawArrays(PrimitiveType.TriangleFan, glInterface.Paths[_pathOffset + i].FillOffset, (uint)glInterface.Paths[_pathOffset + i].FillCount);
            }
            gl.Enable(EnableCap.CullFace);

            gl.ColorMask(true, true, true, true);

            glInterface.SetUniforms(_uniformOffset, 0);
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
