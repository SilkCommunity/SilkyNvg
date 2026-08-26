using System;
using Silk.NET.OpenGL;

namespace SilkyNvg.Rendering.OpenGL;

public sealed class SilkyOpenGLRenderer : ISilkyRenderer
{

    private readonly GL _gl;

    public SilkyOpenGLRenderer(GL gl)
    {
        _gl = gl;
    }

    public void Resize(float newWidth, float newHeight, RenderTolerances tolerances)
    {
        
    }

    public void Dispose()
    {
        
    }
}