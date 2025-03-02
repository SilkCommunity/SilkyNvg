using AngleSharp.Css;
using AngleSharp.Css.Dom;
using Silk.NET.Maths;

namespace SilkyNvg.Extensions.Svg.Paint
{
    internal readonly struct SvgGradientStop(ICssValue offset, Colour colour)
    {

        internal readonly Colour Colour = colour;

        internal readonly Vector2D<float> Offset(Vector2D<float> start, Vector2D<float> direction, IRenderDimensions dimensions)
        {
            return start + (float)offset.AsPx(dimensions, RenderMode.Undefined) * direction;
        }

    }
}
