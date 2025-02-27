using Silk.NET.Maths;
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
            _instructionParsers = new(){
                ['M'] = source => ParseMoveTo(source, false),
                ['m'] = source => ParseMoveTo(source, true),
                ['L'] = source => ParseLineTo(source, false),
                ['l'] = source => ParseLineTo(source, true),
                ['Z'] = _ => ParseClose(),
                ['z'] = _ => ParseClose(),
            };
        }

        private bool ParseMoveTo(StringSource source, bool relative)
        {
            while (!source.Current.IsLetter())
            {
                Vector2D<float>? coord = ParseCoordinatePair(source);
                // Faulty if coord does not exist or no whitespace follows.
                // However if a new instruction is concatenated without wsp, it is allowed.
                if (!coord.HasValue || (!ConsumeCommaWsp(source) && !_instructionParsers.ContainsKey(source.Current)))
                {
                    return false;
                }
                if (relative)
                {
                    coord += _currentPosition;
                }
                _currentPosition = coord.Value;
                _parser.PathMoveTo(_currentPosition);
            }
            return true;
        }

        private bool ParseLineTo(StringSource source, bool relative)
        {
            while (!source.Current.IsLetter())
            {
                Vector2D<float>? coord = ParseCoordinatePair(source);
                // Faulty if coord does not exist or no whitespace follows.
                // However if a new instruction is concatenated without wsp, it is allowed.
                if (!coord.HasValue || (!ConsumeCommaWsp(source) && !_instructionParsers.ContainsKey(source.Current)))
                {
                    return false;
                }
                if (relative)
                {
                    coord += _currentPosition;
                }
                _currentPosition = coord.Value;
                _parser.PathLineTo(_currentPosition);
            }
            return true;
        }

        private bool ParseClose()
        {
            _parser.PathClose();
            return true;
        }

        private bool ParseElipticalArcArgument(StringSource source)
        {
            float? rx = ParseCoordinate(source);
            if (!rx.HasValue        || !ConsumeCommaWsp(source)) goto failure;
            float? ry = ParseCoordinate(source);
            if (!ry.HasValue        || !ConsumeCommaWsp(source)) goto failure;
            float? xRot = ParseCoordinate(source);
            if (!xRot.HasValue      || !ConsumeCommaWsp(source)) goto failure;
            bool? largeFlag = ParseFlag(source);
            if (!largeFlag.HasValue || !ConsumeCommaWsp(source)) goto failure;
            bool? sweepFlag = ParseFlag(source);
            if (!sweepFlag.HasValue || !ConsumeCommaWsp(source)) goto failure;
            Vector2D<float>? pos = ParseCoordinatePair(source);
            if (!pos.HasValue       || !ConsumeCommaWsp(source)) goto failure;


            return true;
failure:
            return false;
        }

        private void ParseD(StringSource content)
        {
            _currentPosition = Vector2D<float>.Zero;
            while (!content.IsDone)
            {
                ConsumeWsp(content);
                if (!_instructionParsers.TryGetValue(content.Current, out ParseInstruction? instructionParser))
                {
                    // Stop path when failing to parse
                    return;
                }
                _ = content.Next();
                ConsumeWsp(content);
                if (!instructionParser.Invoke(content))
                {
                    return;
                }
            }
            _parser.Shapes.Add(new Shape(_parser.PathCache, _parser.State));
        }

        public void Parse(XmlElement element)
        {
            ISvgElementParser.ParseAttr(ParseD, element.GetAttribute(SvgAttributes.D));
        }

        private static Vector2D<float>[]? ParseCoordinatePairDouble(StringSource source)
        {
            var pairs = new Vector2D<float>[2];
            for (int i = 0; i < 2; i++)
            {
                Vector2D<float>? pair = ParseCoordinatePair(source);
                if (!pair.HasValue)
                {
                    return null;
                }
                pairs[i] = pair.Value;

                if (i != 1)
                {
                    if (!ConsumeCommaWsp(source))
                    {
                        return null;
                    }
                }
            }
            return pairs;
        }

        private static Vector2D<float>[]? ParseCoordinatePairTriplet(StringSource source)
        {
            var pairs = new Vector2D<float>[3];
            for (int i = 0; i < 3; i++)
            {
                Vector2D<float>? pair = ParseCoordinatePair(source);
                if (!pair.HasValue)
                {
                    return null;
                }
                pairs[i] = pair.Value;

                if (i != 2)
                {
                    if (!ConsumeCommaWsp(source))
                    {
                        return null;
                    }
                }
            }
            return pairs;
        }

        private static Vector2D<float>? ParseCoordinatePair(StringSource source)
        {
            float? first = ParseCoordinate(source);
            if (!first.HasValue)
            {
                return null;
            }
            if (!ConsumeCommaWsp(source))
            {
                return null;
            }
            float? second = ParseCoordinate(source);
            if (!second.HasValue)
            {
                return null;
            }
            return new(first.Value, second.Value);
        }

        private static float? ParseCoordinate(StringSource source)
        {
            int sign = 1;
            if (source.Current == Symbols.PLUS)
            {
                sign = 1;
                source.Next();
            }
            else if (source.Current == Symbols.MINUS)
            {
                sign = -1;
                source.Next();
            }

            int? num = ParseInteger(source);
            if (num != null)
            {
                return sign * num;
            }
            else
            {
                return null;
            }
        }

        private static int? ParseInteger(StringSource source)
        {
            var buffer = new StringBuilder();
            while (source.Current.IsDigit())
            {
                buffer.Append(source.Current);
                source.Next();
            }
            if (buffer.Length == 0)
            {
                return null;
            }
            return int.Parse(buffer.ToString(), NumberStyles.None, CultureInfo.InvariantCulture);
        }

        private static bool? ParseFlag(StringSource source)
        {
            if (source.Current == '0')
            {
                return false;
            }
            else if (source.Current == '1')
            {
                return true;
            }
            else
            {
                return null;
            }
        }

        private static bool ConsumeCommaWsp(StringSource source)
        {
            if (source.Current.IsSpaceCharacter())
            {
                _ = ConsumeWsp(source);
                if (source.Current == Symbols.COMMA)
                {
                    _ = ConsumeCommaWsp(source);
                }
                return true;
            }
            else if (source.Current == Symbols.COMMA)
            {
                _ = source.Next();
                _ = ConsumeWsp(source);
                return true;
            }
            return false;
        }

        private static bool ConsumeWsp(StringSource source)
        {
            bool seen = false;
            while (source.Current.IsSpaceCharacter())
            {
                _ = source.Next();
                seen = true;
            }
            return seen;
        }

    }
}
