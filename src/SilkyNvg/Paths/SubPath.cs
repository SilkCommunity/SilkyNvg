using System;
using System.Collections.Generic;
using System.Numerics;
using SilkyNvg.Rendering;
using SilkyNvg.Utils;

namespace SilkyNvg.Paths
{
    internal class SubPath
    {
        
        private readonly List<Vector2> _points = [];
        private readonly List<PathSegment> _segments = [];
        
        internal bool IsClosed { get; private set; }
        
        internal Vector2 Start => _points[0]; // _points always has at least one element
        
        private Vector2 Last => _points[^1];

        internal SubPath(Vector2 start)
        {
            _points.Add(start);
        }

        internal void BuildGeometry(Matrix3x2 transform, GeometryBuilder geometry)
        {
            int pointIndex = 0;
            foreach (var segmentType in _segments)
            {
                Vector2 p0, p1, p2, p3;
                switch (segmentType)
                {
                    case PathSegment.Line:
                        p0 = Vector2.Transform(_points[pointIndex + 0], transform);
                        p1 = Vector2.Transform(_points[pointIndex + 1], transform);
                        geometry.AddLine(Start, p0, p1);
                        pointIndex += 1;
                        break;
                    case PathSegment.Quadratic:
                        p0 = Vector2.Transform(_points[pointIndex + 0], transform);
                        p1 = Vector2.Transform(_points[pointIndex + 1], transform);
                        p2 = Vector2.Transform(_points[pointIndex + 2], transform);
                        geometry.AddQuadratic(Start, p0, p1, p2);
                        pointIndex += 2;
                        break;
                    case PathSegment.Cubic:
                        p0 = Vector2.Transform(_points[pointIndex + 0], transform);
                        p1 = Vector2.Transform(_points[pointIndex + 1], transform);
                        p2 = Vector2.Transform(_points[pointIndex + 2], transform);
                        p3 = Vector2.Transform(_points[pointIndex + 3], transform);
                        geometry.AddCubic(Start, p0, p1, p2, p3);
                        pointIndex += 3;  
                        break;
                }
            }
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