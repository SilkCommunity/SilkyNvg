using System;
using System.Collections.Generic;
using System.Text;
using SilkyNvg.Common;
using SilkyNvg.Paths;
using SilkyNvg.Core.Instructions;
using SilkyNvg.Core.States;

namespace SilkyNvg.Core
{
    public sealed class Shapes
    {

        private readonly InstructionQueue _instructionManager;
        private readonly StateManager _stateManager;


        internal Shapes(InstructionQueue instructionQueue, StateManager stateManager)
        {
            _instructionManager = instructionQueue;
            _stateManager = stateManager;
        }


        public void Arc(float cx, float cy, float r, float a0, float a1, Winding dir = Winding.CCW)
        {
            float a, da, hda, kappa, dx, dy, x, y, tanx, tany, px = 0, py = 0, ptanx = 0, ptany = 0;

            int ndivs;

            InstructionSequence instructionSequence;

            da = a1 - a0;

            if (dir == Winding.CW)
            {
                if (Maths.Abs(da) >= Maths.Pi * 2)
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
                if (Maths.Abs(da) >= Maths.Pi * 2)
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
            ndivs = Maths.Maxi(1, Maths.Mini((int)(Maths.Abs(da) / (Maths.Pi * 0.5F) + 0.5F), 5));
            hda = (da / (float)ndivs) / 2.0F;
            kappa = Maths.Abs(4.0F / 3.0F * (1.0F - Maths.Cos(hda)) / Maths.Sin(hda));

            if (dir == Winding.CCW)
                kappa = -kappa;

            instructionSequence = new InstructionSequence(ndivs + 1);

            for (int i = 0; i <= ndivs; i++)
            {
                a = a0 + da * (i / (float)ndivs);
                dx = Maths.Cos(a);
                dy = Maths.Sin(a);
                x = cx + dx * r;
                y = cy + dy * r;
                tanx = -dx * r * kappa;
                tany = dx * r * kappa;

                if (i == 0)
                {
                    if (_instructionManager.QueueLength > 0)
                        instructionSequence.AddLineTo(x, y);
                    else
                        instructionSequence.AddMoveTo(x, y);
                }
                else
                {
                    instructionSequence.AddBezireTo(px + ptanx, py + ptany, x - tanx, y - tany, x, y);
                }
                px = x;
                py = y;
                ptanx = tanx;
                ptany = tany;
            }
            Add(instructionSequence);
        }

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

        public void RoundedRect(float x, float y, float w, float h, float r)
        {
            RoundedRect(x, y, w, h, r, r, r, r);
        }

        public void RoundedRect(float x, float y, float w, float h, float rTL, float rTR, float rBR, float rBL)
        {
            if (rTL < 0.1F && rTR < 0.1F && rBL < 0.1F && rBR < 0.1F)
            { 
                Rect(x, y, w, h);
                return;
            }

            float halfw = Maths.Abs(w) * 0.5F;
            float halfh = Maths.Abs(h) * 0.5F;
            float rxBL = Maths.Min(rBL, halfw) * Maths.Sign(w);
            float ryBL = Maths.Min(rBL, halfh) * Maths.Sign(h);
            float rxBR = Maths.Min(rBR, halfw) * Maths.Sign(w);
            float ryBR = Maths.Min(rBR, halfh) * Maths.Sign(h);
            float rxTR = Maths.Min(rTR, halfw) * Maths.Sign(w);
            float ryTR = Maths.Min(rTR, halfh) * Maths.Sign(h);
            float rxTL = Maths.Min(rTL, halfw) * Maths.Sign(w);
            float ryTL = Maths.Min(rTL, halfh) * Maths.Sign(h);


            //Needs testing :D
            InstructionSequence sequence = new InstructionSequence(10);
            sequence.AddMoveTo(x, y + ryTL);
            sequence.AddLineTo(x, y + h - ryBL);
            sequence.AddBezireTo(x, y + h - ryBL * (1F - Maths.Kappa), x + rxBL * (1F - Maths.Kappa), y + h, x + rxBL, y + h);
            sequence.AddLineTo(x + w -rxBR, y + h);
            sequence.AddBezireTo(x + w - rxBR * (1F - Maths.Kappa), y + h, x + w, y + h - ryBR * (1F - Maths.Kappa), x + w, y + h - ryBR);
            sequence.AddLineTo(x, y + h + ryTR);
            sequence.AddBezireTo(x + w, y + ryTR * (1F - Maths.Kappa), x + w - rxTR * (1F - Maths.Kappa), y, x + w - rxTR, y);
            sequence.AddLineTo(x + rxTL, y);
            sequence.AddBezireTo(x + rxTL * (1F - Maths.Kappa), y, x , y + ryTL * (1F - Maths.Kappa), x, y + ryTL);
            sequence.AddClose();

            Add(sequence);
        }

        public void Ellipse(float cx, float cy, float rx, float ry)
        {
            InstructionSequence sequence = new InstructionSequence(6);
            sequence.AddMoveTo(cx - ry, cy);
            sequence.AddBezireTo(cx - ry, cy + rx * Maths.Kappa, cx - ry * Maths.Kappa, cy + rx, cx, cy + rx);
            sequence.AddBezireTo(cx + ry * Maths.Kappa, cy + rx, cx + ry, cy + rx * Maths.Kappa, cx + ry, cy);
            sequence.AddBezireTo(cx + ry, cy - rx * Maths.Kappa, cx + ry * Maths.Kappa, cy - rx, cx, cy - rx);
            sequence.AddBezireTo(cx - ry * Maths.Kappa, cy - rx, cx - ry, cy - rx * Maths.Kappa, cx - ry, cy);
            sequence.AddClose();
            Add(sequence);
        }

        public void Circle(float x, float y, float radius)
        {
            Ellipse(x, y, radius, radius);
        }

        private void Add(InstructionSequence iseq)
        {
            _instructionManager.AddSequence(iseq, _stateManager.GetState());
        }

        

    }
}
