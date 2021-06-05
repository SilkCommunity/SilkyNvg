using Silk.NET.OpenGL;
using SilkyNvg.Blending;
using SilkyNvg.Rendering.OpenGL.Blending;
using SilkyNvg.Rendering.OpenGL.Shaders;
using System;

namespace SilkyNvg.Rendering.OpenGL.Calls
{
    internal class StencilStrokeCall : Call
    {

        private readonly FragUniforms _stencilUniforms;

        public StencilStrokeCall(int image, Path[] paths, FragUniforms stencilUniforms, FragUniforms uniforms, CompositeOperationState op, OpenGLRenderer renderer)
            : base(image, paths, 0, 0, uniforms, new Blend(op, renderer), renderer)
        {
            _stencilUniforms = stencilUniforms;
        }

        public override void Run()
        {
            GL gl = renderer.Gl;

            gl.Enable(EnableCap.StencilTest);
            renderer.StencilMask(0xff);

            // Fill the stroke base without overlap.
            renderer.StencilFunc(StencilFunction.Equal, 0x0, 0xff);
            gl.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Incr);
            _stencilUniforms.LoadToShader(renderer.Shader, image);
            renderer.CheckError("stroke fill 0");
            foreach (Path path in paths)
            {
                gl.DrawArrays(PrimitiveType.TriangleStrip, path.StrokeOffset, path.StrokeCount);
            }

            // Draw anti-aliased pixels.
            uniforms.LoadToShader(renderer.Shader, image);
            renderer.StencilFunc(StencilFunction.Equal, 0x0, 0xff);
            gl.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Keep);
            foreach (Path path in paths)
            {
                gl.DrawArrays(PrimitiveType.TriangleStrip, path.StrokeOffset, path.StrokeCount);
            }

            // Clear stencil buffer.
            gl.ColorMask(false, false, false, false);
            renderer.StencilFunc(StencilFunction.Always, 0x0, 0xff);
            gl.StencilOp(StencilOp.Zero, StencilOp.Zero, StencilOp.Zero);
            renderer.CheckError("stroke fill 1");
            foreach (Path path in paths)
            {
                gl.DrawArrays(PrimitiveType.TriangleStrip, path.StrokeOffset, path.StrokeCount);
            }
            gl.ColorMask(true, true, true, true);

            gl.Disable(EnableCap.StencilTest);
        }

    }
}
