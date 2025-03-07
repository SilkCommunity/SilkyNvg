using System;
using System.Drawing;
using System.Numerics;

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
        public static void Scissor(this Nvg nvg, RectangleF rect)
        {
            Vector2 pos = (Vector2)rect.Location;
            Vector2 size = (Vector2)rect.Size;

            size = Vector2.Max(Vector2.Zero, size);

            Matrix3x2 transform = Matrix3x2.CreateTranslation(pos + size * 0.5f);

            nvg.stateStack.CurrentState.Scissor = new(
                Transforms.NvgTransforms.Multiply(transform, nvg.stateStack.CurrentState.Transform),
                (SizeF)(size * 0.5f)
            );
        }

        /// <inheritdoc cref="Scissor(Nvg, RectangleF)"/>
        public static void Scissor(this Nvg nvg, Vector4 rect)
            => Scissor(nvg, (RectangleF)rect);

        /// <inheritdoc cref="Scissor(Nvg, RectangleF)"/>
        public static void Scissor(this Nvg nvg, PointF pos, SizeF size)
            => Scissor(nvg, new RectangleF(pos, size));

        /// <inheritdoc cref="Scissor(Nvg, RectangleF)"/>
        public static void Scissor(this Nvg nvg, Vector2 pos, Vector2 size)
            => Scissor(nvg, (PointF)pos, (SizeF)size);

        /// <inheritdoc cref="Scissor(Nvg, RectangleF)"/>
        public static void Scissor(this Nvg nvg, float x, float y, float width, float height)
            => Scissor(nvg, RectangleF.FromLTRB(x, y, x + width, y + height));

        /// <summary>
        /// <para>Intersects current scissor rectangle with the specified rectangle.
        /// The scissor rectangle is transformed by the current transform.</para>
        /// <para>Note: in case the rotation of previous scissor rect differs from
        /// the current one, the intersection will be done between the specified
        /// rectangle and the previous scissor rectangle transformed in the current
        /// transform space. The resulting shape is always a rectangle.</para>
        /// </summary>
        public static void IntersectScissor(this Nvg nvg, RectangleF rect)
        {
            if (nvg.stateStack.CurrentState.Scissor.Extent.Width < 0)
            {
                Scissor(nvg, rect);
                return;
            }

            Matrix3x2 ptransform = nvg.stateStack.CurrentState.Scissor.Transform;
            SizeF e = nvg.stateStack.CurrentState.Scissor.Extent;

            _ = Transforms.NvgTransforms.Inverse(out Matrix3x2 invtransform, nvg.stateStack.CurrentState.Transform);
            ptransform = Transforms.NvgTransforms.Multiply(ptransform, invtransform);

            Vector2 te = new(
                e.Width * MathF.Abs(ptransform.M11) + e.Height * MathF.Abs(ptransform.M21),
                e.Width * MathF.Abs(ptransform.M12) + e.Height * MathF.Abs(ptransform.M22)
            );

            RectangleF r = RectangleF.Intersect(rect, RectangleF.FromLTRB(ptransform.M31 - te.X, ptransform.M32 - te.Y, te.X * 2.0f, te.Y * 2.0f));

            Scissor(nvg, r);
        }

        /// <inheritdoc cref="IntersectScissor(Nvg, RectangleF)"/>
        public static void IntersectScissor(this Nvg nvg, Vector4 rect)
            => IntersectScissor(nvg, (RectangleF)rect);

        /// <inheritdoc cref="IntersectScissor(Nvg, RectangleF)"/>
        public static void IntersectScissor(this Nvg nvg, PointF pos, SizeF size)
            => IntersectScissor(nvg, new RectangleF(pos, size));

        /// <inheritdoc cref="IntersectScissor(Nvg, RectangleF)"/>
        public static void IntersectScissor(this Nvg nvg, Vector2 pos, Vector2 size)
            => IntersectScissor(nvg, (PointF)pos, (SizeF)size);

        /// <inheritdoc cref="IntersectScissor(Nvg, RectangleF)"/>
        public static void IntersectScissor(this Nvg nvg, float x, float y, float w, float h)
            => IntersectScissor(nvg, RectangleF.FromLTRB(x, y, x + w, y + h));

        /// <summary>
        /// Resets and disables scissoring.
        /// </summary>
        public static void ResetScissor(this Nvg nvg)
        {
            nvg.stateStack.CurrentState.Scissor = new(new SizeF(-1.0f, -1.0f));
        }

    }
}
