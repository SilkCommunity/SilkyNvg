using Silk.NET.Maths;
using SilkyNvg.Transforms;

namespace SilkyNvg.Graphics
{
    public static class NvgRenderStyle
    {

        public static void ShapeAntiAlias(this Nvg nvg, bool enabled)
        {
            nvg.stateStack.CurrentState.ShapeAntiAlias = enabled;
        }

        public static void StrokeWidth(this Nvg nvg, float width)
        {
            nvg.stateStack.CurrentState.StrokeWidth = width;
        }

        public static void MiterLimit(this Nvg nvg, float limit)
        {
            nvg.stateStack.CurrentState.MiterLimit = limit;
        }

        public static void LineCap(this Nvg nvg, LineCap cap)
        {
            nvg.stateStack.CurrentState.LineCap = cap;
        }

        public static void LineJoin(this Nvg nvg, LineCap join)
        {
            nvg.stateStack.CurrentState.LineJoin = join;
        }

        public static void GlobalAlpha(this Nvg nvg, float alpha)
        {
            nvg.stateStack.CurrentState.Alpha = alpha;
        }

        public static void StrokeColour(this Nvg nvg, Colour colour)
        {
            nvg.stateStack.CurrentState.Stroke = new Paint(colour);
        }

        public static void StrokePaint(this Nvg nvg, Paint paint)
        {
            nvg.stateStack.CurrentState.Stroke = paint;
            paint.MultiplyTransform(nvg.stateStack.CurrentState.Transform);
        }

        public static void FillColour(this Nvg nvg, Colour colour)
        {
            nvg.stateStack.CurrentState.Fill = new Paint(colour);
        }

        public static void FillPaint(this Nvg nvg, Paint paint)
        {
            nvg.stateStack.CurrentState.Fill = paint;
            paint.MultiplyTransform(nvg.stateStack.CurrentState.Transform);
        }

        public static void Transform(this Nvg nvg, Matrix3X2<float> transform)
        {
            nvg.stateStack.CurrentState.Transform = nvg.TransformPremultiply(nvg.stateStack.CurrentState.Transform, transform);
        }

        public static void ResetTransform(this Nvg nvg)
        {
            nvg.stateStack.CurrentState.Transform = nvg.TransformIdentity();
        }

        public static void Translate(this Nvg nvg, Vector2D<float> position)
        {
            Matrix3X2<float> t = nvg.TransformTranslate(position);
            nvg.stateStack.CurrentState.Transform = nvg.TransformPremultiply(nvg.stateStack.CurrentState.Transform, t);
        }

        public static void Translate(this Nvg nvg, float x, float y) => Translate(nvg, new Vector2D<float>(x, y));

        public static void Rotate(this Nvg nvg, float angle)
        {
            Matrix3X2<float> t = nvg.TransformRotate(angle);
            nvg.stateStack.CurrentState.Transform = nvg.TransformPremultiply(nvg.stateStack.CurrentState.Transform, t);
        }

        public static void SkewX(this Nvg nvg, float angle)
        {
            Matrix3X2<float> t = nvg.TransformSkewX(angle);
            nvg.stateStack.CurrentState.Transform = nvg.TransformPremultiply(nvg.stateStack.CurrentState.Transform, t);
        }

        public static void SkewY(this Nvg nvg, float angle)
        {
            Matrix3X2<float> t = nvg.TransformSkewY(angle);
            nvg.stateStack.CurrentState.Transform = nvg.TransformPremultiply(nvg.stateStack.CurrentState.Transform, t);
        }

        public static void Scale(this Nvg nvg, Vector2D<float> scale)
        {
            Matrix3X2<float> t = nvg.TransformScale(scale);
            nvg.stateStack.CurrentState.Transform = nvg.TransformPremultiply(nvg.stateStack.CurrentState.Transform, t);
        }

        public static void Scale(this Nvg nvg, float x, float y) => Scale(nvg, new Vector2D<float>(x, y));

        public static Matrix3X2<float> CurrentTransform(this Nvg nvg)
        {
            return nvg.stateStack.CurrentState.Transform;
        }

    }
}
