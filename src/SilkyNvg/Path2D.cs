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
        
        private readonly List<SubPath> _subPaths = [];
        
        private bool _needNewSubPath = true;
        
        private bool HasSubPaths => _subPaths.Count > 0;
        
        private SubPath CurrentSubPath => _subPaths[^1];
        
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

        internal void FillPath()
        {
            if (!HasSubPaths)
            {
                return;
            }

            Vector2 start = _subPaths[0].Start;
            foreach (var subPath in _subPaths)
            {
                subPath.BuildFillGeometry();
            }
        }

        private void CreateNewSubpath(float startX, float startY)
        {
            _subPaths.Add(new SubPath(startX, startY));
            _needNewSubPath = false;
        }

        private void EnsureSubPathExists(float x, float y)
        {
            if (_needNewSubPath)
            {
                CreateNewSubpath(x, y);
            }
        }

        #region Path

        public void MoveTo(float x, float y)
        {
            if (x.IsInfinityOrNan() || y.IsInfinityOrNan()) return;
            CreateNewSubpath(x, y);
        }

        public void ClosePath()
        {
            if (!HasSubPaths) return;

            CurrentSubPath.Close();
            CreateNewSubpath(CurrentSubPath.Start.X, CurrentSubPath.Start.Y);
        }

        public void LineTo(float x, float y)
        {
            if (x.IsInfinityOrNan() || y.IsInfinityOrNan()) return;
            if (!HasSubPaths)
            {
                EnsureSubPathExists(x, y);
            }
            else
            {
                CurrentSubPath.AddLineTo(x, y);
            }
        }

        #endregion

    }
}