namespace SilkyNvg.Extensions.Svg
{
    public readonly struct SvgImage
    {

        internal readonly Shape[] Shapes;

        public readonly float Width;

        public readonly float Height;

        internal SvgImage(Shape[] shapes, float width, float height)
        {
            Shapes = shapes;
            Width = width;
            Height = height;
        }

    }
}
