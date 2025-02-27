using SilkyNvg.Extensions.Svg.Parser.Constants;
using SilkyNvg.Extensions.Svg.Parser.Utils;
using System.Xml;

namespace SilkyNvg.Extensions.Svg.Parser.Attributes
{
    internal class AttribStack
    {

        private readonly Dictionary<string, IAttributeParser> Parsers = new()
        {
            [SvgAttributes.Display] = new DisplayAttributeParser()
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
                    parser.Parse(content, ref currentState);
                }
            }
            return currentState;
        }

    }
}
