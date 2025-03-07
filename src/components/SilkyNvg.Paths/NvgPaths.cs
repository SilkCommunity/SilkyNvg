using SilkyNvg.Common;
using SilkyNvg.Core.Instructions;
using SilkyNvg.Core.States;
using SilkyNvg.Rendering;
using System;
using System.Drawing;
using System.Numerics;

namespace SilkyNvg.Paths
{
    /// <summary>
    /// <para>Drawing a new shape starts with <see cref="BeginPath(Nvg)"/>, it clears all the currently defined paths.
    /// Then you define one or more paths and sub-paths which descripe the shape. There are functions
    /// to draw common shapes like rectangles and circles, and lower level step-by-step functions,
    /// which allow to define a path curve by curve.</para>
    /// </summary>
    public static class NvgPaths
    {

        private const float KAPPA90 = 0.5522847493f;

        /// <summary>
        /// Clears the current path and sub-paths.
        /// </summary>
        public static void BeginPath(this Nvg nvg)
        {
            nvg.instructionQueue.Clear();
            nvg.pathCache.Clear();
        }

        /// <summary>
        /// Starts a new sub-path with specified point as first point.
        /// </summary>
        public static void MoveTo(this Nvg nvg, Vector2 p)
        {
            nvg.instructionQueue.AddMoveTo(p);
        }

        /// <inheritdoc cref="MoveTo(Nvg, Vector2)"/><br/>
        public static void MoveTo(this Nvg nvg, PointF p)
        {
            nvg.instructionQueue.AddMoveTo(p.ToVector2());
        }

        /// <inheritdoc cref="MoveTo(Nvg, Vector2)"/><br/>
        public static void MoveTo(this Nvg nvg, float x, float y)
            => MoveTo(nvg, new Vector2(x, y));

        /// <summary>
        /// Adds line segment from the last point in the path to the specified point.
        /// </summary>
        public static void LineTo(this Nvg nvg, Vector2 p)
        {
            nvg.instructionQueue.AddLineTo(p);
        }

        /// <inheritdoc cref="LineTo(Nvg, Vector2)"/><br/>
        public static void LineTo(this Nvg nvg, PointF p)
        {
            nvg.instructionQueue.AddLineTo(p.ToVector2());
        }

        /// <inheritdoc cref="LineTo(Nvg, Vector2)"/><br/>
        public static void LineTo(this Nvg nvg, float x, float y)
            => LineTo(nvg, new Vector2(x, y));

        /// <summary>
        /// Adds cubic bezier segment from last point in the path via two control points to the specified point.
        /// </summary>
        public static void BezierTo(this Nvg nvg, Vector2 cp0, Vector2 cp1, Vector2 p)
        {
            nvg.instructionQueue.AddBezierTo(cp0, cp1, p);
        }

        /// <inheritdoc cref="BezierTo(Nvg, Vector2, Vector2, Vector2)"/><br/>
        public static void BezierTo(this Nvg nvg, PointF cp0, PointF cp1, PointF p)
            => BezierTo(nvg, cp0.ToVector2(), cp1.ToVector2(), p.ToVector2());

        /// <inheritdoc cref="BezierTo(Nvg, Vector2, Vector2, Vector2)"/><br/>
        public static void BezierTo(this Nvg nvg, float c0x, float c0y, float c1x, float c1y, float x, float y)
            => BezierTo(nvg, new Vector2(c0x, c0y), new Vector2(c1x, c1y), new Vector2(x, y));

        /// <summary>
        /// Adds quadratic bezier segment from last point in the path via a control point to the specified point.
        /// </summary>
        public static void QuadTo(this Nvg nvg, Vector2 cp, Vector2 p)
        {
            Vector2 lastPos = nvg.instructionQueue.EndPosition;
            nvg.instructionQueue.AddBezierTo(
                lastPos + 2.0f / 3.0f * (cp - lastPos),
                p + 2.0f / 3.0f * (cp - p),
                p
            );
        }

        /// <inheritdoc cref="QuadTo(Nvg, Vector2, Vector2)"/><br/>
        public static void QuadTo(this Nvg nvg, PointF cp, PointF p)
            => QuadTo(nvg, cp.ToVector2(), p.ToVector2());

        /// <inheritdoc cref="QuadTo(Nvg, Vector2, Vector2)"/><br/>
        public static void QuadTo(this Nvg nvg, float cx, float cy, float x, float y)
            => QuadTo(nvg, new Vector2(cx, cy), new Vector2(x, y));

