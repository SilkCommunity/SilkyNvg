using Silk.NET.Maths;
using SilkyNvg.Common;
using SilkyNvg.Core.Instructions;
using SilkyNvg.Paths;
using System;
using System.Collections.Generic;

namespace SilkyNvg.Core.Paths
{
    internal class PathCache
    {

        private readonly List<Path> _paths;
        private readonly List<Vertex> _vertices;

        private Vector4D<float> _bounds;
        private int _pointCount;

        public Vector4D<float> Bounds
        {
            get => _bounds;
            set => _bounds = value;
        }

        public List<Path> Paths => _paths;

        public PathCache()
        {
            _paths = new List<Path>();
            _vertices = new List<Vertex>();
            _bounds = new Vector4D<float>();
        }

        public void Clear()
        {
            _pointCount = 0;
            _paths.Clear();
        }

        public Path LastPath()
        {
            if (_paths.Count > 0)
                return _paths[_paths.Count - 1];
            return null;
        }

        public void AddPath()
        {
            var path = new Path(Winding.CCW);
            _paths.Add(path);
        }

        public Point LastPoint()
        {
            if (_paths.Count > 0)
                return LastPath().Points[^1];
            return null;
        }

        public void AddPoint(float x, float y, uint flags, Style style)
        {
            var path = LastPath();

            if (path == null)
                return;

            if (path.Points.Count > 0 && _pointCount > 0)
            {
                var pt = LastPoint();
                if (pt.Equals(x, y, style.DistributionTollerance))
                {
                    pt.Flags |= flags;
                    return;
                }
            }

            _pointCount++;
            path.Points.Add(new Point(x, y, flags));
        }

        public void ClosePath()
        {
            var path = LastPath();
            if (path == null)
                return;
            path.Close();
        }

        public void FlattenPaths(InstructionQueue iq, Style style)
        {
            if (_paths.Count > 0)
                return;

            while (iq.QueueLength > 0)
            {
                iq.Next().Execute(this, style);
            }

            _bounds.X = _bounds.Y = 1e6f;
            _bounds.Z = _bounds.W = -1e6f;

            for (int i = 0; i <_paths.Count; i++)
            {
                var path = _paths[i];
                path.Flatten(this, style);
            }
        }

        private void ChooseBevel(bool bevel, Point p0, Point p1, float w, out float x0, out float y0, out float x1, out float y1)
        {
            if (bevel)
            {
                x0 = p1.X + p0.Dy * w;
                y0 = p1.Y - p0.Dx * w;
                x1 = p1.X + p1.Dy * w;
                y1 = p1.Y - p1.Dx * w;
            }
            else
            {
                x0 = p1.X + p1.DMx * w;
                y0 = p1.Y + p1.DMy * w;
                x1 = p1.X + p1.DMx * w;
                y1 = p1.Y + p1.DMy * w;
            }
        }

