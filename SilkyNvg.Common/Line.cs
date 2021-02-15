using System;
using System.Collections.Generic;
using System.Text;

namespace SilkyNvg.Common
{
    public static class Line
    {
        public static Instruction StraightLine(float x, float y)
        {
            Instruction instruction = new Instruction();
            instruction.Queue( Instruction.LINE, x, y);
            return instruction;
        }

        public static Instruction BezierLine(float x1, float y1, float x2, float y2, float x3, float y3)
        {
            Instruction instruction = new Instruction();
            instruction.Queue( Instruction.BEZIER, x1, y1, x2, y2, x3, y3);
            return instruction;
        }
    }
}
