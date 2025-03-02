using SilkyNvg.Extensions.Svg.Parser;
using System.Xml;

namespace SilkyNvg.Extensions.Svg;

public static class NvgSvgEXT
{

    public static SvgImage CreateSvg(this Nvg nvg, string xmlCode)
    {
        var doc = new XmlDocument();
        doc.LoadXml(xmlCode);

        var parser = new SvgParser(doc);
        return parser.Parse();
    }

    public static SvgImage? CreateSvgFromFile(this Nvg nvg, string filename)
    {
        string xmlCode;
        try
        {
            xmlCode = File.ReadAllText(filename);
        }
        catch
        {
            return null;
        }
        return CreateSvg(nvg, xmlCode);
    }

    public static void DrawSvgImage(this Nvg nvg, SvgImage image)
    {
        foreach (Shape shape in image.Shapes)
        {
            shape.Draw(nvg);
        }
    }

}
