using SilkyNvg.Core;
using SilkyNvg.Paths;
using System;

namespace SilkyNvg.Instructions
{
    internal class CloseInstruction : IInstruction
    {

        public const float INSTRUCTION_ID = 3.0F;

        public bool RequiresPosition => false;

        public float ID => INSTRUCTION_ID;

        public float[] FieldsAsFloatArray
        {
            get
            {
                return new float[] { };
            }
        }

        public void Prepare() { }

        public void FlattenPath(PathCache _, Style _2) { }

        public void Execute()
        {
            throw new NotImplementedException();
        }

    }
}
