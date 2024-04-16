using Silk.NET.Maths;
using SilkyNvg.Common;
using SilkyNvg.Core.Paths;
using System;

namespace SilkyNvg.Core.Instructions
{
    internal class BezierToInstruction : IInstruction
    {
        private const byte MAX_TESSELATION_DEPTH = 10;

        private readonly Vector2D<float> _p0;
        private readonly Vector2D<float> _p1;
        private readonly Vector2D<float> _p2;

        private readonly float _tessTol;
        private readonly PathCache _pathCache;

        public BezierToInstruction(Vector2D<float> p0, Vector2D<float> p1, Vector2D<float> p2, float tessTol, PathCache pathCache)
        {
            _p0 = p0;
            _p1 = p1;
            _p2 = p2;

            _tessTol = tessTol;
            _pathCache = pathCache;
        }

        private void TesselateBezier(Vector2D<float> p1, Vector2D<float> p2, Vector2D<float> p3, Vector2D<float> p4, byte level, PointFlags flags)
        {
            if (level > MAX_TESSELATION_DEPTH)
            {
                return;
            }

            Vector2D<float> p12 = (p1 + p2) * 0.5f;
            Vector2D<float> p23 = (p2 + p3) * 0.5f;
            Vector2D<float> p34 = (p3 + p4) * 0.5f;
            Vector2D<float> p123 = (p12 + p23) * 0.5f;

            Vector2D<float> d = p4 - p1;
            float d2 = MathF.Abs((p2.X - p4.X) * d.Y - (p2.Y - p4.Y) * d.X);
            float d3 = MathF.Abs((p3.X - p4.X) * d.Y - (p3.Y - p4.Y) * d.X);

            if ((d2 + d3) * (d2 + d3) < _tessTol * (d.X * d.X + d.Y * d.Y))
            {
                _pathCache.LastPath.AddPoint(p4, flags);
                return;
            }

            Vector2D<float> p234 = (p23 + p34) * 0.5f;
            Vector2D<float> p1234 = (p123 + p234) * 0.5f;

            TesselateBezier(p1, p12, p123, p1234, (byte)(level + 1), 0);
            TesselateBezier(p1234, p234, p34, p4, (byte)(level + 1), flags);
        }

        public void BuildPaths()
        {
            if (_pathCache.LastPath.PointCount > 0)
            {
                Vector2D<float> last = _pathCache.LastPath.LastPoint;
                TesselateBezier(last, _p0, _p1, _p2, 0, PointFlags.Corner);
            }
        }

    }
}
