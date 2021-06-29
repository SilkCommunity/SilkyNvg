namespace SilkyNvg.Text
{
    public struct TextRow
    {

        public string Start { get; internal set; }

        public string End { get; internal set; }

        public string Next { get; internal set; }

        public float Width { get; internal set; }

        public float MinX { get; internal set; }

        public float MaxX { get; internal set; }

    }
}
