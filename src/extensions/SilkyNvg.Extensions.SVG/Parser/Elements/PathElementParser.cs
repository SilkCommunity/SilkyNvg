using SilkyNvg.Extensions.Svg.Parser.Attributes;
using SilkyNvg.Extensions.Svg.Parser.Constants;
using SilkyNvg.Extensions.Svg.Parser.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SilkyNvg.Extensions.Svg.Parser.Elements
{
    internal class PathElementParser(SvgParser parser) : ISvgElementParser
    {

        private static void ParseD(StringSource content)
        {

        }

        public void Parse(XmlElement element)
        {
            ISvgElementParser.ParseAttr(ParseD, SvgAttributes.D);
        }

    }
}
