using Silk.NET.Maths;
using SilkyNvg.Core.Instructions;
using SilkyNvg.Extensions.Svg.Parser.Attributes;
using SilkyNvg.Extensions.Svg.Parser.Constants;
using SilkyNvg.Extensions.Svg.Parser.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SilkyNvg.Extensions.Svg.Parser.Elements
{
    internal class PathElementParser : ISvgElementParser
    {

        private delegate bool ParseInstruction(StringSource source);

        private readonly Dictionary<char, ParseInstruction> _instructionParsers;

        private readonly SvgParser _parser;

        private Vector2D<float> _currentPosition;

        internal PathElementParser(SvgParser parser)
        {
            _parser = parser;
            _instructionParsers = new()
            {
                ['M'] = source => ParseMoveTo(source, false),
                ['m'] = source => ParseMoveTo(source, true)
            };
        }

        private bool ParseMoveTo(StringSource source, bool relative)
        {
            return true;
        }

        private void ParseD(StringSource content)
        {
            _currentPosition = Vector2D<float>.Zero;
            List<IInstruction> instructions = [
                new MoveToInstruction(new Vector2D<float>(200.0f, 50.0f)),
                new LineToInstruction(new Vector2D<float>(300.0f, 50.0f)),
                new LineToInstruction(new Vector2D<float>(300.0f, 150.0f)),
                new LineToInstruction(new Vector2D<float>(200.0f, 150.0f)),
                new LineToInstruction(new Vector2D<float>(200.0f, 50.0f)),
                new CloseInstruction()
            ];
            _parser.Shapes.Add(new Shape(instructions, _parser.State));
        }

        public void Parse(XmlElement element)
        {
            ISvgElementParser.ParseAttr(ParseD, element.GetAttribute(SvgAttributes.D));
        }

    }
}
