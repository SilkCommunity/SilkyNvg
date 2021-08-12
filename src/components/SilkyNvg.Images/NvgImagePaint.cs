using Silk.NET.Maths;

namespace SilkyNvg.Images
{
    /// <inheritdoc cref="Paint"/>
    public static class NvgImagePaint
    {

        /// <inheritdoc cref="Paint.ImagePattern(Rectangle{float}, float, int, float)"/>
        public static Paint ImagePattern(this Nvg _, Rectangle<float> bounds, float angle, int image, float alpha)
            => Paint.ImagePattern(bounds, angle, image, alpha);

        /// <inheritdoc cref="Paint.ImagePattern(Vector2D{float}, Vector2D{float}, float, int, float)"/>
        public static Paint ImagePattern(this Nvg _, Vector2D<float> pos, Vector2D<float> size, float angle, int image, float alpha)
            => Paint.ImagePattern(pos, size, angle, image, alpha);

        /// <inheritdoc cref="Paint.ImagePattern(float, float, float, float, float, int, float)"/>
        public static Paint ImagePattern(this Nvg _, float ox, float oy, float width, float height, float angle, int image, float alpha)
            => Paint.ImagePattern(ox, oy, width, height, angle, image, alpha);

    }
}
