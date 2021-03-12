using Silk.NET.Maths;
using SilkyNvg.Common;
using SilkyNvg.Paths;
using System;
using System.Collections.Generic;

namespace SilkyNvg.Core.Paths
{
    public class Path
    {

        private readonly Winding _winding;

        private readonly IList<Vertex> _fill = new List<Vertex>();
        private readonly IList<Vertex> _stroke = new List<Vertex>();
        private readonly IList<Point> _points = new List<Point>();

        private int _bevelCount;
        private bool _convex;
        private bool _closed;

        public List<Vertex> Fill => (List<Vertex>)_fill;
        public List<Vertex> Stroke => (List<Vertex>)_stroke;
        public List<Point> Points => (List<Point>)_points;

        public Winding Winding => _winding;

        public int BevelCount
        {
            get => _bevelCount;
            set => _bevelCount = value;
        }

        public bool Convex
        {
            get => _convex;
            set => _convex = value;
        }

        public bool Closed => _closed;

        private float PolyArea()
        {
            float area = 0;
            for (int i = 2; i < _points.Count; i++)
            {
                var a = _points[0];
                var b = _points[i - 1];
                var c = _points[i];
                area += Maths.Triarea2(a.Position, b.Position, c.Position);
            }
            return area * 0.5f;
        }

        private void PolyReverse()
        {
            int i = 0, j = _points.Count - 1;
            while (i < j)
            {
                var tmp = _points[i];
                _points[i] = _points[j];
                _points[j] = tmp;
                i++;
                j--;
            }
        }

        internal void CalculateJoins(float iw, float miterLimit, LineCap lineJoin)
        {
            var p0 = _points[^1];
            var p1 = _points[0];
            int nleft = 0;

            _bevelCount = 0;

            for (int i = 0; i < _points.Count; i++)
            {
                p1 = _points[i];

                float dlx0 = p0.Dy;
                float dly0 = -p0.Dx;
                float dlx1 = p1.Dy;
                float dly1 = -p1.Dx;

                p1.DMx = (dlx0 + dlx1) * 0.5f;
                p1.DMy = (dly0 + dly1) * 0.5f;

                float dmr2 = p1.DMx * p1.DMx + p1.DMy * p1.DMy;
                if (dmr2 > 0.000001f)
                {
                    float scale = 1.0f / dmr2;
                    if (scale > 600.0f)
                    {
                        scale = 600.0f;
                    }
                    p1.DMx *= scale;
                    p1.DMy *= scale;
                }

                p1.Flags = p1.IsCorner() ? (uint)PointFlags.PointCorner : 0;

                float cross = p1.Dx * p0.Dy - p0.Dx * p1.Dy;
                if (cross > 0.0f)
                {
                    nleft++;
                    p1.Flags |= (uint)PointFlags.PointLeft;
                }

                float limit = MathF.Max(1.01f, MathF.Min(p0.Length, p1.Length) * iw);
                if ((dmr2 * limit * limit) < 1.0f)
                    p1.Flags |= (uint)PointFlags.PointInnerbevel;

                if (p1.IsCorner())
                {
                    if ((dmr2 * miterLimit * miterLimit) < 1.0f ||
                        lineJoin == LineCap.Bevel || lineJoin == LineCap.Round)
                    {
                        p1.Flags |= (uint)PointFlags.PointInnerbevel;
                    }
                }

                if (p1.IsBevel() || p1.IsInnerbevel())
                {
                    _bevelCount++;
                }

                p0 = p1;
            }

            _convex = nleft == _points.Count;
        }

        internal void Flatten(PathCache cache, Style style)
        {
            var p0 = _points[^1];
            var p1 = _points[0];
            if (Point.PtEquals(p0, p1, style.DistributionTollerance))
            {
                _points.RemoveAt(_points.Count - 1);
                p0 = _points[^1];
                Close();
            }

            if (_points.Count > 2)
            {
                float area = PolyArea();
                if (_winding == Winding.CCW && area < 0.0f)
                {
                    PolyReverse();
                }
                if (_winding == Winding.CW && area > 0.0f)
                {
                    PolyReverse();
                }
            }

            for (int i = 0; i < _points.Count; i++)
            {
                p1 = _points[i];

                p0.Dx = p1.X - p0.X;
                p0.Dy = p1.Y - p0.Y;
                p0.Length = Maths.Normalize(p0.D);
                p0.D = Vector2D.Normalize(p0.D);

                var bounds = cache.Bounds;
                bounds.Origin.X = MathF.Min(bounds.Origin.X, p0.X);
                bounds.Origin.Y = MathF.Min(bounds.Origin.Y, p0.Y);
                bounds.Size.X = MathF.Max(bounds.Size.X, 0);
                bounds.Size.Y = MathF.Max(bounds.Size.Y, 0);
                cache.Bounds = bounds;

                p0 = p1;
            }

        }

        public Path(Winding winding)
        {
            _winding = winding;
        }

        public void Close()
        {
            _closed = true;
        }

    }
}