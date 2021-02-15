using System;
using System.Collections.Generic;
using System.Text;

namespace SilkyNvg.Common
{
    public sealed class Instruction
    {
        Queue<float> sequence = new Queue<float>();

        //Constant Placeholders
        public const ushort CLOSE = (1 << 0);
        public const ushort MOVE = (1 << 1);
        public const ushort LINE =  (1 << 2);
        public const ushort BEZIER = (1 << 3);
        public const ushort WINDING = (1 << 4);

        public void Queue(float instruction, params float[] args)
        {
            sequence.Enqueue(instruction);
            foreach (float arg in args)
            {
                sequence.Enqueue(arg);
            }
        }


        //Basic Default Instructions
        public void Move(float x, float y)
        {
            Queue(MOVE);
            Queue(x);
            Queue(y);
        }

        public void Line(float x, float y)
        {
            Queue(LINE);
            Queue(x);
            Queue(y);
        }

        public void Bezier(float x1, float y1, float x2, float y2, float x3, float y3)
        {
            Queue(BEZIER);
            Queue(x1);
            Queue(y1);
            Queue(x2);
            Queue(y2);
            Queue(x3);
            Queue(y3);
        }

        //Do we need that
        public void Close()
        {
            Queue(CLOSE);
        }

        public void Winding(float w)
        {
            Queue(WINDING);
        }

        //Merges two Queues of the Instructions
        public static Instruction operator +(Instruction instruction_a, Instruction instruction_b)
        {
            Instruction new_instruction = new Instruction();
            float[] array_a = instruction_a.sequence.ToArray();
            float[] array_b = instruction_b.sequence.ToArray();

            float[] combinedArray = new float[array_a.Length + array_b.Length];
            Array.Copy(array_a, combinedArray, array_a.Length);
            Array.Copy(array_b, 0, combinedArray, array_a.Length, array_b.Length);

            new_instruction.sequence = new Queue<float>(combinedArray);

            return new_instruction;
        }

    }
}
