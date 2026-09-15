using System.Collections.Generic;
using System.Numerics;

namespace SilkyNvg.Paths
{
    internal class SubPath
    {
        
        private readonly List<Vector2> _points = [];
        private readonly List<float> _segmentArcW1s = [];
        private readonly List<PathSegment> _segments = [];

        private Vector2 _endPoint;
        
        internal bool IsClosed { get; private set; }

        internal Vector2 Start => _points[0]; // _points always has at least one element
        
        internal SubPath(Vector2 start)
        {
            _endPoint = start;
        }

        internal void BuildFillGeometry()
        {
            
        }

        internal void AddLineTo(Vector2 p)
        {
            _points.Add(p);
            _points.Add(_endPoint);

            _endPoint = p;
            _segments.Add(PathSegment.Line);
        }

        internal void Close()
        {
            IsClosed = true;
            AddLineTo(Start);
        }

    }
}