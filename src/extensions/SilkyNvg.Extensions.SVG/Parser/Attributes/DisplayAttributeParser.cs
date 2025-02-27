using SilkyNvg.Extensions.Svg.Parser.Constants;
using SilkyNvg.Extensions.Svg.Parser.Utils;

namespace SilkyNvg.Extensions.Svg.Parser.Attributes
{
    internal class DisplayAttributeParser : IAttributeParser
    {

        public void Parse(StringSource content, ref AttribState state)
        {
            if (content.Content == CssKeywords.None)
            {
                state.IsVisible = false;
            }
        }

    }
}
