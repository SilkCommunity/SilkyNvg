using Silk.NET.OpenGL;

namespace SilkyNvg.Rendering.OpenGL.Shaders
{
    internal class SimpleShader : ShaderProgramme
    {
        
        public SimpleShader(GL gl) : base(gl,
            ("shader.vert.glsl", ShaderType.VertexShader), ("shader.frag.glsl", ShaderType.FragmentShader))
        { }
        
    }
}