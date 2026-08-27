using System;
using System.Numerics;

namespace SilkyNvg.Rendering;

public interface ISceneContainer
{
    
    uint PointCount { get; }

    uint CurveCount { get; }
    
    uint SubpathCount { get; }
    
    uint PathCount { get; }

    uint AddLine(Vector2 p0, Vector2 p1);

    uint AddQuadratic(Vector2 p0, Vector2 p1, Vector2 p2);
    
    uint AddCubic(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3);

    uint AddRational(Vector2 p0, Vector2 p1, Vector2 p2, float w);

    uint AddSubpath(bool isClosed);

    uint AddPath(SilkyFillRule fillRule);

    void Reserve(uint path, uint subpath, uint curve, uint point);
    
    void Clear();

    void ReduceDegenerate();

}