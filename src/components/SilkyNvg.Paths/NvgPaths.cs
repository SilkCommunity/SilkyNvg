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

        private const float KAPPA90 = 0.5522847493f;

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
            Vector2D<float> valuesC, valuesA;
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
            Vector2D<float> pPos = default;
            Vector2D<float> pTan = default;

            InstructionQueue queue = nvg.instructionQueue;

            bool line = queue.Count > 0;

            float da = a.Y - a.X;
            if (dir == Winding.Cw)
            {
                if (MathF.Abs(da) >= MathF.PI * 2.0f)
                {
                    da = MathF.PI * 2.0f;
                }
                else
                {
                    while (da < 0.0f)
                    {
                        da += MathF.PI * 2.0f;
                    }
                }
            }
            else
            {
                if (MathF.Abs(da) >= MathF.PI * 2.0f)
                {
                    da = -MathF.PI * 2.0f;
                }
                else
                {
                    while (da > 0.0f)
                    {
                        da -= MathF.PI * 2.0f;
                    }
                }
            }

            int ndivs = Math.Max(1, Math.Min((int)(MathF.Abs(da) / (MathF.PI * 0.5f) + 0.5f), 5));
            float hda = (da / (float)ndivs) / 2.0f;
            float kappa = MathF.Abs(4.0f / 3.0f * (1.0f - MathF.Cos(hda)) / MathF.Sin(hda));

            if (dir == Winding.Ccw)
            {
                kappa *= -1.0f;
            }

            for (int i = 0; i <= ndivs; i++)
            {
                float alpha = a.X + da * ((float)i / (float)ndivs);
                Vector2D<float> d = new(MathF.Cos(alpha), MathF.Sin(alpha));
                Vector2D<float> pos = new(c.X + d.X * r, c.Y + d.Y * r);
                Vector2D<float> tan = new(-d.Y * r * kappa, d.X * r * kappa);

                if (i == 0)
                {
                    if (line)
                    {
                        queue.AddLineTo(pos);
                    }
                    else
                    {
                        queue.AddMoveTo(pos);
                    }
                }
                else
                {
                    queue.AddBezierTo(pPos + pTan, pos - tan, pos);
                }

                pPos = pos;
                pTan = tan;
            }
        }

        public static void Arc(this Nvg nvg, float cx, float cy, float r, float a0, float a1, Winding dir)
            => Arc(nvg, new Vector2D<float>(cx, cy), r, new Vector2D<float>(a0, a1), dir);

        public static void Arc(this Nvg nvg, Vector2D<float> c, float r, Vector2D<float> a, Solidity solidity)
            => Arc(nvg, c, r, a, (Winding)solidity);

        public static void Arc(this Nvg nvg, float cx, float cy, float r, float a0, float a1, Solidity solidity)
            => Arc(nvg, new Vector2D<float>(cx, cy), r, new Vector2D<float>(a0, a1), (Winding)solidity);

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

        public static void RoundedRect(this Nvg nvg, Vector2D<float> pos, Vector2D<float> size, float r)
        {
            RoundedRectVarying(nvg, pos, size, r, r, r, r);
        }

        public static void RoundedRect(this Nvg nvg, float x, float y, float width, float height, float r)
            => RoundedRect(nvg, new(x, y), new(width, height), r);

        public static void RoundedRectVarying(this Nvg nvg, Vector2D<float> pos, Vector2D<float> size, float radTopLeft, float radTopRight, float radBottomRight, float radBottomLeft)
        {
            if (radTopLeft < 0.1f && radTopRight < 0.1f && radBottomRight < 0.1f && radBottomLeft < 0.1f)
            {
                Rect(nvg, pos, size);
            }
            else
            {
                InstructionQueue queue = nvg.instructionQueue;

                float factor = 1 - KAPPA90;
                Vector2D<float> half = new(MathF.Abs(size.X) * 0.5f, MathF.Abs(size.Y) * 0.5f);
                Vector2D<float> rBL = new(MathF.Min(radBottomLeft, half.X) * Maths.Sign(size.X), MathF.Min(radBottomLeft, half.Y) * Maths.Sign(size.Y));
                Vector2D<float> rBR = new(MathF.Min(radBottomRight, half.X) * Maths.Sign(size.X), MathF.Min(radBottomRight, half.Y) * Maths.Sign(size.Y));
                Vector2D<float> rTR = new(MathF.Min(radTopRight, half.X) * Maths.Sign(size.X), MathF.Min(radTopRight, half.Y) * Maths.Sign(size.Y));
                Vector2D<float> rTL = new(MathF.Min(radTopLeft, half.X) * Maths.Sign(size.X), MathF.Min(radTopLeft, half.Y) * Maths.Sign(size.Y));
                queue.AddMoveTo(new(pos.X, pos.Y + rTL.Y));
                queue.AddLineTo(new(pos.X, pos.Y + size.Y - rBL.Y));
                queue.AddBezierTo(
                    new(pos.X, pos.Y + size.Y - rBL.Y * factor),
                    new(pos.X + rBL.X * factor, pos.Y + size.Y),
                    new(pos.X + rBL.X, pos.Y + size.Y)
                );
                queue.AddLineTo(new(pos.X + size.X - rBR.X, pos.Y + size.Y));
                queue.AddBezierTo(
                    new(pos.X + size.X - rBR.X * factor, pos.Y + size.Y),
                    new(pos.X + size.X, pos.Y + size.Y - rBR.Y * factor),
                    new(pos.X + size.X, pos.Y + size.Y - rBR.Y)
                );
                queue.AddLineTo(new(pos.X + size.X, pos.Y + rTR.Y));
                queue.AddBezierTo(
                    new(pos.X + size.X, pos.Y + rTR.Y * factor),
                    new(pos.X + size.X - rTR.X * factor, pos.Y),
                    new(pos.X + size.X - rTR.X, pos.Y)
                );
                queue.AddLineTo(new(pos.X + rTL.X, pos.Y));
                queue.AddBezierTo(
                    new(pos.X + rTL.X * factor, pos.Y),
                    new(pos.X, pos.Y + rTL.Y * factor),
                    new(pos.X, pos.Y + rTL.Y)
                );
                queue.AddClose();
            }
        }

        public static void RoundedRectVarying(this Nvg nvg, float x, float y, float width, float height, float radTopLeft, float radTopRight, float radBottomRight, float radBottomLeft)
            => RoundedRectVarying(nvg, x, y, width, height, radTopLeft, radTopRight, radBottomRight, radBottomLeft);

        public static void Ellipse(this Nvg nvg, Vector2D<float> pos, Vector2D<float> radii)
        {
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

        public static void Circle(this Nvg nvg, Vector2D<float> centre, float radius)
        {
            Ellipse(nvg, centre, new(radius));
        }

        public static void Circle(this Nvg nvg, float centreX, float centreY, float radius)
            => Circle(nvg, new(centreX, centreY), radius);

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
            float strokeWidth = Maths.Clamp(state.StrokeWidth * scale, 0.0f, 200.0f);
            Paint strokePaint = state.Stroke.Clone();

            if (strokeWidth < nvg.pixelRatio.FringeWidth)
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
