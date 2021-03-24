using System;
using SilkyNvg.Common;
using SilkyNvg.Paths;
using SilkyNvg.Core.Instructions;
using SilkyNvg.Core.States;
using Silk.NET.Maths;

namespace SilkyNvg.Core
{

    /// <summary>
    /// <inheritdoc cref="Docs.Paths"/>
    /// </summary>
    public sealed class Draw
    {

        private readonly InstructionQueue _instructionManager;
        private readonly StateManager _stateManager;
        private readonly Style _style;

        internal Draw(InstructionQueue instructionQueue, StateManager stateManager, Style style)
        {
            _instructionManager = instructionQueue;
            _stateManager = stateManager;
            _style = style;
        }

        private void Add(InstructionSequence iseq)
        {
            _instructionManager.AddSequence(iseq, _stateManager.GetState());
        }

        /// <summary>
        /// <inheritdoc cref="Docs.Paths"/>
        /// 
        /// Start s an new sub path.
        /// </summary>
        /// <param name="x">The X Position of the new sub path</param>
        /// <param name="y">The Y Position of the new sub path</param>
        public void MoveTo(float x, float y)
        {
            var sequence = new InstructionSequence(1);
            sequence.AddMoveTo(x, y);
            Add(sequence);
        }

        /// <summary>
        /// <inheritdoc cref="Docs.Paths"/>
        /// 
        /// Adds a line segment from the last point in the path
        /// to the specified point.
        /// </summary>
        /// <param name="x">The X Position of the line's end</param>
        /// <param name="y">The Y Position of the line's end</param>
        public void LineTo(float x, float y)
        {
            var sequence = new InstructionSequence(1);
            sequence.AddLineTo(x, y);
            Add(sequence);
        }

        /// <summary>
        /// <inheritdoc cref="Docs.Paths"/>
        /// 
        /// Adds a cubic bezier segemnt to the current path using 2 controll points.
        /// </summary>
        /// <param name="cx1">The 1st controll point's X Position</param>
        /// <param name="cy1">The 1st controll point's Y Position</param>
        /// <param name="cx2">The 2nd controll point's X Position</param>
        /// <param name="cy2">The 2nd controll point's Y Position</param>
        /// <param name="x">The X Position of the bezier's end</param>
        /// <param name="y">The Y Position of the bezier's end</param>
        public void BezierTo(float cx1, float cy1, float cx2, float cy2, float x, float y)
        {
            var sequence = new InstructionSequence(1);
            sequence.AddBezireTo(cx1, cy1, cx2, cy2, x, y);
            Add(sequence);
        }

        /// <summary>
        /// <inheritdoc cref="Docs.Paths"/>
        /// 
        /// Adds a quadratic bezier segemnt to the current path using 2 controll points.
        /// </summary>
        /// <param name="cx">The controll point's X Position</param>
        /// <param name="cy">The controll point's Y Position</param>
        /// <param name="x">The X Position of the bezier's end</param>
        /// <param name="y">The Y Position of the bezier's end</param>
        public void QuadTo(float cx, float cy, float x, float y)
        {
            var position = _instructionManager.InstructionPosition;
            var sequence = new InstructionSequence(1);
            sequence.AddBezireTo(
                position.X + 2.0f / 3.0f * (cx - position.X),
                position.Y + 2.0f / 3.0f * (cy - position.Y),
                x + 2.0f / 3.0f * (cx - x),
                y + 2.0f / 3.0f * (cy - y),
                x,
                y
            );
            Add(sequence);
        }

