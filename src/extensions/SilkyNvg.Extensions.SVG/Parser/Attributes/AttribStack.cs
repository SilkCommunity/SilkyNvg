using System.Xml;

namespace SilkyNvg.Extensions.Svg.Parser.Attributes
{
    internal class AttribStack
    {

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

        internal void Pop()
        {
            _ = _stack.Pop();
        }

        internal void ParseAttribs(XmlAttributeCollection attributes)
        {

        }

    }
}
