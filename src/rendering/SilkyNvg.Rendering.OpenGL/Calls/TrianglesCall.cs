using Silk.NET.OpenGL;
using SilkyNvg.Rendering.OpenGL.Blending;

namespace SilkyNvg.Rendering.OpenGL.Calls
{
    internal class TrianglesCall : Call
    {

        public TrianglesCall(int image, Blend blend, int triangleOffset, uint triangleCount, int uniformOffset, OpenGLRenderer renderer)
            : base(image, null, triangleOffset, triangleCount, uniformOffset, blend, renderer) { }

        public override void Run()
        {
            renderer.Shader.SetUniforms(uniformOffset, image);
            renderer.CheckError("triangles fill");

            renderer.Gl.DrawArrays(PrimitiveType.Triangles, triangleOffset, triangleCount);
        }

    }
}