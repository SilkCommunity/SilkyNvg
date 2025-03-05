using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace SilkyNvg.Common.Geometry
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct RectU(uint x, uint y, uint width, uint height) : IEquatable<RectU>
    {

        public static readonly RectU Empty = new(0, 0, 0, 0);

        public uint X = x;
        public uint Y = y;
        public uint Width = width;
        public uint Height = height;

        public uint MinX
        {
            readonly get => X;
            set
            {
                uint max = MaxX;
                X = value;
                Width = max - X;
            }
        }

        public uint MinY
        {
            readonly get => Y;
            set
            {
                uint max = MaxY;
                Y = value;
                Height = max - Y;
            }
        }

        public uint MaxX
        {
            readonly get => X + Width;
            set => X = value - Width;
        }

        public uint MaxY
        {
            readonly get => Y + Height;
            set => Y = value - Height;
        }

        public uint CenterX
        {
            readonly get => X + (Width / 2);
            set => X = value - (Width / 2);
        }

        public uint CenterY
        {
            readonly get => Y + (Height / 2);
            set => Y = value - (Height / 2);
        }

        public readonly bool IsEmpty => (Width == 0) || (Height == 0);

        public readonly bool Contains(uint x, uint y)
        {
            return (x >= MinX) && (y >= MinY) && (x <= MaxX) && (y <= MaxY);
        }

        public readonly bool Contains(RectU other)
        {
            return (MinX <= other.MinX) && (MinY <= other.MinY) && (MaxX >= other.MaxX) && (MaxY >= other.MaxY);
        }

        public readonly bool IsIntersecting(RectU other)
        {
            return (other.MinX < MaxX) && (other.MaxX > MinX) && (other.MinY < MaxY) && (other.MaxY > MinY);
        }

        public readonly RectU Intersect(RectU other)
        {
            uint minX = Math.Max(MinX, other.MinX);
            uint minY = Math.Max(MinY, other.MinY);
            uint maxX = Math.Min(MaxX, other.MaxX);
            uint maxY = Math.Min(MaxY, other.MaxY);

            if ((minX <= maxX) && (minY <= maxY))
            {
                return FromLTRB(minX, minY, maxX, maxY);
            }

            return Empty;
        }

        public void Inflate(uint deltaX, uint deltaY)
        {
            X -= deltaX;
            Y -= deltaY;
            Width += 2 * deltaX;
            Height += 2 * deltaY;
        }

        public void InflateFactor(float factorX, float factorY)
            => Inflate((uint)(Width * factorX), (uint)(Height * factorY));

        public void Move(uint deltaX, uint deltaY)
        {
            X += deltaX;
            Y += deltaY;
        }

        public override readonly bool Equals([NotNullWhen(true)] object obj)
        {
            return (obj is RectU other) && Equals(other);
        }

        public readonly bool Equals(RectU other)
        {
            return (X == other.X) && (Y == other.Y) && (Width == other.Width) && (Height == other.Height);
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(X, Y, Width, Height);
        }

        public override readonly string ToString()
        {
            return $"{{x={X} y={Y} width={Width} height={Height}}}";
        }

        public static bool operator ==(RectU left, RectU right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(RectU left, RectU right)
        {
            return !left.Equals(right);
        }

        public static implicit operator RectF(RectU rect)
            => new(rect.X, rect.Y, rect.Width, rect.Height);

        public static RectU FromLTRB(uint left, uint top, uint right, uint bottom)
            => new(left, top, right - left, bottom - top);

    }
}
