using Silk.NET.Maths;
using System;

namespace SilkyNvg.Scissoring
{
    public static class NvgScissoring
    {

        public static void Scissor(this Nvg nvg, Vector2D<float> pos, Vector2D<float> size)
        {
            size.X = MathF.Max(0.0f, size.X);
            size.Y = MathF.Max(0.0f, size.Y);

            Vector2D<float> lastRow = pos + size * 0.5f;
            Matrix3X2<float> transform = Matrix3X2<float>.Identity;
            transform.M31 = lastRow.X;
            transform.M32 = lastRow.Y;

            nvg.stateStack.CurrentState.Scissor = new(
                Transforms.Transforms.Multiply(transform, nvg.stateStack.CurrentState.Transform),
                size * 0.5f
            );
        }

        public static void Scissor(this Nvg nvg, float x, float y, float width, float height)
            => Scissor(nvg, new(x, y), new(width, height));

        public static void Scissor(this Nvg nvg, Vector4D<float> rect)
            => Scissor(nvg, new(rect.X, rect.Y), new(rect.Z, rect.W));

        public static void IntersectScissor(this Nvg nvg, Vector2D<float> pos, Vector2D<float> size)
        {
            static Vector4D<float> IsectRects(Vector4D<float> a, Vector4D<float> b)
            {
                float minx = MathF.Max(a.X, b.X);
                float miny = MathF.Max(a.Y, b.Y);
                float maxx = MathF.Min(a.X + a.Z, b.X + b.Z);
                float maxy = MathF.Min(a.Y + a.W, b.Y + b.W);
                return new Vector4D<float>(minx, miny, MathF.Max(0.0f, maxx - minx), MathF.Max(0.0f, maxy - miny));
            }

            if (nvg.stateStack.CurrentState.Scissor.Extent.X < 0)
            {
                Scissor(nvg, pos, size);
                return;
            }

            Matrix3X2<float> ptransform = nvg.stateStack.CurrentState.Scissor.Transform;
            Vector2D<float> e = nvg.stateStack.CurrentState.Scissor.Extent;

            _ = Transforms.Transforms.Inverse(out Matrix3X2<float> invtransform, nvg.stateStack.CurrentState.Transform);
            ptransform = Transforms.Transforms.Multiply(ptransform, invtransform);

            Vector2D<float> te = new(
                e.X * MathF.Abs(ptransform.M11) + e.Y * MathF.Abs(ptransform.M21),
                e.X * MathF.Abs(ptransform.M12) + e.Y * MathF.Abs(ptransform.M22)
            );

            Vector4D<float> rect = IsectRects(new(ptransform.M31 - te.X, ptransform.M32 - te.Y, te.X * 2.0f, te.Y * 2.0f), new(pos.X, pos.Y, size.X, size.Y));

            Scissor(nvg, rect.X, rect.Y, rect.Z, rect.W);
        }

        public static void IntersectScissor(this Nvg nvg, float x, float y, float width, float height)
            => IntersectScissor(nvg, new(x, y), new(width, height));

        public static void IntersectScissor(this Nvg nvg, Vector4D<float> rect)
            => IntersectScissor(nvg, new(rect.X, rect.Y), new(rect.Z, rect.W));

        public static void ResetScissor(this Nvg nvg)
        {
            nvg.stateStack.CurrentState.Scissor = new(new Vector2D<float>(-1.0f));
        }

    }
}