        /// <summary>
        /// <inheritdoc cref="Docs.Paths"/>
        /// 
        /// Adds an arc segment at the corner defined by the last path point, and two specified points.
        /// </summary>
        /// <param name="x1">First specified point's X value</param>
        /// <param name="y1">First specified point's Y value</param>
        /// <param name="x2">Second specified point's X value</param>
        /// <param name="y2">Second specified point's Y value</param>
        /// <param name="r">The arc radius</param>
        public void ArcTo(float x1, float y1, float x2, float y2, float radius)
        {
            Vector2D<float> p0 = _instructionManager.InstructionPosition;
            Vector2D<float> p1 = new Vector2D<float>(x1, y1);
            Vector2D<float> p2 = new Vector2D<float>(x2, y2);
            Vector2D<float> d0 = new Vector2D<float>();
            Vector2D<float> d1 = new Vector2D<float>();
            Vector2D<float> c = new Vector2D<float>();

            float a, d, a0, a1;

            Winding dir;

            if (_instructionManager.QueueLength == 0)
            {
                return;
            }

            // Handle degenerate cases. (wondering how they look O_o)
            if (Maths.PtEquals(p0.X, p0.Y, p1.X, p1.Y, _style.DistributionTollerance) ||
                Maths.PtEquals(p1.X, p1.Y, p2.X, p2.Y, _style.DistributionTollerance) ||
                Maths.distancePtSegment(p1.X, p1.Y, p0.X, p0.Y, p2.X, p2.Y) < _style.DistributionTollerance * _style.DistributionTollerance ||
                radius < _style.DistributionTollerance)
            {
                InstructionSequence sequence = new InstructionSequence(1);
                sequence.AddLineTo(p1.X, p1.Y);
                Add(sequence);
                return;
            }

            //Calculate tangential circle to lines (x0, y0)-(x1, y1) and (x1, y1) - (x2, y2).
            d0.X = p0.X - p1.X;
            d0.Y = p0.Y - p1.Y;
            d1.X = p2.X - p1.X;
            d1.Y = p2.Y - p1.Y;

            d0 = d0 / Maths.Normalize(d0);
            d1 = d1 / Maths.Normalize(d1);
            a = MathF.Acos(d0.X * d1.X + d0.Y * d1.Y);
            d = radius / MathF.Tan(a / 2F);

            if (d > 10000F)
            {
                InstructionSequence sequence = new InstructionSequence(1);
                sequence.AddLineTo(p1.X, p1.Y);
                Add(sequence);
                return;
            }

            if (Maths.Cross(d0.X, d0.Y, d1.X, d1.Y) > 0F)
            {
                c = p1 + d0 * d + (new Vector2D<float>(d0.Y, -d0.X) * radius);
                a0 = MathF.Atan2(d0.X, -d0.Y);
                a1 = MathF.Atan2(-d1.X, d1.Y);
                dir = Winding.CW;
            }
            else
            {
                c = p1 + d0 * d + (new Vector2D<float>(-d0.Y, d0.X) * radius);
                a0 = MathF.Atan2(-d0.X, d0.Y);
                a1 = MathF.Atan2(d1.X, -d1.Y);
                dir = Winding.CCW;
            }

            Arc(c.X, c.Y, radius, a0, a1, dir);
        }

