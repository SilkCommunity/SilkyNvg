using Silk.NET.Maths;
using SilkyNvg.Common;
using SilkyNvg.Core.Instructions;
using SilkyNvg.Core.States;
using SilkyNvg.Rendering;
using System;

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

        public static void BezierTo(this Nvg nvg, Vector2D<float> cp0, Vector2D<float> cp1, Vector2D<float> pos)
        {
            nvg.instructionQueue.AddBezierTo(cp0, cp1, pos);
        }

        public static void BezierTo(this Nvg nvg, float c0x, float c0y, float c1x, float c1y, float x, float y)
            => BezierTo(nvg, new Vector2D<float>(c0x, c0y), new Vector2D<float>(c1x, c1y), new Vector2D<float>(x, y));

        public static void QuadTo(this Nvg nvg, Vector2D<float> cp, Vector2D<float> pos)
        {
            Vector2D<float> lastPos = nvg.instructionQueue.EndPosition;
            nvg.instructionQueue.AddBezierTo(
                lastPos + 2.0f / 3.0f * (cp - lastPos),
                pos + 2.0f / 3.0f * (cp - pos),
                pos
            );
        }

        public static void QuadTo(this Nvg nvg, float cx, float cy, float x, float y)
            => QuadTo(nvg, new Vector2D<float>(cx, cy), new Vector2D<float>(x, y));

        public static void ArcTo(this Nvg nvg, Vector2D<float> p1, Vector2D<float> p2, float radius)
        {
            Vector2D<float> p0 = nvg.instructionQueue.EndPosition;

            if (nvg.instructionQueue.Count == 0)
            {
                return;
            }

            float distToll = nvg.pixelRatio.DistTol;

            if (Maths.PtEquals(p0, p1, distToll) ||
                Maths.PtEquals(p1, p2, distToll) ||
                Maths.DistPtSeg(p1, p0, p2) < distToll ||
                radius < distToll)
            {
                LineTo(nvg, p1);
                return;
            }

            Vector2D<float> d0 = p0 - p1;
            Vector2D<float> d1 = p2 - p1;
            d0 = Vector2D.Normalize(d0);
            d1 = Vector2D.Normalize(d1);
            float a = MathF.Acos(d0.X * d1.X + d0.Y * d1.Y);
            float d = radius / MathF.Tan(a / 2.0f);

            if (d > 10000.0f)
            {
                LineTo(nvg, p1);
                return;
            }

            Winding dir;
            Vector2D<float> valuesC = default;
            Vector2D<float> valuesA = default;
            if (Maths.Cross(d0, d1) > 0.0f)
            {
                float cx = p1.X + d0.X * d + d0.Y * radius;
                float cy = p1.Y + d0.Y * d + -d0.X * radius;
                float a0 = MathF.Atan2(d0.X, -d0.Y);
                float a1 = MathF.Atan2(-d1.X, d1.Y);
                dir = Winding.Cw;
                valuesC = new Vector2D<float>(cx, cy);
                valuesA = new Vector2D<float>(a0, a1);
            }
            else
            {
                float cx = p1.X + d0.X * d + -d0.Y * radius;
                float cy = p1.Y + d0.Y * d + d0.X * radius;
                float a0 = MathF.Atan2(-d0.X, d0.Y);
                float a1 = MathF.Atan2(d1.X, -d1.Y);
                dir = Winding.Ccw;
                valuesC = new Vector2D<float>(cx, cy);
                valuesA = new Vector2D<float>(a0, a1);
            }

            Arc(nvg, valuesC, radius, valuesA, dir);
        }

        public static void ClosePath(this Nvg nvg)
        {
            nvg.instructionQueue.AddClose();
        }

        public static void PathWinding(this Nvg nvg, Winding dir)
        {
            nvg.instructionQueue.AddWinding(dir);
        }

        public static void PathWinding(this Nvg nvg, Solidity solidity)
            => PathWinding(nvg, (Winding)solidity);

        public static void Arc(this Nvg nvg, Vector2D<float> c, float r, Vector2D<float> a, Winding dir)
        {
            // FIXME: Implement this!!!!!!!!!!!!!
        }

        public static void Arc(this Nvg nvg, float cx, float cy, float r, float ax, float ay, Winding dir)
            => Arc(nvg, new Vector2D<float>(cx, cy), r, new Vector2D<float>(ax, ay), dir);

        public static void Arc(this Nvg nvg, Vector2D<float> c, float r, Vector2D<float> a, Solidity solidity)
            => Arc(nvg, c, r, a, (Winding)solidity);

        public static void Arc(this Nvg nvg, float cx, float cy, float r, float ax, float ay, Solidity solidity)
            => Arc(nvg, new Vector2D<float>(cx, cy), r, new Vector2D<float>(ax, ay), (Winding)solidity);

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

        public static void Ellipse(this Nvg nvg, Vector2D<float> pos, Vector2D<float> radii)
        {
            const float KAPPA90 = 0.5522847493f;

            InstructionQueue queue = nvg.instructionQueue;
            queue.AddMoveTo(new(pos.X - radii.X, pos.Y));
            queue.AddBezierTo(
                new(pos.X - radii.X, pos.Y + radii.Y * KAPPA90),
                new(pos.X - radii.X * KAPPA90, pos.Y + radii.Y),
                new(pos.X, pos.Y + radii.Y));
            queue.AddBezierTo(
                new(pos.X + radii.X * KAPPA90, pos.Y + radii.Y),
                new(pos.X + radii.X, pos.Y + radii.Y * KAPPA90),
                new(pos.X + radii.X, pos.Y));
            queue.AddBezierTo(
                new(pos.X + radii.X, pos.Y - radii.Y * KAPPA90),
                new(pos.X + radii.X * KAPPA90, pos.Y - radii.Y),
                new(pos.X, pos.Y - radii.Y));
            queue.AddBezierTo(
                new(pos.X - radii.X * KAPPA90, pos.Y - radii.Y),
                new(pos.X - radii.X, pos.Y - radii.Y * KAPPA90),
                new(pos.X - radii.X, pos.Y));
            queue.AddClose();
        }

        public static void Ellipse(this Nvg nvg, float x, float y, float rx, float ry)
            => Ellipse(nvg, new Vector2D<float>(x, y), new Vector2D<float>(rx, ry));

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
