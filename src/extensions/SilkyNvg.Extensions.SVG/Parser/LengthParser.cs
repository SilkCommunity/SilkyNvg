using SilkyNvg.Extensions.Svg.Parser.Constants;
using SilkyNvg.Extensions.Svg.Parser.DataTypes;
using SilkyNvg.Extensions.Svg.Parser.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SilkyNvg.Extensions.Svg.Parser
{
    internal static class LengthParser
    {

        private static Length? GetLength((string, LengthType)? value)
        {
            if (value.HasValue &&
                float.TryParse(value.Value.Item1, NumberStyles.Float, CultureInfo.InvariantCulture, out float result))
            {
                LengthType unit = value.Value.Item2;
                return new Length(result, unit);
            }
            return null;
        }

        private static bool IsDimension(StringSource source, char current)
            => (current != 'e') && (current != 'E') && (current.IsNameStart() || source.IsValidEscape());

        private static (string, LengthType) Dimension(StringSource source, string number, StringBuilder buffer)
        {
            char current = source.Current;

            while (true)
            {
                if (current.IsLetter())
                {
                    buffer.Append(current);
                }
                else if (source.IsValidEscape())
                {
                    source.Next();
                    buffer.Append(source.ConsumeEscape());
                }
                else
                {
                    bool success = Enum.TryParse(buffer.ToString(), true, out LengthType result);
                    return (number, success ? result : LengthType.Unknown);
                }

                current = source.Next();
            }
        }

        private static (string, LengthType) SciNotation(StringSource source, StringBuilder buffer)
        {
            while (true)
            {
                char current = source.Next();

                if (current.IsDigit())
                {
                    buffer.Append(current);
                }
                else if (current.IsNameStart() || source.IsValidEscape())
                {
                    string number = buffer.ToString();
                    return Dimension(source, number, buffer.Clear());
                }
                else
                {
                    return (buffer.ToString(), LengthType.Px); // default to user units (px)
                }
            }
        }

        private static (string, LengthType) NumberDash(StringSource source, StringBuilder buffer)
        {
            char current = source.Next();

            if (current.IsNameStart() || source.IsValidEscape())
            {
                string number = buffer.ToString();
                return Dimension(source, number, buffer.Clear());
            }
            else
            {
                source.Back();
                return (buffer.ToString(), LengthType.Px); // default to user units (px)
            }
        }

        private static (string, LengthType) NumberExponential(StringSource source, StringBuilder buffer)
        {
            char letter = source.Current;
            char current = source.Next();

            if (current.IsDigit())
            {
                buffer.Append(letter).Append(current);
                return SciNotation(source, buffer);
            }
            else if ((current == Symbols.PLUS) || (current == Symbols.MINUS))
            {
                char op = current;
                current = source.Next();

                if (current.IsDigit())
                {
                    buffer.Append(letter).Append(op).Append(current);
                    return SciNotation(source, buffer);
                }

                source.Back();
            }

            string number = buffer.ToString();
            return Dimension(source, number, buffer.Clear().Append(letter));
        }

        private static (string, LengthType) UnitRest(StringSource source, StringBuilder buffer)
        {
            char current = source.Next();

            while (true)
            {
                if (current.IsDigit())
                {
                    buffer.Append(current);
                }
                else if (IsDimension(source, current))
                {
                    string number = buffer.ToString();
                    return Dimension(source, number, buffer.Clear());
                }
                else
                {
                    break;
                }

                current = source.Next();
            }

            switch (current)
            {
                case Symbols.DOT:
                    current = source.Next();
                    if (current.IsDigit())
                    {
                        buffer.Append(Symbols.DOT).Append(current);
                        return UnitFraction(source, buffer);
                    }
                    return (buffer.ToString(), LengthType.Px); // default to user units (px)
                case '%':
                    source.Next();
                    return (buffer.ToString(), LengthType.Percentage);
                case 'e':
                case 'E':
                    return NumberExponential(source, buffer);
                case Symbols.MINUS:
                    return NumberDash(source, buffer);
                default:
                    return (buffer.ToString(), LengthType.Px); // default to user units (px)
            }
        }

        private static (string, LengthType) UnitFraction(StringSource source, StringBuilder buffer)
        {
            char current = source.Next();

            while (true)
            {
                if (current.IsDigit())
                {
                    buffer.Append(current);
                }
                else if (IsDimension(source, current))
                {
                    string number = buffer.ToString();
                    return Dimension(source, number, buffer.Clear());
                }
                else
                {
                    break;
                }

                current = source.Next();
            }

            switch (current)
            {
                case 'e':
                case 'E':
                    return NumberExponential(source, buffer);
                case '%':
                    source.Next();
                    return (buffer.ToString(), LengthType.Percentage);
                case Symbols.MINUS:
                    return NumberDash(source, buffer);
                default:
                    return (buffer.ToString(), LengthType.Px); // default to user units (px)
            }
        }

        private static (string, LengthType)? UnitStart(StringSource source)
        {
            char current = source.Current;

            if (current is Symbols.PLUS or Symbols.MINUS)
            {
                char next = source.Next();

                if (next == Symbols.DOT)
                {
                    var buffer = new StringBuilder().Append(current).Append(next);
                    return UnitFraction(source, buffer);
                }
                else if (next.IsDigit())
                {
                    var buffer = new StringBuilder().Append(current).Append(next);
                    return UnitRest(source, buffer);
                }
            }
            else if (current == Symbols.DOT)
            {
                return UnitFraction(source, new StringBuilder().Append(current));
            }
            else if (current.IsDigit())
            {
                return UnitRest(source, new StringBuilder().Append(current));
            }
            return null;
        }

        internal static (string, LengthType)? ParseUnit(this StringSource source)
        {
            int pos = source.Index;
            (string, LengthType)? result = UnitStart(source);

            if (!result.HasValue)
            {
                source.BackTo(pos);
            }

            return result;
        }

        internal static Length? ParseAutoLength(this StringSource source)
        {
            if (source.IsIdentifier(CssKeywords.Auto))
            {
                return Length.Auto;
            }
            return null;
        }

        internal static Length? ParseNormalLength(this StringSource source)
        {
            if (source.IsIdentifier(CssKeywords.Normal))
            {
                return Length.Normal;
            }
            return null;
        }

        internal static Length ParseLength(this StringSource source)
        {
            (string, LengthType)? value = source.ParseUnit();
            return GetLength(value) ?? ParseAutoLength(source) ??
                ParseNormalLength(source) ?? throw new InvalidOperationException("Failed to parse length");
        }

    }
}
