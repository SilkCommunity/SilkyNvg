using SilkyNvg.Extensions.Svg.Parser;
using SilkyNvg.Graphics;
using SilkyNvg.Paths;
using SilkyNvg.Transforms;
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

    public static void DrawSvgImage(this Nvg nvg, float x, float y, float width, float height, SvgImage image)
    {
        float scaleX = width / image.Width;
        float scaleY = height / image.Height;

        nvg.Save();

        nvg.Translate(x, y);
        nvg.Scale(scaleX, scaleY);

        foreach (Shape shape in image.Shapes)
        {
            shape.Draw(nvg);
        }

        nvg.Restore();
    }

}
