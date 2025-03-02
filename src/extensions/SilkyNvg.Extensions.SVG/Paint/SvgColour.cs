using Silk.NET.Maths;
using SilkyNvg.Extensions.Svg.Rendering;

namespace SilkyNvg.Extensions.Svg.Paint
{
    internal readonly struct SvgColour(Colour colour) : IPaintProvider
    {

        public SilkyNvg.Paint GetPaint(Box2D<float> shapeBounds, SvgViewport viewport)
        {
            return new SilkyNvg.Paint(colour);
        }

    }
}
