using Silk.NET.Maths;
using SilkyNvg.Common;
using SilkyNvg.Core.Paths;
using SilkyNvg.Core.States;
using System;

namespace SilkyNvg.Core.Instructions
{
    internal class BezierToInstruction : IInstruction
    {

        public const int MAX_TESSELATION_LEVEL = 10;

        private Vector2D<float> _position0;
        private Vector2D<float> _position1;
        private Vector2D<float> _position2;

        public InstructionType Type => InstructionType.BezireTo;
        public float[] Data => 
            new float[] { (float)InstructionType.BezireTo, _position0.X, _position0.Y, _position1.X, _position1.Y, _position2.X, _position2.Y };

        public BezierToInstruction(float p0x, float p0y, float p1x, float p1y, float p2x, float p2y)
        {
            _position0 = new Vector2D<float>(p0x, p0y);
            _position1 = new Vector2D<float>(p1x, p1y);
            _position2 = new Vector2D<float>(p2x, p2y);
        }

        public void Setup(State state)
        {
            _position0 = Maths.TransformPoint(_position0, state.Transform);
            _position1 = Maths.TransformPoint(_position1, state.Transform);
            _position2 = Maths.TransformPoint(_position2, state.Transform);
        }

        private void Tesselate(float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4,
            int level, PointFlags type, PathCache cache, Style style)
        {
            if (level > MAX_TESSELATION_LEVEL)
                return;

            float x12 = (x1 + x2) * 0.5f;
            float y12 = (y1 + y2) * 0.5f;
            float x23 = (x2 + x3) * 0.5f;
            float y23 = (y2 + y3) * 0.5f;
            float x34 = (x3 + x4) * 0.5f;
            float y34 = (y3 + y4) * 0.5f;
            float x123 = (x12 + x23) * 0.5f;
            float y123 = (y12 + y23) * 0.5f;

            float dx = x4 - x1;
            float dy = y4 - y1;
            float d2 = MathF.Abs((x2 - x4) * dy - (y2 - y4) * dx);
            float d3 = MathF.Abs((x3 - x4) * dy - (y3 - y4) * dx);

            if ((d2 + d3) * (d2 + d3) < style.TesselationTollerance * (dx * dx + dy * dy))
            {
                cache.AddPoint(x4, y4, (uint)type, style);
                return;
            }

            float x234 = (x23 + x34) * 0.5f;
            float y234 = (y23 + y34) * 0.5f;
            float x1234 = (x123 + x234) * 0.5f;
            float y1234 = (y123 + y234) * 0.5f;

            Tesselate(x1, y1, x12, y12, x123, y123, x1234, y1234, level + 1, 0, cache, style);
            Tesselate(x1234, y1234, x234, y234, x34, y34, x4, y4, level + 1, type, cache, style);
        }

        public void Execute(PathCache cache, Style style)
        {
            var last = cache.LastPoint();
            if (last != null)
            {
                Tesselate(last.X, last.Y, _position0.X, _position0.Y, _position1.X, _position1.Y, _position2.X, _position2.Y, 0, PointFlags.Corner, cache, style);
            }
        }

    }
}
