using System.Numerics;
using Silk.NET.OpenGL;

namespace SilkyNvg.Rendering.OpenGL.Shaders
{
    internal class PaintingShader : ShaderProgramme
    {

        private readonly int _location_pathBounds;
        private readonly int _location_viewSize;
        
        internal PaintingShader(GL gl)
            : base(gl,(LoadShaderCode("paintingShader.vert.glsl"), ShaderType.VertexShader),
                (LoadShaderCode("paintingShader.frag.glsl"), ShaderType.FragmentShader))
        {
            _location_pathBounds = Gl.GetUniformLocation(ProgrammeID, "pathBounds");
            _location_viewSize = Gl.GetUniformLocation(ProgrammeID, "viewSize");
        }

        internal void LoadPathBounds(Vector4 pathBounds)
        {
            Gl.Uniform4(_location_pathBounds, pathBounds);
        }

        internal void LoadViewSize(Vector2 viewSize)
        {
            Gl.Uniform2(_location_viewSize, viewSize);
        }
        
    }
}