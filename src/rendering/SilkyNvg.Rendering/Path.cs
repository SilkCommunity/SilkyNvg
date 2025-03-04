using Silk.NET.Maths;
using SilkyNvg.Common;
using SilkyNvg.Graphics;
using SilkyNvg.Paths;
using System;
using System.Collections.Generic;

namespace SilkyNvg.Rendering
{
    public sealed class Path
    {

        private const uint INIT_POINTS_SIZE = 128;
        private const uint INIT_VERTS_SIZE = 256;

        private readonly IList<Point> _points = new List<Point>((int)INIT_POINTS_SIZE);

        private readonly IList<Vertex> _fill = new List<Vertex>((int)INIT_VERTS_SIZE);
        private readonly IList<Vertex> _stroke = new List<Vertex>((int)INIT_VERTS_SIZE);

        private readonly PixelRatio _pixelRatio;
        private uint _bevelCount;
        private Box2D<float> _bounds;

        public bool Closed { get; private set; }

        public uint BevelCount => _bevelCount;

        public IList<Vertex> Fill => _fill;

        public IList<Vertex> Stroke => _stroke;

        public bool Convex { get; private set; }

        public Winding Winding { get; internal set; }

        internal Box2D<float> Bounds => _bounds;

        internal Path(Winding winding, PixelRatio pixelRatio)
        {
            Winding = winding;
            _pixelRatio = pixelRatio;

            _bounds = new Box2D<float>(new Vector2D<float>(1e6f, 1e6f), new Vector2D<float>(-1e6f, -1e6f));
        }

        internal Vector2D<float> LastPoint
        {
            get
            {
                if (_points.Count > 0)
                {
                    return _points[^1].Position;
                }
                return default;
            }
        }

        internal uint PointCount => (uint)_points.Count;

        internal void AddPoint(Vector2D<float> position, PointFlags flags)
        {
            if (_points.Count > 0)
            {
                Point pt = _points[^1];
                if (Point.Equals(pt.Position, position, _pixelRatio.DistTol))
                {
                    pt.Flags |= flags;
                    return;
                }
            }

            Point point = new(position, flags);
            _points.Add(point);
        }

        internal void Close()
        {
            Closed = true;
        }

        private void PolyReverse()
        {
            int i = 0, j = _points.Count - 1;
            while (i < j)
            {
                Point tmp = _points[i];
                _points[i] = _points[j];
                _points[j] = tmp;
                i++;
                j--;
            }
        }

        internal void Flatten()
        {
            Point p0 = _points[^1];
            Point p1 = _points[0];
            if (Point.Equals(p0, p1, _pixelRatio.DistTol))
            {
                _points.RemoveAt(_points.Count - 1);
                p0 = _points[^1];
                Close();
            }

            if (_points.Count > 2)
            {
                float area = Point.PolyArea(_points);
                if (Winding == Winding.Ccw && area < 0.0f)
                {
                    PolyReverse();
                    p0 = _points[^1];
                }
                if (Winding == Winding.Cw && area > 0.0f)
                {
                    PolyReverse();
                    p0 = _points[^1];
                }
            }

            foreach (Point point in _points)
            {
                p1 = point;
                p0.SetDeterminant(p1);

                _bounds.Min.X = MathF.Min(_bounds.Min.X, p0.Position.X);
                _bounds.Min.Y = MathF.Min(_bounds.Min.Y, p0.Position.Y);
                _bounds.Max.X = MathF.Max(_bounds.Max.X, p0.Position.X);
                _bounds.Max.Y = MathF.Max(_bounds.Max.Y, p0.Position.Y);

                p0 = p1;
            }
        }

        private void ButtCapStart(Point p, Vector2D<float> delta, float w, float d, float aa, float u0, float u1)
        {
            Vector2D<float> pPos = p.Position - delta * d;
            Vector2D<float> dl = new(delta.Y, -delta.X);
            _stroke.Add(new Vertex(pPos + (dl * w) - (delta * aa), u0, 0.0f));
            _stroke.Add(new Vertex(pPos - (dl * w) - (delta * aa), u1, 0.0f));
            _stroke.Add(new Vertex(pPos + (dl * w), u0, 1.0f));
            _stroke.Add(new Vertex(pPos - (dl * w), u1, 1.0f));
        }

