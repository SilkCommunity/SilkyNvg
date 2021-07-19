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

        public static Paint LinearGradient(Vector2D<float> start, Vector2D<float> end, Colour innerColour, Colour outerColour)
        {
            const float large = 1e5f;

            Vector2D<float> delta = end - start;
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
                start.X - delta.X * large, start.Y - delta.Y * large
            );

            Vector2D<float> extent = new(large, large + d * 0.5f);

            return new Paint(transform, extent, 0.0f, MathF.Max(1.0f, d), innerColour, outerColour, default);
        }

        public static Paint LinearGradient(float startX, float startY, float endX, float endY, Colour icol, Colour ocol)
            => LinearGradient(new(startX, startY), new(endX, endY), icol, ocol);

        public static Paint RadialGradient(Vector2D<float> centre, float innerRadius, float outerRadius, Colour innerColour, Colour outerColour)
        {
            float r = (innerRadius + outerRadius) * 0.5f;
            float f = (outerRadius - innerRadius);

            Matrix3X2<float> transform = Matrix3X2<float>.Identity;
            transform.M31 = centre.X;
            transform.M32 = centre.Y;

            Vector2D<float> extent = new(r);

            return new Paint(transform, extent, r, MathF.Max(1.0f, f), innerColour, outerColour, default);
        }

        public static Paint RadialGradient(float centreX, float centreY, float innerRadius, float outerRadius, Colour innerColour, Colour outerColour)
            => RadialGradient(new(centreX, centreY), innerRadius, outerRadius, innerColour, outerColour);

        public static Paint BoxGradient(Rectangle<float> bounds, float radius, float feather, Colour innerColour, Colour outerColour)
        {
            Matrix3X2<float> transform = Matrix3X2<float>.Identity;
            transform.M31 = bounds.Origin.X + (bounds.Size.X * 0.5f);
            transform.M32 = bounds.Origin.Y + (bounds.Size.Y * 0.5f);

            Vector2D<float> extent = bounds.Size * 0.5f;

            return new Paint(transform, extent, radius, MathF.Max(1.0f, feather), innerColour, outerColour, default);
        }

        public static Paint BoxGradient(float x, float y, float width, float height, float radius, float feather, Colour innerColour, Colour outerColour)
            => BoxGradient(new(new(x, y), new(width, height)), radius, feather, innerColour, outerColour);

        public static Paint ImagePattern(Rectangle<float> bounds, float angle, int image, float alpha)
        {
            Matrix3X2<float> transform = Matrix3X2.CreateRotation(angle);
            transform.M31 = bounds.Origin.X;
            transform.M32 = bounds.Origin.Y;

            Vector2D<float> extent = bounds.Size;

            return new Paint(transform, extent, default, default, new Colour(1.0f, 1.0f, 1.0f, alpha), new Colour(1.0f, 1.0f, 1.0f, alpha), image);
        }

        public static Paint ImagePattern(float x, float y, float width, float height, float angle, int image, float alpha)
            => ImagePattern(new(new(x, y), new(width, height)), angle, image, alpha);

    }
}