        private List<Vertex> RoundJoin(List<Vertex> verts, Point p0, Point p1, float lw, float rw, float lu, float ru, float ncap)
        {
            float dlx0 = p0.Dy;
            float dly0 = -p0.Dx;
            float dlx1 = p1.Dy;
            float dly1 = -p1.Dx;

            if (p1.IsLeft())
            {
                ChooseBevel(p1.IsInnerbevel(), p0, p1, lw, out float lx0, out float ly0, out float lx1, out float ly1);
                float a0 = MathF.Atan2(-dly0, -dlx0);
                float a1 = MathF.Atan2(-dly1, -dlx1);
                if (a1 > a0)
                    a1 -= Maths.Pi * 2;

                verts.Add(new Vertex(lx0, ly0, lu, 1));
                verts.Add(new Vertex(p1.X - dlx0 * rw, p1.Y - dly0 * rw, ru, 1));

                int n = Maths.Clamp((int)MathF.Ceiling((a0 - a1) / Maths.Pi * ncap), 2, (int)ncap);
                for (int i = 0; i < n; i++)
                {
                    float u = i / (float)(n - 1);
                    float a = a0 + u * (a1 - a0);
                    float rx = p1.X + MathF.Cos(a) * rw;
                    float ry = p1.Y + MathF.Sin(a) * rw;
                    verts.Add(new Vertex(p1.X, p1.Y, 0.5f, 1));
                    verts.Add(new Vertex(rx, ry, ru, 1));
                }

                verts.Add(new Vertex(lx1, ly1, lu, 1));
                verts.Add(new Vertex(p1.X - dlx1 * rw, p1.Y - dly1 * rw, ru, 1));
            }
            else
            {
                ChooseBevel(p1.IsInnerbevel(), p0, p1, -rw, out float rx0, out float ry0, out float rx1, out float ry1);
                float a0 = MathF.Atan2(dly0, dlx0);
                float a1 = MathF.Atan2(dly1, dlx1);
                if (a1 < a0)
                    a1 += Maths.Pi * 2;

                verts.Add(new Vertex(p1.X + dlx0 * rw, p1.Y + dly0 * rw, lu, 1));
                verts.Add(new Vertex(rx0, ry0, lu, 1));

                int n = Maths.Clamp((int)MathF.Ceiling((a1 - a0) / Maths.Pi * ncap), 2, (int)ncap);
                for (int i = 0; i < n; i++)
                {
                    float u = i / (float)(n - 1);
                    float a = a0 + u * (a1 - a0);
                    float lx = p1.X + MathF.Cos(a) * lw;
                    float ly = p1.Y + MathF.Sin(a) * lw;
                    verts.Add(new Vertex(lx, ly, lu, 1));
                    verts.Add(new Vertex(p1.X, p1.Y, 0.5f, 1));
                }

                verts.Add(new Vertex(p1.X + dlx1 * rw, p1.Y + dly1 * rw, lu, 1));
                verts.Add(new Vertex(rx1, ry1, ru, 1));
            }

            return verts;
        }

        private List<Vertex> BevelJoin(List<Vertex> verts, Point p0, Point p1, float lw, float rw, float lu, float ru, float fringe)
        {
            float dlx0 = p0.Dy;
            float dly0 = -p0.Dx;
            float dlx1 = p1.Dy;
            float dly1 = -p1.Dx;

            if (p1.IsLeft())
            {
                ChooseBevel(p1.IsInnerbevel(), p0, p1, lw, out float lx0, out float ly0, out float lx1, out float ly1);

                verts.Add(new Vertex(lx0, ly0, lu, 1));
                verts.Add(new Vertex(p1.X - dlx0 * rw, p1.Y - dly0 * rw, ru, 1));

                if (p1.IsBevel())
                {
                    verts.Add(new Vertex(lx0, ly0, lu, 1));
                    verts.Add(new Vertex(p1.X - dlx0 * rw, p1.Y - dly0 * rw, ru, 1));

                    verts.Add(new Vertex(lx1, ly1, lu, 1));
                    verts.Add(new Vertex(p1.X - dlx1 * rw, p1.Y - dly1 * rw, ru, 1));
                }
                else
                {
                    float rx0 = p1.X - p1.DMx * rw;
                    float ry0 = p1.Y - p1.DMy * rw;

                    verts.Add(new Vertex(p1.X, p1.Y, 0.5f, 1));
                    verts.Add(new Vertex(p1.X - dlx0 * rw, p1.Y - dly0 * rw, ru, 1));

                    verts.Add(new Vertex(rx0, ry0, ru, 1));
                    verts.Add(new Vertex(rx0, ry0, ru, 1));

                    verts.Add(new Vertex(p1.X, p1.Y, 0.5f, 1));
                    verts.Add(new Vertex(p1.X - dlx1 * rw, p1.Y - dly1 * rw, ru, 1));
                }

                verts.Add(new Vertex(lx1, ly1, lu, 1));
                verts.Add(new Vertex(p1.X - dlx1 * rw, p1.Y - dly1 * rw, ru, 1));

            }
            else
            {
                ChooseBevel(p1.IsInnerbevel(), p0, p1, -rw, out float rx0, out float ry0, out float rx1, out float ry1);

                verts.Add(new Vertex(p1.X + dlx0 * lw, p1.Y + dly0 * lw, lu, 1));
                verts.Add(new Vertex(rx0, ry0, ru, 1));

                if (p1.IsBevel())
                {
                    verts.Add(new Vertex(p1.X + dlx0 * lw, p1.Y + dly0 * lw, lu, 1));
                    verts.Add(new Vertex(rx0, ry0, ru, 1));

                    verts.Add(new Vertex(p1.X + dlx1 * lw, p1.Y + dly1 * lw, lu, 1));
                    verts.Add(new Vertex(rx1, ry1, ru, 1));
                }
                else
                {
                    float lx0 = p1.X + p1.DMx * lw;
                    float ly0 = p1.Y + p1.DMy * lw;

                    verts.Add(new Vertex(p1.X + dlx0 * lw, p1.Y + dly0 * lw, lu, 1));
                    verts.Add(new Vertex(p1.X, p1.Y, 0.5f, 1));

                    verts.Add(new Vertex(lx0, ly0, lu, 1));
                    verts.Add(new Vertex(lx0, ly0, lu, 1));

                    verts.Add(new Vertex(p1.X + dlx1 * lw, p1.Y + dly1 * lw, lu, 1));
                    verts.Add(new Vertex(p1.X, p1.Y, 0.5f, 1));
                }

                verts.Add(new Vertex(p1.X + dlx1 * lw, p1.Y + dly1 * lw, lu, 1));
                verts.Add(new Vertex(rx1, ry1, ru, 1));
            }

            return verts;
        }

