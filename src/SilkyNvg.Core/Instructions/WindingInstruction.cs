using SilkyNvg.Core.Paths;
using SilkyNvg.Core.States;
using SilkyNvg.Paths;

namespace SilkyNvg.Core.Instructions
{
    internal class WindingInstruction : IInstruction
    {

        public InstructionType Type => InstructionType.Winding;
        public float[] Data => new float[] { (float)InstructionType.Winding, (float)_direction };


        private readonly Winding _direction;

        public WindingInstruction(Winding direction)
        {
            _direction = direction;
        }

        public void Setup(State _) { }

        public void Execute(PathCache cache, Style style)
        {
            cache.LastPath().Winding = _direction;
        }

    }
}
