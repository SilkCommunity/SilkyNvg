using SilkyNvg.Extensions.Svg.Parser.Utils;
using System.Text;

namespace SilkyNvg.Extensions.Svg.Parser
{
    internal static class IdentParser
    {

        private static string Rest(StringSource source, char current, StringBuilder buffer)
        {
            while (true)
            {
                if (current.IsName())
                {
                    buffer.Append(current);
                }
                else if (source.IsValidEscape())
                {
                    buffer.Append(source.ConsumeEscape());
                }
                else
                {
                    return buffer.ToString();
                }

                current = source.Next();
            }
        }

        private static string? Start(StringSource source, char current, StringBuilder buffer)
        {
            if (current == Symbols.MINUS)
            {
                current = source.Next();

                if (current.IsNameStart() || source.IsValidEscape())
                {
                    buffer.Append(Symbols.MINUS);
                    return Rest(source, current, buffer);
                }

                source.Back();
            }
            else if (current.IsNameStart())
            {
                buffer.Append(current);
                return Rest(source, source.Next(), buffer);
            }
            else if ((current == Symbols.BACKSLASH) && source.IsValidEscape())
            {
                buffer.Append(source.ConsumeEscape());
                return Rest(source, source.Next(), buffer);
            }

            buffer.ToString();
            return null;
        }

        internal static string? ParseIdent(this StringSource source)
        {
            char current = source.Current;
            var buffer = new StringBuilder();
            return Start(source, current, buffer);
        }

        internal static bool IsIdentifier(this StringSource source, string identifier)
        {
            int pos = source.Index;
            string? ident = source.ParseIdent();

            if ((ident != null) && ident.Equals(identifier, StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            source.BackTo(pos);
            return false;
        }

    }
}
