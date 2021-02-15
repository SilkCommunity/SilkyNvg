using Silk.NET.Maths;
using SilkyNvg.Core.Geometry;
using System.Collections.Generic;

namespace SilkyNvg.Core.Paths
{
    public class PathCache
    {

        private const int INITIAL_POINTS_SIZE = 128;
        private const int INITIAL_PATHS_SIZE = 16;
        private const int INITIAL_VERTICES_SIZE = 256;

        private IList<Point> _points;
        private int _pointsCapacity;

        private IList<Path> _paths;
        private int _pathsCapacity;

        private IList<Vertex> _vertices;
        private int _verticesCapacity;

        private Rectangle<float> _bounds;

        public PathCache()
        {
            _points = new List<Point>(INITIAL_POINTS_SIZE);
            _pointsCapacity = INITIAL_POINTS_SIZE;

            _paths = new List<Path>(INITIAL_PATHS_SIZE);
            _pathsCapacity = INITIAL_PATHS_SIZE;

            _vertices = new List<Vertex>(INITIAL_VERTICES_SIZE);
            _verticesCapacity = INITIAL_VERTICES_SIZE;
        }

    }
}