        private void CalculateJoins(float w, LineCap lineJoin, float miterLimit)
        {
            float iw = 0.0f;
            if (w > 0.0f)
                iw = 1.0f / w;

            for (int i = 0; i < _paths.Count; i++)
            {
                var path = _paths[i];
                path.CalculateJoins(iw, miterLimit, lineJoin);
            }
        }

        private List<Vertex> ButtCapStart(List<Vertex> dst, Point p, Vector2D<float> delta, float w, float d, float aa, float u0, float u1)
        {
            float px = p.X - delta.X * d;
            float py = p.Y - delta.Y * d;
            float dlx = delta.Y;
            float dly = -delta.X;
            dst.Add(new Vertex(px + dlx * w - delta.X * aa, py + dly * w - delta.Y * aa, u0, 0));
            dst.Add(new Vertex(px - dlx * w - delta.X * aa, py - dly * w - delta.Y * aa, u1, 0));
            dst.Add(new Vertex(px + dlx * w, py + dly * w, u0, 1));
            dst.Add(new Vertex(px - dlx * w, py - dly * w, u1, 1));
            return dst;
        }

        private List<Vertex> ButtCapEnd(List<Vertex> dst, Point p, Vector2D<float> delta, float w, float d, float aa, float u0, float u1)
        {
            float px = p.X + delta.X * d;
            float py = p.Y + delta.Y * d;
            float dlx = delta.Y;
            float dly = -delta.X;
            dst.Add(new Vertex(px + dlx * w, py + dly * w, u0, 1));
            dst.Add(new Vertex(px - dlx * w, py - dly * w, u1, 1));
            dst.Add(new Vertex(px + dlx * w + delta.X * aa, py + dly * w + delta.Y * aa, u0, 0));
            dst.Add(new Vertex(px - dlx * w + delta.X * aa, py - dly * w + delta.Y * aa, u1, 0));
            return dst;
        }

        private List<Vertex> RoundedCapStart(List<Vertex> dst, Point p, Vector2D<float> delta, float w, int ncap, float u0, float u1)
        {
            float px = p.X;
            float py = p.Y;
            float dlx = delta.Y;
            float dly = -delta.X;

            for (int i = 0; i < ncap; i++)
            {
                float a = i / (float)(ncap - 1) * Maths.Pi;
                float ax = MathF.Cos(a) * w;
                float ay = MathF.Sin(a) * w;
                dst.Add(new Vertex(px - dlx * ax - delta.X * ay, py - dly * ax - delta.Y * ay, u0, 1));
                dst.Add(new Vertex(px, py, 0.5f, 1));
            }

            dst.Add(new Vertex(px + dlx * w, py + dly * w, u0, 1));
            dst.Add(new Vertex(px - dlx * w, py - dly * w, u1, 1));
            return dst;
        }

