using Silk.NET.Maths;
using SilkyNvg.Blending;
using SilkyNvg.Text;
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

        public float FontSize { get; set; }

        public float LetterSpacing { get; set; }

        public float LineHeight { get; set; }

        public float FontBlur { get; set; }

        public Align TextAlign { get; set; }

        public int FontId { get; set; }

        public static State Default
        {
            get
            {
                State state = new();

                state.Fill = new Paint(Colour.White);
                state.Stroke = new Paint(Colour.Black);
                state.CompositeOperation = new CompositeOperationState(Blending.CompositeOperation.SourceOver);
                state.ShapeAntiAlias = true;
                state.StrokeWidth = 1.0f;
                state.MiterLimit = 10.0f;
                state.LineCap = LineCap.Butt;
                state.LineJoin = LineCap.Miter;
                state.Alpha = 1.0f;
                state.Transform = Matrix3X2<float>.Identity;

                state.Scissor = new Scissor(new Vector2D<float>(-1.0f));

                state.FontSize = 16.0f;
                state.LetterSpacing = 0.0f;
                state.LineHeight = 1.0f;
                state.FontBlur = 0.0f;
                state.TextAlign = Align.Left | Align.Baseline;
                state.FontId = 0;

                return state;
            }
        }

        public State Clone()
        {
            return (State)MemberwiseClone();
        }

    }
}
