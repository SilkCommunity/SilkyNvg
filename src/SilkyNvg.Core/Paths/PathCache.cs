using SilkyNvg.Common;
using System.Collections.Generic;

namespace SilkyNvg.Core.Paths
{
    internal class PathCache
    {

        private readonly List<Point> _points;
        private readonly List<Path> _paths;
        private readonly List<Vertex> _vertices;

        public PathCache()
        {
            _points = new();
            _paths = new();
            _vertices = new();
        }

    }
}
