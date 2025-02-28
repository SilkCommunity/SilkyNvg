using AngleSharp.Text;

namespace SilkyNvg.Extensions.Svg.Parser.Utils
{
    internal static class SvgStringSourceExtensions
    {

        internal static bool ConsumeWsp(this StringSource source)
        {
            int start = source.Index;
            source.SkipSpaces();
            return source.Index != start;
        }

        internal static bool ConsumeCommaWsp(this StringSource source)
        {
            int start = source.Index;
            source.SkipSpaces();
            if (source.Current == Symbols.Comma)
            {
                source.Next();
            }
            source.SkipSpaces();
            return source.Index != start;
        }

    }
}
