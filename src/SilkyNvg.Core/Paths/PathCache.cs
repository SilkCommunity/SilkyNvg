﻿using SilkyNvg.Common;
using SilkyNvg.Graphics;
using SilkyNvg.Paths;
using SilkyNvg.Rendering;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace SilkyNvg.Core.Paths
{
    internal class PathCache
    {

        private const uint INIT_PATHS_SIZE = 16;

        private readonly IList<Path> _paths = new List<Path>((int)INIT_PATHS_SIZE);
        private readonly Nvg _nvg;

        private RectangleF _bounds;

        public IReadOnlyList<Path> Paths => (IReadOnlyList<Path>)_paths;

        public RectangleF Bounds => _bounds;

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
            _bounds = RectangleF.FromLTRB(1e6f, 1e6f, -1e6f, -1e6f);

            foreach (Path path in _paths)
            {
                path.Flatten();

                float xMin = MathF.Min(_bounds.Left, path.Bounds.Left);
                float yMin = MathF.Min(_bounds.Top, path.Bounds.Top);
                float xMax = MathF.Max(_bounds.Right, path.Bounds.Right);
                float yMax = MathF.Max(_bounds.Bottom, path.Bounds.Bottom);

                _bounds = RectangleF.FromLTRB(xMin, yMin, xMax, yMax);
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
