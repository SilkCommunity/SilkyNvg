using AngleSharp.Text;
using System.Text;
using SilkyNvg.Extensions.Svg.Parser.Utils;
using AngleSharp.Css.Parser;

namespace SilkyNvg.Extensions.Svg.Parser
{
    internal static class UnitParserExtensions
    {

        internal static SvgValueUnit? ParseUnit(this StringSource source)
        {
            var pos = source.Index;
            var result = UnitStart(source);

            if (result == null)
            {
                source.BackTo(pos);
            }

            return result;
        }

        private static SvgValueUnit? UnitStart(StringSource source)
        {
            var current = source.Current;

            if (current is Symbols.Plus or Symbols.Minus)
            {
                var next = source.Next();

                if (next == Symbols.Dot)
                {
                    var buffer = StringBuilderPool.Obtain().Append(current).Append(next);
                    return UnitFraction(source, buffer);
                }
                else if (next.IsDigit())
                {
                    var buffer = StringBuilderPool.Obtain().Append(current).Append(next);
                    return UnitRest(source, buffer);
                }
            }
            else if (current == Symbols.Dot)
            {
                return UnitFraction(source, StringBuilderPool.Obtain().Append(current));
            }
            else if (current.IsDigit())
            {
                return UnitRest(source, StringBuilderPool.Obtain().Append(current));
            }

            return null;
        }

        private static bool IsDimension(StringSource source, char current) =>
            current != 'e' && current != 'E' && (current.IsNameStart() || source.IsValidEscape());

        private static SvgValueUnit? UnitRest(StringSource source, StringBuilder buffer)
        {
            var current = source.Next();

            while (true)
            {
                if (current.IsDigit())
                {
                    buffer.Append(current);
                }
                else if (IsDimension(source, current))
                {
                    var number = buffer.ToString();
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
                case Symbols.Dot:
                    current = source.Next();

                    if (current.IsDigit())
                    {
                        buffer.Append(Symbols.Dot).Append(current);
                        return UnitFraction(source, buffer);
                    }

                    return new SvgValueUnit(buffer.ToPool(), string.Empty);

                case '%':
                    source.Next();
                    return new SvgValueUnit(buffer.ToPool(), "%");

                case 'e':
                case 'E':
                    return NumberExponential(source, buffer);

                case Symbols.Minus:
                    return NumberDash(source, buffer);

                default:
                    return new SvgValueUnit(buffer.ToPool(), string.Empty);
            }
        }

        private static SvgValueUnit? UnitFraction(StringSource source, StringBuilder buffer)
        {
            var current = source.Next();

            while (true)
            {
                if (current.IsDigit())
                {
                    buffer.Append(current);
                }
                else if (IsDimension(source, current))
                {
                    var number = buffer.ToString();
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
                    return new SvgValueUnit(buffer.ToPool(), "%");

                case Symbols.Minus:
                    return NumberDash(source, buffer);

                default:
                    return new SvgValueUnit(buffer.ToPool(), string.Empty);
            }
        }

        private static SvgValueUnit Dimension(StringSource source, string number, StringBuilder buffer)
        {
            var current = source.Current;

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
                    return new SvgValueUnit(number, buffer.ToPool());
                }

                current = source.Next();
            }
        }

        private static SvgValueUnit SciNotation(StringSource source, StringBuilder buffer)
        {
            while (true)
            {
                var current = source.Next();

                if (current.IsDigit())
                {
                    buffer.Append(current);
                }
                else if (current.IsNameStart() || source.IsValidEscape())
                {
                    var number = buffer.ToString();
                    return Dimension(source, number, buffer.Clear());
                }
                else
                {
                    return new SvgValueUnit(buffer.ToPool(), string.Empty);
                }
            }
        }

        private static SvgValueUnit NumberDash(StringSource source, StringBuilder buffer)
        {
            var current = source.Next();

            if (current.IsNameStart() || source.IsValidEscape())
            {
                var number = buffer.ToString();
                return Dimension(source, number, buffer.Clear().Append(Symbols.Minus));
            }
            else
            {
                source.Back();
                return new SvgValueUnit(buffer.ToPool(), string.Empty);
            }
        }

        private static SvgValueUnit NumberExponential(StringSource source, StringBuilder buffer)
        {
            var letter = source.Current;
            var current = source.Next();

            if (current.IsDigit())
            {
                buffer.Append(letter).Append(current);
                return SciNotation(source, buffer);
            }
            else if (current == Symbols.Plus || current == Symbols.Minus)
            {
                var op = current;
                current = source.Next();

                if (current.IsDigit())
                {
                    buffer.Append(letter).Append(op).Append(current);
                    return SciNotation(source, buffer);
                }

                source.Back();
            }

            var number = buffer.ToString();
            return Dimension(source, number, buffer.Clear().Append(letter));
        }

    }
}
