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
        private readonly List<float> _scalars = [];
        private readonly List<PathSegment> _segments = new List<PathSegment>();
        
        internal bool IsClosed { get; private set; }
        
        internal Vector2 Start => _points[0]; // _points always has at least one element
        
        private Vector2 Last => _points[^1];

        internal SubPath(Vector2 start)
        {
            _points.Add(start);
        }

        internal void BuildFillGeometry(GeometryBuilder geometry)
        {
            // Cannot be closed if less than three points
            if (!IsClosed || _points.Count < 3)
            {
                return;
            }
            
            geometry.BeginPath(Start);
            
            int pointIndex = 0;
            int scalarIndex = 0;
            foreach (var segmentType in _segments)
            {
                Vector2 p0, p1, p2, p3;
                switch (segmentType)
                {
                    case PathSegment.Line:
                        p0 = _points[pointIndex + 0];
                        p1 = _points[pointIndex + 1];
                        geometry.AddLine(p0, p1);
                        pointIndex += 1;
                        break;
                    case PathSegment.Quadratic:
                        p0 = _points[pointIndex + 0];
                        p1 = _points[pointIndex + 1];
                        p2 = _points[pointIndex + 2];
                        geometry.AddQuadratic(p0, p1, p2);
                        pointIndex += 2;
                        break;
                    case PathSegment.Cubic:
                        p0 = _points[pointIndex + 0];
                        p1 = _points[pointIndex + 1];
                        p2 = _points[pointIndex + 2];
                        p3 = _points[pointIndex + 3];
                        geometry.AddCubic(p0, p1, p2, p3);
                        pointIndex += 3;  
                        break;
                    case PathSegment.ArcTo:
                        Vector2 s = _points[pointIndex + 0];
                        Vector2 c = _points[pointIndex + 1];
                        p1 = _points[pointIndex + 2];
                        Vector2 e = _points[pointIndex + 3];
                        float radius = _scalars[scalarIndex + 0];
                        geometry.AddArcTo(s, p1, e, c, radius);
                        pointIndex += 3;
                        scalarIndex += 1;
                        break;
                }
            }
            
            geometry.EndPath();
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

        internal void AddArcTo(Vector2 p1, Vector2 p2, float radius)
        {
            Vector2 p0 = Last;

            if (p0.FpEquals(p1) || p1.FpEquals(p2) || radius.FpEquals(0))
            {
                AddLine(p1);
                return;
            }

            if (Maths.PointsAreCollinear(p0, p1, p2))
            {
                AddLine(p1);
                return;
            }
            
            // let ln be length of side opposite pn in triangle p0-p1-p2
            // See: https://www.analyzemath.com/Geometry_calculators/radius_inscribed_circle.html
            float l0 = (p2 - p1).Length();
            float l1 = (p2 - p0).Length();
            float l2 = (p1 - p0).Length();

            float s = (l0 + l1 + l2) / 2;
            
            // Find inscribed circle
            float inscribedRadius = MathF.Sqrt((s - l0) * (s - l1) * (s - l2) / s);
            Vector2 inscribedCenter = (l0 * p0 + l1 * p1 + l2 * p2) / (l0 + l1 + l2);
            
            // ratio of inscribed radius to wanted radius
            float k = radius / inscribedRadius;
            
            // center of circle arc
            Vector2 center = p1 + k * (inscribedCenter - p1);
            
            // Tangent points that touch the lines
            Vector2 inscribedStart = (p0 + p1) / 2 + (l0 - l1) / (2 * l2) * (p0 - p1);
            Vector2 inscribedEnd = (p1 + p2) / 2 + (l1 - l2) / (2 * l0) * (p1 - p2);
            
            Vector2 toInscribedStart = inscribedStart - inscribedCenter;
            Vector2 toInscribedEnd = inscribedEnd - inscribedCenter;

            Vector2 start = center + k * toInscribedStart;
            Vector2 end = center + k * toInscribedEnd;

            AddLine(start);
            
            // NOTE: End needs to be last!
            // This is because the following segment needs to start where this one ended!
            _points.Add(center);
            _points.Add(p1);
            _points.Add(end);
            _scalars.Add(radius);
            _segments.Add(PathSegment.ArcTo);
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