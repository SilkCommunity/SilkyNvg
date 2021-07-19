using Silk.NET.Maths;

namespace SilkyNvg.Images
{
    public static class NvgImagePaint
    {

        public static Paint ImagePattern(this Nvg _, Rectangle<float> bounds, float angle, int image, float alpha)
            => Paint.ImagePattern(bounds, angle, image, alpha);

        public static Paint ImagePattern(this Nvg _, float x, float y, float width, float height, float angle, int image, float alpha)
            => Paint.ImagePattern(x, y, width, height, angle, image, alpha);

    }
}
