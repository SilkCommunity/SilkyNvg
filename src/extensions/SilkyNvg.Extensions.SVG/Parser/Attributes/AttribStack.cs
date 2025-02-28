using AngleSharp.Text;
using SilkyNvg.Extensions.Svg.Parser.Constants;
using System.Xml;

namespace SilkyNvg.Extensions.Svg.Parser.Attributes
{
    internal class AttribStack
    {

        private delegate void AttributeParser(StringSource source, ref AttribState state);

        private readonly Dictionary<string, AttributeParser> Parsers = new()
        {
            [SvgAttributes.Display] = DisplayAttributeParser,
            [SvgAttributes.Fill] = FillAttributeParser
        };

        private readonly Stack<AttribState> _stack = new();

        internal AttribState Top
        {
            get => _stack.Peek();
            set
            {
                _stack.Pop();
                _stack.Push(value);
            }
        }

        internal AttribStack() { }

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
                if (Parsers.TryGetValue(name, out var parser))
                {
                    var content = new StringSource(attrib.Value);
                    parser.Invoke(content, ref currentState);
                }
            }
            return currentState;
        }

        private static void FillAttributeParser(StringSource source, ref AttribState state)
        {
            if (source.Content == CssKeywords.None)
            {
                state.HasFill = false;
            }
            else
            {
                state.FillPaint = new Paint(Colour.DarkMagenta);
            }
        }

        private static void DisplayAttributeParser(StringSource source, ref AttribState state)
        {
            if (source.Content == CssKeywords.None)
            {
                state.IsVisible = false;
            }
        }

    }
}
