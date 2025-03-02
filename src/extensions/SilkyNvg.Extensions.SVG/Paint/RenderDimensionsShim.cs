using AngleSharp.Css.Values;
using AngleSharp.Css;

namespace SilkyNvg.Extensions.Svg.Paint
{
    internal readonly struct RenderDimensionsShim(float width, float height) : IRenderDimensions
    {

        public double RenderWidth => width;

        public double RenderHeight => height;

        public double FontSize => Length.Medium.Value;

    }
}