        /// <summary>
        /// <inheritdoc cref="Docs.Paths"/>
        /// 
        /// Creates a new arc circle shaped sub-path.
        /// </summary>
        /// <param name="x">The arc's centre X Position</param>
        /// <param name="y">The arc's centre Y Position</param>
        /// <param name="r">The arc radius</param>
        /// <param name="a0">The first angle to draw from</param>
        /// <param name="a1">The second angle to draw to</param>
        /// <param name="dir">The direction the arc is swept in (clockwise CW or counter clockwise CCW)</param>
        public void Arc(float x, float y, float r, float a0, float a1, Winding dir = Winding.CCW)
        {
            float a, da, hda, kappa, dx, dy, x_, y_, tanx, tany, px = 0, py = 0, ptanx = 0, ptany = 0;

            int ndivs;

            InstructionSequence instructionSequence;

            da = a1 - a0;

            if (dir == Winding.CW)
            {
                if (MathF.Abs(da) >= Maths.Pi * 2)
                {
                    da = Maths.Pi * 2;
                }
                else
                {
                    while (da < 0.0F)
                    {
                        da += Maths.Pi * 2;
                    }
                }
            }
            else
            {
                if (MathF.Abs(da) >= Maths.Pi * 2)
                {
                    da = -Maths.Pi * 2;
                }
                else
                {
                    while (da > 0.0F)
                    {
                        da -= Maths.Pi * 2;
                    }
                }
            }

            // Split Arc into segments with max 90 degree
            ndivs = Math.Max(1, Math.Min((int)(MathF.Abs(da) / (Maths.Pi * 0.5F) + 0.5F), 5));
            hda = (da / (float)ndivs) / 2.0F;
            kappa = MathF.Abs(4.0F / 3.0F * (1.0F - MathF.Cos(hda)) / MathF.Sin(hda));

            if (dir == Winding.CCW)
                kappa = -kappa;

            instructionSequence = new InstructionSequence(ndivs + 1);

            for (int i = 0; i <= ndivs; i++)
            {
                a = a0 + da * (i / (float)ndivs);
                dx = MathF.Cos(a);
                dy = MathF.Sin(a);
                x_ = x + (dx * r);
                y_ = y + (dy * r);
                tanx = -dy * r * kappa;
                tany = dx * r * kappa;

                if (i == 0)
                {
                    if (_instructionManager.QueueLength > 0)
                        instructionSequence.AddLineTo(x_, y_);
                    else
                        instructionSequence.AddMoveTo(x_, y_);
                }
                else
                {
                    instructionSequence.AddBezireTo(px + ptanx, py + ptany, x_ - tanx, y_ - tany, x_, y_);
                }
                px = x_;
                py = y_;
                ptanx = tanx;
                ptany = tany;
            }
            Add(instructionSequence);
        }

        /// <summary>
        /// <inheritdoc cref="Docs.Paths"/>
        /// 
        /// Creates a new rectangle shaped sub-path.
        /// </summary>
        /// <param name="x">The rectangle's X-Position</param>
        /// <param name="y">The rectangle's Y-Position</param>
        /// <param name="w">The rectangle's width</param>
        /// <param name="h">The rectangle's height</param>
        public void Rect(float x, float y, float width, float heigth)
        {
            InstructionSequence sequence = new InstructionSequence(5);
            sequence.AddMoveTo(x, y);
            sequence.AddLineTo(x, y + heigth);
            sequence.AddLineTo(x + width, y + heigth);
            sequence.AddLineTo(x + width, y);
            sequence.AddClose();
            Add(sequence);
        }

        /// <summary>
        /// <inheritdoc cref="Docs.Paths"/>
        /// 
        /// Creates a new rounded rectangle shaped sub-path.
        /// </summary>
        /// <param name="x">The rectangle's X Position</param>
        /// <param name="y">The rectangle's Y Position</param>
        /// <param name="width">The rectangle's width</param>
        /// <param name="height">The rectangle's height</param>
        /// <param name="radius">The radius of the corner rounding</param>
        public void RoundedRect(float x, float y, float width, float height, float radius)
        {
            RoundedRectVarying(x, y, width, height, radius, radius, radius, radius);
        }

