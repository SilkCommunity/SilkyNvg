using SilkyNvg.Extensions.Svg.Parser.Utils;
using System.Globalization;

namespace SilkyNvg.Extensions.Svg.Parser
{
    internal static class NumberParser
    {

        internal static float? ParseNumber(this StringSource source)
        {
            if (float.TryParse(source.Content, NumberStyles.Float, CultureInfo.InvariantCulture, out float number))
            {
                return number;
            }
            return null;
        }

        internal static float?[] ParseNumberList(this StringSource source)
        {
            IReadOnlyList<StringSource> potentialNumbers = source.ParseList();
            float?[] result = new float?[potentialNumbers.Count];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = ParseNumber(potentialNumbers[i]);
            }
            return result;
        }

    }
}
