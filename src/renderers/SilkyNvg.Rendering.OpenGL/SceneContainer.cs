using System;
using System.Numerics;
using Silk.NET.OpenGL;
using SilkyNvg.Rendering.OpenGL.Buffers;
using SilkyNvg.Rendering.OpenGL.Data;
using SilkyNvg.Rendering.OpenGL.Synchronization;

namespace SilkyNvg.Rendering.OpenGL;

internal class SceneContainer : ISceneContainer, IDisposable
{

    private const uint InitialVertexCount = 256;
    private const uint InitialSegmentCount = 256;
    private const uint InitialSubpathCount = 64;
    private const uint InitialPathCount = 16;
    
    internal readonly GpuArrayList<Vector2> _vertices;
    internal readonly GpuArrayList<SegmentData> _segments;
    internal readonly GpuArrayList<SubpathData> _subpaths;
    internal readonly GpuArrayList<PathData> _paths;

    internal uint VertexCount => _vertices.Count;

    internal uint SegmentCount => _segments.Count;
    
    internal uint SubpathCount => _subpaths.Count;
    
    internal uint PathCount => _paths.Count;
    
    internal SceneContainer(FrameManager frameManager, GL gl)
    {
        _vertices = new GpuArrayList<Vector2>(InitialVertexCount, frameManager, "vertex_buffer", gl);
        _segments = new GpuArrayList<SegmentData>(InitialSegmentCount, frameManager, "segment_buffer", gl);
        _subpaths = new GpuArrayList<SubpathData>(InitialSubpathCount, frameManager, "subpath_buffer", gl);
        _paths = new  GpuArrayList<PathData>(InitialPathCount, frameManager, "path_buffer", gl);
    }

    internal void MakeCurrentFrameCurrent()
    {
        _vertices.MakeCurrentFrameCurrent();
        _segments.MakeCurrentFrameCurrent();
        _subpaths.MakeCurrentFrameCurrent();
        _paths.MakeCurrentFrameCurrent();
    }

    internal void BindAll()
    {
        _vertices.Bind(0);
        _segments.Bind(1);
        _subpaths.Bind(2);
        _paths.Bind(3);
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
        AddSegment(SegmentType.Linear, offset);
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