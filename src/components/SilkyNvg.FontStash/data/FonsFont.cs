using System;

namespace FontStash.NET
{
    public class FonsFont
    {

        public FonsTtImpl font;
        public string name;
        public byte[] data;
        public int dataSize;
        public byte freeData;
        public float ascender;
        public float descender;
        public float lineh;
        public FonsGlyph[] glyphs;
        public int cglyphs;
        public int nglyphs;
        public int[] lut = new int[Fontstash.HASH_LUT_SIZE];
        public int[] fallbacks = new int[Fontstash.MAX_FALLBACKS];
        public int nfallbacks;

        public FonsFont()
        {
            font = new FonsTtImpl();
        }

        public FonsGlyph AllocGlyph()
        {
            if (nglyphs + 1 > cglyphs)
            {
                cglyphs = cglyphs == 0 ? 8 : cglyphs * 2;
                Array.Resize(ref glyphs, cglyphs);
            }
            FonsGlyph glyph = new();
            glyphs[nglyphs] = glyph;
            nglyphs++;
            return glyphs[nglyphs - 1];
        }

    }
}
