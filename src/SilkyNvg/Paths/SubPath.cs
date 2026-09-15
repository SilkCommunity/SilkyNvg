using System.Collections.Generic;
using System.Numerics;
using SilkyNvg.Rendering;

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

        internal void BuildFillGeometry(ISceneContainer scene)
        {
            int pointIdx = 0;
            for (int i = 0; i < _segments.Count; i++)
            {
                var segmentType = _segments[i];

                Vector2 p0, p1, p2, p3;
                switch (segmentType)
                {
                    case PathSegment.Line:
                        p0 = _points[pointIdx++];
                        p1 = _points[pointIdx++];
                        scene.AddLinear(p0, p1, 0);
                        break;
                }
            }
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