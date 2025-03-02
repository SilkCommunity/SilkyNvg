using AngleSharp.Css;
using AngleSharp.Css.Dom;
using AngleSharp.Css.Parser;
using AngleSharp.Css.Values;
using AngleSharp.Text;
using Silk.NET.Maths;
using SilkyNvg.Extensions.Svg.Paint;
using SilkyNvg.Extensions.Svg.Parser.Constants;
using System.Xml;

namespace SilkyNvg.Extensions.Svg.Parser.Elements
{
    internal static class GradientStopParser
    {

        internal static SvgGradientStop ParseStop(XmlElement element)
        {
            ICssValue offset = new StringSource(element.GetAttribute(SvgAttributes.Offset)).ParseLengthOrCalc() ?? Length.Zero;
            var colour = new StringSource(element.GetAttribute(SvgAttributes.StopColour)).ParseColor();
            return new SvgGradientStop(offset, colour.HasValue ? new Colour(colour.Value.R, colour.Value.G, colour.Value.B, colour.Value.A) : Colour.Black);
        }

    }
}
