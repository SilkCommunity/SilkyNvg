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
        
        internal bool IsClosed { get; private set; }
        
        internal Vector2 Start => _points[0]; // _points always has at least one element
        
        private Vector2 Last => _points[_points.Count - 1];

        internal SubPath(Vector2 start)
        {
            _points.Add(start);
            _segments.Add(PathSegment.Move);
        }

        internal uint BuildGeometry(DataAccessibleArrayList<float> verts, Matrix3x2 transform)
        {
            int pointIndex = 0;
            uint vertexCount = 0;
            foreach (var segmentType in _segments)
            {
                Vector2 p0, p1, p2;
                switch (segmentType)
                {
                    case PathSegment.Line:
                        p0 = Vector2.Transform(_points[pointIndex + 0], transform);
                        p1 = Vector2.Transform(_points[pointIndex + 1], transform);

                        // Make sure triangle is a triangle
                        if (!Start.FpEquals(p0))
                        {
                            verts.AddRange(
                                Start.X, Start.Y, 1f, 1f, 1f,
                                p0.X, p0.Y, 1f, 1f, 1f,
                                p1.X, p1.Y, 1f, 1f, 1f
                            );
                            vertexCount += 3;
                        }
                        
                        pointIndex += 1;
                        break;
                    case PathSegment.Quadratic:
                        p0 = _points[pointIndex + 0];
                        p1 = _points[pointIndex + 1];
                        p2 = _points[pointIndex + 2];

                        if (!Start.FpEquals(p0))
                        {
                            verts.AddRange(
                                Start.X, Start.Y, 1f, 1f, 1f,
                                p0.X, p0.Y, 1f, 1f, 1f,
                                p2.X, p2.Y, 1f, 1f, 1f
                            );
                            vertexCount += 3;
                        }

                        verts.AddRange(
                            p0.X, p0.Y, 0f, 0f, 0f,
                            p1.X, p1.Y, 0.5f, 0f, 0.5f,
                            p2.X, p2.Y, 1f, 1f, 1f
                        );
                        vertexCount += 3;
                        
                        pointIndex += 2;
                        break;
                }
            }

            return vertexCount;
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