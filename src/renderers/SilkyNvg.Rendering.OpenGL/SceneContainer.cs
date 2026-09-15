using System;
using System.Numerics;
using Silk.NET.OpenGL;
using SilkyNvg.Rendering.OpenGL.Buffers;
using SilkyNvg.Rendering.OpenGL.Data;

namespace SilkyNvg.Rendering.OpenGL;

internal class SceneContainer : ISceneContainer, IDisposable
{

    private readonly GpuArrayList<Vector2> _vertices;
    private readonly GpuArrayList<SegmentData> _segments;
    private readonly GpuArrayList<SubpathData> _subpaths;
    private readonly GpuArrayList<PathData> _paths;

    internal uint VertexCount => _vertices.Count;

    internal uint SegmentCount => _segments.Count;
    
    internal uint SubpathCount => _subpaths.Count;
    
    internal uint PathCount => _paths.Count;
    
    internal SceneContainer(int framesInFlight, GL gl)
    {
        _vertices = new GpuArrayList<Vector2>(1, framesInFlight, "vertex_buffer", gl);
        _segments = new GpuArrayList<SegmentData>(1, framesInFlight, "segment_buffer", gl);
        _subpaths = new GpuArrayList<SubpathData>(1, framesInFlight, "subpath_buffer", gl);
        _paths = new  GpuArrayList<PathData>(1, framesInFlight, "path_buffer", gl);
    }

    internal void BeginFrame()
    {
        _vertices.BeginFrame();
        _segments.BeginFrame();
    }

    internal void EndFrame()
    {
        _vertices.EndFrame();
        _segments.EndFrame();
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

    private void AddSegment(SegmentType segmentType, float offset)
    {
        _segments.Add(new SegmentData(
            vertexIndex: VertexCount,
            type: segmentType,
            reversed: false,
            arcW1: 0x7f7fffff,
            offset: offset
        ));

        _subpaths.Span[(int)SubpathCount - 1].SegmentCount++;
    }
    
    public void AddLinear(Vector2 p0, Vector2 p1, float offset)
    {
        AddVertex(p0, p1);
        AddSegment(SegmentType.Linear, offset);
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
        _subpaths.Add(new SubpathData(
            segmentIndex: SegmentCount,
            segmentCount: 0,
            closed: false
        ));

        _paths.Span[(int)PathCount - 1].SubpathCount++;
    }

    public void AddPath()
    {
        _paths.Add(new PathData(
            subpathIndex: SubpathCount,
            subpathCount: 0
        ));
    }

    public void Clear()
    {
        _vertices.Clear();
        _segments.Clear();
        _subpaths.Clear();
        _paths.Clear();
    }

    public void Dispose()
    {
        _vertices.Dispose();
        _segments.Dispose();
        _subpaths.Dispose();
        _paths.Dispose();
    }
    
}