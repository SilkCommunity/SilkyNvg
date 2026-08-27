using System.Numerics;
using Silk.NET.OpenGL;

namespace SilkyNvg.Rendering.OpenGL;

internal sealed class SceneContainer : ISceneContainer
{

    private readonly GL _gl;
    
    public uint PointCount { get; }
    
    public uint CurveCount { get; }
    
    public uint SubpathCount { get; }
    
    public uint PathCount { get; }
    
    public SceneContainer(GL gl)
    {
        _gl = gl;
    }
    
    public uint AddLine(Vector2 p0, Vector2 p1)
    {
        return 0;
    }

    public uint AddQuadratic(Vector2 p0, Vector2 p1, Vector2 p2)
    {
        return 0;
    }

    public uint AddCubic(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
    {
        return 0;
    }

    public uint AddRational(Vector2 p0, Vector2 p1, Vector2 p2, float w)
    {
        return 0;
    }

    public uint AddSubpath(bool isClosed)
    {
        return 0;
    }

    public uint AddPath(SilkyFillRule fillRule)
    {
        return 0;
    }

    public void Reserve(uint path, uint subpath, uint curve, uint point)
    {
        
    }

    public void Clear()
    {
        
    }

    public void ReduceDegenerate()
    {
        
    }
    
}