using Silk.NET.Maths;
using SilkyNvg.Core.Paths;
using SilkyNvg.Extensions.Svg.Parser.Attributes;
using SilkyNvg.Extensions.Svg.Parser.Elements;
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

        private readonly Dictionary<string, ISvgElementParser> _elementParsers;

        internal float? Width;
        internal float? Height;

        internal AttribState State;

        internal SvgParser(Nvg nvg)
        {
            Attribs = new();
            PathCache = new(nvg);
            Width = Height = null;

            _elementParsers = new()
            {
                ["svg"] = new SvgElementParser(this)
            };

            Attribs.Initialise();
        }

        private void ParseElement(XmlElement element)
        {
            State = Attribs.ParseAttribs(element.Attributes);
            if (!_elementParsers.TryGetValue(element.Name, out var elementParser))
            {
                return;
            }
            elementParser.Parse(element);
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

        internal void PathMoveTo(float x, float y)
        {
            PathCache.AddPath();
        }

        internal void Parse(XmlElement top)
        {
            ParseElement(top);
            if (!Width.HasValue)
            {
                Width = Attribs.Top.Viewport.Size.X;
            }
            if (!Height.HasValue)
            {
                Height = Attribs.Top.Viewport.Size.Y;
            }
            ParseChildren(top.ChildNodes);
            EndElement(top);
        }

    }
}
