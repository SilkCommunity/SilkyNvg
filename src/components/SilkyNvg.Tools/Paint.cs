using System;
using System.Drawing;
using System.Numerics;

namespace SilkyNvg
{

    /// <summary>
    /// NanoVG supports four types of paints: linear gradient, box gradient, radial gradient and image pattern.
    /// These can be used as paints for strokes and fill.
    /// </summary>
    public struct Paint
    {

        public Matrix3x2 Transform { get; private set; }

        public SizeF Extent { get; }

        public float Radius { get; }

        public float Feather { get; }

        public Colour InnerColour { get; private set; }

        public Colour OuterColour { get; private set; }

        public int Image { get; }

        public Paint(Matrix3x2 transform, SizeF extent, float radius, float feather, Colour innerColour, Colour outerColour, int image)
        {
            Transform = transform;
            Extent = extent;
            Radius = radius;
            Feather = feather;
            InnerColour = innerColour;
            OuterColour = outerColour;
            Image = image;
        }

        public Paint(Matrix3x2 transform, Vector2 extent, float radius, float feather, Colour innerColour, Colour outerColour, int image)
            : this(transform, (SizeF)extent, radius, feather, innerColour, outerColour, image) { }

        internal Paint(Colour colour)
            : this(Matrix3x2.Identity, SizeF.Empty, default, default, colour, colour, default) { }

        internal void MultiplyTransform(Matrix3x2 globalTransform)
        {
            Transform = Transform * globalTransform;
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
        public static Paint LinearGradient(Vector2 s, Vector2 e, Colour icol, Colour ocol)
        {
            const float large = 1e5f;

            Vector2 delta = e - s;
            float d = delta.Length();

            if (d > 0.0001f)
            {
                delta /= d;
            }
            else
            {
                delta = Vector2.UnitY;
            }

            Matrix3x2 transform = new
            (
                delta.Y, -delta.X,
                delta.X, delta.Y,
                s.X - delta.X * large, s.Y - delta.Y * large
            );

            SizeF extent = new(large, large + d * 0.5f);

            return new Paint(transform, extent, 0.0f, MathF.Max(1.0f, d), icol, ocol, default);
        }

        /// <inheritdoc cref="LinearGradient(Vector2, Vector2, Colour, Colour)"/>
        public static Paint LinearGradient(PointF s, PointF e, Colour icol, Colour ocol)
            => LinearGradient(s.ToVector2(), e.ToVector2(), icol, ocol);

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
            => LinearGradient(new Vector2(sx, sy), new Vector2(ex, ey), icol, ocol);


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
        public static Paint BoxGradient(RectangleF box, float r, float f, Colour icol, Colour ocol)
        {
            Matrix3x2 transform = Matrix3x2.CreateTranslation((Vector2)(box.Location + box.Size / 2.0f));
            SizeF extent = box.Size * 0.5f;

            return new Paint(transform, extent, r, MathF.Max(1.0f, f), icol, ocol, default);
        }

        /// <inheritdoc cref="BoxGradient(RectangleF, float, float, Colour, Colour)"/>
        public static Paint BoxGradient(Vector4 box, float r, float f, Colour icol, Colour ocol)
            => BoxGradient((RectangleF)box, r, f, icol, ocol);

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
        public static Paint BoxGradient(PointF pos, SizeF size, float r, float f, Colour icol, Colour ocol)
            => BoxGradient(new RectangleF(pos, size), r, f, icol, ocol);

        /// <inheritdoc cref="BoxGradient(PointF, SizeF, float, float, Colour, Colour)"/>
        public static Paint BoxGradient(Vector2 pos, Vector2 size, float r, float f, Colour icol, Colour ocol)
            => BoxGradient((PointF)pos, (SizeF)size, r, f, icol, ocol);

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
            => BoxGradient(new RectangleF(x, y, w, h), r, f, icol, ocol);

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
        public static Paint RadialGradient(Vector2 c, float inr, float outr, Colour icol, Colour ocol)
        {
            float r = (inr + outr) * 0.5f;
            float f = (outr - inr);

            Matrix3x2 transform = Matrix3x2.CreateTranslation(c);
            SizeF extent = new(r, r);

            return new Paint(transform, extent, r, MathF.Max(1.0f, f), icol, ocol, default);
        }

        /// <inheritdoc cref="RadialGradient(Vector2, float, float, Colour, Colour)"/>
        public static Paint RadialGradient(PointF c, float inr, float outr, Colour icol, Colour ocol)
            => RadialGradient(c.ToVector2(), inr, outr, icol, ocol);

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
            => RadialGradient(new Vector2(cx, cy), inr, outr, icol, ocol);

        /// <summary>
        /// Creates and returns an image pattern.
        /// The gradient is transformed by the current transform when it is passed to Nvg.FillPaint() or Nvg.StrokePaint().
        /// </summary>
        /// <param name="bounds">Specifies the bounds of the image pattern.</param>
        /// <param name="angle">Specified rotation around the top-left corner</param>
        /// <param name="image">Is handle to the image to render</param>
        /// <returns>An image pattern.</returns>
        public static Paint ImagePattern(RectangleF bounds, float angle, int image, float alpha)
        {
            Matrix3x2 transform = Matrix3x2.CreateRotation(angle);
            transform.M31 = bounds.Location.X;
            transform.M32 = bounds.Location.Y;

            SizeF extent = bounds.Size;

            return new Paint(transform, extent, default, default, new Colour(1.0f, 1.0f, 1.0f, alpha), new Colour(1.0f, 1.0f, 1.0f, alpha), image);
        }

        /// <inheritdoc cref="ImagePattern(RectangleF, float, int, float)"/>
        public static Paint ImagePattern(Vector4 bounds, float angle, int image, float alpha)
            => ImagePattern((RectangleF)bounds, angle, image, alpha);

        /// <summary>
        /// Creates and returns an image pattern.
        /// The gradient is transformed by the current transform when it is passed to Nvg.FillPaint() or Nvg.StrokePaint().
        /// </summary>
        /// <param name="origin">Top-left choordinates</param>
        /// <param name="angle">Specified rotation around the top-left corner</param>
        /// <param name="image">Is handle to the image to render</param>
        /// <returns>An image pattern.</returns>
        public static Paint ImagePattern(PointF origin, SizeF size, float angle, int image, float alpha)
            => ImagePattern(new RectangleF(origin, size), angle, image, alpha);

        /// <inheritdoc cref="ImagePattern(PointF, SizeF, float, int, float)"/>
        public static Paint ImagePattern(Vector2 origin, Vector2 size, float angle, int image, float alpha)
            => ImagePattern((PointF)origin, (SizeF)size, angle, image, alpha);

        /// <summary>
        /// Creates and returns an image pattern.
        /// The gradient is transformed by the current transform when it is passed to Nvg.FillPaint() or Nvg.StrokePaint().
        /// </summary>
        /// <param name="ox">Top-left X-Choordinate</param>
        /// <param name="oy">Top-left Y-Choordinate</param>
        /// <param name="angle">Specified rotation around the top-left corner</param>
        /// <param name="image">Is handle to the image to render</param>
        /// <returns>An image pattern.</returns>
        public static Paint ImagePattern(float ox, float oy, float w, float h, float angle, int image, float alpha)
            => ImagePattern(new RectangleF(ox, oy, w, h), angle, image, alpha);

    }
}
