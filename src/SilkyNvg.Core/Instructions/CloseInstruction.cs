using SilkyNvg.Core.Paths;
using System;

namespace SilkyNvg.Core.Instructions
{
    public class CloseInstruction : IInstruction
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

        public void FlattenPath(PathCache cache, Style _)
        {
            cache.ClosePath();
        }

        public void Execute()
        {
            throw new NotImplementedException();
        }

    }
}
