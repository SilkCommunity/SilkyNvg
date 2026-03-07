using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using SilkyNvg.Paths.Commands;
using SilkyNvg.Rendering;
using SilkyNvg.Utils;

namespace SilkyNvg.Paths;

public class Path
{

    private readonly List<IPathCommand> _pathCommands = [];

    internal void Flatten(ISilkyRenderer renderer, uint width, uint height)
    {
        var verts = new List<Vector2>();
        foreach (var command in _pathCommands)
        {
            command.Flatten(verts);
        }
        renderer.Render(verts, new Vector2(width, height));
    }
    
    #region PathAPI

    public Path(Path? path)
    {
        if (path == null)
        {
            return;
        }
        _pathCommands.AddRange(path._pathCommands);
    }
    
    public Path()
        : this(null) {}

    public void AddPath(Path path, Matrix3x2 transform)
    {
        _pathCommands.AddRange(path._pathCommands);
    }

    #endregion
    
    #region PathCommands
    
    public void ClosePath()
    {
        if (_pathCommands.Count == 0)
        {
            return;
        }
        
        _pathCommands.Add(new CloseCommand());
    }

    public void MoveTo(float x, float y)
        => MoveTo(new Vector2(x, y));

    public void MoveTo(PointF p)
        => MoveTo(p.X, p.Y);

    public void MoveTo(Vector2 p)
    {
        if (!p.IsNumeric())
        {
            return;
        }
        
        _pathCommands.Add(new MoveToCommand(p));
    }

    public void LineTo(float x, float y)
        => LineTo(new Vector2(x, y));

    public void LineTo(PointF p)
        => LineTo(p.X, p.Y);

    public void LineTo(Vector2 p)
    {
        if (!p.IsNumeric())
        {
            return;
        }
        if (_pathCommands.Count == 0)
        {
            MoveTo(p);
        }
        
        _pathCommands.Add(new LineToCommand(p));
    }

    public void QuadraticCurveTo(float cpx, float cpy, float x, float y)
        => QuadraticCurveTo(new Vector2(cpx, cpy), new Vector2(x, y));

    public void QuadraticCurveTo(PointF cp, PointF p)
        => QuadraticCurveTo(cp.X, cp.Y, p.X, p.Y);

    public void QuadraticCurveTo(Vector2 cp, Vector2 p)
    {
        if (!cp.IsNumeric() || !p.IsNumeric())
        {
            return;
        }
        if (_pathCommands.Count == 0)
        {
            MoveTo(cp);
        }

        _pathCommands.Add(new QuadraticToCommand(cp, p));
    }

    public void BezierCurveTo(float cp1x, float cp1y, float cp2x, float cp2y, float x, float y)
        => BezierCurveTo(new Vector2(cp1x, cp1y), new Vector2(cp2x, cp2y), new Vector2(x, y));

    public void BezierCurveTo(PointF cp1, PointF cp2, PointF p)
        => BezierCurveTo(cp1.X, cp1.Y, cp2.X, cp2.Y, p.X, p.Y);

    public void BezierCurveTo(Vector2 cp1, Vector2 cp2, Vector2 p)
    {
        if (!cp1.IsNumeric() || !cp2.IsNumeric() || !p.IsNumeric())
        {
            return;
        }
        if (_pathCommands.Count == 0)
        {
            MoveTo(cp1);
        }

        _pathCommands.Add(new BezierToCommand(cp1, cp2, p));
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

    public void Rect(RectangleF rect)
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
    
    #endregion    
    
}