        private List<Vertex> RoundedCapEnd(List<Vertex> dst, Point p, Vector2D<float> delta, float w, int ncap, float u0, float u1)
        {
            float px = p.X;
            float py = p.Y;
            float dlx = delta.Y;
            float dly = -delta.X;

            dst.Add(new Vertex(px + dlx * w, py + dly * w, u0, 1));
            dst.Add(new Vertex(px - dlx * w, py - dly * w, u1, 1));

            for (int i = 0; i < ncap; i++)
            {
                float a = i / (float)(ncap - 1) * Maths.Pi;
                float ax = MathF.Cos(a) * w;
                float ay = MathF.Sin(a) * w;
                dst.Add(new Vertex(px, py, 0.5f, 1));
                dst.Add(new Vertex(px - dlx * ax + delta.X * ay, py - dly * ax + delta.Y * ay, u0, 1));
            }
            return dst;
        }

        public void ExpandStroke(float w, float fringe, LineCap lineCap, LineCap lineJoin, float miterLimit, Style style)
        {
            float aa = fringe;
            float u0 = 0.0f;
            float u1 = 1.0f;
            int capCount = Maths.CurveDivs(w, Maths.Pi, style.TesselationTollerance);

            w += aa * 0.5f;

            if (aa == 0.0f)
            {
                u0 = 0.5f;
                u1 = 0.5f;
            }

            CalculateJoins(w, lineJoin, miterLimit);

            Point p0 = default, p1 = default;
            int s, e;

            for (int i = 0; i < _paths.Count; i++)
            {
                var path = _paths[i];
                var points = path.Points;

                path.Fill.Clear();

                bool loop = path.Closed;
                var dst = new List<Vertex>();

                if (loop)
                {
                    if (points.Count > 0)
                    {
                        p0 = points[^1];
                        p1 = points[0];
                    }
                    s = 0;
                    e = points.Count;
                }
                else
                {
                    p0 = points[0];
                    p1 = points[1];
                    s = 1;
                    e = points.Count - 1;
                }

                if (!loop)
                {
                    var d = p1.Position - p0.Position;
                    d = Vector2D.Normalize(d);
                    if (lineCap == LineCap.Butt)
                    {
                        dst = ButtCapStart(dst, p0, d, w, -aa * 0.5f, aa, u0, u1);
                    } else if (lineCap == LineCap.Butt || lineCap == LineCap.Square)
                    {
                        dst = ButtCapStart(dst, p0, d, w, w - aa, aa, u0, u1);
                    } else if (lineCap == LineCap.Round)
                    {
                        dst = RoundedCapStart(dst, p0, d, w, capCount, u0, u1);
                    }
                }

                for (int j = s; j < e; j++)
                {
                    p1 = points[j];

                    if (p1.IsBevel() || p1.IsInnerbevel())
                    {
                        if (lineJoin == LineCap.Round)
                        {
                            dst = RoundJoin(dst, p0, p1, w, w, u0, u1, capCount);
                        }
                        else
                        {
                            dst = BevelJoin(dst, p0, p1, w, w, u0, u1, aa);
                        }
                    }
                    else
                    {
                        dst.Add(new Vertex(p1.X + (p1.DMx * w), p1.Y + (p1.DMy * w), u0, 1));
                        dst.Add(new Vertex(p1.X - (p1.DMx * w), p1.Y - (p1.DMy * w), u1, 1));
                    }

                    p0 = p1;
                    if (j < points.Count - 1)
                        p1 = points[j + 1];
                }

                if (loop)
                {
                    if (dst.Count > 0)
                    {
                        dst.Add(new Vertex(dst[0].X, dst[0].Y, u0, 1));
                        dst.Add(new Vertex(dst[1].X, dst[1].Y, u1, 1));
                    }
                    else
                    {
                        dst.Add(new Vertex(-1, -1, u0, 1));
                        dst.Add(new Vertex(-1, -1, u1, 1));
                    }
                }
                else
                {
                    var d = p1.Position - p0.Position;
                    d = Vector2D.Normalize(d);
                    if (lineCap == LineCap.Butt)
                    {
                        dst = ButtCapEnd(dst, p1, d, w, -aa * 0.5f, aa, u0, u1);
                    } else if (lineCap == LineCap.Butt || lineCap == LineCap.Square)
                    {
                        dst = ButtCapEnd(dst, p1, d, w, w - aa, aa, u0, u1);
                    }
                    else if (lineCap == LineCap.Round)
                    {
                        dst = RoundedCapEnd(dst, p1, d, w, capCount, u0, u1);
                    }
                }

                _vertices.AddRange(dst);
                path.Stroke.AddRange(dst);

            }
        }

