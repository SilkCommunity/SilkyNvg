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
        internal readonly PathCache PathCache;

        internal readonly PixelRatio PixelRatio;

        private readonly Dictionary<string, ISvgElementParser> _elementParsers;

        internal float? Width;
        internal float? Height;

        internal AttribState State;

        internal SvgParser()
        {
            Attribs = new();
            PathCache = new();
            PixelRatio = new();

            Width = Height = null;

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

        internal void PathMoveTo(Vector2D<float> position)
        {
            PathCache.AddPath(PixelRatio);
            PathCache.LastPath.AddPoint(Vector2D.Transform(position, State.Transform), PointFlags.Corner);
        }

        internal void PathLineTo(Vector2D<float> position)
        {
            PathCache.LastPath.AddPoint(Vector2D.Transform(position, State.Transform), PointFlags.Corner);
        }

        internal void PathClose()
        {
            PathCache.LastPath.Close();
        }

        internal SvgImage Parse(XmlElement top)
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
            return new SvgImage([.. Shapes], (float)Width, (float)Height);
        }

    }
}
