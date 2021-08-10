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

        public static void MoveTo(this Nvg nvg, float x, float y)
            => MoveTo(nvg, new Vector2D<float>(x, y));

        public static void LineTo(this Nvg nvg, Vector2D<float> pos)
        {
            nvg.instructionQueue.AddLineTo(pos);
        }

        public static void LineTo(this Nvg nvg, float x, float y)
            => LineTo(nvg, new Vector2D<float>(x, y));

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

        public static void ArcTo(this Nvg nvg, Vector2D<float> pos1, Vector2D<float> pos2, float radius)
        {
            Vector2D<float> pos0 = nvg.instructionQueue.EndPosition;

            if (nvg.instructionQueue.Count == 0)
            {
                return;
            }

            float distToll = nvg.pixelRatio.DistTol;

            if (Maths.PtEquals(pos0, pos1, distToll) ||
                Maths.PtEquals(pos1, pos2, distToll) ||
                Maths.DistPtSeg(pos1, pos0, pos2) < distToll ||
                radius < distToll)
            {
                LineTo(nvg, pos1);
                return;
            }

            Vector2D<float> d0 = pos0 - pos1;
            Vector2D<float> d1 = pos2 - pos1;
            d0 = Vector2D.Normalize(d0);
            d1 = Vector2D.Normalize(d1);
            float a = MathF.Acos(d0.X * d1.X + d0.Y * d1.Y);
            float d = radius / MathF.Tan(a / 2.0f);

            if (d > 10000.0f)
            {
                LineTo(nvg, pos1);
                return;
            }

            Winding dir;
            Vector2D<float> valuesC;
            float a0, a1;
            if (Maths.Cross(d0, d1) > 0.0f)
            {
                float cx = pos1.X + d0.X * d + d0.Y * radius;
                float cy = pos1.Y + d0.Y * d + -d0.X * radius;
                a0 = MathF.Atan2(d0.X, -d0.Y);
                a1 = MathF.Atan2(-d1.X, d1.Y);
                dir = Winding.Cw;
                valuesC = new Vector2D<float>(cx, cy);
            }
            else
            {
                float cx = pos1.X + d0.X * d + -d0.Y * radius;
                float cy = pos1.Y + d0.Y * d + d0.X * radius;
                a0 = MathF.Atan2(-d0.X, d0.Y);
                a1 = MathF.Atan2(d1.X, -d1.Y);
                dir = Winding.Ccw;
                valuesC = new Vector2D<float>(cx, cy);
            }

            Arc(nvg, valuesC, radius, a0, a1, dir);
        }

        public static void ArcTo(this Nvg nvg, float x1, float y1, float x2, float y2, float radius)
            => ArcTo(nvg, new Vector2D<float>(x1, y1), new Vector2D<float>(x2, y2), radius);

        public static void ClosePath(this Nvg nvg)
        {
            nvg.instructionQueue.AddClose();
        }

        public static void PathWinding(this Nvg nvg, Winding dir)
        {
            nvg.instructionQueue.AddWinding(dir);
        }

        public static void PathWinding(this Nvg nvg, Solidity sol)
            => PathWinding(nvg, (Winding)sol);

        public static void Arc(this Nvg nvg, Vector2D<float> c, float r, float a0, float a1, Winding dir)
        {
            Vector2D<float> pPos = default;
            Vector2D<float> pTan = default;

            InstructionQueue queue = nvg.instructionQueue;

            bool line = queue.Count > 0;

            float da = a1 - a0;
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
                float alpha = a0 + da * ((float)i / (float)ndivs);
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
            => Arc(nvg, new Vector2D<float>(cx, cy), r, a0, a1, dir);

        public static void Arc(this Nvg nvg, Vector2D<float> c, float r, float a0, float a1, Solidity solidity)
            => Arc(nvg, c, r, a0, a1, (Winding)solidity);

        public static void Arc(this Nvg nvg, float cx, float cy, float r, float a0, float a1, Solidity solidity)
            => Arc(nvg, new Vector2D<float>(cx, cy), r, a0, a1, (Winding)solidity);

        public static void Rect(this Nvg nvg, Rectangle<float> rect)
        {
            InstructionQueue queue = nvg.instructionQueue;
            queue.AddMoveTo(rect.Origin);
            queue.AddLineTo(new(rect.Origin.X, rect.Max.Y));
            queue.AddLineTo(rect.Max);
            queue.AddLineTo(new(rect.Max.X, rect.Origin.Y));
            queue.AddClose();
        }

        public static void Rect(this Nvg nvg, float x, float y, float w, float h)
            => Rect(nvg, new Rectangle<float>(new Vector2D<float>(x, y), new Vector2D<float>(w, h)));

        public static void RoundedRectVarying(this Nvg nvg, Rectangle<float> rect, float radTopLeft, float radTopRight, float radBottomRight, float radBottomLeft)
        {
            if (radTopLeft < 0.1f && radTopRight < 0.1f && radBottomRight < 0.1f && radBottomLeft < 0.1f)
            {
                Rect(nvg, rect);
            }
            else
            {
                InstructionQueue queue = nvg.instructionQueue;

                float factor = 1 - KAPPA90;
                Vector2D<float> half = Vector2D.Abs(rect.Size) * 0.5f;
                Vector2D<float> rBL = new(MathF.Min(radBottomLeft, half.X) * Maths.Sign(rect.Size.X),   MathF.Min(radBottomLeft, half.Y) * Maths.Sign(rect.Size.Y));
                Vector2D<float> rBR = new(MathF.Min(radBottomRight, half.X) * Maths.Sign(rect.Size.X),  MathF.Min(radBottomRight, half.Y) * Maths.Sign(rect.Size.Y));
                Vector2D<float> rTR = new(MathF.Min(radTopRight, half.X) * Maths.Sign(rect.Size.X),     MathF.Min(radTopRight, half.Y) * Maths.Sign(rect.Size.Y));
                Vector2D<float> rTL = new(MathF.Min(radTopLeft, half.X) * Maths.Sign(rect.Size.X),      MathF.Min(radTopLeft, half.Y) * Maths.Sign(rect.Size.Y));
                queue.AddMoveTo(new(rect.Origin.X, rect.Origin.Y + rTL.Y));
                queue.AddLineTo(new(rect.Origin.X, rect.Origin.Y + rect.Size.Y - rBL.Y));
                queue.AddBezierTo(
                    new(rect.Origin.X,                  rect.Origin.Y + rect.Size.Y - rBL.Y * factor),
                    new(rect.Origin.X + rBL.X * factor, rect.Origin.Y + rect.Size.Y),
                    new(rect.Origin.X + rBL.X,          rect.Origin.Y + rect.Size.Y)
                );
                queue.AddLineTo(new(rect.Origin.X + rect.Size.X - rBR.X, rect.Origin.Y + rect.Size.Y));
                queue.AddBezierTo(
                    new(rect.Origin.X + rect.Size.X - rBR.X * factor,   rect.Origin.Y + rect.Size.Y),
                    new(rect.Origin.X + rect.Size.X,                    rect.Origin.Y + rect.Size.Y - rBR.Y * factor),
                    new(rect.Origin.X + rect.Size.X,                    rect.Origin.Y + rect.Size.Y - rBR.Y)
                );
                queue.AddLineTo(new(rect.Origin.X + rect.Size.X, rect.Origin.Y + rTR.Y));
                queue.AddBezierTo(
                    new(rect.Origin.X + rect.Size.X,                    rect.Origin.Y + rTR.Y * factor),
                    new(rect.Origin.X + rect.Size.X - rTR.X * factor,   rect.Origin.Y),
                    new(rect.Origin.X + rect.Size.X - rTR.X,            rect.Origin.Y)
                );
                queue.AddLineTo(new(rect.Origin.X + rTL.X, rect.Origin.Y));
                queue.AddBezierTo(
                    new(rect.Origin.X + rTL.X * factor, rect.Origin.Y),
                    new(rect.Origin.X,                  rect.Origin.Y + rTL.Y * factor),
                    new(rect.Origin.X,                  rect.Origin.Y + rTL.Y)
                );
                queue.AddClose();
            }
        }

        public static void RoundedRectVarying(this Nvg nvg, float x, float y, float width, float height, float radTopLeft, float radTopRight, float radBottomRight, float radBottomLeft)
            => RoundedRectVarying(nvg, x, y, width, height, radTopLeft, radTopRight, radBottomRight, radBottomLeft);

        public static void RoundedRect(this Nvg nvg, Rectangle<float> rect, float r)
        {
            RoundedRectVarying(nvg, rect, r, r, r, r);
        }

        public static void RoundedRect(this Nvg nvg, float x, float y, float width, float height, float r)
            => RoundedRect(nvg, new Rectangle<float>(new Vector2D<float>(x, y), new Vector2D<float>(width, height)), r);

        public static void Ellipse(this Nvg nvg, Vector2D<float> c, float rx, float ry)
        {
            InstructionQueue queue = nvg.instructionQueue;
            queue.AddMoveTo(new(c.X - rx, c.Y));
            queue.AddBezierTo(
                new(c.X - rx,           c.Y + ry * KAPPA90),
                new(c.X - rx * KAPPA90, c.Y + ry),
                new(c.X,                c.Y + ry));
            queue.AddBezierTo(
                new(c.X + rx * KAPPA90, c.Y + ry),
                new(c.X + rx,           c.Y + ry * KAPPA90),
                new(c.X + rx,           c.Y));
            queue.AddBezierTo(
                new(c.X + rx,           c.Y - ry * KAPPA90),
                new(c.X + rx * KAPPA90, c.Y - ry),
                new(c.X,                c.Y - ry));
            queue.AddBezierTo(
                new(c.X - rx * KAPPA90, c.Y - ry),
                new(c.X - rx,           c.Y - ry * KAPPA90),
                new(c.X - rx,           c.Y));
            queue.AddClose();
        }

        public static void Ellipse(this Nvg nvg, float cx, float cy, float rx, float ry)
            => Ellipse(nvg, new Vector2D<float>(cx, cy), rx, ry);

        public static void Circle(this Nvg nvg, Vector2D<float> c, float r)
        {
            Ellipse(nvg, c, r, r);
        }

        public static void Circle(this Nvg nvg, float cx, float cy, float r)
            => Circle(nvg, new Vector2D<float>(cx, cy), r);

        public static void Fill(this Nvg nvg)
        {
            State state = nvg.stateStack.CurrentState;
            Paint fillPaint = state.Fill;

            nvg.instructionQueue.FlattenPaths();

            if (nvg.renderer.EdgeAntiAlias && nvg.stateStack.CurrentState.ShapeAntiAlias)
            {
                nvg.pathCache.ExpandFill(nvg.pixelRatio.FringeWidth, Graphics.LineCap.Miter, 2.4f, nvg.pixelRatio);
            }
            else
            {
                nvg.pathCache.ExpandFill(0.0f, Graphics.LineCap.Miter, 2.4f, nvg.pixelRatio);
            }

            fillPaint.PremultiplyAlpha(nvg.stateStack.CurrentState.Alpha);

            nvg.renderer.Fill(fillPaint, state.CompositeOperation, state.Scissor, nvg.pixelRatio.FringeWidth, nvg.pathCache.Bounds, nvg.pathCache.Paths);

            foreach (Path path in nvg.pathCache.Paths)
            {
                nvg.FrameMeta.Update(0, 0, (uint)path.Fill.Count - 2 + (uint)path.Stroke.Count - 2, 2);
            }
        }

        public static void Stroke(this Nvg nvg)
        {
            State state = nvg.stateStack.CurrentState;
            float scale = Maths.GetAverageScale(state.Transform);
            float strokeWidth = Maths.Clamp(state.StrokeWidth * scale, 0.0f, 200.0f);
            Paint strokePaint = state.Stroke;

            if (strokeWidth < nvg.pixelRatio.FringeWidth)
            {
                float alpha = Maths.Clamp(strokeWidth / nvg.pixelRatio.FringeWidth, 0.0f, 1.0f);
                strokePaint.PremultiplyAlpha(alpha * alpha);
                strokeWidth = nvg.pixelRatio.FringeWidth;
            }

            strokePaint.PremultiplyAlpha(state.Alpha);

            nvg.instructionQueue.FlattenPaths();

            if (nvg.renderer.EdgeAntiAlias && state.ShapeAntiAlias)
            {
                nvg.pathCache.ExpandStroke(strokeWidth * 0.5f, nvg.pixelRatio.FringeWidth, state.LineCap, state.LineJoin, state.MiterLimit, nvg.pixelRatio);
            }
            else
            {
                nvg.pathCache.ExpandStroke(strokeWidth * 0.5f, 0.0f, state.LineCap, state.LineJoin, state.MiterLimit, nvg.pixelRatio);
            }

            nvg.renderer.Stroke(strokePaint, state.CompositeOperation, state.Scissor, nvg.pixelRatio.FringeWidth, strokeWidth, nvg.pathCache.Paths);

            foreach (Path path in nvg.pathCache.Paths)
            {
                nvg.FrameMeta.Update(0, (uint)path.Stroke.Count - 2, 0, 1);
            }
        }

    }
}
