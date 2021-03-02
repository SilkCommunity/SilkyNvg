using Silk.NET.Maths;
using SilkyNvg.Core.Geometry;
using SilkyNvg.Core.Instructions;
using System;
using System.Collections.Generic;

namespace SilkyNvg.Core.Paths
{
    public class PathCache
    {

        private IList<Point> _points;

        private IList<Path> _paths;

        private IList<Vertex> _vertices;

        private Rectangle<float> _bounds;

        public Rectangle<float> Bounds => _bounds;

        public List<Path> Paths => (List<Path>)_paths;

        public PathCache()
        {
            _points = new List<Point>();
            _paths = new List<Path>();
            _vertices = new List<Vertex>();
        }

        public void Clear()
        {
            _points.Clear();
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
            var path = new Path(_points.Count, API.Winding.CCW);
            _paths.Add(path);
        }

        public Point LastPoint()
        {
            if (_points.Count > 0)
                return _points[_points.Count - 1];
            return null;
        }

        public void AddPoint(float x, float y, uint flags, Style style)
        {
            var path = LastPath();

            if (path == null)
                return;

            if (path.Count > 0 && _points.Count > 0)
            {
                var pt = LastPoint();
                if (pt.Equals(x, y, style.DistributionTollerance))
                {
                    pt.Flags |= flags;
                    return;
                }
            }

            var p = new Point(x, y, flags);
            _points.Add(p);
            path.Count++;
        }

        public void ClosePath()
        {
            var path = LastPath();
            if (path == null)
                return;
            path.Close();
        }

        private float PolyArea(int first, int count)
        {
            float area = 0;
            for (int i = first + 2; i < first + count; i++)
            {
                var a = _points[first];
                var b = _points[i - 1];
                var c = _points[i];
                area += Maths.Triarea2(a.Position, b.Position, c.Position);
            }
            return area * 0.5f;
        }

        private void PolyReverse(int first, int count)
        {
            int i = first;
            int j = first + count - 1;
            while (i < j)
            {
                var tmp = _points[i];
                _points[i] = _points[j];
                _points[j] = tmp;
                i++;
                j--;
            }
        }

        public void FlattenPaths(InstructionManager im, Style style)
        {
            if (_paths.Count > 0)
                return;

            for (int i = 0; i < im.QueueLength; i++)
            {
                var instruction = im.QueueAt(i);
                instruction.Execute(this, style);
            }

            _bounds = new Rectangle<float>(new Vector2D<float>(1e6f), new Vector2D<float>(0.0f));

            for (int i = 0; i < _paths.Count; i++)
            {
                var path = _paths[i];
                int start = path.First;

                var p0 = _points[start + path.Count - 1];
                var p1 = _points[start];
                if (Point.PtEquals(p0, p1, style.DistributionTollerance))
                {
                    path.Count--;
                    p0 = _points[path.Count - 1];
                    path.Close();
                }

                if (path.Count > 2)
                {
                    float area = PolyArea(start, path.Count);
                    if (path.Winding == API.Winding.CCW && area < 0.0f)
                    {
                        PolyReverse(start, path.Count);
                    }
                    if (path.Winding == API.Winding.CW && area > 0.0f)
                    {
                        PolyReverse(start, path.Count);
                    }
                }

                for (int j = 0; j < path.Count; j++)
                {
                    p1 = _points[start + j];

                    p0.Dx = p1.X - p0.X;
                    p0.Dy = p1.Y - p0.Y;
                    p0.Length = Maths.Normalize(p0.D);
                    p0.D = Vector2D.Normalize(p0.D);

                    _bounds.Origin.X = MathF.Min(_bounds.Origin.X, p0.X);
                    _bounds.Origin.Y = MathF.Min(_bounds.Origin.Y, p0.Y);
                    _bounds.Size.X = MathF.Max(_bounds.Size.X, 0);
                    _bounds.Size.Y = MathF.Max(_bounds.Size.Y, 0);

                    p0 = p1;
                }

            }

        }

