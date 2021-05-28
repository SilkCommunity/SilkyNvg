using Silk.NET.OpenGL.Legacy;
using SilkyNvg.Rendering.OpenGL.Legacy.Blending;
using SilkyNvg.Rendering.OpenGL.Legacy.Paths;
using SilkyNvg.Rendering.OpenGL.Legacy.Shaders;

namespace SilkyNvg.Rendering.OpenGL.Legacy.Calls
{
    internal class ConvexFillCall : Call
    {

        public ConvexFillCall(Uniforms uniforms, Blend blend, int image, Path[] paths) : base(uniforms, blend, image, paths)
        {

        }

        public override void Run(LegacyOpenGLRenderer renderer)
        {
            blend.BlendFuncSeperate(renderer.Filter);
            uniforms.LoadToShader(renderer.Shader);
            CheckError("convex fill", renderer);

            GL gl = renderer.Gl;
            for (int i = 0; i < paths.Length; i++)
            {
                gl.DrawArrays(PrimitiveType.TriangleFan, (int)paths[i].FillOffset, paths[i].FillCount);
                if (paths[i].HasStroke)
                {
                    gl.DrawArrays(PrimitiveType.TriangleStrip, (int)paths[i].StrokeOffset, paths[i].StrokeCount);
                }
            }
        }

    }
}
