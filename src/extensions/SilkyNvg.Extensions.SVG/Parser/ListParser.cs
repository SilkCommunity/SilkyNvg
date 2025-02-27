using SilkyNvg.Extensions.Svg.Parser.Utils;
using System.Text;

namespace SilkyNvg.Extensions.Svg.Parser
{
    internal static class ListParser
    {

        private static bool IsDelimiter(char c)
        {
            return c.IsSpaceCharacter() || (c == Symbols.COMMA);
        }

        private static void SkipWhiteSpace(StringSource source)
        {
            while (IsDelimiter(source.Current)) source.Next();
        }

        private static StringSource ReadContent(StringSource source)
        {
            var buffer = new StringBuilder();
            while (!IsDelimiter(source.Current))
            {
                buffer.Append(source.Current);
                source.Next();
            }
            return new StringSource(buffer.ToString());
        }

        internal static IReadOnlyList<StringSource> ParseList(this StringSource source)
        {
            var list = new List<StringSource>();
            while (!source.IsDone)
            {
                SkipWhiteSpace(source);
                list.Add(ReadContent(source));
            }
            return list.AsReadOnly();
        }

    }
}
