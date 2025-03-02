using SilkyNvg.Graphics;

namespace SilkyNvg.Extensions.Svg.Parser.Constants
{
    internal static class LineCaps
    {

        internal static readonly Dictionary<string, LineCap> Caps = new()
        {
            ["butt"] = LineCap.Butt,
            ["round"] = LineCap.Round,
            ["square"] = LineCap.Square
        };

        internal static readonly Dictionary<string, LineCap> Joins = new()
        {
            ["miter"] = LineCap.Miter,
            ["bevel"] = LineCap.Bevel,
            ["round"] = LineCap.Round
        };

    }
}
