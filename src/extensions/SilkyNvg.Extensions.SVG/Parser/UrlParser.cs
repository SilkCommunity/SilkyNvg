using SilkyNvg.Extensions.Svg.Parser.Constants;
using SilkyNvg.Extensions.Svg.Parser.Utils;
using System.Text;

namespace SilkyNvg.Extensions.Svg.Parser
{
    internal static class UrlParser
    {

        internal static string? ParseUrl(this StringSource source)
        {
            if (source.IsFunction(CssFunctions.Url))
            {
                char current = source.Next();
                source.ConsumeWsp();

                return current switch
                {
                    Symbols.DOUBLE_QUOTE => DoubleQuoted(source),
                    Symbols.SINGLE_QUOTE => SingleQuoted(source),
                    Symbols.CLOSE_PARENS => string.Empty,
                    Symbols.EOF => string.Empty,
                    _ => Unquoted(source)
                };
            }

            return null;
        }

        private static string? DoubleQuoted(StringSource source)
        {
            var buffer = new StringBuilder();

            while (true)
            {
                char current = source.Next();

                if (current.IsLineBreak())
                {
                    return null;
                }
                else if (current == Symbols.EOF)
                {
                    return buffer.ToString();
                }
                else if (current == Symbols.DOUBLE_QUOTE)
                {
                    return End(source, buffer);
                }
                else
                {
                    buffer.Append(current);
                }
            }
        }

        private static string? SingleQuoted(StringSource source)
        {
            var buffer = new StringBuilder();

            while (true)
            {
                char current = source.Next();

                if (current.IsLineBreak())
                {
                    return null;
                }
                else if (current == Symbols.EOF)
                {
                    return buffer.ToString();
                }
                else if (current == Symbols.SINGLE_QUOTE)
                {
                    return End(source, buffer);
                }
                else
                {
                    buffer.Append(current);
                }
            }
        }

        private static string? Unquoted(StringSource source)
        {
            var buffer = new StringBuilder();

            while (true)
            {
                char current = source.Next();

                if (current.IsSpaceCharacter())
                {
                    return End(source, buffer);
                }
                else if ((current == Symbols.CLOSE_PARENS) | (current == Symbols.EOF))
                {
                    source.Next();
                    return buffer.ToString();
                }
                else if (current is Symbols.DOUBLE_QUOTE or Symbols.SINGLE_QUOTE or Symbols.OPEN_PARENS || current.IsNonPrintable())
                {
                    return null;
                }
                else
                {
                    buffer.Append(current);
                }
            }
        }

        private static string? End(StringSource source, StringBuilder buffer)
        {
            char current = source.Next();
            source.ConsumeWsp();

            if (current == Symbols.CLOSE_PARENS)
            {
                source.Next();
                return buffer.ToString();
            }

            return null;
        }

    }
}