        /// <summary>
        /// Adds an arc segment at the corner defined by the last path point and two specified points.
        /// </summary>
        public static void ArcTo(this Nvg nvg, Vector2 p1, Vector2 p2, float radius)
        {
            Vector2 p0 = nvg.instructionQueue.EndPosition;

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

            Vector2 d0 = p0 - p1;
            Vector2 d1 = p2 - p1;
            d0 = Vector2.Normalize(d0);
            d1 = Vector2.Normalize(d1);
            float a = MathF.Acos(d0.X * d1.X + d0.Y * d1.Y);
            float d = radius / MathF.Tan(a / 2.0f);

            if (d > 10000.0f)
            {
                LineTo(nvg, p1);
                return;
            }

            Winding dir;
            Vector2 valuesC;
            float a0, a1;
            if (Maths.Cross(d0, d1) > 0.0f)
            {
                float cx = p1.X + d0.X * d + d0.Y * radius;
                float cy = p1.Y + d0.Y * d + -d0.X * radius;
                a0 = MathF.Atan2(d0.X, -d0.Y);
                a1 = MathF.Atan2(-d1.X, d1.Y);
                dir = Winding.Cw;
                valuesC = new Vector2(cx, cy);
            }
            else
            {
                float cx = p1.X + d0.X * d + -d0.Y * radius;
                float cy = p1.Y + d0.Y * d + d0.X * radius;
                a0 = MathF.Atan2(-d0.X, d0.Y);
                a1 = MathF.Atan2(d1.X, -d1.Y);
                dir = Winding.Ccw;
                valuesC = new Vector2(cx, cy);
            }

            Arc(nvg, valuesC, radius, a0, a1, dir);
        }

        /// <inheritdoc cref="ArcTo(Nvg, Vector2, Vector2, float)"/><br/>
        public static void ArcTo(this Nvg nvg, PointF p1, PointF p2, float radius)
            => ArcTo(nvg, p1.ToVector2(), p2.ToVector2(), radius);

        /// <inheritdoc cref="ArcTo(Nvg, Vector2, Vector2, float)"/><br/>
        public static void ArcTo(this Nvg nvg, float x1, float y1, float x2, float y2, float radius)
            => ArcTo(nvg, new Vector2(x1, y1), new Vector2(x2, y2), radius);

        /// <summary>
        /// Closes current sub-path with a line segment.
        /// </summary>
        public static void ClosePath(this Nvg nvg)
        {
            nvg.instructionQueue.AddClose();
        }

        /// <summary>
        /// Sets the current sub-path winding, <see cref="Winding"/> and <see cref="Solidity"/>
        /// </summary>
        public static void PathWinding(this Nvg nvg, Winding dir)
        {
            nvg.instructionQueue.AddWinding(dir);
        }

        /// <inheritdoc cref="PathWinding(Nvg, Winding)"/><br/>
        public static void PathWinding(this Nvg nvg, Solidity sol)
            => PathWinding(nvg, (Winding)sol);

