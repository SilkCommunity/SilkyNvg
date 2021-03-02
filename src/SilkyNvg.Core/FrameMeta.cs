namespace SilkyNvg.Core
{
    public class FrameMeta
    {

        private int _drawCallCount;
        private int _fillTriCount;
        private int _strokeTriCount;
        private int _textTriCount;

        public int DrawCallCount
        {
            get => _drawCallCount;
            set => _drawCallCount = value;
        }

        public int FillTriCount
        {
            get => _fillTriCount;
            set => _fillTriCount = value;
        }

        public int StrokeTriCount
        {
            get => _strokeTriCount;
            set => _strokeTriCount = value;
        }

        public int TextTriCount
        {
            get => _textTriCount;
            set => _textTriCount = value;
        }

        public void Reset()
        {
            _drawCallCount = 0;
            _fillTriCount = 0;
            _strokeTriCount = 0;
            _textTriCount = 0;
        }

    }
}
