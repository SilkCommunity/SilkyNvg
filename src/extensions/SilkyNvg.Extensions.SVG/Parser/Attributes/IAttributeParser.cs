using SilkyNvg.Extensions.Svg.Parser.Utils;

namespace SilkyNvg.Extensions.Svg.Parser.Attributes
{
    internal interface IAttributeParser
    {

        void Parse(StringSource content, ref AttribState state);

    }
}
