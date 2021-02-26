namespace SilkyNvg
{
    public enum BlendFactor
    {

        Zero = 1 << 0,
        One = 1 << 1,
        SrcColour = 1 << 2,
        OneMinusSrcColour = 1 << 3,
        DstColour = 1 << 4,
        OneMinusDstColour = 1 << 5,
        SrcAlpha = 1 << 6,
        OneMinusSrcAlpha = 1 << 7,
        DstAlpha = 1 << 8,
        OneMinusDstAlpha = 1 << 9,
        SrcAlphaSaturate = 1 << 10

    }
}
