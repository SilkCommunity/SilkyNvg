using System.Numerics;
using Silk.NET.OpenGL;

namespace SilkyNvg.Rendering.OpenGL.Shaders
{
    internal class DebugShader : ShaderProgramme
    {

        private readonly int _locationViewSize;

        public DebugShader(GL gl) : base(gl,
            ("debug.vert.glsl", ShaderType.VertexShader), ("debug.frag.glsl", ShaderType.FragmentShader))
        {
            _locationViewSize = Gl.GetUniformLocation(ProgrammeId, "viewSize");
        }

        internal void LoadViewSize(Vector2 viewSize)
        {
            Gl.Uniform2(_locationViewSize, viewSize);
        }
        
    }
}