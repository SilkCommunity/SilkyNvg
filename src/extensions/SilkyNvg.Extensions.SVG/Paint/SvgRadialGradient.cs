using AngleSharp.Css;
using AngleSharp.Css.Dom;
using AngleSharp.Css.Values;
using Silk.NET.Maths;
using Silk.NET.SDL;
using SilkyNvg.Extensions.Svg.Parser.Utils;
using SilkyNvg.Extensions.Svg.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SilkyNvg.Extensions.Svg.Paint
{
    internal readonly struct SvgRadialGradient(GradientUnits units, ICssValue cx, ICssValue cy, ICssValue radius, ICssValue focalRadius,
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
            IRenderDimensions dimensions = GetAppropriateRenderDimensions(shapeBounds, viewport, units);
            float xOffset = (units == GradientUnits.ObjectBoundingBox) ? shapeBounds.Min.X : 0f;
            float yOffset = (units == GradientUnits.ObjectBoundingBox) ? shapeBounds.Min.Y : 0f;

            float inr = ParseCoord(focalRadius, dimensions, 0f);
            float outr = ParseCoord(radius, dimensions, 0f);
            float centerX = ParseCoord(cx, dimensions, xOffset);
            float centerY = ParseCoord(cy, dimensions, yOffset);

            var c = new Vector2D<float>(centerX, centerY);

            float r = (inr + outr) * 0.5f;
            float f = (outr - inr);

            Matrix3X3<float> transform = Matrix3X3<float>.Identity;
            transform.M31 = c.X;
            transform.M32 = c.Y;
            transform = Matrix3X3.Multiply(transform, ParseTransform(dimensions));

            Vector2D<float> extent = new(r);

            return new SilkyNvg.Paint(new Matrix3X2<float>(transform), extent, r, MathF.Max(1.0f, f), stops[0].Colour, stops[^1].Colour, default);
        }

    }
}
