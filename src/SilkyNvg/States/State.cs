using Silk.NET.Maths;
using SilkyNvg.Core;

namespace SilkyNvg.States
{
    internal struct State
    {

        // TODO: Composite operations.
        public bool ShapeAntiAlias { get; set; }
        public Paint Fill { get; set; }
        public Paint Stroke { get; set; }
        public float StrokeWidth { get; set; }
        public float MiterLimit { get; set; }
        public LineCap LineJoin { get; set; }
        public LineCap LineCap { get; set; }
        public float Alpha { get; set; }
        public Matrix3X2<float> XForm { get; set; }
        // TODO: Scissor
        // TODO: Font

    }
}
