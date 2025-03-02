using Silk.NET.Maths;
using SilkyNvg.Extensions.Svg.Rendering;

namespace SilkyNvg.Extensions.Svg.Paint
{
    internal interface IPaintProvider
    {

        SilkyNvg.Paint GetPaint(Box2D<float> shapeBounds, SvgViewport viewport);

    }
}
