using System;
using System.Numerics;
using Silk.NET.OpenGL;
using SilkyNvg.Rendering.OpenGL.Buffers;

namespace SilkyNvg.Rendering.OpenGL;

internal class SceneContainer : ISceneContainer, IDisposable
{

    private readonly GpuArrayList<Vector2> _vertices;

    internal SceneContainer(int framesInFlight, GL gl)
    {
        _vertices = new GpuArrayList<Vector2>(1, framesInFlight, "vertex_buffer", gl);
    }

    internal void BeginFrame()
    {
        _vertices.BeginFrame();
    }

    internal void EndFrame()
    {
        _vertices.EndFrame();
    }

    private void AddVertex(Vector2 v)
    {
        _vertices.Add(v);
    }

    private void AddVertex(Vector2 v0, Vector2 v1)
    {
        _vertices.Add(v0);
        _vertices.Add(v1);
    }

    private void AddVertex(Vector2 v0, Vector2 v1, Vector2 v2)
    {
        _vertices.Add(v0);
        _vertices.Add(v1);
        _vertices.Add(v2);
    }

    private void AddVertex(Vector2 v0, Vector2 v1, Vector2 v2, Vector2 v3)
    {
        _vertices.Add(v0);
        _vertices.Add(v1);
        _vertices.Add(v2);
        _vertices.Add(v3);
    }
    
    public void AddLinear(Vector2 p0, Vector2 p1, float offset)
    {
        AddVertex(p0, p1);
    }

    public void AddQuadratic(Vector2 p0, Vector2 p1, Vector2 p2, float offset)
    {
        
    }

    public void AddCubic(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float offset)
    {
        
    }

    public void AddRational(Vector2 p0, Vector2 p1, Vector2 p2, float w, float offset)
    {
        
    }

    public void AddSubpath()
    {
        
    }

    public void AddPath()
    {
        
    }

    public void Clear()
    {
        
    }

    public void Dispose()
    {
        _vertices.Dispose();
    }
    
}