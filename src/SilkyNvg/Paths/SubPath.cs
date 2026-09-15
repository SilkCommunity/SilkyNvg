using System.Collections.Generic;
using System.Numerics;

namespace SilkyNvg.Paths
{
    internal class SubPath
    {
        
        private readonly List<float> _segmentData = [];
        private readonly List<bool> _segmentFlags = [];
        private readonly List<PathSegment> _segments = [];

        private float _lastEndX;
        private float _lastEndY;
        
        internal bool IsClosed { get; private set; }
        
        internal Vector2 Start => new(_segmentData[0], _segmentData[1]); // _points always has at least one element
        
        internal SubPath(float startX, float startY)
        {
            _segmentData.Add(startX);
            _segmentData.Add(startY);
            _lastEndX = startX;
            _lastEndY = startY;
        }
        
        private void AddPoint(float x, float y)
        {
            _segmentData.Add(x);
            _segmentData.Add(y);
        }

        internal void BuildFillGeometry()
        {
            
        }

        internal void AddLineTo(float x, float y)
        {
            AddPoint(_lastEndX, _lastEndY);
            AddPoint(x, y);
            _segments.Add(PathSegment.Line);
        }

        internal void Close()
        {
            IsClosed = true;
            AddLineTo(Start.X, Start.Y);
        }

    }
}