using Silk.NET.Maths;
using SilkyNvg.Common;
using System;

namespace SilkyNvg
{

    /// <summary>
    /// NanoVG supports four types of paints: linear gradient, box gradient, radial gradient and image pattern.
    /// These can be used as paints for strokes and fill.
    /// </summary>
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

        /// <summary>
        /// Creates and returns a linerar gradient.
        /// The gradient is transformed by the current transform when it is passed to NvgRenderStyle.FillPaint() or NvgRenderStyle.StrokePaint().
        /// </summary>
        /// <param name="s">Specifies the start coordinates of the linear gradient.</param>
        /// <param name="e">Specifies the end coordinates of the linear gradient.</param>
        /// <param name="icol">Specifies the start colour.</param>
        /// <param name="ocol">Specifies the end colour.</param>
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

        /// <summary>
        /// Creates and returns a linerar gradient.
        /// The gradient is transformed by the current transform when it is passed to NvgRenderStyle.FillPaint() or NvgRenderStyle.StrokePaint().
        /// </summary>
        /// <param name="sx">Specifies the start X-coordinates of the linear gradient.</param>
        /// <param name="sy">Specifies the start Y-coordinates of the linear gradient.</param>
        /// <param name="ex">Specifies the end X-coordinates of the linear gradient.</param>
        /// <param name="ey">Specifies the end Y-coordinates of the linear gradient.</param>
        /// <param name="icol">Specifies the start colour.</param>
        /// <param name="ocol">Specifies the end colour.</param>
        public static Paint LinearGradient(float sx, float sy, float ex, float ey, Colour icol, Colour ocol)
            => LinearGradient(new Vector2D<float>(sx, sy), new Vector2D<float>(ex, ey), icol, ocol);


        /// <summary>
        /// Creates and returns a linear gradient. Box gradient is a feathered rounded rectangle, it is useful for rendering
        /// drop shadows or highlitghts for boxes. Feather defines how blurry the corner of the rectangle is. The gradient
        /// is transformed by the current transform when it is passed to NvgRenderStyle.FillPaint() or NvgRenderStyle.StrokePaint();
        /// </summary>
        /// <param name="box">The rectangle.</param>
        /// <param name="r">Defines the corner radius.</param>
        /// <param name="f">Defines the feather.</param>
        /// <param name="icol">Inner colour of the gradient.</param>
        /// <param name="ocol">Outer colour of the gradient.</param>
        /// <returns></returns>
        public static Paint BoxGradient(Rectangle<float> box, float r, float f, Colour icol, Colour ocol)
        {
            Matrix3X2<float> transform = Matrix3X2<float>.Identity;
            transform.M31 = box.Center.X;
            transform.M32 = box.Center.Y;

            Vector2D<float> extent = box.Size * 0.5f;

            return new Paint(transform, extent, r, MathF.Max(1.0f, f), icol, ocol, default);
        }

        /// <summary>
        /// Creates and returns a linear gradient. Box gradient is a feathered rounded rectangle, it is useful for rendering
        /// drop shadows or highlitghts for boxes. Feather defines how blurry the corner of the rectangle is. The gradient
        /// is transformed by the current transform when it is passed to NvgRenderStyle.FillPaint() or NvgRenderStyle.StrokePaint();
        /// </summary>
        /// <param name="pos">The top-left corner of the rectangle.</param>
        /// <param name="size">The size of the rectangle.</param>
        /// <param name="r">Defines the corner radius.</param>
        /// <param name="f">Defines the feather.</param>
        /// <param name="icol">Inner colour of the gradient.</param>
        /// <param name="ocol">Outer colour of the gradient.</param>
        /// <returns></returns>
        public static Paint BoxGradient(Vector2D<float> pos, Vector2D<float> size, float r, float f, Colour icol, Colour ocol)
            => BoxGradient(new Rectangle<float>(pos, size), r, f, icol, ocol);

        /// <summary>
        /// Creates and returns a linear gradient. Box gradient is a feathered rounded rectangle, it is useful for rendering
        /// drop shadows or highlitghts for boxes. Feather defines how blurry the corner of the rectangle is. The gradient
        /// is transformed by the current transform when it is passed to NvgRenderStyle.FillPaint() or NvgRenderStyle.StrokePaint();
        /// </summary>
        /// <param name="x">The left size of the rectangle.</param>
        /// <param name="y">The top of the rectangle.</param>
        /// <param name="w">The width of the rectangle.</param>
        /// <param name="h">The height of the rectangle.</param>
        /// <param name="r">Defines the corner radius.</param>
        /// <param name="f">Defines the feather.</param>
        /// <param name="icol">Inner colour of the gradient.</param>
        /// <param name="ocol">Outer colour of the gradient.</param>
        /// <returns></returns>
        public static Paint BoxGradient(float x, float y, float w, float h, float r, float f, Colour icol, Colour ocol)
            => BoxGradient(new Rectangle<float>(new Vector2D<float>(x, y), new Vector2D<float>(w, h)), r, f, icol, ocol);

