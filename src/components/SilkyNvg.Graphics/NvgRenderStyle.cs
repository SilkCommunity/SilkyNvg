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

    }
}
