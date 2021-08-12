using Silk.NET.OpenGL;
using SilkyNvg.Blending;
using SilkyNvg.Rendering.OpenGL.Blending;

namespace SilkyNvg.Rendering.OpenGL.Calls
{
    internal class StrokeCall : Call
    {

        public StrokeCall(int image, Path[] paths, int uniformOffset, CompositeOperationState op, OpenGLRenderer renderer)
            : base(image, paths, 0, 0, uniformOffset, new Blend(op, renderer), renderer) { }

        public override void Run()
        {
            GL gl = renderer.Gl;

            renderer.Shader.SetUniforms(uniformOffset, image);
            renderer.CheckError("stroke fill");

            foreach (Path path in paths)
            {
                gl.DrawArrays(PrimitiveType.TriangleStrip, path.StrokeOffset, path.StrokeCount);
            }
        }

    }
}