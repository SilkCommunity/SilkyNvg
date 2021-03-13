using Silk.NET.Maths;
using SilkyNvg.Common;
using SilkyNvg.Core.Instructions;
using SilkyNvg.Paths;
using System.Collections.Generic;

namespace SilkyNvg.Core.Paths
{
    internal class PathCache
    {

        private readonly List<Path> _paths;
        private readonly List<Vertex> _vertices;

        private Vector4D<float> _bounds;

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

            if (path.Points.Count > 0)
            {
                var pt = LastPoint();
                if (pt.Equals(x, y, style.DistributionTollerance))
                {
                    pt.Flags |= flags;
                    return;
                }
            }

            path.Points.Add(new Point(x, y, flags));
        }

        public void ClosePath()
        {
            var path = LastPath();
            if (path == null)
                return;
            path.Close();
        }

        public void FlattenPaths(InstructionQueue im, Style style)
        {
            if (_paths.Count > 0)
                return;

            while (im.QueueLength > 0)
            {
                im.Next().Execute(this, style);
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
                path.CalculateJoins(iw, miterLimit, lineJoin);
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
                            dst.Add(new Vertex(p1.X - (p1.DMx * rw), p1.Y - (p1.DMy * rw), lu, 1));
                        }
                        p0 = p1;
                    }

                    dst.Add(new Vertex(_vertices[0].X, _vertices[0].Y, lu, 1));
                    dst.Add(new Vertex(_vertices[1].X, _vertices[1].Y, ru, 1));

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