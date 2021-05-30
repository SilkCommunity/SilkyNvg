using Silk.NET.OpenGL;
using SilkyNvg.Blending;
using SilkyNvg.Rendering.OpenGL.Blending;
using SilkyNvg.Rendering.OpenGL.Shaders;

namespace SilkyNvg.Rendering.OpenGL.Calls
{
    internal class ConvexFillCall : Call
    {

        public ConvexFillCall(int image, Path[] paths, FragUniforms uniforms, CompositeOperationState op, OpenGLRenderer renderer)
            : base(image, paths, 0, 0, uniforms, new Blend(op, renderer), renderer) { }

        public override void Run()
        {
            GL gl = renderer.Gl;

            uniforms.LoadToShader(renderer.Shader);
            renderer.CheckError("convex fill");

            foreach (Path path in paths)
            {
                gl.DrawArrays(PrimitiveType.TriangleFan, path.FillOffset, path.FillCount);
                if (path.StrokeCount > 0)
                {
                    gl.DrawArrays(PrimitiveType.TriangleStrip, path.StrokeOffset, path.StrokeCount);
                }
            }
        }

    }
}
