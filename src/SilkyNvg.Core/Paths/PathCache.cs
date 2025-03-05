using SilkyNvg.Common;
using SilkyNvg.Common.Geometry;
using SilkyNvg.Graphics;
using SilkyNvg.Paths;
using SilkyNvg.Rendering;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace SilkyNvg.Core.Paths
{
    internal class PathCache
    {

        private const uint INIT_PATHS_SIZE = 16;

        private readonly IList<Path> _paths = new List<Path>((int)INIT_PATHS_SIZE);
        private readonly Nvg _nvg;

        private RectF _bounds;

        public IReadOnlyList<Path> Paths => (IReadOnlyList<Path>)_paths;

        public RectF Bounds => _bounds;

        public PathCache(Nvg nvg)
        {
            _nvg = nvg;

            _bounds = default;
        }

        public void Clear()
        {
            _paths.Clear();
        }

        public Path LastPath
        {
            get
            {
                if (_paths.Count > 0)
                {
                    return _paths[^1];
                }
                return null;
            }
        }

        public Path AddPath()
        {
            Path path = new(Winding.Ccw, _nvg.pixelRatio);
            _paths.Add(path);
            return path;
        }

        public void FlattenPaths()
        {
            _bounds.Min = new Vector2(1e6f);
            _bounds.Max = new Vector2(-1e6f);

            foreach (Path path in _paths)
            {
                path.Flatten();

                _bounds.Min = Vector2.Min(_bounds.Min, path.Bounds.Min);
                _bounds.Max = Vector2.Max(_bounds.Max, path.Bounds.Max);
            }
        }

        private void CalculateJoins(float w, LineCap lineJoin, float miterLimit)
        {
            float iw = 0.0f;

            if (w > 0.0f)
            {
                iw = 1.0f / w;
            }

            foreach (Path path in _paths)
            {
                path.CalculateJoins(iw, lineJoin, miterLimit);
            }
        }

        public void ExpandStroke(float w, float fringe, LineCap lineCap, LineCap lineJoin, float miterLimit, PixelRatio pixelRatio)
        {
            float aa = fringe;
            float u0 = 0.0f;
            float u1 = 1.0f;
            uint ncap = Maths.CurveDivs(w, MathF.PI, pixelRatio.TessTol);

            w += aa * 0.5f;

            if (aa == 0.0f)
            {
                u0 = 0.5f;
                u1 = 0.5f;
            }

            CalculateJoins(w, lineJoin, miterLimit);

            foreach (Path path in _paths)
            {
                path.ExpandStroke(aa, u0, u1, w, lineCap, lineJoin, ncap);
            }
        }

        public void ExpandFill(float w, LineCap lineJoin, float miterLimit, PixelRatio pixelRatio)
        {
            float aa = pixelRatio.FringeWidth;
            bool fringe = w > 0.0f;

            CalculateJoins(w, lineJoin, miterLimit);

            bool convex = _paths.Count == 1 && _paths[0].Convex;

            foreach (Path path in _paths)
            {
                path.ExpandFill(aa, fringe, convex, w);
            }
        }

        public void Dump()
        {
            Console.WriteLine("Dumping " + _paths.Count + " cached paths:");
            for (int i = 0; i < _paths.Count; i++)
            {
                Path path = _paths[i];
                Console.WriteLine(" - Path " + i);
                if (path.Fill.Count > 0)
                {
                    Console.WriteLine("     - fill: " + path.Fill.Count);
                    IEnumerator<Vertex> enumerator = path.Fill.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        Console.WriteLine("         " + enumerator.Current.X + "    " + enumerator.Current.Y);
                    }
                }
                if (path.Stroke.Count > 0)
                {
                    Console.WriteLine("     - stroke: " + path.Stroke.Count);
                    IEnumerator<Vertex> enumerator = path.Stroke.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        Console.WriteLine("         " + enumerator.Current.X + "    " + enumerator.Current.Y);
                    }
                }
            }
        }

    }
}