        /// <summary>
        /// Creates new circle arc shaped sub-path.
        /// The arc is drawn from angle a0 to a1.
        /// </summary>
        /// <param name="c">The arc center.</param>
        /// <param name="r">The arc radius.</param>
        /// <param name="dir">The direction the arc is swept in.</param>
        public static void Arc(this Nvg nvg, Vector2 c, float r, float a0, float a1, Winding dir)
        {
            Vector2 pPos = default;
            Vector2 pTan = default;

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
                Vector2 d = new(MathF.Cos(alpha), MathF.Sin(alpha));
                Vector2 pos = new(c.X + d.X * r, c.Y + d.Y * r);
                Vector2 tan = new(-d.Y * r * kappa, d.X * r * kappa);

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

        /// <inheritdoc cref="Arc(Nvg, Vector2, float, float, float, Winding)"/><br/>
        public static void Arc(this Nvg nvg, PointF c, float r, float a0, float a1, Winding dir)
            => Arc(nvg, c.ToVector2(), r, a0, a1, dir);

        /// <summary>
        /// Creates new circle arc shaped sub-path.
        /// The arc is drawn from angle a0 to a1.
        /// </summary>
        /// <param name="cx">The arc center X-Choordinate.</param>
        /// <param name="cy">The arc center Y-Choordinate.</param>
        /// <param name="r">The arc radius.</param>
        /// <param name="dir">The direction the arc is swept in.</param>
        public static void Arc(this Nvg nvg, float cx, float cy, float r, float a0, float a1, Winding dir)
            => Arc(nvg, new Vector2(cx, cy), r, a0, a1, dir);

        /// <inheritdoc cref="Arc(Nvg, Vector2, float, float, float, Winding)"/><br/>
        public static void Arc(this Nvg nvg, Vector2 c, float r, float a0, float a1, Solidity solidity)
            => Arc(nvg, c, r, a0, a1, (Winding)solidity);

        /// <inheritdoc cref="Arc(Nvg, Vector2, float, float, float, Winding)"/><br/>
        public static void Arc(this Nvg nvg, PointF c, float r, float a0, float a1, Solidity solidity)
            => Arc(nvg, c.ToVector2(), r, a0, a1, (Winding)solidity);

        /// <inheritdoc cref="Arc(Nvg, float, float, float, float, float, Winding)"/><br/>
        public static void Arc(this Nvg nvg, float cx, float cy, float r, float a0, float a1, Solidity solidity)
            => Arc(nvg, new Vector2(cx, cy), r, a0, a1, (Winding)solidity);

        /// <summary>
        /// Creates a new rectangle shaped sub-path.
        /// </summary>
        public static void Rect(this Nvg nvg, RectangleF rect)
        {
            InstructionQueue queue = nvg.instructionQueue;
            queue.AddMoveTo(new Vector2(rect.Left, rect.Top));
            queue.AddLineTo(new Vector2(rect.Left, rect.Bottom));
            queue.AddLineTo(new Vector2(rect.Right, rect.Bottom));
            queue.AddLineTo(new Vector2(rect.Right, rect.Top));
            queue.AddClose();
        }

        /// <inheritdoc cref="Rect(Nvg, RectangleF)"/>
        public static void Rect(this Nvg nvg, Vector4 rect)
            => Rect(nvg, (RectangleF)rect);

        /// <inheritdoc cref="Rect(Nvg, RectangleF)"/>
        public static void Rect(this Nvg nvg, PointF pos, SizeF size)
            => Rect(nvg, new RectangleF(pos, size));

        /// <inheritdoc cref="Rect(Nvg, RectangleF)"/>
        public static void Rect(this Nvg nvg, Vector2 pos, Vector2 size)
            => Rect(nvg, (PointF)pos, (SizeF)size);

        /// <inheritdoc cref="Rect(Nvg, RectangleF)"/>
        public static void Rect(this Nvg nvg, float x, float y, float w, float h)
            => Rect(nvg, RectangleF.FromLTRB(x, y, x + w, y + h));

        /// <summary>
        /// Creates a new rounded rectangle shaped sub-path.
        /// </summary>
        public static void RoundedRect(this Nvg nvg, RectangleF rect, float r)
        {
            RoundedRectVarying(nvg, rect, r, r, r, r);
        }

        /// <inheritdoc cref="RoundedRect(Nvg, RectangleF, float)"/>
        public static void RoundedRect(this Nvg nvg, Vector4 rect, float r)
            => RoundedRect(nvg, (RectangleF)rect, r);

        /// <inheritdoc cref="RoundedRect(Nvg, RectangleF, float)"/>
        public static void RoundedRect(this Nvg nvg, PointF pos, SizeF size, float r)
            => RoundedRect(nvg, new RectangleF(pos, size), r);

        /// <inheritdoc cref="RoundedRect(Nvg, RectangleF, float)"/>
        public static void RoundedRect(this Nvg nvg, Vector2 pos, Vector2 size, float r)
            => RoundedRect(nvg, (PointF)pos, (SizeF)size, r);

        /// <inheritdoc cref="RoundedRect(Nvg, RectangleF, float)"/>
        public static void RoundedRect(this Nvg nvg, float x, float y, float w, float h, float r)
            => RoundedRect(nvg, RectangleF.FromLTRB(x, y, x + w, y + h), r);

        /// <summary>
        /// Creates a new rounded rectangle shaped sub-path with varying radii for each corner.
        /// </summary>
        public static void RoundedRectVarying(this Nvg nvg, RectangleF rect, float radTopLeft, float radTopRight, float radBottomRight, float radBottomLeft)
        {
            if (radTopLeft < 0.1f && radTopRight < 0.1f && radBottomRight < 0.1f && radBottomLeft < 0.1f)
            {
                Rect(nvg, rect);
            }
            else
            {
                InstructionQueue queue = nvg.instructionQueue;

                float factor = 1 - KAPPA90;
                Vector2 half = Vector2.Abs((Vector2)rect.Size) * 0.5f;
                Vector2 rBL = new(MathF.Min(radBottomLeft, half.X) * Maths.Sign(rect.Width),   MathF.Min(radBottomLeft, half.Y) * Maths.Sign(rect.Height));
                Vector2 rBR = new(MathF.Min(radBottomRight, half.X) * Maths.Sign(rect.Width),  MathF.Min(radBottomRight, half.Y) * Maths.Sign(rect.Height));
                Vector2 rTR = new(MathF.Min(radTopRight, half.X) * Maths.Sign(rect.Width),     MathF.Min(radTopRight, half.Y) * Maths.Sign(rect.Height));
                Vector2 rTL = new(MathF.Min(radTopLeft, half.X) * Maths.Sign(rect.Width),      MathF.Min(radTopLeft, half.Y) * Maths.Sign(rect.Height));
                queue.AddMoveTo(new(rect.Location.X, rect.Location.Y + rTL.Y));
                queue.AddLineTo(new(rect.Location.X, rect.Location.Y + rect.Height - rBL.Y));
                queue.AddBezierTo(
                    new(rect.Left,                  rect.Bottom - rBL.Y * factor),
                    new(rect.Left + rBL.X * factor, rect.Bottom),
                    new(rect.Left + rBL.X,          rect.Bottom)
                );
                queue.AddLineTo(new(rect.Location.X + rect.Width - rBR.X, rect.Location.Y + rect.Height));
                queue.AddBezierTo(
                    new(rect.Right - rBR.X * factor,   rect.Bottom),
                    new(rect.Right,                    rect.Bottom - rBR.Y * factor),
                    new(rect.Right,                    rect.Bottom - rBR.Y)
                );
                queue.AddLineTo(new(rect.Location.X + rect.Width, rect.Location.Y + rTR.Y));
                queue.AddBezierTo(
                    new(rect.Right,                    rect.Top + rTR.Y * factor),
                    new(rect.Right - rTR.X * factor,   rect.Top),
                    new(rect.Right - rTR.X,            rect.Top)
                );
                queue.AddLineTo(new(rect.Location.X + rTL.X, rect.Location.Y));
                queue.AddBezierTo(
                    new(rect.Left + rTL.X * factor, rect.Top),
                    new(rect.Left,                  rect.Top + rTL.Y * factor),
                    new(rect.Left,                  rect.Top + rTL.Y)
                );
                queue.AddClose();
            }
        }

        /// <inheritdoc cref="RoundedRectVarying(Nvg, RectangleF, float, float, float, float)"/>
        public static void RoundedRectVarying(this Nvg nvg, Vector4 rect, float radTopLeft, float radTopRight, float radBottomRight, float radBottomLeft)
            => RoundedRectVarying(nvg, (RectangleF)rect, radTopLeft, radTopRight, radBottomRight, radBottomLeft);

        /// <inheritdoc cref="RoundedRectVarying(Nvg, RectangleF, float, float, float, float)"/>
        public static void RoundedRectVarying(this Nvg nvg, PointF pos, SizeF size, float radTopLeft, float radTopRight, float radBottomRight, float radBottomLeft)
            => RoundedRectVarying(nvg, new RectangleF(pos, size), radTopLeft, radTopRight, radBottomRight, radBottomLeft);

        /// <inheritdoc cref="RoundedRectVarying(Nvg, RectangleF, float, float, float, float)"/>
        public static void RoundedRectVarying(this Nvg nvg, Vector2 pos, Vector2 size, float radTopLeft, float radTopRight, float radBottomRight, float radBottomLeft)
            => RoundedRectVarying(nvg, (PointF)pos, (SizeF)size, radTopLeft, radTopRight, radBottomRight, radBottomLeft);

        /// <inheritdoc cref="RoundedRectVarying(Nvg, RectangleF, float, float, float, float)"/>
        public static void RoundedRectVarying(this Nvg nvg, float x, float y, float w, float h, float radTopLeft, float radTopRight, float radBottomRight, float radBottomLeft)
            => RoundedRectVarying(nvg, new RectangleF(x, y, w, h), radTopLeft, radTopRight, radBottomRight, radBottomLeft);

        /// <summary>
        /// Creates a new ellipse shaped sub-path.
        /// </summary>
        public static void Ellipse(this Nvg nvg, Vector2 c, float rx, float ry)
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

        /// <inheritdoc cref="Ellipse(Nvg, Vector2, float, float)"/>
        public static void Ellipse(this Nvg nvg, PointF c, float rx, float ry)
            => Ellipse(nvg, c.ToVector2(), rx, ry);

        /// <inheritdoc cref="Ellipse(Nvg, Vector2, float, float)"/>
        public static void Ellipse(this Nvg nvg, float cx, float cy, float rx, float ry)
            => Ellipse(nvg, new Vector2(cx, cy), rx, ry);
        
        /// <summary>
        /// Creates a new circle shaped sub-path.
        /// </summary>
        public static void Circle(this Nvg nvg, Vector2 c, float r)
            => Ellipse(nvg, c, r, r);

        /// <inheritdoc cref="Circle(Nvg, Vector2, float)"/>
        public static void Circle(this Nvg nvg, PointF c, float r)
            => Circle(nvg, c.ToVector2(), r);

        /// <inheritdoc cref="Circle(Nvg, Vector2, float)"/>
        public static void Circle(this Nvg nvg, float cx, float cy, float r)
            => Circle(nvg, new Vector2(cx, cy), r);

        /// <summary>
        /// Fills the current path with current fill style.
        /// </summary>
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

        /// <summary>
        /// Fills the current path with current stroke style.
        /// </summary>
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
