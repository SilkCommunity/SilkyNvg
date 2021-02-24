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
        public int PathCount => _paths.Count;
        public IList<Path> Paths => _paths;

        public PathCache()
        {
            _points = new List<Point>();
            _paths = new List<Path>();
            _vertices = new List<Vertex>();
        }

        public void FlattenPaths(InstructionManager im, Style style)
        {
            if (_paths.Count > 0)
                return;

            for (int i = 0; i < im.QueueLength; i++)
            {
                var instr = im.QueueAt(i);
                instr.FlattenPath(this, style);
            }

            float million = 1_000_000;
            _bounds = new Rectangle<float>(new Vector2D<float>(million, -million), Vector2D<float>.Zero);

            for (int i = 0; i < _paths.Count; i++)
            {
                var path = _paths[i];
                int first = path.First;
                int count = path.Count;

                var p0 = _points[first + count - 1];
                var p1 = _points[first];
                if (Maths.PtEquals(p0.Position, p1.Position, style.DistributionTollerance))
                {
                    path.Count--;
                    p0 = _points[first + count - 1];
                    path.Close();
                }


                if (path.Count > 2)
                {
                    Point[] pts = new Point[count];
                    for (int j = first; j < first + count; j++)
                        pts[j - first] = _points[j];
                    float area = Maths.PolyArea(pts);
                    if ((path.Winding == Winding.CCW && area < 0.0f) || (path.Winding == Winding.CW && area > 0.0f))
                        pts = Maths.PolyReverse(pts);
                    for (int j = first; j < first + count; j++)
                        _points[j] = pts[j - first];
                }

                for (int j = 0; j < path.Count; j++)
                {
                    p0.DX = p1.X - p0.X;
                    p0.DY = p1.Y - p0.Y;
                    Vector2D<float> newPos = p0.Position;
                    p0.Length = Maths.Normalize(p0.Position, ref newPos);
                    p0.Position = newPos;

                    var max = _bounds.Max;
                    _bounds.Origin.X = Math.Min(_bounds.Origin.X, p0.X);
                    _bounds.Origin.Y = Math.Min(_bounds.Origin.Y, p0.Y);
                    _bounds.Size.X = Math.Max(max.X, p0.X) - _bounds.Origin.X;
                    _bounds.Size.Y = Math.Max(max.Y, p0.Y) - _bounds.Origin.Y;

                    p0 = p1;
                }

            }
        }

        public void CalculateJoins(float w, LineCap lineJoin, float miterLimit)
        {
            float iw = 0.0f;

            if (w > 0.0f)
                iw = 1.0f / w;

            for (int i = 0; i < _paths.Count; i++)
            {
                var path = _paths[i];
                int first = path.First;
                int count = path.Count;
                var p0 = _points[first + count - 1];
                var p1 = _points[first];
                int nleft = 0;

                path.NBevel = 0;

                for (int j = 0; j < path.Count; j++)
                {
                    float dlx0 = p0.DY;
                    float dly0 = -p0.DX;
                    float dlx1 = p1.DY;
                    float dly1 = -p1.DX;

                    p1.DMX = (dlx0 + dlx1) * 0.5f;
                    p1.DMY = (dly0 + dly1) * 0.5f;
                    float dmr2 = p1.DMX * p1.DMX + p1.DMY * p1.DMY;
                    if (dmr2 > 0.000001f)
                    {
                        float scale = 1.0f / dmr2;
                        scale = Math.Min(scale, 600.0f);
                        p1.DMX *= scale;
                        p1.DMY *= scale;
                    }

                    bool isCorner = (p1.Flags & (uint)PointFlags.PointCorner) != 0;
                    p1.Flags = 0;
                    if (isCorner)
                        p1.Flags |= (uint)PointFlags.PointCorner;

                    float cross = p1.DX * p0.DY - p0.DX * p1.DY;
                    if (cross > 0.0f)
                    {
                        nleft++;
                        p1.Flags |= (uint)PointFlags.PointLeft;
                    }

                    float limit = Math.Max(1.0f, Math.Min(p0.Length, p1.Length) * iw);
                    if ((dmr2 * limit * limit) < 1.0f)
                        p1.Flags |= (uint)PointFlags.PointInnerbevel;

                    if ((p1.Flags & (uint)PointFlags.PointCorner) != 0)
                    {
                        if ((dmr2 * miterLimit * miterLimit) < 1.0f || lineJoin == LineCap.Bevel || lineJoin == LineCap.Round)
                            p1.Flags |= (uint)PointFlags.PointBevel;
                    }

                    if ((p1.Flags & ((uint)PointFlags.PointBevel | (uint)PointFlags.PointInnerbevel)) != 0)
                        path.NBevel++;

                    p0 = p1;
                }

                path.Convex = (nleft == path.Count);
            }
        }

        public void ChooseBevel(bool bevel, Point p0, Point p1, float w, out float x0, out float y0, out float x1, out float y1)
        {
            if (bevel)
            {
                x0 = p1.X + p0.DY * w;
                y0 = p1.Y - p0.DX * w;
                x1 = p1.X + p1.DY * w;
                y1 = p1.Y + p1.DX * w;
            }
            else
            {
                x0 = p1.X + p1.DMX * w;
                y0 = p1.Y + p1.DMY * w;
                x1 = p1.X + p1.DMX * w;
                y1 = p1.Y + p1.DMY * w;
            }
        }

        public List<Vertex> BevelJoin(List<Vertex> src, Point p0, Point p1, float lw, float rw, float lu, float ru, float fringe)
        {
            float dlx0 = p0.DY;
            float dly0 = -p0.DX;
            float dlx1 = p1.DY;
            float dly1 = -p1.DX;

            if ((p1.Flags & (uint)PointFlags.PointLeft) != 0)
            {
                ChooseBevel((p1.Flags & (uint)PointFlags.PointInnerbevel) != 0, p0, p1, lw, out float lx0, out float ly0, out float lx1, out float ly1);

                src.Add(new Vertex(lx0, ly0, lu, 1.0f));
                src.Add(new Vertex(p1.X - dlx0 * rw, p1.Y - dly0 * rw, ru, 1.0f));

                if ((p1.Flags & (uint)PointFlags.PointBevel) != 0)
                {
                    src.Add(new Vertex(lx0, ly0, ly0, 1.0f));
                    src.Add(new Vertex(p1.X - dlx0 * rw, p1.Y - dly0 * rw, ru, 1.0f));

                    src.Add(new Vertex(lx1, ly1, lu, 1.0f));
                    src.Add(new Vertex(p1.X - dlx1 * rw, p1.Y - dly1 * rw, ru, 1.0f));
                }
                else
                {
                    float rx0 = p1.X - p1.DMX * rw;
                    float ry0 = p1.Y - p1.DMY * rw;

                    src.Add(new Vertex(p1.X, p1.Y, 0.5f, 1.0f));
                    src.Add(new Vertex(p1.X - dlx0 * rw, p1.Y - dly0 * rw, ru, 1.0f));

                    src.Add(new Vertex(rx0, ry0, ru, 1.0f));
                    src.Add(new Vertex(rx0, ry0, ru, 1.0f));

                    src.Add(new Vertex(p1.X, p1.Y, 0.5f, 1.0f));
                    src.Add(new Vertex(p1.X - dlx1 * rw, p1.Y - dly1 * rw, ru, 1.0f));
                }

                src.Add(new Vertex(lx1, ly1, lu, 1.0f));
                src.Add(new Vertex(p1.X - dlx1 * rw, p1.Y - dly1 * rw, ru, 1.0f));
            }
            else
            {
                ChooseBevel((p1.Flags & (uint)PointFlags.PointInnerbevel) != 0, p0, p1, -rw, out float rx0, out float ry0, out float rx1, out float ry1);

                src.Add(new Vertex(p1.X + dlx0 * lw, p1.Y + dly0 * lw, lu, 1.0f));
                src.Add(new Vertex(rx0, ry0, ru, 1.0f));

                if ((p1.Flags & (uint)PointFlags.PointBevel) != 0)
                {
                    src.Add(new Vertex(p1.X + dlx0 * lw, p1.Y + dly0 * lw, lu, 1.0f));
                    src.Add(new Vertex(rx0, ry0, lu, 1.0f));

                    src.Add(new Vertex(p1.X + dlx1 * lw, p1.Y + dly1 * lw, lu, 1.0f));
                    src.Add(new Vertex(rx1, ry1, ru, 1.0f));
                }
                else
                {
                    float lx0 = p1.X + p1.DMX * lw;
                    float ly0 = p1.Y + p1.DMY * lw;

                    src.Add(new Vertex(p1.X + dlx0 * rw, p1.Y + dly0 * rw, lu, 1.0f));
                    src.Add(new Vertex(p1.X, p1.Y, 0.5f, 1.0f));

                    src.Add(new Vertex(lx0, ly0, lu, 1.0f));
                    src.Add(new Vertex(lx0, ly0, lu, 1.0f));

                    src.Add(new Vertex(p1.X + dlx1 * rw, p1.Y + dly1 * rw, lu, 1.0f));
                    src.Add(new Vertex(p1.X, p1.Y, 0.5f, 1.0f));
                }

                src.Add(new Vertex(p1.X + dlx1 * lw, p1.Y + dly1 * lw, lu, 1.0f));
                src.Add(new Vertex(rx1, ry1, ru, 1.0f));
            }

            return src;
        }

        public bool ExpandFill(float w, LineCap lineJoin, float miterLimit, Style style)
        {
            float aa = style.FringeWidth;
            bool fringe = w > 0.0f;

            CalculateJoins(w, lineJoin, miterLimit);

            int cverts = 0;
            for (int i = 0; i < _paths.Count; i++)
            {
                var path = _paths[i];
                cverts += path.Count + path.NBevel + 1;
                if (fringe)
                    cverts += (path.Count + path.NBevel * 5 + 1) * 2;
            }

            var verts = new List<Vertex>(cverts);

            bool convex = _paths.Count == 1 && _paths[0].Convex;

            for (int i = 0; i < _paths.Count; i++)
            {
                var path = _paths[i];
                var first = path.First;

                float woff = 0.5f * aa;
                path.Fill.Clear();
                path.Fill.AddRange(_vertices);

                if (fringe)
                {
                    var p0 = _points[first + path.Count - 1];
                    var p1 = _points[first];
                    for (int j = 0; j < path.Count; j++)
                    {
                        if ((p1.Flags & (uint)PointFlags.PointBevel) != 0)
                        {
                            p1 = _points[first + j];
                            float dlx0 = p0.DY;
                            float dly0 = -p0.DX;
                            float dlx1 = p1.DY;
                            float dly1 = -p1.DX;
                            if ((p1.Flags & (uint)PointFlags.PointLeft) != 0)
                            {
                                float lx = p1.X + p1.DMX * woff;
                                float ly = p1.Y + p1.DMY * woff;
                                verts.Add(new Vertex(lx, ly, 0.5f, 1.0f));
                            }
                            else
                            {
                                float lx0 = p1.X + dlx0 * woff;
                                float ly0 = p1.Y + dly0 * woff;
                                float lx1 = p1.X + dlx1 * woff;
                                float ly1 = p1.Y + dly1 * woff;
                                verts.Add(new Vertex(lx0, ly0, 0.5f, 1.0f));
                                verts.Add(new Vertex(lx1, ly1, 0.5f, 1.0f));
                            }
                        }
                        else
                        {
                            verts.Add(new Vertex(p1.X + (p1.DMX * woff), p1.Y + (p1.DMY * woff), 0.5f, 1.0f));
                        }
                        p0 = p1;
                    }
                }
                else
                {
                    for (int j = 0; j < path.Count; j++)
                    {
                        verts.Add(new Vertex(_points[first + j].X, _points[first + j].Y, 0.5f, 1.0f));
                    }
                }

                path.Fill.Clear();
                path.Fill.AddRange(verts);
                ((List<Vertex>)_vertices).AddRange(verts);
                path.NFill = verts.Count;

                if (fringe)
                {
                    float lw = w + woff;
                    float rw = w - woff;
                    float lu = 0;
                    float ru = 1;
                    verts = new List<Vertex>(_vertices);

                    if (convex)
                    {
                        lw = woff;
                        lu = 0.5f;
                    }

                    var p0 = _points[first + path.Count - 1];
                    var p1 = _points[first];

                    for (int j = 0; j < path.Count; j++)
                    {
                        p1 = _points[first + j];
                        if ((p1.Flags & ((uint)PointFlags.PointBevel | (uint)PointFlags.PointInnerbevel)) != 0)
                        {
                            verts = BevelJoin(verts, p0, p1, lw, rw, lu, ru, style.FringeWidth);
                        }
                        else
                        {
                            verts.Add(new Vertex(p1.X + (p1.DMX * lw), p1.Y + (p1.DMY * lw), lu, 1.0f));
                            verts.Add(new Vertex(p1.X - (p1.DMX * lw), p1.Y + (p1.DMY * lw), lu, 1.0f));
                        }
                        p0 = p1;
                    }

                    verts.Add(new Vertex(verts[0].X, verts[0].Y, lu, 1.0f));
                    verts.Add(new Vertex(verts[1].X, verts[1].Y, lu, 1.0f));

                    path.Stroke.Clear();
                    path.Stroke.AddRange(verts);
                    ((List<Vertex>)_vertices).AddRange(verts);
                }
                else
                {
                    path.Stroke.Clear();
                }

            }

            return true;

        }

        public Path LastPath()
        {
            if (_paths.Count > 0)
                return _paths[_paths.Count - 1];
            return null;
        }

        public void AddPath()
        {
            _paths.Add(new Path(_points.Count, Winding.CCW));
        }

        public Point LastPoint()
        {
            if (_points.Count > 0)
                return _points[_points.Count - 1];
            return null;
        }

        public void AddPoint(Vector2D<float> pos, uint flags, Style style)
        {
            var path = LastPath();
            if (path == null)
                return;

            if (path.Count > 0 && _points.Count > 0)
            {
                var pot = LastPoint();
                if (Maths.PtEquals(pot.Position, pos, style.DistributionTollerance))
                {
                    pot.Flags |= flags;
                    return;
                }
            }

            var pt = new Point(pos, flags);
            _points.Add(pt);
            path.Count++;
        }

        public void ClosePath()
        {
            var path = LastPath();
            if (path == null)
                return;
            path.Close();
        }

        public void Clear()
        {
            _points.Clear();
            _paths.Clear();
        }

        public void Purge()
        {
            Clear();
            _vertices.Clear();
        }

    }
}
