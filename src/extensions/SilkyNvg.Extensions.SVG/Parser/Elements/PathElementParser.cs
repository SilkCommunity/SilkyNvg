﻿using Silk.NET.Maths;
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

        private readonly List<IInstruction> _instructions = new();
        private readonly StringBuilder _buffer = new();

        private readonly Dictionary<char, ParseInstruction> _instructionParsers;
        private readonly SvgParser _parser;

        private Vector2D<float> _currentPosition;

        internal PathElementParser(SvgParser parser)
        {
            _parser = parser;
            _instructionParsers = new()
            {
                ['M'] = source => ParseMoveTo(source, false),
                ['m'] = source => ParseMoveTo(source, true),

                ['L'] = source => ParseLineTo(source, false),
                ['l'] = source => ParseLineTo(source, true),

                ['Z'] = source => ParseClose(),
                ['z'] = source => ParseClose()
            };
        }

        private bool ParseMoveTo(StringSource source, bool relative)
        {
            var sequence = source.ParseCoordinatePairSequence();
            if (sequence == null)
            {
                return false;
            }
            foreach (var pair in sequence)
            {
                _currentPosition = relative ? (_currentPosition + pair) : pair;
                _instructions.Add(new MoveToInstruction(_currentPosition));
            }
            return true;
        }

        private bool ParseLineTo(StringSource source, bool relative)
        {
            var sequence = source.ParseCoordinatePairSequence();
            if (sequence == null)
            {
                return false;
            }
            foreach (var pair in sequence)
            {
                _currentPosition = relative ? (_currentPosition + pair) : pair;
                _instructions.Add(new LineToInstruction(_currentPosition));
            }
            return true;
        }


        private bool ParseClose()
        {
            _instructions.Add(new CloseInstruction());
            return true;
        }

        private void ParseD(StringSource content)
        {
            _currentPosition = Vector2D<float>.Zero;
            while (!content.IsDone)
            {
                content.ConsumeWsp();
                if (!_instructionParsers.TryGetValue(content.Current, out ParseInstruction? instructionParser))
                {
                    return;
                }
                content.Next(); // Instruction name
                content.ConsumeWsp();
                if (!instructionParser.Invoke(content))
                {
                    return;
                }
            }

            List<IInstruction> instructions = [
                new MoveToInstruction(new Vector2D<float>(200.0f, 50.0f)),
                new LineToInstruction(new Vector2D<float>(300.0f, 50.0f)),
                new LineToInstruction(new Vector2D<float>(300.0f, 150.0f)),
                new LineToInstruction(new Vector2D<float>(200.0f, 150.0f)),
                new LineToInstruction(new Vector2D<float>(200.0f, 50.0f)),
                new CloseInstruction()
            ];
            _parser.Shapes.Add(new Shape(_instructions, _parser.State));
        }

        public void Parse(XmlElement element)
        {
            ISvgElementParser.ParseAttr(ParseD, element.GetAttribute(SvgAttributes.D));
        }

        private static bool? ParseFlag(StringSource source)
        {
            char current = source.Current;
            source.Next();
            if (current == '0')
            {
                return false;
            }
            else if (current == '1')
            {
                return true;
            }
            source.Back();
            return null;
        }

    }
}