        /// <summary>
        /// <inheritdoc cref="Docs.Paths"/>
        /// 
        /// Creates a new rounded rectangle shaped sub-path with varying radii for each corner.
        /// </summary>
        /// <param name="x">The rectangle's X Position</param>
        /// <param name="y">The rectangle's Y Position</param>
        /// <param name="width">The rectangle's width</param>
        /// <param name="height">The rectangle's height</param>
        /// <param name="radTopLeft">The radius of the top left corner rounding</param>
        /// <param name="radTopRight">The radius of the top right corner rounding</param>
        /// <param name="radBottomRight">The radius of the bottom right corner rounding</param>
        /// <param name="radBottomLeft">The radius of the bottom left corner rounding</param>
        public void RoundedRectVarying(float x, float y, float width, float height, float radTopLeft, float radTopRight, float radBottomRight, float radBottomLeft)
        {
            float w = width;
            float h = height;

            if (radTopLeft < 0.1F && radTopRight < 0.1F && radBottomLeft < 0.1F && radBottomRight < 0.1F)
            { 
                Rect(x, y, w, h);
                return;
            }

            float halfw = MathF.Abs(w) * 0.5F;
            float halfh = MathF.Abs(h) * 0.5F;
            float rxBL = MathF.Min(radBottomLeft, halfw) * MathF.Sign(w);
            float ryBL = Math.Min(radBottomLeft, halfh) * MathF.Sign(h);
            float rxBR = MathF.Min(radBottomRight, halfw) * MathF.Sign(w);
            float ryBR = MathF.Min(radBottomRight, halfh) * MathF.Sign(h);
            float rxTR = MathF.Min(radTopRight, halfw) * MathF.Sign(w);
            float ryTR = MathF.Min(radTopRight, halfh) * MathF.Sign(h);
            float rxTL = MathF.Min(radTopLeft, halfw) * MathF.Sign(w);
            float ryTL = MathF.Min(radTopLeft, halfh) * MathF.Sign(h);

            InstructionSequence sequence = new InstructionSequence(10);
            sequence.AddMoveTo(x, y + ryTL);
            sequence.AddLineTo(x, y + h - ryBL);
            sequence.AddBezireTo(x, y + h - ryBL * (1F - Maths.Kappa), x + rxBL * (1F - Maths.Kappa), y + h, x + rxBL, y + h);
            sequence.AddLineTo(x + w - rxBR, y + h);
            sequence.AddBezireTo(x + w - rxBR * (1F - Maths.Kappa), y + h, x + w, y + h - ryBR * (1F - Maths.Kappa), x + w, y + h - ryBR);
            sequence.AddLineTo(x + w, y + ryTR);
            sequence.AddBezireTo(x + w, y + ryTR * (1F - Maths.Kappa), x + w - rxTR * (1F - Maths.Kappa), y, x + w - rxTR, y);
            sequence.AddLineTo(x + rxTL, y);
            sequence.AddBezireTo(x + rxTL * (1F - Maths.Kappa), y, x , y + ryTL * (1F - Maths.Kappa), x, y + ryTL);
            sequence.AddClose();

            Add(sequence);
        }

        /// <summary>
        /// <inheritdoc cref="Docs.Paths"/>
        /// 
        /// Creates a new ellipse shaped sub-path-
        /// </summary>
        /// <param name="x">The ellipse's center x</param>
        /// <param name="y">The ellipse's center y</param>
        /// <param name="radiusX">The ellipse's radius on the X-Achsis</param>
        /// <param name="radiusY">The ellipse's radius on the Y-Achsis</param>
        public void Ellipse(float cx, float cy, float radiusX, float radiusY)
        {
            float rx = radiusX;
            float ry = radiusY;
            InstructionSequence sequence = new InstructionSequence(6);
            sequence.AddMoveTo(cx - ry, cy);
            sequence.AddBezireTo(cx - ry, cy + rx * Maths.Kappa, cx - ry * Maths.Kappa, cy + rx, cx, cy + rx);
            sequence.AddBezireTo(cx + ry * Maths.Kappa, cy + rx, cx + ry, cy + rx * Maths.Kappa, cx + ry, cy);
            sequence.AddBezireTo(cx + ry, cy - rx * Maths.Kappa, cx + ry * Maths.Kappa, cy - rx, cx, cy - rx);
            sequence.AddBezireTo(cx - ry * Maths.Kappa, cy - rx, cx - ry, cy - rx * Maths.Kappa, cx - ry, cy);
            sequence.AddClose();
            Add(sequence);
        }


        /// <summary>
        /// <inheritdoc cref="Docs.Paths"/>
        /// 
        /// Creates a new circle shaped sub-path.
        /// </summary>
        /// <param name="x">The circle's center x</param>
        /// <param name="y">The circle's center y</param>
        /// <param name="radius">The circle's radius</param>
        public void Circle(float x, float y, float radius)
        {
            Ellipse(x, y, radius, radius);
        }

    }
}
