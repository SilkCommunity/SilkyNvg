using Silk.NET.Maths;
using System;

namespace SilkyNvg.Scissoring
{
    /// <summary>
    /// <para>Scissoring allows you to clip the rendering into a rectangle. This is useful for various
    /// user interface cases like rendering text edit or a timeline.</para>
    /// </summary>
    public static class NvgScissoring
    {

        /// <summary>
        /// Sets the current scissor rectangle.
        /// The scissor rectangle is transformed by the current transform.
        /// </summary>
        public static void Scissor(this Nvg nvg, Rectangle<float> rect)
        {
            Vector2D<float> pos = rect.Origin;
            Vector2D<float> size = rect.Size;

            size.X = MathF.Max(0.0f, size.X);
            size.Y = MathF.Max(0.0f, size.Y);

            Vector2D<float> lastRow = pos + size * 0.5f;
            Matrix3X2<float> transform = Matrix3X2<float>.Identity;
            transform.M31 = lastRow.X;
            transform.M32 = lastRow.Y;

            nvg.stateStack.CurrentState.Scissor = new(
                Transforms.NvgTransforms.Multiply(transform, nvg.stateStack.CurrentState.Transform),
                size * 0.5f
            );
        }

        /// <inheritdoc cref="Scissor(Nvg, Rectangle{float})"/>
        public static void Scissor(this Nvg nvg, Vector2D<float> pos, Vector2D<float> size)
            => Scissor(nvg, new Rectangle<float>(pos, size));


        /// <inheritdoc cref="Scissor(Nvg, Rectangle{float})"/>
        public static void Scissor(this Nvg nvg, float x, float y, float width, float height)
            => Scissor(nvg, Rectangle.FromLTRB(x, y, x + width, y + height));

        /// <summary>
        /// <para>Intersects current scissor rectangle with the specified rectangle.
        /// The scissor rectangle is transformed by the current transform.</para>
        /// <para>Note: in case the rotation of previous scissor rect differs from
        /// the current one, the intersection will be done between the specified
        /// rectangle and the previous scissor rectangle transformed in the current
        /// transform space. The resulting shape is always a rectangle.</para>
        /// </summary>
        public static void IntersectScissor(this Nvg nvg, Rectangle<float> rect)
        {
            static Rectangle<float> IsectRects(Rectangle<float> a, Rectangle<float> b)
            {
                Vector2D<float> min = Vector2D.Max(a.Origin, b.Origin);
                Vector2D<float> max = Vector2D.Min(a.Max, b.Max);
                return new Rectangle<float>(min, Vector2D.Max(new Vector2D<float>(0.0f), max - min));
            }

            if (nvg.stateStack.CurrentState.Scissor.Extent.X < 0)
            {
                Scissor(nvg, rect);
                return;
            }

            Matrix3X2<float> ptransform = nvg.stateStack.CurrentState.Scissor.Transform;
            Vector2D<float> e = nvg.stateStack.CurrentState.Scissor.Extent;

            _ = Transforms.NvgTransforms.Inverse(out Matrix3X2<float> invtransform, nvg.stateStack.CurrentState.Transform);
            ptransform = Transforms.NvgTransforms.Multiply(ptransform, invtransform);

            Vector2D<float> te = new(
                e.X * MathF.Abs(ptransform.M11) + e.Y * MathF.Abs(ptransform.M21),
                e.X * MathF.Abs(ptransform.M12) + e.Y * MathF.Abs(ptransform.M22)
            );

            Rectangle<float> r = IsectRects(Rectangle.FromLTRB(ptransform.M31 - te.X, ptransform.M32 - te.Y, te.X * 2.0f, te.Y * 2.0f), rect);

            Scissor(nvg, r);
        }


        /// <inheritdoc cref="IntersectScissor(Nvg, Rectangle{float})"/>
        public static void IntersectScissor(this Nvg nvg, Vector2D<float> pos, Vector2D<float> size)
            => IntersectScissor(nvg, new Rectangle<float>(pos, size));

        /// <inheritdoc cref="IntersectScissor(Nvg, Rectangle{float})"/>
        public static void IntersectScissor(this Nvg nvg, float x, float y, float w, float h)
            => IntersectScissor(nvg, Rectangle.FromLTRB(x, y, x + w, y + h));

        /// <summary>
        /// Resets and disables scissoring.
        /// </summary>
        public static void ResetScissor(this Nvg nvg)
        {
            nvg.stateStack.CurrentState.Scissor = new(new Vector2D<float>(-1.0f));
        }

    }
}
