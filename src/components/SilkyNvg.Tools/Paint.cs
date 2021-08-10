using Silk.NET.Maths;
using SilkyNvg.Common;
using System;

namespace SilkyNvg
{
    public struct Paint
    {

        public Matrix3X2<float> Transform { get; private set; }

        public Vector2D<float> Extent { get; }

        public float Radius { get; }

        public float Feather { get; }

        public Colour InnerColour { get; private set; }

        public Colour OuterColour { get; private set; }

        public int Image { get; }

        public Paint(Matrix3X2<float> transform, Vector2D<float> extent, float radius, float feather, Colour innerColour, Colour outerColour, int image)
        {
            Transform = transform;
            Extent = extent;
            Radius = radius;
            Feather = feather;
            InnerColour = innerColour;
            OuterColour = outerColour;
            Image = image;
        }

        internal Paint(Colour colour) : this(Matrix3X2<float>.Identity, default, default, default, colour, colour, default) { }

        internal void MultiplyTransform(Matrix3X2<float> globalTransform)
        {
            Transform = Maths.Multiply(Transform, globalTransform);
        }

        internal void PremultiplyAlpha(float alpha)
        {
            InnerColour = InnerColour.Premultiply(alpha);
            OuterColour = OuterColour.Premultiply(alpha);
        }

        internal static Paint ForText(int fontAtlas, Paint original)
        {
            return new Paint(original.Transform, original.Extent, original.Radius, original.Feather, original.InnerColour, original.OuterColour, fontAtlas);
        }

        public static Paint LinearGradient(Vector2D<float> s, Vector2D<float> e, Colour icol, Colour ocol)
        {
            const float large = 1e5f;

            Vector2D<float> delta = e - s;
            float d = MathF.Sqrt(delta.X * delta.X + delta.Y * delta.Y);

            if (d > 0.0001f)
            {
                delta /= d;
            }
            else
            {
                delta = new(0, 1);
            }

            Matrix3X2<float> transform = new
            (
                delta.Y, -delta.X,
                delta.X, delta.Y,
                s.X - delta.X * large, s.Y - delta.Y * large
            );

            Vector2D<float> extent = new(large, large + d * 0.5f);

            return new Paint(transform, extent, 0.0f, MathF.Max(1.0f, d), icol, ocol, default);
        }

        public static Paint LinearGradient(float sx, float sy, float ex, float ey, Colour icol, Colour ocol)
            => LinearGradient(new Vector2D<float>(sx, sy), new Vector2D<float>(ex, ey), icol, ocol);

        public static Paint RadialGradient(Vector2D<float> c, float inr, float outr, Colour icol, Colour ocol)
        {
            float r = (inr + outr) * 0.5f;
            float f = (outr - inr);

            Matrix3X2<float> transform = Matrix3X2<float>.Identity;
            transform.M31 = c.X;
            transform.M32 = c.Y;

            Vector2D<float> extent = new(r);

            return new Paint(transform, extent, r, MathF.Max(1.0f, f), icol, ocol, default);
        }

        public static Paint RadialGradient(float cx, float cy, float inr, float outr, Colour icol, Colour ocol)
            => RadialGradient(new Vector2D<float>(cx, cy), inr, outr, icol, ocol);

        public static Paint BoxGradient(Box2D<float> box, float r, float f, Colour icol, Colour ocol)
        {
            Matrix3X2<float> transform = Matrix3X2<float>.Identity;
            transform.M31 = box.Min.X + (box.Size.X * 0.5f);
            transform.M32 = box.Min.Y + (box.Size.Y * 0.5f);

            Vector2D<float> extent = box.Size * 0.5f;

            return new Paint(transform, extent, r, MathF.Max(1.0f, f), icol, ocol, default);
        }

        public static Paint BoxGradient(float x, float y, float w, float h, float r, float f, Colour icol, Colour ocol)
            => BoxGradient(new Box2D<float>(new Vector2D<float>(x, y), new Vector2D<float>(x + w, y + h)), r, f, icol, ocol);

        public static Paint ImagePattern(Rectangle<float> bounds, float angle, int image, float alpha)
        {
            Matrix3X2<float> transform = Matrix3X2.CreateRotation(angle);
            transform.M31 = bounds.Origin.X;
            transform.M32 = bounds.Origin.Y;

            Vector2D<float> extent = bounds.Size;

            return new Paint(transform, extent, default, default, new Colour(1.0f, 1.0f, 1.0f, alpha), new Colour(1.0f, 1.0f, 1.0f, alpha), image);
        }

        public static Paint ImagePattern(float ox, float oy, float w, float h, float angle, int image, float alpha)
            => ImagePattern(new Rectangle<float>(new Vector2D<float>(ox, oy), new Vector2D<float>(w, h)), angle, image, alpha);

    }
}
