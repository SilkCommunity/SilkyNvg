namespace FontStash.NET
{
    internal class FonsState
    {

        public int font;
        public int align;
        public float size;
        public uint colour;
        public float blur;
        public float spacing;

        public FonsState Copy()
        {
            return (FonsState)MemberwiseClone();
        }

    }
}
