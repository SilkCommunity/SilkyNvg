using SilkyNvg.Extensions.Svg.Parser.Constants;
using SilkyNvg.Extensions.Svg.Parser.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SilkyNvg.Extensions.Svg.Parser
{
    internal static class ColourParser
    {

        private static byte? ParseRgbComponent(StringSource source)
        {
            Unit? unit = source.ParseUnit();

            if (!unit.HasValue)
            {
                return null;
            }

            if (string.IsNullOrEmpty(unit.Value.Dimension) && int.TryParse(unit.Value.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int num))
            {
                return (byte)Math.Max(Math.Min(num, 255), 0);
            }

            if ((unit.Value.Dimension == "%") && double.TryParse(unit.Value.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out double val))
            {
                return (byte)Math.Round((255.0 * val) / 100.0);
            }

            return null;
        }

        private static byte? ParseRgbOrNoneComponent(StringSource source)
        {
            int pos = source.Index;
            byte? value = ParseRgbComponent(source);

            if (value.HasValue)
            {
                return value;
            }

            source.BackTo(pos);

            if (source.IsIdentifier(CssKeywords.None))
            {
                return 0;
            }

            return null;
        }

        private static double? ParseDouble(StringSource source)
        {
            Unit? unit = source.ParseUnit();

            if (unit.HasValue &&
                double.TryParse(unit.Value.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out double val))
            {
                if (unit.Value.Dimension == Symbols.PERCENT.ToString())
                {
                    return val * 0.01;
                }
                else if (unit.Value.Dimension == string.Empty)
                {
                    return Math.Max(Math.Min(val, 1.0), 0.0);
                }
            }

            return null;
        }

        private static Colour? ParseRgba(StringSource source)
        {
            int pos = source.Current;
            return null;
        }

        private static Colour? Start(StringSource source)
        {
            if (source.Current != Symbols.NUM)
            {
                string? ident = source.ParseIdent();

                if (ident != null)
                {

                }

                return null;
            }
            return null;
        }

    }
}
