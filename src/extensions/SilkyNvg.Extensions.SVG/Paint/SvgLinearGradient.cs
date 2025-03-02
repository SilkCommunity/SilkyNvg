using AngleSharp.Css;
using AngleSharp.Css.Dom;
using AngleSharp.Css.Values;
using Silk.NET.Maths;
using SilkyNvg.Extensions.Svg.Parser.Utils;
using SilkyNvg.Extensions.Svg.Rendering;

namespace SilkyNvg.Extensions.Svg.Paint
{
    internal readonly struct SvgLinearGradient(GradientUnits units, ICssValue x1, ICssValue y1, ICssValue x2, ICssValue y2,
        ICssTransformFunctionValue[] transformFuncs, SvgGradientStop[] stops) : IPaintProvider
    {

        private static IRenderDimensions GetAppropriateRenderDimensions(Box2D<float> boundingBox, SvgViewport viewport, GradientUnits units)
        {
            return units switch
            {
                GradientUnits.UserSpaceOnUse => new RenderDimensionsShim(viewport.Width, viewport.Height),
                GradientUnits.ObjectBoundingBox => new RenderDimensionsShim(boundingBox.Size.X, boundingBox.Size.Y),
                _ => throw new ArgumentException("Should not reach here")
            };
        }

        private static float ParseCoord(ICssValue v, IRenderDimensions dimensions, float offset)
        {
            return (float)v.AsPx(dimensions, RenderMode.Horizontal) + offset;
        }

        private Matrix3X3<float> ParseTransform(IRenderDimensions dimensions)
        {
            Matrix3X3<float> transform = Matrix3X3<float>.Identity;
            foreach (ICssTransformFunctionValue func in transformFuncs)
            {
                var mat = func.ComputeMatrix(dimensions);
                if (mat != null)
                {
                    transform = Matrix3X3.Multiply(transform, mat.ToMatrix3X3());
                }
            }
            return transform;
        }

        public SilkyNvg.Paint GetPaint(Box2D<float> shapeBounds, SvgViewport viewport)
        {
            const float large = 1e5f;

            IRenderDimensions dimensions = GetAppropriateRenderDimensions(shapeBounds, viewport, units);
            float xOffset = (units == GradientUnits.ObjectBoundingBox) ? shapeBounds.Min.X : 0f;
            float yOffset = (units == GradientUnits.ObjectBoundingBox) ? shapeBounds.Min.Y : 0f;

            float x1f = ParseCoord(x1, dimensions, xOffset);
            float y1f = ParseCoord(y1, dimensions, yOffset);
            float x2f = ParseCoord(x2, dimensions, xOffset);
            float y2f = ParseCoord(y2, dimensions, yOffset);

            Vector2D<float> start = new(x1f, y1f);
            Vector2D<float> end = new(x2f, y2f);
            Vector2D<float> delta = end - start;
            // Vector2D<float> dir = Vector2D.Normalize(delta); (Currently not used as paint stops don't have position yet)

            float d = delta.Length;
            if (d > 0.0001f)
            {
                delta /= d;
            }
            else
            {
                delta = new(0, 1);
            }

            Matrix3X3<float> paintTransform = new
            (
                delta.Y, -delta.X, 0f,
                delta.X, delta.Y, 0f,
                start.X - delta.X * large, start.Y - delta.Y * large, 0f
            );
            paintTransform = Matrix3X3.Multiply(paintTransform, ParseTransform(dimensions));

            Vector2D<float> extent = new(large, large + d * 0.5f);

            if (stops.Length == 0)
            {
                return new SilkyNvg.Paint(Colour.Black);
            }

            return new SilkyNvg.Paint(new Matrix3X2<float>(paintTransform), extent, 0.0f, MathF.Max(1.0f, d), stops[0].Colour, stops[^1].Colour, default);
        }

    }
}
