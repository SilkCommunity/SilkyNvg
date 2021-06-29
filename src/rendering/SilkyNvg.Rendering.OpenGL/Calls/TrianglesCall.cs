using Silk.NET.OpenGL;
using SilkyNvg.Rendering.OpenGL.Blending;
using SilkyNvg.Rendering.OpenGL.Shaders;

namespace SilkyNvg.Rendering.OpenGL.Calls
{
    internal class TrianglesCall : Call
    {

        public TrianglesCall(int image, Blend blend, int triangleOffset, uint triangleCount, FragUniforms uniforms, OpenGLRenderer renderer)
            : base(image, null, triangleOffset, triangleCount, uniforms, blend, renderer) { }

        public override void Run()
        {
            uniforms.LoadToShader(renderer.Shader, image);
            renderer.CheckError("triangles fill");

            renderer.Gl.DrawArrays(PrimitiveType.Triangles, triangleOffset, triangleCount);
        }

    }
}
