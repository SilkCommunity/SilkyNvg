using SilkyNvg.Extensions.Svg.Parser.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SilkyNvg.Extensions.Svg.Parser
{
    internal static class ColourParser
    {

        private static byte? ParseRgbComponent(StringSource source)
        {
            source.ParseUnit();
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
