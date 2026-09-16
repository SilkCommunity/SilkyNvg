using Silk.NET.OpenGL;

namespace SilkyNvg.Rendering.OpenGL.Shaders;

internal sealed class ComputeShader : ShaderProgramme
{
    
    internal ComputeShader(GL gl)
        : base(gl, ("compute.comp.glsl", ShaderType.ComputeShader))
    {
        
    }
    
}