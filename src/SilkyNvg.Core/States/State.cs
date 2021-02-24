using Silk.NET.Maths;
using SilkyNvg.Core;
using SilkyNvg.Core.Composite;

namespace SilkyNvg.Core.States
{
    public struct State
    {

        public ICompositeOperation CompositeOperation { get; set; }
        public Scissor Scissor { get; set; }
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
