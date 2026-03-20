using System.Numerics;
using Silk.NET.OpenGL;

namespace SilkyNvg.Rendering.OpenGL.Shaders
{
    internal class AnchorGeometryShader : ShaderProgramme
    {

        private readonly int _location_viewSize;
        
        public AnchorGeometryShader(GL gl)
            : base(gl, (LoadShaderCode("anchorGeometryShader.vert.glsl"), ShaderType.VertexShader),
                (LoadShaderCode("anchorGeometryShader.frag.glsl"), ShaderType.FragmentShader))
        {
            _location_viewSize = Gl.GetUniformLocation(ProgrammeID, "viewSize");
        }

        internal void LoadViewSize(Vector2 viewSize)
        {
            Gl.Uniform2(_location_viewSize, viewSize);
        }
        
    }
}