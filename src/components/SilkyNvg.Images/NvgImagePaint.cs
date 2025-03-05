using SilkyNvg.Common.Geometry;
using System.Numerics;

namespace SilkyNvg.Images
{
    /// <inheritdoc cref="Paint"/>
    public static class NvgImagePaint
    {

        /// <inheritdoc cref="Paint.ImagePattern(RectF, float, int, float)"/>
        public static Paint ImagePattern(this Nvg _, RectF bounds, float angle, int image, float alpha)
            => Paint.ImagePattern(bounds, angle, image, alpha);

        /// <inheritdoc cref="Paint.ImagePattern(Vector2, Vector2, float, int, float)"/>
        public static Paint ImagePattern(this Nvg _, Vector2 pos, Vector2 size, float angle, int image, float alpha)
            => Paint.ImagePattern(pos, size, angle, image, alpha);

        /// <inheritdoc cref="Paint.ImagePattern(float, float, float, float, float, int, float)"/>
        public static Paint ImagePattern(this Nvg _, float ox, float oy, float width, float height, float angle, int image, float alpha)
            => Paint.ImagePattern(ox, oy, width, height, angle, image, alpha);

    }
}
