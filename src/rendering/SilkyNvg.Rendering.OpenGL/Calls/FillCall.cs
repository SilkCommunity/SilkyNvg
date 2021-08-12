using Silk.NET.OpenGL;
using SilkyNvg.Blending;
using SilkyNvg.Rendering.OpenGL.Blending;

namespace SilkyNvg.Rendering.OpenGL.Calls
{
    internal class FillCall : Call
    {

        public FillCall(int image, Path[] paths, int triangleOffset, int uniformOffset, CompositeOperationState op, OpenGLRenderer renderer)
            : base(image, paths, triangleOffset, 4, uniformOffset, new Blend(op, renderer), renderer) { }

        public override void Run()
        {
            GL gl = renderer.Gl;

            gl.Enable(EnableCap.StencilTest);
            renderer.StencilMask(0xff);
            renderer.StencilFunc(StencilFunction.Always, 0, 0xff);
            gl.ColorMask(false, false, false, false);

            renderer.Shader.SetUniforms(uniformOffset, 0);
            renderer.CheckError("fill simple");

            gl.StencilOpSeparate(StencilFaceDirection.Front, StencilOp.Keep, StencilOp.Keep, StencilOp.IncrWrap);
            gl.StencilOpSeparate(StencilFaceDirection.Back, StencilOp.Keep, StencilOp.Keep, StencilOp.DecrWrap);
            gl.Disable(EnableCap.CullFace);
            foreach (Path path in paths)
            {
                gl.DrawArrays(PrimitiveType.TriangleFan, path.FillOffset, path.FillCount);
            }
            gl.Enable(EnableCap.CullFace);

            gl.ColorMask(true, true, true, true);

            renderer.Shader.SetUniforms(uniformOffset + renderer.Shader.FragSize, image);
            renderer.CheckError("fill fill");

            if (renderer.EdgeAntiAlias)
            {
                renderer.StencilFunc(StencilFunction.Equal, 0x0, 0xff);
                gl.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Keep);
                foreach (Path path in paths)
                {
                    gl.DrawArrays(PrimitiveType.TriangleStrip, path.StrokeOffset, path.StrokeCount);
                }
            }

            renderer.StencilFunc(StencilFunction.Notequal, 0x0, 0xff);
            gl.StencilOp(StencilOp.Zero, StencilOp.Zero, StencilOp.Zero);
            gl.DrawArrays(PrimitiveType.TriangleStrip, triangleOffset, triangleCount);

            gl.Disable(EnableCap.StencilTest);
        }

    }
}