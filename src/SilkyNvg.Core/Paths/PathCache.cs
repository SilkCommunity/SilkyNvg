using Silk.NET.Maths;
using SilkyNvg.Common;
using SilkyNvg.Graphics;
using SilkyNvg.Paths;
using SilkyNvg.Rendering;
using System;
using System.Collections.Generic;

namespace SilkyNvg.Core.Paths
{
    internal class PathCache
    {

        private readonly List<Path> _paths = new();
        private readonly Nvg _nvg;

        private Vector4D<float> _bounds;

        public Path[] Paths => _paths.ToArray();

        public Vector4D<float> Bounds => _bounds;

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
            _bounds.X = _bounds.Y = 1e6f;
            _bounds.Z = _bounds.W = -1e6f;

            foreach (Path path in _paths)
            {
                path.Flatten();

                _bounds.X = MathF.Min(_bounds.X, path.Bounds.X);
                _bounds.Y = MathF.Min(_bounds.Y, path.Bounds.Y);
                _bounds.Z = MathF.Max(_bounds.Z, path.Bounds.Z);
                _bounds.W = MathF.Max(_bounds.W, path.Bounds.W);
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
