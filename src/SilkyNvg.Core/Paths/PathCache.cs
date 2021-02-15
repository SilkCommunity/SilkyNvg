using Silk.NET.Maths;
using SilkyNvg.Core.Geometry;
using System.Collections.Generic;

namespace SilkyNvg.Core.Paths
{
    public class PathCache
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

    }
}
