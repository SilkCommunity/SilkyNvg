namespace SilkyNvg.Text
{
    public struct GlyphPosition
    {

        public string Str { get; }

        public float X { get; }

        public float MinX { get; }

        public float MaxX { get; }

        internal GlyphPosition(string str, float x, float minX, float maxX)
        {
            Str = str;
            X = x;
            MinX = minX;
            MaxX = maxX;
        }

    }
}