        private void ButtCapEnd(Point p, Vector2D<float> delta, float w, float d, float aa, float u0, float u1)
        {
            Vector2D<float> pPos = p.Position + delta * d;
            Vector2D<float> dl = new(delta.Y, -delta.X);
            _stroke.Add(new Vertex(pPos + (dl * w), u0, 1.0f));
            _stroke.Add(new Vertex(pPos - (dl * w), u1, 1.0f));
            _stroke.Add(new Vertex(pPos + (dl * w) + (delta * aa), u0, 0.0f));
            _stroke.Add(new Vertex(pPos - (dl * w) + (delta * aa), u1, 0.0f));
        }

        private void RoundCapStart(Point p, Vector2D<float> delta, float w, uint ncap, float u0, float u1)
        {
            Vector2D<float> pPos = p.Position;
            Vector2D<float> dl = new(delta.Y, -delta.X);
            for (int i = 0; i < ncap; i++)
            {
                float a = i / (float)(ncap - 1) * MathF.PI;
                float ax = MathF.Cos(a) * w;
                float ay = MathF.Sin(a) * w;
                _stroke.Add(new Vertex(pPos - (dl * ax) - (delta * ay), u0, 1.0f));
                _stroke.Add(new Vertex(pPos, 0.5f, 1.0f));
            }
            _stroke.Add(new Vertex(pPos + (dl * w), u0, 1.0f));
            _stroke.Add(new Vertex(pPos - (dl * w), u1, 1.0f));
        }

        private void RoundCapEnd(Point p, Vector2D<float> delta, float w, uint ncap, float u0, float u1)
        {
            Vector2D<float> pPos = p.Position;
            Vector2D<float> dl = new(delta.Y, -delta.X);
            _stroke.Add(new Vertex(pPos + (dl * w), u0, 1.0f));
            _stroke.Add(new Vertex(pPos - (dl * w), u1, 1.0f));
            for (int i = 0; i < ncap; i++)
            {
                float a = i / (float)(ncap - 1) * MathF.PI;
                float ax = MathF.Cos(a) * w;
                float ay = MathF.Sin(a) * w;
                _stroke.Add(new Vertex(pPos, 0.5f, 1.0f));
                _stroke.Add(new Vertex(pPos - (dl * ax) + (delta * ay), u0, 1.0f));
            }
        }

        private void BevelJoin(Point p0, Point p1, float lw, float rw, float lu, float ru)
        {
            Vector2D<float> dl0 = new(p0.Determinant.Y, -p0.Determinant.X);
            Vector2D<float> dl1 = new(p1.Determinant.Y, -p1.Determinant.X);

            p1.JoinBevel(lw, rw, lu, ru, dl0, dl1, p0, _stroke);
        }

        internal void CalculateJoins(float iw, LineCap lineJoin, float miterLimit)
        {
            Point p0 = _points[^1];
            Point p1 = _points[0];
            uint nleft = 0;

            _bevelCount = 0;

            foreach (Point point in _points)
            {
                p1 = point;
                bool bevelOrRound = (lineJoin == LineCap.Bevel) || (lineJoin == LineCap.Round);
                p1.Join(p0, iw, bevelOrRound, miterLimit, ref nleft, ref _bevelCount);

                p0 = p1;
            }

            Convex = nleft == _points.Count;
        }

