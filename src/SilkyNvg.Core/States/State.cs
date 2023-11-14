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
        
        public LineStyle LineStyle { get; set; }

        public float Alpha { get; set; }

        public Matrix3X2<float> Transform { get; set; }

        public Scissor Scissor { get; set; }

        public float FontSize { get; set; }

        public float LetterSpacing { get; set; }

        public float LineHeight { get; set; }

        public float FontBlur { get; set; }

        public Align TextAlign { get; set; }

        public int FontId { get; set; }

        public void Reset()
        {
            Fill = new Paint(Colour.White);
            Stroke = new Paint(Colour.Black);
            CompositeOperation = new CompositeOperationState(Blending.CompositeOperation.SourceOver);
            ShapeAntiAlias = true;
            StrokeWidth = 1.0f;
            MiterLimit = 10.0f;
            LineCap = LineCap.Butt;
            LineStyle = LineStyle.Solid;
            LineJoin = LineCap.Miter;
            Alpha = 1.0f;
            Transform = Matrix3X2<float>.Identity;

            Scissor = new Scissor(new Vector2D<float>(-1.0f));

            FontSize = 16.0f;
            LetterSpacing = 0.0f;
            LineHeight = 1.0f;
            FontBlur = 0.0f;
            TextAlign = Align.Left | Align.Baseline;
            FontId = 0;
        }

        /*public void CopyFrom(State other)
        {
            Fill = other.Fill;
            Stroke = other.Stroke;
            CompositeOperation = other.CompositeOperation;
            ShapeAntiAlias = other.ShapeAntiAlias;
            StrokeWidth = other.StrokeWidth;
            MiterLimit = other.MiterLimit;
            LineCap = other.LineCap;
            LineJoin = other.LineJoin;
            Alpha = other.Alpha;
            Transform = other.Transform;

            Scissor = other.Scissor;

            FontSize = other.FontSize;
            LetterSpacing = other.LetterSpacing;
            LineHeight = other.LineHeight;
            FontBlur = other.FontBlur;
            TextAlign = other.TextAlign;
            FontId = other.FontId;
        }*/

        public static State Default
        {
            get
            {
                State state = new();
                state.Reset();
                return state;
            }
        }

        public State Clone()
        {
            return (State)MemberwiseClone();
        }

    }
}
