using AngleSharp.Css;
using AngleSharp.Css.Dom;
using AngleSharp.Css.Parser;
using AngleSharp.Css.Values;
using AngleSharp.Text;
using Silk.NET.Maths;
using SilkyNvg.Extensions.Svg.Paint;
using SilkyNvg.Extensions.Svg.Parser.Constants;
using SilkyNvg.Extensions.Svg.Rendering;
using System.Xml;

namespace SilkyNvg.Extensions.Svg.Parser.Elements
{
    internal static class GradientParser
    {

        private static readonly Dictionary<string, GradientUnits> GradientUnits = new()
        {
            [GradientUnitNames.UserSpaceOnUse] = Paint.GradientUnits.UserSpaceOnUse,
            [GradientUnitNames.ObjectBoundingBox] = Paint.GradientUnits.ObjectBoundingBox
        };

        private static GradientUnits ParseGradientUnits(StringSource source)
        {
            var parsedValue = source.ParseStatic(GradientUnits);
            return parsedValue.HasValue ? parsedValue.Value.Value : Paint.GradientUnits.UserSpaceOnUse;
        }

        private readonly struct RenderDimensionsShim(float width, float height) : IRenderDimensions
        {
            public double RenderWidth => width;

            public double RenderHeight => height;

            public double FontSize => Length.Medium.Value;
        }

        private static IRenderDimensions GetAppropriateRenderDimensions(Box2D<float> boundingBox, SvgViewport viewport, GradientUnits units)
        {
            return units switch
            {
                Paint.GradientUnits.UserSpaceOnUse => new RenderDimensionsShim(viewport.Width, viewport.Height),
                Paint.GradientUnits.ObjectBoundingBox => new RenderDimensionsShim(boundingBox.Size.X, boundingBox.Size.Y),
                _ => throw new ArgumentException("Should not reach here")
            };
        }

        private static ICssValue ParseCoord(StringSource source)
        {
            return source.ParseLength() ?? Length.Zero;
        }

        private static ICssTransformFunctionValue[] ParseTransform(string source)
        {
            var list = new List<ICssTransformFunctionValue>();
            foreach (string func in source.SplitSpaces())
            {
                var mat = new StringSource(func).ParseTransform();
                if (mat != null)
                {
                    list.Add(mat);
                }
            }
            return [.. list];
        }

        private static SvgGradientStop[] ParseStops(XmlNodeList stops)
        {
            var svgStops = new List<SvgGradientStop>();
            foreach (XmlElement stop in stops)
            {
                if (stop.LocalName != "stop")
                {
                    continue;
                }
                svgStops.Add(GradientStopParser.ParseStop(stop));
            }
            return svgStops.ToArray();
        }

        internal static SvgLinearGradient ParseLinearGradient(XmlElement element)
        {
            GradientUnits units = ParseGradientUnits(new StringSource(element.GetAttribute(SvgAttributes.GradientUnits)));
            ICssValue x1 = ParseCoord(new StringSource(element.GetAttribute("x1")));
            ICssValue y1 = ParseCoord(new StringSource(element.GetAttribute("y1")));
            ICssValue x2 = ParseCoord(new StringSource(element.GetAttribute("x2")));
            ICssValue y2 = ParseCoord(new StringSource(element.GetAttribute("y2")));

            ICssTransformFunctionValue[] transformFuncs = ParseTransform(element.GetAttribute("gradientTransform"));
            SvgGradientStop[] stops = ParseStops(element.ChildNodes);

            return new SvgLinearGradient(units, x1, y1, x2, y2, transformFuncs, stops);
        }

        internal static SvgRadialGradient ParseRadialGradient(XmlElement element)
        {
            GradientUnits units = ParseGradientUnits(new StringSource(element.GetAttribute(SvgAttributes.GradientUnits)));
            ICssValue cx = ParseCoord(new StringSource(element.GetAttribute("cx")));
            ICssValue cy = ParseCoord(new StringSource(element.GetAttribute("cy")));
            ICssValue r = ParseCoord(new StringSource(element.GetAttribute("r")));
            ICssValue fr = ParseCoord(new StringSource(element.GetAttribute("fr")));

            ICssTransformFunctionValue[] transformFuncs = ParseTransform(element.GetAttribute("gradientTransform"));
            SvgGradientStop[] stops = ParseStops(element.ChildNodes);

            return new SvgRadialGradient(units, cx, cy, r, fr, transformFuncs, stops);
        }

    }
}
