using Silk.NET.Maths;
using SilkyNvg.Common;
using System;

namespace SilkyNvg
{
    public class Paint
    {

        private Colour _innerColour;
        private Colour _outerColour;

        public Matrix3X2<float> Transform { get; private set; }

        public Vector2D<float> Extent { get; }

        public float Radius { get; }

        public float Feather { get; }

        public ref Colour InnerColour { get => ref _innerColour; }

        public ref Colour OuterColour { get => ref _outerColour; }

        public int Image { get; }

        public Paint(Matrix3X2<float> transform, Vector2D<float> extent, float radius, float feather, Colour innerColour, Colour outerColour)
        {
            Transform = transform;
            Extent = extent;
            Radius = radius;
            Feather = feather;
            InnerColour = innerColour;
            OuterColour = outerColour;
        }
        public Paint(Matrix3X2<float> transform, Vector2D<float> extent, float radius, float feather, Colour innerColour, Colour outerColour, int image)
            : this(transform, extent, radius, feather, innerColour, outerColour)
        {
            Image = image;
        }

        internal Paint(Colour colour)
        {
            Transform = Matrix3X2<float>.Identity;
            Radius = 0;
            Feather = 1;
            InnerColour = colour;
            OuterColour = colour;
        }

        internal Paint(Matrix3X2<float> transform, Vector2D<float> extent, int image, Colour innerColour, Colour outerColour)
        {
            Transform = transform;
            Extent = extent;
            Image = image;
            InnerColour = innerColour;
            OuterColour = outerColour;
        }

        internal void MultiplyTransform(Matrix3X2<float> globalTransform)
        {
            Transform = Maths.Multiply(Transform, globalTransform);
        }

        internal void PremultiplyAlpha(float alpha)
        {
            InnerColour.Premultiply(alpha);
            OuterColour.Premultiply(alpha);
        }

        internal static Paint ForText(int fontAtlas, Paint original)
        {
            return new Paint(original.Transform, original.Extent, original.Radius, original.Feather, original.InnerColour, original.OuterColour, fontAtlas);
        }

        public static Paint LinearGradient(float sx, float sy, float ex, float ey, Colour icol, Colour ocol)
        {
            const float large = 1e5f;

            float dx = ex - sx;
            float dy = ey - sy;
            float d = MathF.Sqrt(dx * dx + dy * dy);

            if (d > 0.0001f)
            {
                dx /= d;
                dy /= d;
            }
            else
            {
                dx = 0;
                dy = 1;
            }

            Matrix3X2<float> transform = new
            (
                dy, -dx,
                dx, dy,
                sx - dx * large, sy - dy * large
            );

            return new Paint(
                transform,
                new Vector2D<float>(large, large + d * 0.5f),
                0.0f, MathF.Max(1.0f, d),
                icol, ocol
            );
        }

        public static Paint LinearGradient(Vector2D<float> s, Vector2D<float> e, Colour icol, Colour ocol) => LinearGradient(s.X, s.Y, e.X, e.Y, icol, ocol);

        public static Paint RadialGradient(float cx, float cy, float inr, float outr, Colour icol, Colour ocol)
        {
            float r = (inr + outr) * 0.5f;
            float f = (outr - inr);

            Matrix3X2<float> transform = Matrix3X2<float>.Identity;
            transform.M31 = cx;
            transform.M32 = cy;

            return new Paint(
                transform,
                new Vector2D<float>(r),
                r, MathF.Max(1.0f, f),
                icol, ocol
            );
        }

        public static Paint RadialGradient(Vector2D<float> c, Vector2D<float> radii, Colour icol, Colour ocol) => RadialGradient(c.X, c.Y, radii.X, radii.Y, icol, ocol);

        public static Paint BoxGradient(float x, float y, float w, float h, float r, float f, Colour icol, Colour ocol)
        {
            Matrix3X2<float> transform = Matrix3X2<float>.Identity;
            transform.M31 = x + (w * 0.5f);
            transform.M32 = y + (h * 0.5f);

            return new Paint(
                transform,
                new Vector2D<float>(w * 0.5f, h * 0.5f),
                r, MathF.Max(1.0f, f),
                icol, ocol
            );
        }

        public static Paint BoxGradient(Vector2D<float> pos, Vector2D<float> size, float r, float f, Colour icol, Colour ocol) => BoxGradient(pos.X, pos.Y, size.X, size.Y, r, f, icol, ocol);

        public static Paint ImagePattern(float x, float y, float width, float height, float angle, int image, float alpha)
        {
            Matrix3X2<float> transform = Matrix3X2.CreateRotation(angle);
            transform.M31 = x;
            transform.M32 = y;

            return new Paint(
                transform,
                new Vector2D<float>(width, height),
                image,
                new Colour(1.0f, 1.0f, 1.0f, alpha),
                new Colour(1.0f, 1.0f, 1.0f, alpha)
            );
        }

        public static Paint ImagePattern(Vector2D<float> pos, Vector2D<float> size, float angle, int image, float alpha)
            => ImagePattern(pos.X, pos.Y, size.X, size.Y, angle, image, alpha);

    }
}
