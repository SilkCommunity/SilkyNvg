namespace FontStash.NET
{
    public struct FonsTextIter
    {

        public float x, y, nextx, nexty, scale, spacing;
        public uint codepoint;
        public short isize, iblur;
        public FonsFont font;
        public int prevGlyphIndex;
        public string str;
        public string next;
        public string end;
        public uint utf8state;
        public FonsGlyphBitmap bitmapOption;

    }
}
