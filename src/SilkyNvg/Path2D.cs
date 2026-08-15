using System.Collections.Generic;
using System.Numerics;
using SilkyNvg.Paths;
using SilkyNvg.Rendering;
using SilkyNvg.Utils;

namespace SilkyNvg
{
    public class Path2D
    {
        
        private readonly List<SubPath> _subPaths = new List<SubPath>();
        
        private readonly DataAccessibleArrayList<float> _vertices = new DataAccessibleArrayList<float>();
        
        private bool _needNewSubPath;

        private bool HasSubPaths => _subPaths.Count > 0;
        
        private SubPath CurrentSubPath => _subPaths[_subPaths.Count - 1];
        
        public Path2D()
        {
            _needNewSubPath = true;
        }

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

        internal void ClearSubPaths()
        {
            _subPaths.Clear();
        }

        internal void RenderPath(ISilkyRenderer renderer)
        {
            _vertices.Clear();

            uint vertexCount = 0;
            foreach (var subPath in _subPaths)
            {
                vertexCount += subPath.BuildGeometry(_vertices, Matrix3x2.Identity);
            }
            
            renderer.FillPath(_vertices.Data, vertexCount);
        }

        private void CreateNewSubpath(Vector2 start)
        {
            _subPaths.Add(new SubPath(start));
            _needNewSubPath = false;
        }

        private void EnsureSubPathExists(Vector2 p)
        {
            if (_needNewSubPath)
            {
                CreateNewSubpath(p);
            }
        }

        #region Path

        public void ClosePath()
        {
            if (!HasSubPaths)
            {
                return;
            }
            
            CurrentSubPath.Close();
            CreateNewSubpath(CurrentSubPath.Start);
        }

        public void MoveTo(Vector2 p)
        {
            if (p.IsInfinityOrNan())
            {
                return;
            }
            CreateNewSubpath(p);
        }

        public void MoveTo(float x, float y)
            => MoveTo(new Vector2(x, y));

        public void LineTo(Vector2 p)
        {
            if (p.IsInfinityOrNan())
            {
                return;
            }

            if (!HasSubPaths)
            {
                EnsureSubPathExists(p);
            }
            else
            {
                CurrentSubPath.AddLine(p);
            }
        }
        
        public void LineTo(float x, float y)
            => LineTo(new Vector2(x, y));

        public void QuadraticCurveTo(Vector2 cp, Vector2 p)
        {
            if (cp.IsInfinityOrNan() || p.IsInfinityOrNan())
            {
                return;
            }
            
            EnsureSubPathExists(cp);
            CurrentSubPath.AddQuadratic(cp, p);
        }

        public void QuadraticCurveTo(float cpx, float cpy, float x, float y)
            => QuadraticCurveTo(new Vector2(cpx, cpy), new Vector2(x, y));

        public void BezierCurveTo(Vector2 cp1, Vector2 cp2, Vector2 p)
        {
            if (cp1.IsInfinityOrNan() || cp2.IsInfinityOrNan() || p.IsInfinityOrNan())
            {
                return;
            }
            
            EnsureSubPathExists(cp1);
            CurrentSubPath.AddCubic(cp1, cp2, p);
        }
        
        public void BezierCurveTo(float cp1x, float cp1y, float cp2x, float cp2y, float x, float y)
            => BezierCurveTo(new Vector2(cp1x, cp1y), new Vector2(cp2x, cp2y), new Vector2(x, y));
        
        #endregion
        
    }
}