        /// <summary>
        /// Creates and returns a radial gradient.
        /// The gradient is transformed by the current transform when it is passed to NvgRenderStyle.FillPaint() or NvgRenderStyle.StrokePaint().
        /// </summary>
        /// <param name="c">The centre.</param>
        /// <param name="inr">Specifies the inner radius of the gradient.</param>
        /// <param name="outr">Specifies the outer radius of the gradient.</param>
        /// <param name="icol">Specifies the start colour.</param>
        /// <param name="ocol">Specifies the end colour.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Creates and returns a radial gradient.
        /// The gradient is transformed by the current transform when it is passed to NvgRenderStyle.FillPaint() or NvgRenderStyle.StrokePaint().
        /// </summary>
        /// <param name="cx">The centre's x-coordinate.</param>
        /// <param name="cy">The centre's y-coordinate.</param>
        /// <param name="inr">Specifies the inner radius of the gradient.</param>
        /// <param name="outr">Specifies the outer radius of the gradient.</param>
        /// <param name="icol">Specifies the start colour.</param>
        /// <param name="ocol">Specifies the end colour.</param>
        /// <returns></returns>
        public static Paint RadialGradient(float cx, float cy, float inr, float outr, Colour icol, Colour ocol)
            => RadialGradient(new Vector2D<float>(cx, cy), inr, outr, icol, ocol);

        /// <summary>
        /// <inheritdoc cref="Paint"/><br/>
        /// Creates and returns an image pattern.
        /// The gradient is transformed by the current transform when it is passed to Nvg.FillPaint() or Nvg.StrokePaint().
        /// </summary>
        /// <param name="bounds">Specifies the bounds of the image pattern.</param>
        /// <param name="angle">Specified rotation around the top-left corner</param>
        /// <param name="image">Is handle to the image to render</param>
        /// <returns>An image pattern.</returns>
        public static Paint ImagePattern(Rectangle<float> bounds, float angle, int image, float alpha)
        {
            Matrix3X2<float> transform = Matrix3X2.CreateRotation(angle);
            transform.M31 = bounds.Origin.X;
            transform.M32 = bounds.Origin.Y;

            Vector2D<float> extent = bounds.Size;

            return new Paint(transform, extent, default, default, new Colour(1.0f, 1.0f, 1.0f, alpha), new Colour(1.0f, 1.0f, 1.0f, alpha), image);
        }

        /// <summary>
        /// <inheritdoc cref="NvgImagePaint"/><br/>
        /// Creates and returns an image pattern.
        /// The gradient is transformed by the current transform when it is passed to Nvg.FillPaint() or Nvg.StrokePaint().
        /// </summary>
        /// <param name="origin">Top-left choordinates</param>
        /// <param name="angle">Specified rotation around the top-left corner</param>
        /// <param name="image">Is handle to the image to render</param>
        /// <returns>An image pattern.</returns>
        public static Paint ImagePattern(Vector2D<float> origin, Vector2D<float> size, float angle, int image, float alpha)
            => ImagePattern(new Rectangle<float>(origin, size), angle, image, alpha);

        /// <summary>
        /// <inheritdoc cref="NvgImagePaint"/><br/>
        /// Creates and returns an image pattern.
        /// The gradient is transformed by the current transform when it is passed to Nvg.FillPaint() or Nvg.StrokePaint().
        /// </summary>
        /// <param name="ox">Top-left X-Choordinate</param>
        /// <param name="oy">Top-left Y-Choordinate</param>
        /// <param name="angle">Specified rotation around the top-left corner</param>
        /// <param name="image">Is handle to the image to render</param>
        /// <returns>An image pattern.</returns>
        public static Paint ImagePattern(float ox, float oy, float w, float h, float angle, int image, float alpha)
            => ImagePattern(new Rectangle<float>(new Vector2D<float>(ox, oy), new Vector2D<float>(w, h)), angle, image, alpha);

    }
}
