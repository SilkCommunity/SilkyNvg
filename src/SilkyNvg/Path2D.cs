using System;
using System.Collections.Generic;
using System.Numerics;
using SilkyNvg.Paths;
using SilkyNvg.Rendering;
using SilkyNvg.Utils;

namespace SilkyNvg
{
    public class Path2D()
    {
        
        private readonly List<Subpath> _subpaths = [];
        
        private bool _needNewSubPath = true;
        
        private bool HasSubpaths => _subpaths.Count > 0;
        
        private Subpath CurrentSubpath => _subpaths[^1];
        
        public Path2D(Path2D path)
            : this()
        {
            
        }

        public Path2D(string path)
            : this()
        {
            
        }

        public void AddPath(Path2D path, Matrix3x2 transform = default)
        {
            
        }

        #region Path

        private void EnsureSubpathExists(Vector2 p)
        {
            if (_needNewSubPath)
            {
                _subpaths.Add(new Subpath(p));
                _needNewSubPath = false;
            }
        }

        public void MoveTo(Vector2 p)
        {
            if (p.IsInfinityOrNan()) return;
            
            _subpaths.Add(new Subpath(p));
            _needNewSubPath = false;
        }

        public void MoveTo(float x, float y)
            => MoveTo(new Vector2(x, y));
        
        public void ClosePath()
        {
            if (!HasSubpaths)
            {
                return;
            }

            CurrentSubpath.MarkClosed();
            _subpaths.Add(new Subpath(CurrentSubpath.Start));
        }

        public void LineTo(Vector2 p)
        {
            if (p.IsInfinityOrNan()) return;
            
            if (!HasSubpaths)
            {
                EnsureSubpathExists(p);
            }
            else
            {
                CurrentSubpath.AddLine(p);
            }
        }
        
        
        public void LineTo(float x, float y)
            => LineTo(new Vector2(x, y));

        public void QuadraticCurveTo(Vector2 cp, Vector2 p)
        {
            if (cp.IsInfinityOrNan() || p.IsInfinityOrNan()) return;
            
            EnsureSubpathExists(cp);

            CurrentSubpath.AddQuadratic(cp, p);
        }

        public void QuadraticCurveTo(float cpx, float cpy, float x, float y)
            => QuadraticCurveTo(new Vector2(cpx, cpy), new Vector2(x, y));

        public void BezierCurveTo(Vector2 cp1, Vector2 cp2, Vector2 p)
        {
            if (cp1.IsInfinityOrNan() || cp2.IsInfinityOrNan() || p.IsInfinityOrNan()) return;
            
            EnsureSubpathExists(cp1);
            
            CurrentSubpath.AddCubic(cp1, cp2, p);
        }
        
        public void BezierCurveTo(float cp1x, float cp1y, float cp2x, float cp2y, float x, float y)
            => BezierCurveTo(new Vector2(cp1x, cp1y), new Vector2(cp2x, cp2y), new Vector2(x, y));

        public void ArcTo(Vector2 p1, Vector2 p2, float radius)
        {
            if (p1.IsInfinityOrNan() || p2.IsInfinityOrNan() || radius.IsInfinityOrNan()) return;

            EnsureSubpathExists(p1);

            CurrentSubpath.AddArcTo(p1, p2, radius);
        }
        
        public void ArcTo(float x1, float y1, float x2, float y2, float radius)
            => ArcTo(new Vector2(x1, y1), new Vector2(x2, y2), radius);

        public void Ellipse(Vector2 c, float radiusX, float radiusY, float rotation, float startAngle, float endAngle,
            bool counterclockwise = false)
        {
            if (c.IsInfinityOrNan() || radiusX.IsInfinityOrNan() || radiusY.IsInfinityOrNan() ||
                rotation.IsInfinityOrNan() || startAngle.IsInfinityOrNan() || endAngle.IsInfinityOrNan()) return;

            if (radiusX < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(radiusX), "Radius cannot be negative!");
            }

            if (radiusY < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(radiusY), "Radius cannot be negative!");
            }

            
        }
        
        #endregion

    }
}