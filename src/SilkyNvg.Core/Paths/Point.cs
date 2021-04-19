using Silk.NET.Maths;
using SilkyNvg.Common;

namespace SilkyNvg.Core.Paths
{
    internal class Point
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
        private Vector2D<float> _determinant;
        private float _length;
        private Vector2D<float> _matrixDeterminant;

        public float X => _position.X;
        public float Y => _position.Y;
        public Vector2D<float> Position => _position;

        public float Dx
        {
            get => _determinant.X;
            set => _determinant.X = value;
        }

        public float Dy
        {
            get => _determinant.Y;
            set => _determinant.Y = value;
        }

        public Vector2D<float> Determinant
        {
            get => _determinant;
            set => _determinant = value;
        }

        public float DMx
        {
            get => _matrixDeterminant.X;
            set => _matrixDeterminant.X = value;
        }

        public float DMy
        {
            get => _matrixDeterminant.Y;
            set => _matrixDeterminant.Y = value;
        }

        public Vector2D<float> MatrixDeterminat
        {
            get => _matrixDeterminant;
            set => _matrixDeterminant = value;
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
            return (_flags & (uint)PointFlags.Corner) != 0;
        }

        public bool IsLeft()
        {
            return (_flags & (uint)PointFlags.Left) != 0;
        }

        public bool IsBevel()
        {
            return (_flags & (uint)PointFlags.Bevel) != 0;
        }

        public bool IsInnerbevel()
        {
            return (_flags & (uint)PointFlags.Innerbevel) != 0;
        }

        public void Flag(PointFlags flag)
        {
            if ((_flags & (uint)flag) == 0)
                _flags |= (uint)flag;
        }

        public bool Equals(float x, float y, float tol)
        {
            return PtEquals(this, x, y, tol);
        }

    }
}