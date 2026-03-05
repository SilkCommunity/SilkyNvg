using System.Collections.Generic;
using System.Drawing;
using System.Numerics;

namespace SilkyNvg.Paths;

public class Path
{

    private readonly List<SegmentType> _pathSegment = [];
    private readonly List<Vector2> _pathPoints = [];

    public void ClosePath()
    {
        
    }

    public void MoveTo(float x, float y)
    {
        
    }

    public void MoveTo(PointF p)
    {
        
    }

    public void MoveTo(Vector2 p)
    {
        
    }

    public void LineTo(float x, float y)
    {
        
    }

    public void LineTo(PointF p)
    {
        
    }

    public void LineTo(Vector2 p)
    {
        
    }

    public void QuadraticCurveTo(float cpx, float cpy, float x, float y)
    {
        
    }

    public void QuadraticCurveTo(PointF cp, PointF p)
    {
        
    }

    public void QuadraticCurveTo(Vector2 cp, Vector2 p)
    {
        
    }

    public void BezierCurveTo(float cp1x, float cp1y, float cp2x, float cp2y, float x, float y)
    {
        
    }

    public void BezierCurveTo(PointF cp1, PointF cp2, PointF p)
    {
        
    }

    public void BezierCurveTo(Vector2 cp1, Vector2 cp2, Vector2 p)
    {
        
    }

    public void ArcTo(float x1, float y1, float x2, float y2, float radius)
    {
        
    }

    public void ArcTo(PointF p1, PointF p2, float radius)
    {
        
    }

    public void ArcTo(Vector2 p1, Vector2 p2, float radius)
    {
        
    }

    public void Rect(float x, float y, float w, float h)
    {
        
    }

    public void Rect(PointF p, SizeF size)
    {
        
    }

    public void Rect(Vector2 p, Vector2 size)
    {
        
    }

    public void RoundedRect(float x, float y, float w, float h, float radius = 0)
    {
        
    }

    public void RoundedRect(PointF p, SizeF size, float radius = 0)
    {
        
    }

    public void RoundedRect(Vector2 p, Vector2 size, float radius = 0)
    {
        
    }

    public void RoundedRectVarying(float x, float y, float w, float h, float r1 = 0, float r2 = 0, float r3 = 0, float r4 = 0)
    {
        
    }

    public void RoundedRectVarying(PointF p, SizeF size, float r1 = 0, float r2 = 0, float r3 = 0, float r4 = 0)
    {
        
    }

    public void RoundedRectVarying(Vector2 p, Vector2 size, float r1 = 0, float r2 = 0, float r3 = 0, float r4 = 0)
    {
        
    }

    public void Arc(float x, float y, float radius, float startAngle, float endAngle, bool counterClockwise = false)
    {
        
    }

    public void Arc(PointF p, float radius, float startAngle, float endAngle, bool counterClockwise = false)
    {
        
    }

    public void Arc(Vector2 p, float radius, float startAngle, float endAngle, bool counterClockwise = false)
    {
        
    }

    public void Ellipse(float x, float y, float radiusX, float radiusY, float rotation, float startAngle,
        float endAngle, bool counterClockwise = false)
    {
        
    }

    public void Ellipse(PointF p, float radiusX, float radiusY, float rotation, float startAngle,
        float endAngle, bool counterClockwise = false)
    {
        
    }

    public void Ellipse(Vector2 p, float radiusX, float radiusY, float rotation, float startAngle,
        float endAngle, bool counterClockwise = false)
    {
        
    }
    
}