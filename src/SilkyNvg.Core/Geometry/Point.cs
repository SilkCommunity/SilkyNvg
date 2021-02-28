using Silk.NET.Maths;

namespace SilkyNvg.Core.Geometry
{
    public class Point
    {

        public static bool PtEquals(Point p1, Point p2, float tol)
        {
            return Maths.PtEquals(p1.X, p1.Y, p2.X, p2.Y, tol);
        }

        public static bool PtEquals(Point p, float x, float y, float tol)
        {
            return Maths.PtEquals(p.X, p.Y, x, y, tol);
        }

        private readonly Vector2D<float> _position;

        private uint _flags;
        private Vector2D<float> _d;
        private float _length;
        private Vector2D<float> _dm;

        public float X => _position.X;
        public float Y => _position.Y;
        public Vector2D<float> Position => _position;

        public float Dx
        {
            get => _d.X;
            set => _d.X = value;
        }

        public float Dy
        {
            get => _d.Y;
            set => _d.Y = value;
        }

        public Vector2D<float> D
        {
            get => _d;
            set => _d = value;
        }

        public float DMx
        {
            get => _dm.X;
            set => _dm.X = value;
        }

        public float DMy
        {
            get => _dm.Y;
            set => _dm.Y = value;
        }

        public Vector2D<float> DM
        {
            get => _dm;
            set => _dm = value;
        }

        public float Length
        {
            get => _length;
            set => _length = value;
        }

        public uint Flags
        {
            get => _flags;
            set => _flags = value;
        }

        public Point(float x, float y, uint flags) : this(new Vector2D<float>(x, y), flags) { }

        public Point(Vector2D<float> position, uint flags)
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

        public bool Equals(float x, float y, float tol)
        {
            return PtEquals(this, x, y, tol);
        }

    }
}
