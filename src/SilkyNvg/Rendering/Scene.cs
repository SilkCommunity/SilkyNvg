using System;
using System.Numerics;
using SilkyNvg.Utils;

namespace SilkyNvg.Rendering
{
    public sealed class Scene : IDisposable
    {

        private readonly DataAccessibleArrayList<Vector2> _points = new DataAccessibleArrayList<Vector2>();
        
        private readonly DataAccessibleArrayList<uint> _anchorGeometryIndices = new DataAccessibleArrayList<uint>();
        private readonly DataAccessibleArrayList<uint> _quadraticDiscardIndices = new DataAccessibleArrayList<uint>();
        private readonly DataAccessibleArrayList<uint> _cubicDiscardIndices = new DataAccessibleArrayList<uint>();
        
        private readonly DataAccessibleArrayList<SubPathData> _subPaths = new DataAccessibleArrayList<SubPathData>();
        
        private readonly DataAccessibleArrayList<PathData> _paths = new DataAccessibleArrayList<PathData>();
        private readonly DataAccessibleArrayList<Vector4> _pathBounds = new DataAccessibleArrayList<Vector4>();

        public uint PointCount => (uint)_points.Count;
        
        public uint AnchorGeometryIndexCount => (uint)_anchorGeometryIndices.Count;
        
        public uint QuadraticDiscardIndexCount => (uint)_quadraticDiscardIndices.Count;
        
        public uint CubicDiscardIndexCount => (uint)_cubicDiscardIndices.Count;

        public uint SubPathCount => (uint)_subPaths.Count;
        
        public uint PathCount => (uint)_paths.Count;
        
        public ReadOnlySpan<Vector2> Points => _points.Data;
        
        public ReadOnlySpan<uint> AnchorGeometryIndices => _anchorGeometryIndices.Data;
        
        public ReadOnlySpan<uint> QuadraticDiscardIndices => _quadraticDiscardIndices.Data;
        
        public ReadOnlySpan<uint> CubicDiscardIndices => _cubicDiscardIndices.Data;

        public ReadOnlySpan<SubPathData> SubPaths => _subPaths.Data;
        
        public ReadOnlySpan<PathData> Paths => _paths.Data;
        
        public ReadOnlySpan<Vector4> PathBounds => _pathBounds.Data;
        
        // Current sub path cache
        private uint _spAnchorGeometryIndex;
        private uint _spAnchorGeometryCount;
        private uint _spQuadraticDiscardTrianglesIndex;
        private uint _spQuadraticDiscardIndexCount;
        private uint _spCubicDiscardTrianglesIndex;
        private uint _spCubicDiscardIndexCount;
        
        // Current path cache
        private uint _pSubPathIndex;
        private uint _pSubPathCount;
        
        // Do not allow foreign instances
        internal Scene()
        {
            Clear();
        }
        
        internal void AddPoint(Vector2 p)
        {
            _points.Add(p);
        }

        internal void AddLinePoint(Vector2 p)
        {
            _anchorGeometryIndices.Add(PointCount);
            _spAnchorGeometryCount++;
            AddPoint(p);
        }

        internal void AddQuadraticPoints(Vector2 cp, Vector2 p)
        {
            // Quadratic discard index shape:
            // | canonical form position index (8 bits) | points index (24 bits) |
            _quadraticDiscardIndices.Add((0 << 24) & 0xFF000000 | (PointCount - 1) & 0x00FFFFFF);
            _quadraticDiscardIndices.Add((1 << 24) & 0xFF000000 | (PointCount + 0) & 0x00FFFFFF);
            _quadraticDiscardIndices.Add((2 << 24) & 0xFF000000 | (PointCount + 1) & 0x00FFFFFF);
            _spQuadraticDiscardIndexCount += 3;
            
            AddPoint(cp);
            
            _anchorGeometryIndices.Add(PointCount);
            _spAnchorGeometryCount++;
            AddPoint(p);
        }

        internal void AddCubicPoints(Vector2 cp1, Vector2 cp2, Vector2 p)
        {
            _cubicDiscardIndices.Add(PointCount - 1);
            _cubicDiscardIndices.Add(PointCount + 0);
            _cubicDiscardIndices.Add(PointCount + 1);
            _cubicDiscardIndices.Add(PointCount + 2);
            _spCubicDiscardIndexCount += 4;

            AddPoint(cp1);
            AddPoint(cp2);
            
            _anchorGeometryIndices.Add(PointCount);
            _spAnchorGeometryCount++;
            AddPoint(p);
        }

        internal void BeginSubPath(Vector2 p)
        {
            _spAnchorGeometryIndex = PointCount;
            _spAnchorGeometryCount = 1; // First point from MoveToCommand is also part of anchor geometry
            _anchorGeometryIndices.Add(PointCount);
            AddPoint(p);
            
            _spQuadraticDiscardTrianglesIndex = QuadraticDiscardIndexCount;
            _spQuadraticDiscardIndexCount = 0;
            _spCubicDiscardTrianglesIndex = CubicDiscardIndexCount;
            _spCubicDiscardIndexCount = 0;
        }

        internal void EndSubPath()
        {
            _subPaths.Add(new SubPathData(
                anchorGeometryIndex: _spAnchorGeometryIndex,
                anchorGeometryCount: _spAnchorGeometryCount,
                quadraticDiscardTrianglesIndex: _spQuadraticDiscardTrianglesIndex,
                quadraticDiscardTrianglesCount: _spQuadraticDiscardIndexCount,
                cubicDiscardTrianglesIndex: _spCubicDiscardTrianglesIndex,
                cubicDiscardTrianglesCount: _spCubicDiscardIndexCount
            ));
            _pSubPathCount++;
        }

        internal void BeginPath()
        {
            _pSubPathIndex = SubPathCount;
            _pSubPathCount = 0;
        }

        internal void EndPath(Vector4 bounds)
        {
            _paths.Add(new PathData(
                subPathIndex: _pSubPathIndex,
                subPathCount: _pSubPathCount
            ));
            _pathBounds.Add(bounds);
        }

        internal void Clear()
        {
            _points.Clear();
            _anchorGeometryIndices.Clear();
            _quadraticDiscardIndices.Clear();
            _cubicDiscardIndices.Clear();

            _subPaths.Clear();
            
            _paths.Clear();
            _pathBounds.Clear();

            _spAnchorGeometryIndex = 0;
            _spAnchorGeometryCount = 0;
            _spQuadraticDiscardTrianglesIndex = 0;
            _spQuadraticDiscardIndexCount = 0;
            _spCubicDiscardTrianglesIndex = 0;
            _spCubicDiscardIndexCount = 0;

            _pSubPathIndex = 0;
            _pSubPathCount = 0;
        }

        public void Dispose()
        {
            Clear();
        }
    }
}