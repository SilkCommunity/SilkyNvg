using Silk.NET.Maths;
using SilkyNvg.Extensions.Svg.Parser.Utils;
using System.Globalization;
using System.Text;

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

        internal static string ReadDigitString(this StringSource source)
        {
            var builder = new StringBuilder();
            while (source.Current.IsDigit())
            {
                builder.Append(source.Current);
                source.Next();
            }
            return builder.ToString();
        }

        internal static float? ParseCoordinate(this StringSource source)
        {
            float sign = 1.0f;
            if (source.Current == Symbols.PLUS)
            {
                source.Next();
                sign = 1.0f;
            }
            else if (source.Current == Symbols.MINUS)
            {
                source.Next();
                sign = -1.0f;
            }

            string num = ReadDigitString(source);
            if (source.Current == Symbols.DOT)
            {
                source.Next();
                num += "." + ReadDigitString(source);
            }

            if (float.TryParse(num, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out float value))
            {
                return value * sign;
            }
            return null;
        }

        internal static Vector2D<float>? ParseCoordinatePair(this StringSource source)
        {
            float? x = source.ParseCoordinate();
            if (!x.HasValue)
            {
                return null;
            }
            source.ConsumeCommaWsp();
            float? y = source.ParseCoordinate();
            if (!y.HasValue)
            {
                return null;
            }
            return new Vector2D<float>(x.Value, y.Value);
        }

        internal static Vector2D<float>[]? ParseCoordinatePairDouble(this StringSource source)
        {
            Vector2D<float>? pair1 = source.ParseCoordinatePair();
            if (!pair1.HasValue)
            {
                return null;
            }
            source.ConsumeCommaWsp();
            Vector2D<float>? pair2 = source.ParseCoordinatePair();
            if (!pair2.HasValue)
            {
                return null;
            }
            return [pair1.Value, pair2.Value];
        }

        internal static Vector2D<float>[]? ParseCoordinatePairTriplet(this StringSource source)
        {
            Vector2D<float>? pair1 = source.ParseCoordinatePair();
            if (!pair1.HasValue)
            {
                return null;
            }
            source.ConsumeCommaWsp();
            Vector2D<float>? pair2 = source.ParseCoordinatePair();
            if (!pair2.HasValue)
            {
                return null;
            }
            source.ConsumeCommaWsp();
            Vector2D<float>? pair3 = source.ParseCoordinatePair();
            if (!pair3.HasValue)
            {
                return null;
            }
            return [pair1.Value, pair2.Value, pair3.Value];
        }

        internal static IReadOnlyList<float>? ParseCoordinateSequence(this StringSource source)
        {
            float? coord = source.ParseCoordinate();
            if (!coord.HasValue)
            {
                return null;
            }
            List<float> sequence = [];
            int idx;
            do
            {
                source.ConsumeCommaWsp();
                idx = source.Index;
                sequence.Add(coord.Value);
                coord = source.ParseCoordinate();
            } while (coord.HasValue);
            source.BackTo(idx);
            return sequence.AsReadOnly();
        }

        internal static IReadOnlyList<Vector2D<float>>? ParseCoordinatePairSequence(this StringSource source)
        {
            Vector2D<float>? pair = source.ParseCoordinatePair();
            if (!pair.HasValue)
            {
                return null;
            }
            List<Vector2D<float>> sequence = [];
            int idx;
            do
            {
                source.ConsumeCommaWsp();
                idx = source.Index;
                sequence.Add(pair.Value);
                pair = source.ParseCoordinatePair();
            } while (pair.HasValue);
            source.BackTo(idx);
            return sequence.AsReadOnly();
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
