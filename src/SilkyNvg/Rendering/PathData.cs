namespace SilkyNvg.Rendering
{
    public readonly struct PathData
    {

        public readonly int SubpathIndex;
        public readonly int SubpathNumber;

        public readonly FillRule FillRule;

        internal PathData(int subpathIndex, int subpathNumber, FillRule fillRule)
        {
            SubpathIndex = subpathIndex;
            SubpathNumber = subpathNumber;
            FillRule = fillRule;
        }

    }
}