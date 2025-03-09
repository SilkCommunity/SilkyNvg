namespace SilkyNvg.Text
{
    /// <summary>
    /// Represents a font and potential fallback fonts.
    /// </summary>
    public readonly struct Font
    {

        public static readonly Font None = new(null, -1);

        public readonly string Name;
        public readonly int Handle;

        internal Font(string name, int handle)
        {
            Name = name;
            Handle = handle;
        }

        public override int GetHashCode()
            => Handle;

    }
}
