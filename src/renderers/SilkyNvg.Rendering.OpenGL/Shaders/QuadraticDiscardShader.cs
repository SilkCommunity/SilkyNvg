using System.Numerics;
using Silk.NET.OpenGL;

namespace SilkyNvg.Rendering.OpenGL.Shaders
{
    internal class QuadraticDiscardShader : ShaderProgramme
    {

        private readonly int _location_viewSize;
        
        public QuadraticDiscardShader(GL gl)
            : base(gl, (LoadShaderCode("quadraticDiscardShader.vert.glsl"), ShaderType.VertexShader),
                (LoadShaderCode("quadraticDiscardShader.frag.glsl"), ShaderType.FragmentShader))
        {
            _location_viewSize = Gl.GetUniformLocation(ProgrammeID, "viewSize");
        }

        internal void LoadViewSize(Vector2 viewSize)
        {
            Gl.Uniform2(_location_viewSize, viewSize);
        }
        
    }
}