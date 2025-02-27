using Silk.NET.Maths;
using SilkyNvg.Blending;
using SilkyNvg.Core.Paths;
using SilkyNvg.Extensions.Svg.Parser.Attributes;
using SilkyNvg.Rendering;

namespace SilkyNvg.Extensions.Svg
{
    internal class Shape
    {

        private readonly IReadOnlyList<SilkyNvg.Rendering.Path> _paths;
        private readonly AttribState _attribs;

        internal Shape(PathCache pathCache, AttribState attribs)
        {
            pathCache.FlattenPaths();
            pathCache.ExpandStroke(attribs.StrokeWidth * 0.5f, 0.0f, attribs.StrokeLineCap, attribs.StrokeLineJoin, attribs.MiterLimit, new Common.PixelRatio());
            _paths = pathCache.Paths;
            _attribs = attribs;
        }

        internal void Draw(Nvg nvg)
        {
            nvg.renderer.Stroke(_attribs.StrokePaint,
                new CompositeOperationState(CompositeOperation.SourceOver),
                new Scissor(new Vector2D<float>(-1.0f)),
                1.0f, _attribs.StrokeWidth, _paths);
        }

    }
}
