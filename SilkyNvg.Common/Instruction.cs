using System;
using System.Collections.Generic;
using System.Text;

namespace SilkyNvg.Common
{
    public sealed class Instruction
    {
        Queue<float> sequence = new Queue<float>();



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
