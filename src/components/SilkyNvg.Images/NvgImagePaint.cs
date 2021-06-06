using Silk.NET.Maths;

namespace SilkyNvg.Images
{
    public static class NvgImagePaint
    {

        public static Paint ImagePattern(this Nvg _, Vector2D<float> pos, Vector2D<float> size, float angle, int image, float alpha)
            => Paint.ImagePattern(pos, size, angle, image, alpha);

        public static Paint ImagePattern(this Nvg _, float x, float y, float width, float height, float angle, int image, float alpha)
            => Paint.ImagePattern(new(x, y), new(width, height), angle, image, alpha);

    }
}
