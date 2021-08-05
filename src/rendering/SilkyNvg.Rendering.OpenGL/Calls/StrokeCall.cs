using Silk.NET.OpenGL;
using SilkyNvg.Blending;
using SilkyNvg.Rendering.OpenGL.Blending;
using SilkyNvg.Rendering.OpenGL.Shaders;

namespace SilkyNvg.Rendering.OpenGL.Calls
{
    internal class StrokeCall : Call
    {

        public StrokeCall(int image, Path[] paths, FragUniforms uniforms, CompositeOperationState op, OpenGLRenderer renderer)
            : base(image, paths, 0, 0, uniforms, new Blend(op, renderer), renderer) { }

        public override void Run()
        {
            GL gl = renderer.Gl;

            uniforms.LoadToShader(renderer.Shader, image);
            renderer.CheckError("stroke fill");

            foreach (Path path in paths)
            {
                gl.DrawArrays(PrimitiveType.TriangleStrip, path.StrokeOffset, path.StrokeCount);
            }
        }

    }
}