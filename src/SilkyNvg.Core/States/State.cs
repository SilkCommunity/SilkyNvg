using Silk.NET.Maths;
using SilkyNvg.Blending;
using SilkyNvg.Graphics;
using SilkyNvg.Rendering;

namespace SilkyNvg.Core.States
{
    internal class State
    {

        public CompositeOperationState CompositeOperation { get; set; }

        public bool ShapeAntiAlias { get; set; }

        public Paint Fill { get; set; }

        public Paint Stroke { get; set; }

        public float StrokeWidth { get; set; }

        public float MiterLimit { get; set; }

        public LineCap LineJoin { get; set; }

        public LineCap LineCap { get; set; }

        public float Alpha { get; set; }

        public Matrix3X2<float> Transform { get; set; }

        public Scissor Scissor { get; set; }

        private State()
        {
            Fill = new Paint(Colour.White);
            Stroke = new Paint(Colour.Black);
            CompositeOperation = new CompositeOperationState(Blending.CompositeOperation.SourceOver);
            ShapeAntiAlias = true;
            StrokeWidth = 1.0f;
            MiterLimit = 10.0f;
            LineCap = LineCap.Butt;
            LineJoin = LineCap.Miter;
            Alpha = 1.0f;
            Transform = Matrix3X2<float>.Identity;

            Scissor = new Scissor(new Vector2D<float>(-1.0f));
        }

        public State Clone()
        {
            return (State)MemberwiseClone();
        }

        public static State Default => new();

    }
}
