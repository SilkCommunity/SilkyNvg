using System;
using Silk.NET.OpenGL;
using SilkyNvg.Rendering.OpenGL.Buffers;

namespace SilkyNvg.Rendering.OpenGL;

public sealed class SilkyOpenGLRenderer : ISilkyRenderer
{

    private readonly GL _gl;

    public ISceneContainer Container { get; }

    public SilkyOpenGLRenderer(GL gl)
    {
        _gl = gl;

        Container = new SceneContainer(_gl);
    }

    public void Resize(float newWidth, float newHeight, RenderTolerances tolerances)
    {
        
    }

    public void Dispose()
    {
        
    }
    
}