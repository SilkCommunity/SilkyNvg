using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using SilkyNvg.Commands;
using SilkyNvg.Paths;
using SilkyNvg.Rendering;
using SilkyNvg.Utils;

namespace SilkyNvg
{
    public class Path2D
    {
        
        private readonly List<SubPath> _subPaths = new List<SubPath>();

        private bool _needsNewSubpath = true;
        
        private bool HasSubPaths => _subPaths.Count > 0;
        
        private SubPath CurrentSubPath => HasSubPaths ? _subPaths[_subPaths.Count - 1] : null;

        private void EnsureSubPathExists(Vector2 p)
        {
            if (_needsNewSubpath)
            {
                _subPaths.Add(new SubPath(p));
                _needsNewSubpath = false;
            }
        }

        internal void FillToScene(Scene scene, RenderTolerances tolerances)
        {
            var bounds = new Vector4(float.PositiveInfinity, float.PositiveInfinity, float.NegativeInfinity,
                float.NegativeInfinity);
            
            scene.BeginPath();
            foreach (var subPath in _subPaths)
            {
                if (subPath.Closed)
                {
                    Vector4 subPathBounds = subPath.FillToScene(scene, Matrix3x2.Identity, tolerances);
                    bounds.X = Math.Min(bounds.X, subPathBounds.X);
                    bounds.Y = Math.Min(bounds.Y, subPathBounds.Y);
                    bounds.Z = Math.Max(bounds.Z, subPathBounds.Z);
                    bounds.W = Math.Max(bounds.W, subPathBounds.W);
                }
            }
            scene.EndPath(bounds);
        }
        
        public void ClosePath()
        {
            if (!HasSubPaths)
            {
                return;
            }
            
            CurrentSubPath.MarkClosed();
            _subPaths.Add(new SubPath(CurrentSubPath.FirstPoint));
        }

        public void MoveTo(float x, float y)
            => MoveTo(new Vector2(x, y));
        
        public void MoveTo(PointF p)
            => MoveTo(new Vector2(p.X, p.Y));

        public void MoveTo(Vector2 p)
        {
            if (!p.IsNumeric())
            {
                return;
            }
            
            _subPaths.Add(new SubPath(p));
            _needsNewSubpath = false;
        }
        
        public void LineTo(float x, float y)
            => LineTo(new Vector2(x, y));

        public void LineTo(PointF p)
            => LineTo(new Vector2(p.X, p.Y));
        
        public void LineTo(Vector2 p)
        {
            if (!p.IsNumeric())
            {
                return;
            }

            if (_subPaths.Count == 0)
            {
                EnsureSubPathExists(p);
            }
            else
            {
                CurrentSubPath.AddCommand(new LineToCommand(p));
            }
        }

        public void QuadraticCurveTo(float cpx, float cpy, float x, float y)
            => QuadraticCurveTo(new Vector2(cpx, cpy), new Vector2(x, y));
        
        public void QuadraticCurveTo(PointF cp, PointF p)
            => QuadraticCurveTo(new Vector2(cp.X, cp.Y), new Vector2(p.X, p.Y));

        public void QuadraticCurveTo(Vector2 cp, Vector2 p)
        {
            if (!cp.IsNumeric() || !p.IsNumeric())
            {
                return;
            }
            
            EnsureSubPathExists(cp);
            
            CurrentSubPath.AddCommand(new QuadraticCurveToCommand(cp, p));
        }
        
        public void BezierCurveTo(float cp1x, float cp1y, float cp2x, float cp2y, float x, float y)
            => BezierCurveTo(new Vector2(cp1x, cp1y), new Vector2(cp2x, cp2y), new Vector2(x, y));
        
        public void BezierCurveTo(PointF cp1, PointF cp2, PointF p)
            => BezierCurveTo(new Vector2(cp1.X, cp1.Y), new Vector2(cp2.X, cp2.Y), new Vector2(p.X, p.Y));

        public void BezierCurveTo(Vector2 cp1, Vector2 cp2, Vector2 p)
        {
            if (!cp1.IsNumeric() || !cp2.IsNumeric() || !p.IsNumeric())
            {
                return;
            }
            
            EnsureSubPathExists(cp1);
            
            CurrentSubPath.AddCommand(new BezierCurveToCommand(cp1, cp2, p));
        }
        
    }
}