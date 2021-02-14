using Silk.NET.Maths;
using SilkyNvg.Core.Geometry;

namespace SilkyNvg.Core.Paths
{
    public class PathCache
    {

        private const int CAPACITY_GROWTH_FACTOR = 8;
        private const int INITIAL_POINTS_SIZE = 128;
        private const int INITIAL_PATHS_SIZE = 16;
        private const int INITIAL_VERTICES_SIZE = 256;

        private Vector2D<float>[] _points;
        private int _pointsCount;
        private int _pointsLength;

        private Path[] _paths;
        private int _pathsCount;
        private int _pathsLength;

        private Vertex[] _vertices;
        private int _vertexCount;
        private int _verticesLength;

        private Rectangle<float> _bounds;

        public PathCache()
        {
            _points = new Vector2D<float>[INITIAL_POINTS_SIZE];
            _pointsCount = 0;
            _pointsLength = _points.Length;

            _paths = new Path[INITIAL_PATHS_SIZE];
            _pathsCount = 0;
            _pathsLength = _paths.Length;

            _vertices = new Vertex[INITIAL_VERTICES_SIZE];
            _vertexCount = 0;
            _verticesLength = _paths.Length;
        }

    }
}
