using Silk.NET.Maths;
using SilkyNvg.Common;
using SilkyNvg.Core.Instructions;
using SilkyNvg.Core.States;
using SilkyNvg.Rendering;

namespace SilkyNvg.Paths
{
    public static class NvgPaths
    {

        public static void BeginPath(this Nvg nvg)
        {
            nvg.instructionQueue.Clear();
            nvg.pathCache.Clear();
        }

        public static void MoveTo(this Nvg nvg, Vector2D<float> pos)
        {
            nvg.instructionQueue.AddMoveTo(pos);
        }

        public static void MoveTo(this Nvg nvg, float x, float y) => MoveTo(nvg, new Vector2D<float>(x, y));

        public static void LineTo(this Nvg nvg, Vector2D<float> pos)
        {
            nvg.instructionQueue.AddLineTo(pos);
        }

        public static void LineTo(this Nvg nvg, float x, float y) => LineTo(nvg, new Vector2D<float>(x, y));

        public static void ClosePath(this Nvg nvg)
        {
            nvg.instructionQueue.AddClose();
        }

        public static void Rect(this Nvg nvg, Vector2D<float> pos, Vector2D<float> size)
        {
            InstructionQueue queue = nvg.instructionQueue;
            queue.AddMoveTo(pos);
            queue.AddLineTo(new(pos.X, pos.Y + size.Y));
            queue.AddLineTo(pos + size);
            queue.AddLineTo(new(pos.X + size.X, pos.Y));
            queue.AddClose();
        }

        public static void Rect(this Nvg nvg, float x, float y, float w, float h) => Rect(nvg, new Vector2D<float>(x, y), new Vector2D<float>(w, h));

        public static void Fill(this Nvg nvg)
        {
            State state = nvg.stateStack.CurrentState;
            Paint fillPaint = state.Fill;

            nvg.instructionQueue.FlattenPaths();

            if (nvg.graphicsManager.EdgeAntiAlias && nvg.stateStack.CurrentState.ShapeAntiAlias)
            {
                nvg.pathCache.ExpandFill(nvg.pixelRatio.FringeWidth, Graphics.LineCap.Miter, 2.4f, nvg.pixelRatio);
            }
            else
            {
                nvg.pathCache.ExpandFill(0.0f, Graphics.LineCap.Miter, 2.4f, nvg.pixelRatio);
            }

            fillPaint.PremultiplyAlpha(nvg.stateStack.CurrentState.Alpha);

            nvg.graphicsManager.Fill(fillPaint, state.CompositeOperation, state.Scissor, nvg.pixelRatio.FringeWidth, nvg.pathCache.Bounds, nvg.pathCache.Paths);

            foreach (Path path in nvg.pathCache.Paths)
            {
                nvg.FrameMeta.Update(0, 0, (uint)path.Fill.Count - 2, (uint)path.Stroke.Count - 2);
            }
        }

        public static void Stroke(this Nvg nvg)
        {
            State state = nvg.stateStack.CurrentState;
            float scale = Maths.GetAverageScale(state.Transform);
            float strokeWidth = Maths.Clamp(state.StrokeWidth * scale, 0.0f, 1.0f);
            Paint strokePaint = state.Stroke;

            if (strokeWidth > nvg.pixelRatio.FringeWidth)
            {
                float alpha = Maths.Clamp(strokeWidth / nvg.pixelRatio.FringeWidth, 0.0f, 1.0f);
                strokePaint.PremultiplyAlpha(alpha * alpha);
                strokeWidth = nvg.pixelRatio.FringeWidth;
            }

            strokePaint.PremultiplyAlpha(state.Alpha);

            nvg.instructionQueue.FlattenPaths();

            if (nvg.graphicsManager.EdgeAntiAlias && state.ShapeAntiAlias)
            {
                nvg.pathCache.ExpandStroke(strokeWidth * 0.5f, nvg.pixelRatio.FringeWidth, state.LineCap, state.LineJoin, state.MiterLimit, nvg.pixelRatio);
            }
            else
            {
                nvg.pathCache.ExpandStroke(strokeWidth * 0.5f, 0.0f, state.LineCap, state.LineJoin, state.MiterLimit, nvg.pixelRatio);
            }

            nvg.graphicsManager.Stroke(strokePaint, state.CompositeOperation, state.Scissor, nvg.pixelRatio.FringeWidth, strokeWidth, nvg.pathCache.Paths);
        }

    }
}
