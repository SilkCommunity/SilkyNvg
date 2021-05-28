namespace SilkyNvg
{
    public struct FrameMeta
    {

        public uint TextTriCount { get; private set; }

        public uint StrokeTriCount { get; private set; }

        public uint FillTriCount { get; private set; }

        public uint DrawCallCount { get; private set; }

        internal void Update(uint dTextTriCount, uint dStrokeTriCount, uint dFilLTriCount, uint dDrawCallCount)
        {
            TextTriCount += dTextTriCount;
            StrokeTriCount += dStrokeTriCount;
            FillTriCount += dFilLTriCount;
            DrawCallCount += dDrawCallCount;
        }

    }
}