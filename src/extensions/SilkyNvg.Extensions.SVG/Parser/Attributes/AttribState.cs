using AngleSharp.Css;
using Silk.NET.Maths;
using SilkyNvg.Extensions.Svg.Paint;
using SilkyNvg.Extensions.Svg.Rendering;
using SilkyNvg.Graphics;

namespace SilkyNvg.Extensions.Svg.Parser.Attributes
{
    internal struct AttribState : IRenderDimensions
    {

        internal string Id;
        internal Matrix3X2<float> Transform;
        internal IPaintProvider FillPaint;
        internal IPaintProvider StrokePaint;
        internal float Opacity;
        internal float FillOpacity;
        internal float StrokeOpacity;
        internal float StrokeWidth;
        internal float StrokeDashOffset;
        internal LineCap StrokeLineJoin;
        internal LineCap StrokeLineCap;
        internal float MiterLimit;
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
                FillPaint = new SvgColour(Colour.Black),
                StrokePaint = new SvgColour(Colour.Black),
                Opacity = 1f,
                FillOpacity = 1f,
                StrokeOpacity = 1f,
                StopOpacity = 1f,
                StrokeWidth = 1f,
                StrokeLineJoin = LineCap.Miter,
                StrokeLineCap = LineCap.Butt,
                MiterLimit = 4f,
                HasFill = true,
                HasStroke = true,
                IsVisible = true,

                Viewport = default
            };
        }

    }
}
