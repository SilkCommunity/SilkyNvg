using System.Xml;

namespace SilkyNvg.Extensions.Svg.Parser.Elements
{
    internal interface ISvgElementParser
    {

        void Parse(XmlElement element);

    }
}
