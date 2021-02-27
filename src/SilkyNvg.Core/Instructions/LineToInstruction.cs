using Silk.NET.Maths;

namespace SilkyNvg.Core.Instructions
{
    public class LineToInstruction : IInstruction
    {
        public bool RequiresPosition => true;

        private readonly Vector2D<float> _position;

        public Vector2D<float> Position => _position;

        internal LineToInstruction(float x, float y)
        {
            _position = new Vector2D<float>(x, y);
        }

        public void Prepare()
        {

        }

        public void Execute()
        {

        }

    }
}
