using SilkyNvg.Extensions.Svg.Parser;
using System.Xml;

namespace SilkyNvg.Extensions.Svg;

public static class NvgSvgEXT
{

    public static void CreateSvg(this Nvg nvg, string xmlCode)
    {
        var parser = new SvgParser(nvg);

        var doc = new XmlDocument();
        doc.LoadXml(xmlCode);
        parser.ParseSvgElement(doc.DocumentElement ?? throw new ArgumentException("Failed to parse XML code."));
    }

    public static void CreateSvgFromFile(this Nvg nvg, string filename)
    {
        string xmlCode;
        try
        {
            xmlCode = File.ReadAllText(filename);
        }
        catch
        {
            return;
        }
        CreateSvg(nvg, xmlCode);
    }

}
