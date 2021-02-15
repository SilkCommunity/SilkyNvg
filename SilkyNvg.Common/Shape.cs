using System;

namespace SilkyNvg.Common
{
    public static class Shape
    {

        //Return Commands instead of void?
        public static Instruction Ellipse(float cx, float cy, float rx, float ry)
        {
            Instruction instruction = new Instruction();
            //instruction.Queue(Instruction.MOVE, cx - rx, cy);
            //instruction.Queue(Instruction.BEZIER, cx - rx, cy + ry);
            return instruction;
        }

        //Return Commands instead of void?
        public static Instruction Circle(float cx, float cy, float r)
        {
            return Ellipse( cx, cy, r, r);
        }

    }
}
