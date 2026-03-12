using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using SilkyNvg.Commands;
using SilkyNvg.Paths;
using SilkyNvg.Utils;

namespace SilkyNvg
{
    public sealed class Path
    {
        
        private readonly List<Subpath> _subpaths = new List<Subpath>();

        private bool _needNewSubPath;
        
        private Subpath CurrentSubpath => _subpaths.Count == 0 ? null : _subpaths[_subpaths.Count - 1];

        public Path(Path path)
        {
            _needNewSubPath = true;
        }

        public void AddPath(Path path)
            => AddPath(path, Matrix3x2.Identity);

        public void AddPath(Path path, Matrix3x2 transform)
        {
            
        }

        private void EnsureSubpathExists(Vector2 p)
        {
            if (_needNewSubPath)
            {
                var subpath = new Subpath(p);
                _subpaths.Add(subpath);
                _needNewSubPath = false;
            }
        }

        public void Close()
        {
            var currentSubpath = CurrentSubpath;
            if (CurrentSubpath is null)
            {
                return;
            }

            currentSubpath.Close();
            var subpath = new Subpath(currentSubpath.FirstPoint);
            _subpaths.Add(subpath);
        }

        public void MoveTo(float x, float y)
            => MoveTo(new Vector2(x, y));

        public void MoveTo(PointF p)
            => MoveTo(p.ToVector2());
        
        public void MoveTo(Vector2 p)
        {
            if (!p.IsAlgebraic())
            {
                return;
            }

            var subpath = new Subpath(p);
            _subpaths.Add(subpath);
        }
        
        public void LineTo(float x, float y)
            => LineTo(new Vector2(x, y));

        public void LineTo(PointF p)
            => LineTo(p.ToVector2());
        
        public void LineTo(Vector2 p)
        {
            if (!p.IsAlgebraic())
            {
                return;
            }

            var currentSubpath = CurrentSubpath;
            if (currentSubpath is null)
            {
                EnsureSubpathExists(p);
            }
            else
            {
                currentSubpath.AddCommand(new LineToCommand(p));
            }
        }
        
        public void QuadraticCurveTo(float cpx, float cpy, float x, float y)
            => QuadraticCurveTo(new Vector2(cpx, cpy), new Vector2(x, y));

        public void QuadraticCurveTo(PointF cp, PointF p)
            => QuadraticCurveTo(cp.ToVector2(), p.ToVector2());
        
        public void QuadraticCurveTo(Vector2 cp, Vector2 p)
        {
            if (!cp.IsAlgebraic() || !p.IsAlgebraic())
            {
                return;
            }
            
            EnsureSubpathExists(cp);
            
            CurrentSubpath.AddCommand(new QuadraticCurveToCommand(cp, p));
        }
        
        public void BezierCurveTo(float cp1x, float cp1y, float cp2x, float cp2y, float x, float y)
            => BezierCurveTo(new Vector2(cp1x, cp1y), new Vector2(cp2x, cp2y), new Vector2(x, y));

        public void BezierCurveTo(PointF cp1, PointF cp2, PointF p)
            => BezierCurveTo(cp1.ToVector2(), cp2.ToVector2(), p.ToVector2());
        
        public void BezierCurveTo(Vector2 cp1, Vector2 cp2, Vector2 p)
        {
            if (!cp1.IsAlgebraic() || !cp2.IsAlgebraic() || !p.IsAlgebraic())
            {
                return;
            }
            
            EnsureSubpathExists(cp1);
            
            CurrentSubpath.AddCommand(new BezierCurveToCommand(cp1, cp2, p));
        }
        
    }
}