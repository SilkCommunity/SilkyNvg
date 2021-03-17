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


        public void Arc(float xPos, float yPos, float radius, float alpha, float beta, Winding fill)
        {
            //float a, da, hda, kappa, dx, dy, x, y, tanx, tany, px, py, ptanx, ptany;
            //float[] values = new float[3 + 5*7 + 100];

            //int i, ndivs, nvals;
            //int move = 
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

            float halfw = MathF.Abs(w) * 0.5F;
            float halfh = MathF.Abs(h) * 0.5F;
            float rxBL = MathF.Min(rBL, halfw) * MathF.Sign(w);
            float ryBL = MathF.Min(rBL, halfh) * MathF.Sign(h);
            float rxBR = MathF.Min(rBR, halfw) * MathF.Sign(w);
            float ryBR = MathF.Min(rBR, halfh) * MathF.Sign(h);
            float rxTR = MathF.Min(rTR, halfw) * MathF.Sign(w);
            float ryTR = MathF.Min(rTR, halfh) * MathF.Sign(h);
            float rxTL = MathF.Min(rTL, halfw) * MathF.Sign(w);
            float ryTL = MathF.Min(rTL, halfh) * MathF.Sign(h);


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
