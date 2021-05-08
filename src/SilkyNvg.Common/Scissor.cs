using System.Numerics;

namespace SilkyNvg.Common
{
    internal class Scissor
    {

        private Matrix3x2 _transform;
        private Vector2 _extent;

        public Scissor()
        {
            _extent = new Vector2(-1);
        }

    }
}
