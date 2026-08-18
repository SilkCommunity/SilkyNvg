using System;
using System.Collections.Generic;
using System.Numerics;
using SilkyNvg.Rendering;
using SilkyNvg.Utils;

namespace SilkyNvg.Paths
{
    internal class SubPath
    {
        
        private readonly List<float> _segmentData = [];
        private readonly List<PathSegment> _segments = [];
        
        internal bool IsClosed { get; private set; }
        
        internal Vector2 Start => new(_segmentData[0], _segmentData[1]); // _points always has at least one element
        
        private Vector2 Last => new (_segmentData[^2], _segmentData[^1]);

        internal SubPath(float startX, float startY)
        {
            _segmentData.Add(startX);
            _segmentData.Add(startY);
        }
        
        private void AddPoint(float x, float y)
        {
            _segmentData.Add(x);
            _segmentData.Add(y);
        }

        private Vector2 GetPoint(int index)
        {
            return new Vector2(_segmentData[index], _segmentData[index + 1]);
        }

        internal void BuildFillGeometry(GeometryBuilder geometry)
        {
            Vector2 p0 = GetPoint(0);
            int index = 2;  // First point is skipped because it's the initial point
            foreach (var segmentType in _segments)
            {
                Vector2 p1, p2, p3, o;
                float r, rx, ry, rot, alphaS, alphaE;
                switch (segmentType)
                {
                    case PathSegment.Line:
                        p1 = GetPoint(index + 0);
                        p0 = geometry.AddLine(p0, p1);
                        index += 2;
                        break;
                    case PathSegment.Quadratic:
                        p1 = GetPoint(index + 0);
                        p2 = GetPoint(index + 2);
                        p0 = geometry.AddQuadratic(p0, p1, p2);
                        index += 4;
                        break;
                    case PathSegment.Cubic:
                        p1 = GetPoint(index + 0);
                        p2 = GetPoint(index + 2);
                        p3 = GetPoint(index + 4);
                        p0 = geometry.AddCubic(p0, p1, p2, p3);
                        index += 6;
                        break;
                    case PathSegment.ArcTo:
                        p1 = GetPoint(index + 0);
                        p2 = GetPoint(index + 2);
                        r = _segmentData[index + 4];
                        p0 = geometry.AddArcTo(p0, p1, p2, r);
                        index += 5;
                        break;
                    case PathSegment.Ellipse:
                        o = GetPoint(index + 0);
                        rx = _segmentData[index + 2];
                        ry = _segmentData[index + 3];
                        alphaS = _segmentData[index + 4];
                        alphaE = _segmentData[index + 5];
                        rot = _segmentData[index + 6];
                        p0 = geometry.AddEllipse(p0, o, rx, ry, rot, alphaS, alphaE);
                        index += 7;
                        break;
                }
            }
        }

        internal void AddLine(float x, float y)
        {
            AddPoint(x, y);
            _segments.Add(PathSegment.Line);
        }

        internal void AddQuadratic(float cpx, float cpy, float x, float y)
        {
            AddPoint(cpx, cpy);
            AddPoint(x, y);
            _segments.Add(PathSegment.Quadratic);
        }

        internal void AddCubic(float cp1x, float cp1y, float cp2x, float cp2y, float x, float y)
        {
            AddPoint(cp1x, cp1y);
            AddPoint(cp2x, cp2y);
            AddPoint(x, y);
            _segments.Add(PathSegment.Cubic);
        }

        internal void AddArcTo(float x1, float y1, float x2, float y2, float radius)
        {
            AddPoint(x1, y1);
            AddPoint(x2, y2);
            _segmentData.Add(radius);
            _segments.Add(PathSegment.ArcTo);
        }

        internal void AddEllipse(float x, float y, float radiusX, float radiusY, float startAngle, float endAngle, float rotation)
        {
            AddPoint(x, y);
            _segmentData.Add(radiusX);
            _segmentData.Add(radiusY);
            _segmentData.Add(startAngle);
            _segmentData.Add(endAngle);
            _segmentData.Add(rotation);
            _segments.Add(PathSegment.Ellipse);
        }

        internal void Close()
        {
            IsClosed = true;
            if (_segmentData.Count > 2) // Don't close if we only have one point anyway
            {
                AddLine(_segmentData[0], _segmentData[1]);
            }
        }

    }
}