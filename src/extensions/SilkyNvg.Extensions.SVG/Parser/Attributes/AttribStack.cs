using AngleSharp.Css;
using AngleSharp.Css.Dom;
using AngleSharp.Css.Parser;
using AngleSharp.Css.Values;
using AngleSharp.Text;
using SilkyNvg.Extensions.Svg.Paint;
using SilkyNvg.Extensions.Svg.Parser.Constants;
using SilkyNvg.Extensions.Svg.Parser.Elements;
using System.Xml;

namespace SilkyNvg.Extensions.Svg.Parser.Attributes
{
    internal class AttribStack
    {

        private delegate void AttributeParser(StringSource source, ref AttribState state);

        private readonly Stack<AttribState> _stack = new();

        private readonly Dictionary<string, AttributeParser> _parsers;
        private readonly SvgParser _parser;

        internal AttribState Top
        {
            get => _stack.Peek();
            set
            {
                _stack.Pop();
                _stack.Push(value);
            }
        }

        internal AttribStack(SvgParser parser)
        {
            _parsers = new()
            {
                [SvgAttributes.Display] = DisplayAttributeParser,
                [SvgAttributes.Opacity] = OpacityAttributeParser,
                [SvgAttributes.Fill] = FillAttributeParser,
                [SvgAttributes.FillOpacity] = FillOpacityAttributeParser,
                [SvgAttributes.Stroke] = StrokeAttributeParser,
                [SvgAttributes.StrokeOpacity] = StrokeOpacityAttributeParser,
                [SvgAttributes.StrokeWidth] = StrokeWidthAttributeParser,
                [SvgAttributes.StrokeLinecap] = StrokeLineCapAttributeParser,
                [SvgAttributes.StrokeLinejoin] = StrokeLineJoinAttributeParser
            };
            _parser = parser;
        }

        internal void Initialise()
        {
            _stack.Push(AttribState.InitialState());
        }

        internal void Push(AttribState state)
        {
            _stack.Push(state);
        }

        internal void Pop()
        {
            _ = _stack.Pop();
        }

        internal AttribState ParseAttribs(XmlAttributeCollection attributes)
        {
            AttribState currentState = Top;
            foreach (XmlAttribute attrib in attributes)
            {
                string name = attrib.LocalName;
                if (_parsers.TryGetValue(name, out var parser))
                {
                    var content = new StringSource(attrib.Value);
                    parser.Invoke(content, ref currentState);
                }
            }
            return currentState;
        }

        private void DisplayAttributeParser(StringSource source, ref AttribState state)
        {
            if (source.IsIdentifier(CssKeywords.None))
            {
                state.IsVisible = false;
            }
        }

        private void OpacityAttributeParser(StringSource source, ref AttribState state)
        {
            var value = source.ParsePercentOrNumber();
            if (value.HasValue)
            {
                double opacity = (value.Value.Type == Length.Unit.Percent) ? (value.Value.Value * 0.01) : value.Value.Value;
                state.Opacity = (float)opacity;
            }
        }

        private void FillAttributeParser(StringSource source, ref AttribState state)
        {
            if (source.IsIdentifier(CssKeywords.None))
            {
                state.HasFill = false;
            }
            else if (source.IsFunction(FunctionNames.Url))
            {
                source.BackTo(0); // IsFunction skips for some reason
                var url = source.ParseUri();
                if (url == null)
                {
                    return;
                }
                var paintServer = _parser.GetElementById(url.Path);
                if (paintServer != null)
                {
                    if (paintServer.LocalName == "linearGradient")
                    {
                        state.FillPaint = Elements.GradientParser.ParseLinearGradient(paintServer);
                    }
                    else if (paintServer.LocalName == "radialGradient")
                    {
                        state.FillPaint = Elements.GradientParser.ParseRadialGradient(paintServer);
                    }
                }
            }
            else
            {
                var col = source.ParseColor();
                if (col.HasValue)
                {
                    state.FillPaint = new SvgColour(new Colour(col.Value.R, col.Value.G, col.Value.B, col.Value.A));
                }
            }
        }

        private void FillOpacityAttributeParser(StringSource source, ref AttribState state)
        {
            var value = source.ParsePercentOrNumber();
            if (value.HasValue)
            {
                double opacity = (value.Value.Type == Length.Unit.Percent) ? (value.Value.Value * 0.01) : value.Value.Value;
                state.FillOpacity = (float)opacity;
            }
        }

        private void StrokeAttributeParser(StringSource source, ref AttribState state)
        {
            if (source.IsIdentifier(CssKeywords.None))
            {
                state.HasStroke = false;
            }
            else if (source.IsFunction(FunctionNames.Url))
            {
                var url = source.ParseUri();
                // TODO
            }
            else
            {
                var col = source.ParseColor();
                if (col.HasValue)
                {
                    state.StrokePaint = new SvgColour(new Colour(col.Value.R, col.Value.G, col.Value.B, col.Value.A));
                }
            }
        }

        private void StrokeOpacityAttributeParser(StringSource source, ref AttribState state)
        {
            var value = source.ParsePercentOrNumber();
            if (value.HasValue)
            {
                double opacity = (value.Value.Type == Length.Unit.Percent) ? (value.Value.Value * 0.01) : value.Value.Value;
                state.StrokeOpacity = (float)opacity;
            }
        }

        private void StrokeWidthAttributeParser(StringSource source, ref AttribState state)
        {
            var value = source.ParseLength();
            if (value.HasValue)
            {
                double width = value.Value.AsPx(state, RenderMode.Horizontal);
                state.StrokeWidth = (float)width;
            }
        }

        private void StrokeLineCapAttributeParser(StringSource source, ref AttribState state)
        {
            var value = source.ParseStatic(LineCaps.Caps);
            if (!value.HasValue)
            {
                value = new Constant<Graphics.LineCap>("butt", Graphics.LineCap.Butt);
            }
            state.StrokeLineCap = value.Value.Value;
        }

        private void StrokeLineJoinAttributeParser(StringSource source, ref AttribState state)
        {
            var value = source.ParseStatic(LineCaps.Joins);
            if (!value.HasValue)
            {
                value = new Constant<Graphics.LineCap>("miter", Graphics.LineCap.Miter);
            }
            state.StrokeLineJoin = value.Value.Value;
        }

    }
}
