namespace SilkyNvg.Rendering.Vulkan
{
    internal struct Path
    {

        public int FillOffset { get; }

        public uint FillCount { get; }

        public int StrokeOffset { get; }

        public uint StrokeCount { get; }

        public Path(int fillOffset, int fillCount, int strokeOffset, int strokeCount)
        {
            FillOffset = fillOffset;
            FillCount = (uint)fillCount;
            StrokeOffset = strokeOffset;
            StrokeCount = (uint)strokeCount;
        }

    }
}
