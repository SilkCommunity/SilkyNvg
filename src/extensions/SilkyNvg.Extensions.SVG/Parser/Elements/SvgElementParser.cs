using Silk.NET.Maths;
using SilkyNvg.Extensions.Svg.Parser.Attributes;
using SilkyNvg.Extensions.Svg.Parser.Constants;
using SilkyNvg.Extensions.Svg.Parser.DataTypes;
using SilkyNvg.Extensions.Svg.Parser.Utils;
using System.Xml;

namespace SilkyNvg.Extensions.Svg.Parser.Elements
{
    internal class SvgElementParser(SvgParser parser) : ISvgElementParser
    {

        private void ParseWidth(StringSource content)
        {
            if (parser.Width.HasValue)
            {
                return;
            }
            Length width = LengthParser.ParseLength(content);
            parser.Width = width.ToPixel(parser.State.Viewport, directionality: Directionality.Horizontal);
        }

        private void ParseHeight(StringSource content)
        {
            if (parser.Height.HasValue)
            {
                return;
            }
            Length height = LengthParser.ParseLength(content);
            parser.Height = height.ToPixel(parser.State.Viewport, directionality: Directionality.Horizontal);
        }

        private void ParseViewBox(StringSource content)
        {
            float?[] values = content.ParseNumberList();

            parser.State.Viewport = new Rectangle<float>(
                values[0] ?? 0f, values[1] ?? 0f,
                values[2] ?? 0f, values[3] ?? 0f
            );
        }

        public void Parse(XmlElement element)
        {
            // Parse svg specific attributes
            ISvgElementParser.ParseAttr(ParseWidth, element.GetAttribute(SvgAttributes.Width));
            ISvgElementParser.ParseAttr(ParseHeight, element.GetAttribute(SvgAttributes.Height));
            ISvgElementParser.ParseAttr(ParseViewBox, element.GetAttribute(SvgAttributes.ViewBox));
        }

    }
}
