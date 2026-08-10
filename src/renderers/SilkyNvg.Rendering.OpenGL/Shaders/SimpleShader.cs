using System.Numerics;
using Silk.NET.OpenGL;

namespace SilkyNvg.Rendering.OpenGL.Shaders
{
    internal class SimpleShader : ShaderProgramme
    {

        private readonly int _locationViewSize;

        public SimpleShader(GL gl) : base(gl,
            ("shader.vert.glsl", ShaderType.VertexShader), ("shader.frag.glsl", ShaderType.FragmentShader))
        {
            _locationViewSize = Gl.GetUniformLocation(ProgrammeId, "viewSize");
        }

        internal void LoadViewSize(Vector2 viewSize)
        {
            Gl.Uniform2(_locationViewSize, viewSize);
        }
        
    }
}