        internal void ExpandStroke(float aa, float u0, float u1, float w, LineCap lineCap, LineCap lineJoin, uint ncap)
        {
            _fill.Clear();

            bool loop = Closed;
            _stroke.Clear();

            Point p0, p1;
            int s, e;
            if (loop)
            {
                p0 = _points[^1];
                p1 = _points[0];
                s = 0;
                e = _points.Count;
            }
            else
            {
                p0 = _points[0];
                p1 = _points[1];
                s = 1;
                e = _points.Count - 1;
            }

            if (!loop)
            {
                Vector2D<float> d = p1.Position - p0.Position;
                d = Vector2D.Normalize(d);
                if (lineCap is LineCap.Butt)
                {
                    ButtCapStart(p0, d, w, -aa * 0.5f, aa, u0, u1);
                }
                else if (lineCap is LineCap.Butt or LineCap.Square)
                {
                    ButtCapStart(p0, d, w, w - aa, aa, u0, u1);
                }
                else if (lineCap is LineCap.Round)
                {
                    RoundCapStart(p0, d, w, ncap, u0, u1);
                }
            }

            for (int i = s; i < e; i++)
            {
                p1 = _points[i];

                if (p1.Flags.HasFlag(PointFlags.Bevel) || p1.Flags.HasFlag(PointFlags.Innerbevel))
                {
                    if (lineJoin == LineCap.Round)
                    {
                        p1.RoundJoin(w, w, u0, u1, ncap, p0, _stroke);
                    }
                    else
                    {
                        p1.BevelJoin(w, w, u0, u1, p0, _stroke);
                    }
                }
                else
                {
                    _stroke.Add(new Vertex(p1.Position + (p1.MatrixDeterminant * w), u0, 1.0f));
                    _stroke.Add(new Vertex(p1.Position - (p1.MatrixDeterminant * w), u1, 1.0f));
                }

                p0 = p1;
            }
            if (s > 0)
            {
                p1 = _points[e];
            }

            if (loop)
            {
                _stroke.Add(new Vertex(_stroke[0].Pos, u0, 1.0f));
                _stroke.Add(new Vertex(_stroke[1].Pos, u1, 1.0f));
            }
            else
            {
                Vector2D<float> d = p1.Position - p0.Position;
                d = Vector2D.Normalize(d);
                if (lineCap is LineCap.Butt)
                {
                    ButtCapEnd(p1, d, w, -aa * 0.5f, aa, u0, u1);
                }
                else if (lineCap is LineCap.Butt or LineCap.Square)
                {
                    ButtCapEnd(p1, d, w, w - aa, aa, u0, u1);
                }
                else if (lineCap is LineCap.Round)
                {
                    RoundCapEnd(p1, d, w, ncap, u0, u1);
                }
            }
        }

        private void ExpandFillFill(float woff, bool fringe)
        {
            if (fringe)
            {
                Point p0 = _points[^1];
                Point p1 = _points[0];

                foreach (Point point in _points)
                {
                    p1 = point;
                    Point.Vertex(p0, p1, woff, _fill);

                    p0 = p1;
                }
            }
            else
            {
                foreach (Point point in _points)
                {
                    _fill.Add(new Vertex(point.Position, 0.5f, 1.0f));
                }
            }
        }

        private void ExpandFillStroke(float woff, bool fringe, bool convex, float w)
        {
            if (fringe)
            {
                float lw = w + woff;
                float rw = w - woff;
                float lu = 0, ru = 1;

                if (convex)
                {
                    lw = woff;
                    lu = 0.5f;
                }

                Point p0 = _points[^1];
                Point p1 = _points[0];

                foreach (Point point in _points)
                {
                    p1 = point;
                    if ((p1.Flags & (PointFlags.Bevel | PointFlags.Innerbevel)) != 0)
                    {
                        BevelJoin(p0, p1, lw, rw, lu, ru);
                    }
                    else
                    {
                        _stroke.Add(new Vertex(p1.Position + (p1.MatrixDeterminant * lw), lu, 1.0f));
                        _stroke.Add(new Vertex(p1.Position - (p1.MatrixDeterminant * rw), ru, 1.0f));
                    }

                    p0 = p1;
                }

                _stroke.Add(new Vertex(_stroke[0].Pos, lu, 1.0f));
                _stroke.Add(new Vertex(_stroke[1].Pos, ru, 1.0f));
            }
            else
            {
                _stroke.Clear();
            }
        }

        internal void ExpandFill(float aa, bool fringe, bool convex, float w)
        {
            float woff = 0.5f * aa;
            ExpandFillFill(woff, fringe);
            ExpandFillStroke(woff, fringe, convex, w);
        }

    }
}
