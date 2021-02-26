using Silk.NET.Maths;

namespace SilkyNvg.Core.Geometry
{
    public class Point
    {

        private readonly Vector3D<float> _position;
        private readonly uint _flags;

        public float X => _position.X;
        public float Y => _position.Y;
        public float Z => _position.Z;
        public Vector3D<float> Position => _position;

        public uint Flags => _flags;

        public Point(float x, float y, float z, uint flags) : this(new Vector3D<float>(x, y, z), flags) { }

        public Point(Vector3D<float> position, uint flags)
        {
            _position = position;
            _flags = flags;
        }

        public bool IsCorner()
        {
            return (_flags & (uint)PointFlags.PointCorner) != 0;
        }

        public bool IsLeft()
        {
            return (_flags & (uint)PointFlags.PointLeft) != 0;
        }

        public bool IsBevel()
        {
            return (_flags & (uint)PointFlags.PointBevel) != 0;
        }

        public bool IsInnerbevel()
        {
            return (_flags & (uint)PointFlags.PointInnerbevel) != 0;
        }

    }
}
