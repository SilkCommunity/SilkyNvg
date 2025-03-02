using Silk.NET.Maths;
using SilkyNvg.Common;
using SilkyNvg.Core.Paths;
using SilkyNvg.Extensions.Svg.Parser.Attributes;
using SilkyNvg.Extensions.Svg.Parser.Elements;
using System.Xml;

namespace SilkyNvg.Extensions.Svg.Parser
{
    internal class SvgParser
    {

        internal readonly List<Shape> Shapes = [];

        internal readonly AttribStack Attribs;

        internal readonly PixelRatio PixelRatio;

        private readonly Dictionary<string, ISvgElementParser> _elementParsers;

        internal float Width;
        internal float Height;

        internal AttribState State;

        internal XmlDocument Document { get; }

        internal Shape? LastShape
        {
            get
            {
                if (Shapes.Count > 0)
                {
                    return Shapes[^1];
                }
                return null;
            }
        }

        internal SvgParser(XmlDocument document)
        {
            Document = document;

            Attribs = new(this);
            PixelRatio = new();

            // initialise to size 100, so that something might show up
            // if an svg (incorrectly) doesn't specify a size.
            Width = Height = 100;

            _elementParsers = new()
            {
                ["svg"] = new SvgElementParser(this),
                ["path"] = new PathElementParser(this)
            };

            Attribs.Initialise();
        }

        private void ParseElement(XmlElement element)
        {
            State = Attribs.ParseAttribs(element.Attributes);
            if (_elementParsers.TryGetValue(element.Name, out var elementParser))
            {
                elementParser.Parse(element);
            }
            Attribs.Push(State);
        }

        private void ParseChildren(XmlNodeList children)
        {
            foreach (XmlElement child in children)
            {
                ParseSvgElement(child);
            }
        }

        private void EndElement(XmlElement _)
        {
            Attribs.Pop();
        }

        private void ParseSvgElement(XmlElement element)
        {
            ParseElement(element);
            ParseChildren(element.ChildNodes);
            EndElement(element);
        }

        private static XmlElement? GetElementById(XmlElement? e, string id)
        {
            if (e == null || e.GetAttribute("id") == id)
            {
                return e;
            }
            foreach (XmlElement child in e.ChildNodes)
            {
                var elem = GetElementById(child, id);
                if (elem != null)
                {
                    return elem;
                }
            }
            return null;
        }

        internal XmlElement? GetElementById(string id)
        {
            return GetElementById(Document.DocumentElement, id);
        }

        internal SvgImage Parse()
        {
            XmlElement top = Document.DocumentElement ?? throw new InvalidOperationException("DocumentElement is null!");

            ParseElement(top);
            ParseChildren(top.ChildNodes);
            EndElement(top);
            return new SvgImage(Shapes.ToArray(), (float)Width, (float)Height);
        }

    }
}
