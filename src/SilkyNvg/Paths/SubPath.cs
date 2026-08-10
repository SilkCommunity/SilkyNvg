using System.Collections.Generic;
using System.Numerics;
using SilkyNvg.Rendering;
using SilkyNvg.Utils;

namespace SilkyNvg.Paths
{
    internal class SubPath
    {
        
        private readonly List<Vector2> _points = new List<Vector2>();
        private readonly List<PathSegment> _segments = new List<PathSegment>();
        
        private readonly DataAccessibleArrayList<Vector2> _pointCoords = new DataAccessibleArrayList<Vector2>();
        private readonly DataAccessibleArrayList<Vector3> _curveSpaceCoords = new DataAccessibleArrayList<Vector3>();
        
        internal bool IsClosed { get; private set; }
        
        internal Vector2 Start => _points[0]; // _points always has at least one element
        
        private Vector2 Last => _points[_points.Count - 1];

        internal SubPath(Vector2 start)
        {
            _points.Add(start);
            _segments.Add(PathSegment.Move);
        }

        private void BuildGeometry()
        {
            _pointCoords.Clear();
            _curveSpaceCoords.Clear();
            
            Vector2 p0, p1, p2;
            int pointIndex = 0;
            foreach (var segmentType in _segments)
            {
                switch (segmentType)
                {
                    case PathSegment.Line:
                        p0 = _points[pointIndex + 0];
                        p1 = _points[pointIndex + 1];

                        // Make sure triangle is a triangle
                        if (!Start.FpEquals(p0))
                        {
                            _pointCoords.AddRange(Start, p0, p1);
                            _curveSpaceCoords.AddRange(Vector3.One, Vector3.One, Vector3.One);
                        }
                        
                        pointIndex += 1;
                        break;
                    case PathSegment.Quadratic:
                        p0 = _points[pointIndex + 0];
                        p1 = _points[pointIndex + 1];
                        p2 = _points[pointIndex + 2];

                        if (!Start.FpEquals(p0))
                        {
                            _pointCoords.AddRange(Start, p0, p2);
                            _curveSpaceCoords.AddRange(Vector3.One, Vector3.One, Vector3.One);
                        }

                        _pointCoords.AddRange(p0, p1, p2);
                        _curveSpaceCoords.AddRange(Vector3.Zero, new Vector3(0.5f, 0.0f, 0.5f), Vector3.One);

                        pointIndex += 2;
                        break;
                }
            }
        }

        internal void Fill(ISilkyRenderer renderer)
        {
            BuildGeometry();
            
            renderer.FillPath(_pointCoords.Data, _curveSpaceCoords.Data, _pointCoords.Count);
        }

        internal void AddLine(Vector2 p)
        {
            _points.Add(p);
            _segments.Add(PathSegment.Line);
        }

        internal void AddQuadratic(Vector2 cp, Vector2 p)
        {
            if (Last.FpEquals(cp))
            {
                AddLine(p);
            }
            else
            {
                _points.Add(cp);
                _points.Add(p);
                _segments.Add(PathSegment.Quadratic);
            }
        }

        internal void AddCubic(Vector2 cp1, Vector2 cp2, Vector2 p)
        {
            if (Last.FpEquals(cp1))
            {
                AddQuadratic(cp2, p);
            }
            else if (cp1.FpEquals(cp2))
            {
                AddQuadratic((cp1 + cp2) / 2, p);
            }
            else
            {
                _points.Add(cp1);
                _points.Add(cp2);
                _points.Add(p);
                _segments.Add(PathSegment.Cubic);
            }
        }

        internal void Close()
        {
            IsClosed = true;
            if (_points.Count > 1) // Don't close if we only have one point anyway
            {
                AddLine(_points[0]);
            }
        }

    }
}