using Silk.NET.OpenGL;

namespace SilkyNvg.Rendering.OpenGL;

internal sealed class SceneContainer : ISceneContainer
{

    private readonly GL _gl;

    public SceneContainer(GL gl)
    {
        _gl = gl;
    }
    
    public void Clear()
    {
        
    }
    
}