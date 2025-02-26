using Silk.NET.Maths;
using SilkyNvg.Core.Paths;
using SilkyNvg.Extensions.Svg.Parser.Attributes;
using SilkyNvg.Extensions.Svg.Parser.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SilkyNvg.Extensions.Svg.Parser
{
    internal class SvgParser
    {

        internal readonly List<Vector2D<float>> Points = [];
        internal readonly List<Gradient> Gradients = [];
        internal readonly List<Shape> Shapes = [];

        internal readonly AttribStack Attribs;
        internal readonly PathCache PathCache;
        internal readonly Rectangle<float> Viewport;

        private readonly Dictionary<string, ISvgElementParser> _elementParsers;

        internal SvgParser(Nvg nvg)
        {
            Attribs = new();
            PathCache = new(nvg);
            Viewport = default;

            _elementParsers = new()
            {
                ["svg"] = new SvgElementParser(this)
            };

            Attribs.Initialise();
        }

        private void ParseElement(XmlElement element)
        {
            Attribs.ParseAttribs(element.Attributes);
            if (!_elementParsers.TryGetValue(element.Name, out var elementParser))
            {
                return;
            }
            elementParser.Parse(element);
        }

        private void ParseChildren(XmlNodeList children)
        {

        }

        private void EndElement(XmlElement _)
        {
            Attribs.Pop();
        }

        internal void ParseSvgElement(XmlElement element)
        {
            ParseElement(element);
            ParseChildren(element.ChildNodes);
            EndElement(element);
        }

    }
}
