namespace SilkyNvg.Rendering.OpenGL.Legacy.Paths
{
    internal class Path
    {

        public uint FillOffset { get; }

        public uint FillCount { get; }

        public uint StrokeOffset { get; }

        public uint StrokeCount { get; }

        public bool HasStroke => StrokeCount > 0;

        public Path(Renderer.Path path, uint offset)
        {
            FillOffset = offset;
            FillCount = (uint)path.Fill.Count;
            StrokeOffset = FillCount + offset;
            StrokeCount = (uint)path.Stroke.Count;
        }

    }
}