        private void ChooseBevel(bool bevel, Point p0, Point p1, float w, out float x0, out float y0, out float x1, out float y1)
        {
            if (bevel)
            {
                x0 = p1.X + p0.Dy * w;
                y0 = p1.Y - p0.Dx * w;
                x1 = p1.X + p1.Dy * w;
                y1 = p1.Y - p1.Dy * w;
            }
            else
            {
                x0 = p1.X + p1.DMx * w;
                y0 = p1.Y + p1.DMy * w;
                x1 = p1.X + p1.DMx * w;
                y1 = p1.Dy + p1.DMy * w;
            }
        }

        private List<Vertex> BevelJoin(List<Vertex> verts, Point p0, Point p1, float lw, float rw, float lu, float ru, float fringe)
        {
            float dlx0 = p0.Dy;
            float dly0 = -p1.Dx;
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
                    float ry0 = p1.X - p1.DMy * rw;

                    verts.Add(new Vertex(p1.X, p1.Y, 0.5f, 1));
                    verts.Add(new Vertex(p1.X - dlx0 * rw, p1.Y - dly0 * rw, ru, 1));

                    verts.Add(new Vertex(rx0, ry0, ru, 1));
                    verts.Add(new Vertex(rx0, ry0, ru, 1));

                    verts.Add(new Vertex(p1.X, p1.Y, 0.5f, 1));
                    verts.Add(new Vertex(p1.X - dlx1 * rw, p1.Y - dly1 * rw, ru, 1));
                }
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
                int start = path.First;
                var p0 = _points[start + path.Count - 1];
                var p1 = _points[start];
                int nleft = 0;

                path.BevelCount = 0;

                for (int j = 0; j < path.Count; j++)
                {
                    p1 = _points[start + j];

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
                        if ((dmr2 * miterLimit * miterLimit) < 1.0f || lineJoin == LineCap.Bevel ||lineJoin == LineCap.Round)
                        {
                            p1.Flags |= (uint)PointFlags.PointInnerbevel;
                        }
                    }

                    if (p1.IsBevel() || p1.IsInnerbevel())
                    {
                        path.BevelCount++;
                    }

                    p0 = p1;
                }

                path.Convex = nleft == path.Count;
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
                int start = path.First;

                float woff = 0.5f * aa;
                var dst = new List<Vertex>(_vertices);

                if (fringe)
                {
                    var p0 = _points[start + path.Count - 1];
                    var p1 = _points[start];

                    for (int j = 0; j < path.Count; j++)
                    {
                        p1 = _points[start + j];

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
                    for (int j = 0; j < path.Count; j++)
                    {
                        var pt = _points[start + j];
                        dst.Add(new Vertex(pt.X, pt.Y, 0.5f, 1));
                    }
                }

                path.Fill = dst;
                _vertices = new List<Vertex>(dst);

                if (fringe)
                {
                    float lw = w + woff;
                    float rw = w - woff;
                    float lu = 0;
                    float ru = 1;
                    dst = new List<Vertex>(_vertices);

                    if (convex) {
                        lw = woff;
                        lu = 0.5f;
                    }

                    var p0 = _points[start + path.Count - 1];
                    var p1 = _points[start];

                    for (int j = 0; j < path.Count; j++)
                    {
                        p1 = _points[start + j];

                        if (p1.IsBevel() || p1.IsInnerbevel())
                        {
                            dst = BevelJoin(dst, p0, p1, lw, rw, lu, ru, style.FringeWidth);
                        }
                        else
                        {
                            dst.Add(new Vertex(p1.X + (p1.DMx * lw), p1.Y + (p1.DMy * lw), lu, 1));
                            dst.Add(new Vertex(p1.X - (p1.DMx * rw), p1.Y - (p1.DMy * rw), lu, 1));
                        }
                        p0 = p1;
                    }

                    dst.Add(new Vertex(_vertices[0].X, _vertices[0].Y, lu, 1));
                    dst.Add(new Vertex(_vertices[1].X, _vertices[1].Y, ru, 1));

                    _vertices = new List<Vertex>(dst);
                }
                else
                {
                    path.Stroke.Clear();
                }
            }

        }

        public void Purge()
        {
            Clear();
            ClearVerts();
        }

        public void ClearVerts()
        {
            _vertices.Clear();
        }

    }
}
