namespace SilkyNvg.Rendering.Vulkan
{
    public struct StrokePath
    {

        public uint FillOffset { get; }

        public uint FillCount { get; }

        public uint StrokeOffset { get; }

        public uint StrokeCount { get; }

        public StrokePath(int fillOffset, int fillCount, int strokeOffset, int strokeCount)
        {
            FillOffset = (uint)fillOffset;
            FillCount = (uint)fillCount;
            StrokeOffset = (uint)strokeOffset;
            StrokeCount = (uint)strokeCount;
        }

    }
}