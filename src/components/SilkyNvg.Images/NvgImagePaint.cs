using System.Drawing;
using System.Numerics;

namespace SilkyNvg.Images
{
    /// <inheritdoc cref="Paint"/>
    public static class NvgImagePaint
    {

        /// <inheritdoc cref="Paint.ImagePattern(RectangleF, float, int, float)"/>
        public static Paint ImagePattern(this Nvg _, RectangleF bounds, float angle, int image, float alpha)
            => Paint.ImagePattern(bounds, angle, image, alpha);

        /// <inheritdoc cref="Paint.ImagePattern(RectangleF, float, int, float)"/>
        public static Paint ImagePattern(this Nvg _, Vector4 bounds, float angle, int image, float alpha)
            => Paint.ImagePattern((RectangleF)bounds, angle, image, alpha);

        /// <inheritdoc cref="Paint.ImagePattern(PointF, SizeF, float, int, float)"/>
        public static Paint ImagePattern(this Nvg _, PointF pos, SizeF size, float angle, int image, float alpha)
            => Paint.ImagePattern(pos, size, angle, image, alpha);

        /// <inheritdoc cref="Paint.ImagePattern(Vector2, Vector2, float, int, float)"/>
        public static Paint ImagePattern(this Nvg _, Vector2 pos, Vector2 size, float angle, int image, float alpha)
            => Paint.ImagePattern(pos, size, angle, image, alpha);

        /// <inheritdoc cref="Paint.ImagePattern(float, float, float, float, float, int, float)"/>
        public static Paint ImagePattern(this Nvg _, float ox, float oy, float width, float height, float angle, int image, float alpha)
            => Paint.ImagePattern(ox, oy, width, height, angle, image, alpha);

    }
}
