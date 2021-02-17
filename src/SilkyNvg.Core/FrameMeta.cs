namespace SilkyNvg.Core
{
    public struct FrameMeta
    {

        public int DrawCallCount { get; private set; }
        public int FillTriCount { get; private set; }
        public int StrokeTriCount { get; private set; }
        public int TextTriCount { get; private set; }

        public void Reset()
        {
            DrawCallCount = 0;
            FillTriCount = 0;
            StrokeTriCount = 0;
            TextTriCount = 0;
        }

    }
}
