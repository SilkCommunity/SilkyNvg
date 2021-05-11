namespace SilkyNvg
{
    public struct FrameMeta
    {

        private uint drawCallCount;
        private uint fillTriCount;
        private uint strokeTriCount;
        private uint textTriCount;

        public uint TextTriCount { get => textTriCount; internal set => textTriCount = value; }

        public uint StrokeTriCount { get => strokeTriCount; internal set => strokeTriCount = value; }

        public uint FillTriCount { get => fillTriCount; internal set => fillTriCount = value; }

        public uint DrawCallCount { get => drawCallCount; internal set => drawCallCount = value; }

    }
}
