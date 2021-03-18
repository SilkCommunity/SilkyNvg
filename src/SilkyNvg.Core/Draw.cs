using System;
using SilkyNvg.Common;
using SilkyNvg.Paths;
using SilkyNvg.Core.Instructions;
using SilkyNvg.Core.States;

namespace SilkyNvg.Core
{

    /// <summary>
    /// <inheritdoc cref="Docs.Paths"/>
    /// </summary>
    public sealed class Draw
    {

        private readonly InstructionQueue _instructionManager;
        private readonly StateManager _stateManager;

        internal Draw(InstructionQueue instructionQueue, StateManager stateManager)
        {
            _instructionManager = instructionQueue;
            _stateManager = stateManager;
        }

        private void Add(InstructionSequence iseq)
        {
            _instructionManager.AddSequence(iseq, _stateManager.GetState());
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
