using System.Collections.Generic;
using System.Numerics;
using SilkyNvg.Rendering;
using SilkyNvg.Utils;

namespace SilkyNvg.Paths
{
    internal class Subpath
    {

        private readonly List<Vector2> _points = [];
        private readonly List<float> _scalars = [];
        private readonly List<Segments> _segments = [];
        
        internal bool IsClosed { get; private set; }

        internal Vector2 Start => _points[0];
        
        internal Subpath(Vector2 start)
        {
            _points.Add(start);
        }

        internal void MarkClosed()
        {
            IsClosed = true;
        }

        internal uint AddToScene(ISceneContainer scene, RenderTolerances tol)
        {
            uint firstPointId = scene.PointCount;

            int pointHead = 1;
            int scalarHead = 0;
            int segmentHead = 0;
            
            Vector2 lastPoint = Start;
            for (; segmentHead < _segments.Count; segmentHead++)
            {
                var curve = _segments[segmentHead];
                switch (curve)
                {
                    case Segments.Line:
                        scene.AddLine(lastPoint, _points[pointHead]);
                        scene.ReduceDegenerate();
                        
                        lastPoint = _points[pointHead];
                        pointHead++;
                        break;
                    case Segments.Quadratic:
                        scene.AddQuadratic(lastPoint, _points[pointHead], _points[pointHead + 1]);
                        scene.ReduceDegenerate();
                        
                        lastPoint = _points[pointHead + 1];
                        pointHead += 2;
                        break;
                    case Segments.Cubic:
                        scene.AddCubic(lastPoint, _points[pointHead], _points[pointHead + 1], _points[pointHead + 2]);
                        scene.ReduceDegenerate();
                        
                        lastPoint = _points[pointHead + 2];
                        pointHead += 3;
                        break;
                    default:
                        break;
                }
            }

            if (IsClosed)
            {
                if (!Start.FpEquals(lastPoint, tol))
                {
                    scene.AddLine(lastPoint, Start);
                }
            }
            
            uint subpathId = scene.AddSubpath(IsClosed);
            return subpathId;
        }

        internal void AddLine(Vector2 p)
        {
            _points.Add(p);
            _segments.Add(Segments.Line);
        }

        internal void AddQuadratic(Vector2 cp, Vector2 p)
        {
            _points.Add(cp);
            _points.Add(p);
            _segments.Add(Segments.Quadratic);
        }

        internal void AddCubic(Vector2 cp1, Vector2 cp2, Vector2 p)
        {
            _points.Add(cp1);
            _points.Add(cp2);
            _points.Add(p);
            _segments.Add(Segments.Cubic);
        }

        internal void AddArcTo(Vector2 p1, Vector2 p2, float radius)
        {
            _points.Add(p1);
            _points.Add(p2);
            _scalars.Add(radius);
            _segments.Add(Segments.ArcTo);
        }

        internal void AddEllipse(Vector2 c, float radiusX, float radiusY, float rotation, float startAngle,
            float endAngle)
        {
            _points.Add(c);
            _scalars.Add(radiusX);
            _scalars.Add(radiusY);
            _scalars.Add(rotation);
            _scalars.Add(startAngle);
            _scalars.Add(endAngle);
            _segments.Add(Segments.Ellipse);
        }

    }
}