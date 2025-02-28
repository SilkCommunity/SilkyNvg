using SilkyNvg.Extensions.Svg.Parser.Constants;
using SilkyNvg.Extensions.Svg.Parser.DataTypes;
using SilkyNvg.Extensions.Svg.Parser.Utils;
using System.Globalization;
using System.Text;

namespace SilkyNvg.Extensions.Svg.Parser
{
    internal static class UnitParser
    {

        private static readonly Dictionary<string, LengthType> LengthUnitNames = new()
        {
            [Symbols.PERCENT.ToString()] = LengthType.Percentage,
            ["em"] = LengthType.EMs,
            ["ex"] = LengthType.EXs,
            ["px"] = LengthType.Px,
            ["cm"] = LengthType.Cm,
            ["mm"] = LengthType.Mm,
            ["in"] = LengthType.In,
            ["pt"] = LengthType.Pt,
            ["pc"] = LengthType.Pc
        };

        private static Length? GetLength(Unit? value)
        {
            if ((value != null) &&
                float.TryParse(value.Value.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out float result))
            {
                if (!LengthUnitNames.TryGetValue(value.Value.Dimension, out LengthType unit))
                {
                    if (value.Value.Dimension == string.Empty)
                    {
                        unit = LengthType.Number;
                    }
                    else
                    {
                        unit = LengthType.Unknown;
                    }
                }
                return new Length(result, unit);
            }
            return null;
        }

        private static bool IsDimension(StringSource source, char current)
            => (current != 'e') && (current != 'E') && (current.IsNameStart() || source.IsValidEscape());

        private static Unit? Dimension(StringSource source, string number, StringBuilder buffer)
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
                    return new Unit(number, buffer.ToString());
                }

                current = source.Next();
            }
        }

        private static Unit? SciNotation(StringSource source, StringBuilder buffer)
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
                    return new Unit(buffer.ToString(), string.Empty);
                }
            }
        }

        private static Unit? NumberDash(StringSource source, StringBuilder buffer)
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
                return new Unit(buffer.ToString(), string.Empty);
            }
        }

        private static Unit? NumberExponential(StringSource source, StringBuilder buffer)
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

        private static Unit? UnitRest(StringSource source, StringBuilder buffer)
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
                    return new Unit(buffer.ToString(), string.Empty); // default to user units (px)
                case '%':
                    source.Next();
                    return new Unit(buffer.ToString(), "%");
                case 'e':
                case 'E':
                    return NumberExponential(source, buffer);
                case Symbols.MINUS:
                    return NumberDash(source, buffer);
                default:
                    return new Unit(buffer.ToString(), string.Empty);
            }
        }

        private static Unit? UnitFraction(StringSource source, StringBuilder buffer)
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
                    return new Unit(buffer.ToString(), "%");
                case Symbols.MINUS:
                    return NumberDash(source, buffer);
                default:
                    return new Unit(buffer.ToString(), string.Empty);
            }
        }

        private static Unit? UnitStart(StringSource source)
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

        internal static Unit? ParseUnit(this StringSource source)
        {
            int pos = source.Index;
            Unit? result = UnitStart(source);

            if (result == null)
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

        internal static Length? ParseLength(this StringSource source)
        {
            Unit? value = source.ParseUnit();
            Length? length = GetLength(value);

            if (!length.HasValue)
            {
                return null;
            }

            return length.Value;
        }

    }
}
