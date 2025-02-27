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
        private Vector2D<float>? _lastControlPoint;

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
                ['z'] = source => ParseClose(),

                ['H'] = source => ParseHorizontalLineTo(source, false),
                ['h'] = source => ParseHorizontalLineTo(source, true),
                ['V'] = source => ParseVerticalLineTo(source, false),
                ['v'] = source => ParseVerticalLineTo(source, true),

                ['C'] = source => ParseCurveTo(source, false),
                ['c'] = source => ParseCurveTo(source, true),
                ['S'] = source => ParseSmoothCurveTo(source, false),
                ['s'] = source => ParseSmoothCurveTo(source, true)
            };
        }

        private bool ParseMoveTo(StringSource source, bool relative)
        {
            var sequence = source.ParseCoordinatePairSequence();
            if (sequence == null)
            {
                return false;
            }
            Vector2D<float> pos = default;
            foreach (var pair in sequence)
            {
                pos = relative ? (_currentPosition + pair) : pair;
                _instructions.Add(new MoveToInstruction(pos));
            }
            _currentPosition = pos;
            return true;
        }

        private bool ParseLineTo(StringSource source, bool relative)
        {
            var sequence = source.ParseCoordinatePairSequence();
            if (sequence == null)
            {
                return false;
            }
            Vector2D<float> pos = default;
            foreach (var pair in sequence)
            {
                pos = relative ? (_currentPosition + pair) : pair;
                _instructions.Add(new LineToInstruction(pos));
            }
            _currentPosition = pos;
            return true;
        }

        private bool ParseClose()
        {
            _instructions.Add(new CloseInstruction());
            return true;
        }

        private bool ParseHorizontalLineTo(StringSource source, bool relative)
        {
            var sequence = source.ParseCoordinateSequence();
            if (sequence == null)
            {
                return false;
            }
            Vector2D<float> pos = default;
            foreach (var coord in sequence)
            {
                pos = new Vector2D<float>(relative ? (_currentPosition.X + coord) : coord, _currentPosition.Y);
                _instructions.Add(new LineToInstruction(pos));
            }
            _currentPosition = pos;
            return true;
        }

        private bool ParseVerticalLineTo(StringSource source, bool relative)
        {
            var sequence = source.ParseCoordinateSequence();
            if (sequence == null)
            {
                return false;
            }
            Vector2D<float> pos = default;
            foreach (var coord in sequence)
            {
                pos = new Vector2D<float>(_currentPosition.X, relative ? (_currentPosition.Y + coord) : coord);
                _instructions.Add(new LineToInstruction(pos));
            }
            _currentPosition = pos;
            return true;
        }

        private bool ParseCurveTo(StringSource source, bool relative)
        {
            var sequence = ParseCurveToCoordinateSequence(source);
            if (sequence == null)
            {
                return false;
            }
            foreach (var triplet in sequence)
            {
                Vector2D<float> p0 = relative ? (_currentPosition + triplet[0]) : triplet[0];
                Vector2D<float> p1 = relative ? (_currentPosition + triplet[1]) : triplet[1];
                Vector2D<float> p2 = relative ? (_currentPosition + triplet[2]) : triplet[2];
                _instructions.Add(new BezierToInstruction(p0, p1, p2));
                _lastControlPoint = p1;
            }
            _currentPosition = sequence[^1][2];
            return true;
        }

        private bool ParseSmoothCurveTo(StringSource source, bool relative)
        {
            var sequence = ParseSmoothCurveToCoordinateSequence(source);
            if (sequence == null)
            {
                return false;
            }
            Vector2D<float> current = _currentPosition;
            foreach (var @double in sequence)
            {
                Vector2D<float> lastCP2 = _lastControlPoint ?? current;

                Vector2D<float> p0 = 2 * current - lastCP2; // = current - (lastCP2 - current)
                Vector2D<float> p1 = relative ? (_currentPosition + @double[0]) : @double[0];
                Vector2D<float> p2 = relative ? (_currentPosition + @double[1]) : @double[1];
                _instructions.Add(new BezierToInstruction(p0, p1, p2));

                _lastControlPoint = p1;
                current = p2;
            }
            _currentPosition = current;
            return true;
        }

        private void ParseD(StringSource content)
        {
            _currentPosition = Vector2D<float>.Zero;
            _lastControlPoint = null;
            while (!content.IsDone)
            {
                content.ConsumeWsp();
                char instructionName = content.Current;
                if (!_instructionParsers.TryGetValue(instructionName, out ParseInstruction? instructionParser))
                {
                    return;
                }
                content.Next(); // Instruction name
                content.ConsumeWsp();
                if (!instructionParser.Invoke(content))
                {
                    return;
                }

                // We need the last Bezier's CP for S curve instruction. Delete them an instruction afterwards.
                if ((instructionName != 'C') & (instructionName != 'c') & (instructionName != 'S') & (instructionName != 's'))
                {
                    _lastControlPoint = null;
                }
            }

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

        internal static IReadOnlyList<Vector2D<float>[]>? ParseCurveToCoordinateSequence(StringSource source)
        {
            Vector2D<float>[]? triplet = source.ParseCoordinatePairTriplet();
            if (triplet == null)
            {
                return null;
            }
            List<Vector2D<float>[]> sequence = [];
            int idx;
            do
            {
                source.ConsumeCommaWsp();
                idx = source.Index;
                sequence.Add(triplet);
                triplet = source.ParseCoordinatePairTriplet();
            } while (triplet != null);
            source.BackTo(idx);
            return sequence.AsReadOnly();
        }

        internal static IReadOnlyList<Vector2D<float>[]>? ParseSmoothCurveToCoordinateSequence(StringSource source)
        {
            Vector2D<float>[]? @double = source.ParseCoordinatePairDouble();
            if (@double == null)
            {
                return null;
            }
            List<Vector2D<float>[]> sequence = [];
            int idx;
            do
            {
                source.ConsumeCommaWsp();
                idx = source.Index;
                sequence.Add(@double);
                @double = source.ParseCoordinatePairDouble();
            } while (@double != null);
            source.BackTo(idx);
            return sequence.AsReadOnly();
        }

    }
}
