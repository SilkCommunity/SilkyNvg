namespace SilkyNvg.Core
{
    public struct FrameMeta
    {

        public int DrawCallCount;
        public int FillTriCount;
        public int StrokeTriCount;
        public int TextTriCount;

        public void Reset()
        {
            DrawCallCount = 0;
            FillTriCount = 0;
            StrokeTriCount = 0;
            TextTriCount = 0;
        }

    }
}
