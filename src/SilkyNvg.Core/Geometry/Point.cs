using Silk.NET.Maths;

namespace SilkyNvg.Core.Geometry
{
    public class Point
    {

        private Vector2D<float> _position;
        private Vector2D<float> _delta;
        private Vector2D<float> _dm;
        private uint _flags;
        private float _length;

        public float X => _position.X;
        public float Y => _position.Y;
        public Vector2D<float> Position
        {
            get => _position;
            set
            {
                _position = value;
            }
        }
        public Vector2D<float> Delta => _delta;
        public float DX
        {
            get => Delta.X;
            set
            {
                _delta.X = value;
            }
        }
        public float DY
        {
            get => Delta.Y;
            set
            {
                _delta.Y = value;
            }
        }
        public float Length
        {
            get => _length;
            set
            {
                _length = value;
            }
        }
        public Vector2D<float> DM => _dm;
        public float DMX
        {
            get => _dm.X;
            set
            {
                _dm.X = value;
            }
        }
        public float DMY
        {
            get => _dm.Y;
            set
            {
                _dm.Y = value;
            }
        }

        public uint Flags
        {
            get => _flags;
            set
            {
                _flags = value;
            }
        }

        public Point(float x, float y, uint flags) : this(new Vector2D<float>(x, y), flags) { }

        public Point(Vector2D<float> position, uint flags)
        {
            _position = position;
            _flags = flags;
            _delta = Vector2D<float>.Zero;
            _length = _position.Length;
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
