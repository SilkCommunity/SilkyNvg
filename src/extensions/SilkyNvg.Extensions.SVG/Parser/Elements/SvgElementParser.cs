using AngleSharp.Css;
using AngleSharp.Css.Values;
using AngleSharp.Text;
using SilkyNvg.Extensions.Svg.Parser.Constants;
using System.Xml;

namespace SilkyNvg.Extensions.Svg.Parser.Elements
{
    internal class SvgElementParser(SvgParser parser) : ISvgElementParser
    {

        private void ParseWidth(StringSource content)
        {
            if (!Length.TryParse(content.Content, out Length width))
            {
                return;
            }
            parser.Width = (float)width.ToPixel(parser.State, RenderMode.Horizontal);
        }

        private void ParseHeight(StringSource content)
        {
            if (!Length.TryParse(content.Content, out Length height))
            {
                return;
            }
            parser.Height = (float)height.ToPixel(parser.State, RenderMode.Vertical);
        }

        private void ParseViewBox(StringSource content)
        {

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
