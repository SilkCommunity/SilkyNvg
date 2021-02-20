using Silk.NET.Maths;
using SilkyNvg.Core;
using SilkyNvg.Core.Geometry;
using SilkyNvg.Paths;
using SilkyNvg.States;
using System;

namespace SilkyNvg.Instructions
{
    internal class BeziereToInstruction : IInstruction
    {

        public const float INSTRUCTION_ID = 2.0F;

        public bool RequiresPosition => true;

        public float ID => INSTRUCTION_ID;

        public float[] FieldsAsFloatArray
        {
            get
            {
                return new float[] { _a.X, _a.Y, _b.X, _b.Y, _c.X, _c.Y };
            }
        }

        private readonly State _state;

        private Vector2D<float> _a, _b, _c;

        public BeziereToInstruction(Vector2D<float> a, Vector2D<float> b, Vector2D<float> c, State state)
        {
            _a = a;
            _b = b;
            _c = c;
            _state = state;
        }

        public void Prepare()
        {
            _a = Maths.TransformPoint(_a, _state.XForm);
            _b = Maths.TransformPoint(_b, _state.XForm);
            _c = Maths.TransformPoint(_c, _state.XForm);
        }

        private void TesselateBezier(Vector2D<float> last, Vector2D<float> a,
            Vector2D<float> b, Vector2D<float> c, int level, uint type, Style style, PathCache cache)
        {
            if (level > 10)
                return;

            float x0 = last.X;
            float y0 = last.Y;
            float x1 = a.X;
            float y1 = a.Y;
            float x2 = b.X;
            float y2 = b.X;
            float x3 = c.X;
            float y3 = c.Y;

            float x01 = (x0 + x1) * 0.5f;
            float y01 = (y0 + y1) * 0.5f;
            float x12 = (x1 + x2) * 0.5f;
            float y12 = (y1 + y2) * 0.5f;
            float x23 = (x2 + x3) * 0.5f;
            float y23 = (y2 + y3) * 0.5f;
            float x012 = (x01 + x12) * 0.5f;
            float y012 = (y01 + y12) * 0.5f;

            float dx = x3 - x0;
            float dy = y3 - y0;
            float d1 = (float)Math.Abs((x1 - x3) * dy - (y1 - y3) * dx);
            float d2 = (float)Math.Abs((x2 - x3) * dy - (y2 - y3) * dx);

            if ((d1 + d2) * (d1 + d2) < style.TesselationTollerance * (dx * dx + dy * dy))
            {
                cache.AddPoint(new Vector2D<float>(x3, y3), type);
                return;
            }

            float x123 = (x12 + x23) * 0.5f;
            float y123 = (y12 + y23) * 0.5f;
            float x0123 = (x012 + x123) * 0.5f;
            float y0123 = (y012 + y123) * 0.5f;

            var pos0 = new Vector2D<float>(x1, y1);
            var pos1 = new Vector2D<float>(x01, y01);
            var pos2 = new Vector2D<float>(x012, y012);
            var pos3 = new Vector2D<float>(x0123, y0123);

            var pos4 = new Vector2D<float>(x0123, y0123);
            var pos5 = new Vector2D<float>(x123, y123);
            var pos6 = new Vector2D<float>(x23, y23);
            var pos7 = new Vector2D<float>(x3, y3);

            TesselateBezier(pos0, pos1, pos2, pos3, level + 1, (uint)PointFlags.PointNone, style, cache);
            TesselateBezier(pos4, pos5, pos6, pos7, level + 1, type, style, cache);
        }

        public void FlattenPath(PathCache cache, Style style)
        {
            var last = cache.LastPoint();
            if (last != null)
            {
                float lastX = last.Position.X;
                float lastY = last.Position.Y;
                TesselateBezier(new Vector2D<float>(lastX, lastY), _a, _b, _c, 0, (uint)PointFlags.PointCorner, style, cache);
            }
        }

        public void Execute()
        {
            throw new NotImplementedException();
        }

    }
}
