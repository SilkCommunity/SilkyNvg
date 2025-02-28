using AngleSharp.Css;
using Silk.NET.Maths;
using SilkyNvg.Extensions.Svg.Rendering;
using SilkyNvg.Graphics;

namespace SilkyNvg.Extensions.Svg.Parser.Attributes
{
    internal struct AttribState : IRenderDimensions
    {

        internal string Id;
        internal Matrix3X2<float> Transform;
        internal Paint FillPaint;
        internal Paint StrokePaint;
        internal float Opacity;
        internal float FillOpacity;
        internal float StrokeOpacity;
        internal float StrokeWidth;
        internal float StrokeDashOffset;
        internal LineCap StrokeLineJoin;
        internal LineCap StrokeLineCap;
        internal float MiterLimit;
        internal FillRule FillRule;
        internal float FontSize;
        internal Colour StopColour;
        internal float StopOpacity;
        internal float StopOffset;
        internal bool HasFill;
        internal bool HasStroke;
        internal bool IsVisible;

        internal SvgViewport Viewport;

        public readonly double RenderWidth => Viewport.Width;

        public readonly double RenderHeight => Viewport.Height;

        readonly double IRenderDimensions.FontSize => FontSize;

        internal static AttribState InitialState()
        {
            return new AttribState
            {
                Transform = Matrix3X2<float>.Identity,
                FillPaint = new Paint(Matrix3X2<float>.Identity, default, default, default, Colour.Black, Colour.Black, default),
                StrokePaint = new Paint(Matrix3X2<float>.Identity, default, default, default, Colour.Black, Colour.Black, default),
                Opacity = 1f,
                FillOpacity = 1f,
                StrokeOpacity = 1f,
                StopOpacity = 1f,
                StrokeWidth = 1f,
                StrokeLineJoin = LineCap.Miter,
                StrokeLineCap = LineCap.Butt,
                MiterLimit = 4f,
                FillRule = FillRule.Nonzero,
                HasFill = true,
                HasStroke = false,
                IsVisible = true,

                Viewport = default
            };
        }

    }
}
