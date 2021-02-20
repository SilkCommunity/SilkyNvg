using Silk.NET.Maths;
using SilkyNvg.Core;
using SilkyNvg.Core.Geometry;
using SilkyNvg.Instructions;
using System.Collections.Generic;

namespace SilkyNvg.Paths
{
    internal class PathCache
    {

        private IList<Point> _points;

        private IList<Path> _paths;

        private IList<Vertex> _vertices;

        private Rectangle<float> _bounds;

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

        }

        public void ExpandFill()
        {

        }

        public void AddPath()
        {

        }

        public Point LastPoint()
        {
            if (_points.Count > 0)
                return _points[_points.Count - 1];
            return null;
        }

        public void AddPoint(Vector2D<float> pos, uint flags)
        {

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
