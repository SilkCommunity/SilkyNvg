using System.Numerics;

namespace SilkyNvg.Core.Paths
{
    internal class Point
    {

        private readonly Vector2 _position;

        public PointFlags Flags { get; set; }

        public Vector2 Position => _position;

        public Vector2 Determinant { get; set; }

        public float Length { get; set; }

        public Vector2 MatrixDeterminant { get; set; }

        public Point(Vector2 pos, PointFlags flags)
        {
            _position = pos;
            Flags = flags;
        }

    }
}