        public void ExpandFill(float w, LineCap lineJoin, float miterLimit, Style style)
        {
            float aa = style.FringeWidth;
            bool fringe = w > 0.0f;

            CalculateJoins(w, lineJoin, miterLimit);

            bool convex = _paths.Count == 1 && _paths[0].Convex;

            for (int i = 0; i < _paths.Count; i++)
            {
                var path = _paths[i];
                var points = path.Points;

                float woff = 0.5f * aa;
                var dst = new List<Vertex>();

                if (fringe)
                {
                    var p0 = points[^1];
                    var p1 = points[0];

                    for (int j = 0; j < points.Count; j++)
                    {
                        p1 = points[j];

                        if (p1.IsBevel())
                        {
                            float dlx0 = p0.Dy;
                            float dly0 = -p0.Dx;
                            float dlx1 = p1.Dy;
                            float dly1 = -p1.Dx;
                            if (p1.IsLeft())
                            {
                                float lx = p1.X + p1.DMx * woff;
                                float ly = p1.Y + p1.DMy * woff;
                                dst.Add(new Vertex(lx, ly, 0.5f, 1));
                            }
                            else
                            {
                                float lx0 = p1.X + dlx0 * woff;
                                float ly0 = p1.Y + dly0 * woff;
                                float lx1 = p1.X + dlx1 * woff;
                                float ly1 = p1.Y + dly1 * woff;
                                dst.Add(new Vertex(lx0, ly0, 0.5f, 1));
                                dst.Add(new Vertex(lx1, ly1, 0.5f, 1));
                            }
                        }
                        else
                        {
                            dst.Add(new Vertex(p1.X + (p1.DMx * woff), p1.Y + (p1.DMy * woff), 0.5f, 1));
                        }

                        p0 = p1;
                    }
                }
                else
                {
                    for (int j = 0; j < points.Count; j++)
                    {
                        var pt = points[j];
                        dst.Add(new Vertex(pt.X, pt.Y, 0.5f, 1));
                    }
                }

                path.Fill.AddRange(dst);
                _vertices.AddRange(dst);

                if (fringe)
                {
                    float lw = w + woff;
                    float rw = w - woff;
                    float lu = 0;
                    float ru = 1;
                    dst = new List<Vertex>();

                    if (convex)
                    {
                        lw = woff;
                        lu = 0.5f;
                    }

                    var p0 = points[^1];
                    var p1 = points[0];

                    for (int j = 0; j < points.Count; j++)
                    {
                        p1 = points[j];

                        if (p1.IsBevel() || p1.IsInnerbevel())
                        {
                            dst = BevelJoin(dst, p0, p1, lw, rw, lu, ru, style.FringeWidth);
                        }
                        else
                        {
                            dst.Add(new Vertex(p1.X + (p1.DMx * lw), p1.Y + (p1.DMy * lw), lu, 1));
                            dst.Add(new Vertex(p1.X - (p1.DMx * rw), p1.Y - (p1.DMy * rw), ru, 1));
                        }
                        p0 = p1;
                    }

                    dst.Add(new Vertex(dst[0].X, dst[0].Y, lu, 1));
                    dst.Add(new Vertex(dst[1].X, dst[1].Y, ru, 1));

                    path.Stroke.AddRange(dst);
                    _vertices.AddRange(dst);
                }
                else
                {
                    path.Stroke.Clear();
                }
            }

        }

        public void ClearVerts()
        {
            _vertices.Clear();
        }

    }
}