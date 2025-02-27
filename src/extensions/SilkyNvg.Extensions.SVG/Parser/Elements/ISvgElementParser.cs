using SilkyNvg.Extensions.Svg.Parser.Attributes;
using SilkyNvg.Extensions.Svg.Parser.Utils;
using System.Xml;

namespace SilkyNvg.Extensions.Svg.Parser.Elements
{
    internal interface ISvgElementParser
    {

        void Parse(XmlElement element);

        delegate void AttrParser(StringSource content);

        static void ParseAttr(AttrParser parser, string? strValue)
        {
            if (!string.IsNullOrEmpty(strValue))
            {
                parser.Invoke(new StringSource(strValue));
            }
        }

    }
}
