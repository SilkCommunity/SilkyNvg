namespace SilkyNvg.OpenGL
{
    internal struct Path
    {

        private int _fillOffset;
        private int _fillCount;
        private int _strokeOffset;
        private int _strokeCount;

        public int FillOffset
        {
            get => _fillOffset;
            set => _fillOffset = value;
        }

        public int FillCount
        {
            get => _fillCount;
            set => _fillCount = value;
        }

        public int StrokeOffset
        {
            get => _strokeOffset;
            set => _strokeOffset = value;
        }

        public int StrokeCount
        {
            get => _strokeCount;
            set => _strokeCount = value;
        }

        public Path(int fillOffset, int fillCount, int strokeOffset, int strokeCount)
        {
            _fillOffset = fillOffset;
            _fillCount = fillCount;
            _strokeOffset = strokeOffset;
            _strokeCount = strokeCount;
        }

    }
}
