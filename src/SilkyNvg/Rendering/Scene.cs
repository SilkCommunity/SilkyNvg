using System;
using System.Numerics;
using SilkyNvg.Utils;

namespace SilkyNvg.Rendering
{
    public sealed class Scene : IDisposable
    {

        private readonly DataAccessibleArrayList<Vector2> _points = new DataAccessibleArrayList<Vector2>();
        private readonly DataAccessibleArrayList<uint> _quadraticDiscardIndices = new DataAccessibleArrayList<uint>();
        private readonly DataAccessibleArrayList<uint> _cubicDiscardIndices = new DataAccessibleArrayList<uint>();
        
        private readonly DataAccessibleArrayList<PathData> _paths = new DataAccessibleArrayList<PathData>();
        private readonly DataAccessibleArrayList<Vector4> _pathBounds = new DataAccessibleArrayList<Vector4>();

        public uint PointCount => (uint)_points.Count;
        
        public uint QuadraticDiscardIndexCount => (uint)_quadraticDiscardIndices.Count;
        
        public uint CubicDiscardIndexCount => (uint)_cubicDiscardIndices.Count;
        
        public uint PathCount => (uint)_paths.Count;
        
        public ReadOnlySpan<Vector2> Points => _points.Data;
        
        public ReadOnlySpan<uint> QuadraticDiscardIndices => _quadraticDiscardIndices.Data;
        
        public ReadOnlySpan<uint> CubicDiscardIndices => _cubicDiscardIndices.Data;
        
        public ReadOnlySpan<PathData> Paths => _paths.Data;
        
        public ReadOnlySpan<Vector4> PathBounds => _pathBounds.Data;
        
        // Current path cache
        private uint _pPointsIndex;
        private uint _pPointsCount;
        private uint _pQuadraticDiscardTrianglesIndex;
        private uint _pQuadraticDiscardTrianglesCount;
        private uint _pCubicDiscardTrianglesIndex;
        private uint _pCubicDiscardTrianglesCount;
        
        // Do not allow foreign instances
        internal Scene()
        {
            Clear();
        }
        
        internal void AddPoint(Vector2 p)
        {
            _points.Add(p);
        }

        internal void AddPoint(Vector2 p0, Vector2 p1)
        {
            _points.Add(p0);
            _points.Add(p1);
        }

        internal void AddPoint(Vector2 p0, Vector2 p1, Vector2 p2)
        {
            _points.Add(p0);
            _points.Add(p1);
            _points.Add(p2);
        }

        internal void FlagAddingQuadratic()
        {
            _quadraticDiscardIndices.Add(PointCount);
        }

        internal void FlagAddingCubic()
        {
            _cubicDiscardIndices.Add(PointCount);
        }

        internal void BeginPath()
        {
            _pPointsIndex = PointCount;
            _pPointsCount = 0;
            _pQuadraticDiscardTrianglesIndex = QuadraticDiscardIndexCount;
            _pQuadraticDiscardTrianglesCount = 0;
            _pCubicDiscardTrianglesIndex = CubicDiscardIndexCount;
            _pCubicDiscardTrianglesCount = 0;
        }

        internal void EndPath(Vector4 bounds)
        {
            _paths.Add(new PathData(
                pointIndex: _pPointsIndex,
                pointCount: _pPointsCount,
                quadraticDiscardTrianglesIndex: _pQuadraticDiscardTrianglesIndex,
                quadraticDiscardTrianglesCount: _pQuadraticDiscardTrianglesCount,
                cubicDiscardTrianglesIndex: _pCubicDiscardTrianglesIndex,
                cubicDiscardTrianglesCount: _pCubicDiscardTrianglesCount
            ));
            _pathBounds.Add(bounds);
        }

        internal void Clear()
        {
            _points.Clear();
            _quadraticDiscardIndices.Clear();
            _cubicDiscardIndices.Clear();
            
            _paths.Clear();
            _pathBounds.Clear();

            _pPointsIndex = 0;
            _pPointsCount = 0;
            _pQuadraticDiscardTrianglesIndex = 0;
            _pQuadraticDiscardTrianglesCount = 0;
            _pCubicDiscardTrianglesIndex = 0;
            _pCubicDiscardTrianglesCount = 0;
        }

        public void Dispose()
        {
            Clear();
        }
    }
}