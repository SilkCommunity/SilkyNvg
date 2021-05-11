using SilkyNvg.Common;
using System.Collections.Generic;
using System.Numerics;

namespace SilkyNvg.Core.Paths
{
    internal class PathCache
    {

        private readonly List<Point> _points;
        private readonly List<Path> _paths;

        public PathCache()
        {
            _points = new();
            _paths = new();
        }

        public void Clear()
        {
            _points.Clear();
            _paths.Clear();
        }

        public Path LastPath
        {
            get
            {
                if (_paths.Count > 0)
                    return _paths[^1];
                return null;
            }
        }

        public void AddPath()
        {
            _paths.Add(new(_points.Count));
        }

        public Point LastPoint
        {
            get
            {
                if (_points.Count > 0)
                    return _points[^1];
                return null;
            }
        }

        public void AddPoint(Vector2 pos, PointFlags flags)
        {
            _points.Add(new Point(pos, flags));
            LastPath.PointCount++;
        }

        public void ClosePath()
        {
            LastPath.Closed = true;
        }

        public void PathWinding(Winding winding)
        {
            LastPath.Winding = winding;
        }

        private void CalculateJoins(float w, LineCap lineJoin, float miterLimit)
        {
            float iw = 1.0f;

            if (w > 0.0f)
            {
                iw = 1.0f / w;
            }

            foreach (Path path in _paths)
            {
                int pi = path.First;
                Point p0 = _points[pi + path.PointCount - 1];
                Point p1 = _points[pi];
                int nleft = 0;

                path.BevelCount = 0;

                for (int i = pi; i < pi + path.PointCount; i++)
                {
                    float dlx0 = p0.Determinant.Y;
                    float dly0 = -p0.Determinant.X;
                    float dlx1 = p1.Determinant.Y;
                    float dly1 = -p1.Determinant.X;

                    p1.MatrixDeterminant.X = (dlx0 + dlx1) * 0.5f;

                }
            }

        }

    }
}
