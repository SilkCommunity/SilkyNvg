using System;
using System.Collections.Generic;
using System.Text;

namespace SilkyNvg.Common
{
    public sealed class Instruction
    {
        private float[] todo;

        public static Instruction Move(float x, float y)
        {
            return null;
        }

        public static Instruction Line(float x, float y)
        {
            return null;
        }

        public static Instruction Bezier(float x, float y)
        {
            return null;
        }

        public static Instruction Close()
        {
            return null;
        }

        public static Instruction Winding()
        {
            return null;
        }
    }
}
