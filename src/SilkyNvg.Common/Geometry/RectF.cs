using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.InteropServices;

namespace SilkyNvg.Common.Geometry
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct RectF(Vector2 location, SizeF size) : IEquatable<RectF>
    {

        public static readonly RectF Empty = new(Vector2.Zero, SizeF.Zero);

        public Vector2 Location = location;
        public SizeF Size = size;

        public Vector2 Min
        {
            readonly get => Location;
            set
            {
                Vector2 max = Max;
                Location = value;
                Size = max - Location;
            }
        }

        public Vector2 Max
        {
            readonly get => Location + (Vector2)Size;
            set => Size = value - Min;
        }

        public Vector2 Center
        {
            readonly get => Location + (Vector2)Size / 2;
            set => Location = value - (Vector2)Size / 2;
        }

        public float X
        {
            readonly get => Location.X;
            set => Location.X = value;
        }

        public float Y
        {
            readonly get => Location.Y;
            set => Location.Y = value;
        }

        public float Width
        {
            readonly get => Size.Width;
            set => Size.Width = value;
        }

        public float Height
        {
            readonly get => Size.Height;
            set => Size.Height = value;
        }

        public readonly bool IsEmpty => (Size.Width <= 0) || (Size.Height <= 0);

        public RectF(float x, float y, float width, float height)
            : this(new Vector2(x, y), new SizeF(width, height)) { }

        public readonly bool Contains(Vector2 point)
        {
            return (point.X >= Min.X) && (point.Y >= Min.Y) && (point.X <= Max.X) && (point.Y <= Max.Y);
        }

        public readonly bool Contains(float x, float y)
            => Contains(new Vector2(x, y));

        public readonly bool Contains(RectF other)
        {
            return (Min.X <= other.Min.X) && (Min.Y <= other.Min.Y) && (Max.X >= other.Max.X) && (Max.Y >= other.Max.Y);
        }

        public readonly bool IsIntersecting(RectF other)
        {
            return (other.Min.X < Max.X) && (other.Max.X > Min.X) && (other.Min.Y < Max.Y) && (other.Max.Y > Min.Y);
        }

        public readonly RectF Intersect(RectF other)
        {
            Vector2 min = Vector2.Max(Min, other.Min);
            Vector2 max = Vector2.Min(Max, other.Max);

            if ((min.X <= max.X) && (min.Y <= max.Y))
            {
                return FromMinMax(min, max);
            }

            return Empty.At(min);
        }

        public void Inflate(Vector2 delta)
        {
            Location -= delta;
            Size += 2 * (SizeF)delta;
        }

        public void Inflate(float dx, float dy)
            => Inflate(new Vector2(dx, dy));

        public void InflateFactor(float factorX, float factorY)
            => Inflate(Size.Width * factorX, Size.Height * factorY);

        public void Move(Vector2 delta)
        {
            Location += delta;
        }

        public void Move(float dx, float dy)
            => Move(new Vector2(dx, dy));

        public readonly RectF At(Vector2 location)
            => new(location, Size);

        public readonly RectF At(float x, float y)
            => At(new Vector2(x, y));

        public override readonly bool Equals([NotNullWhen(true)] object obj)
        {
            return (obj is RectF other) && Equals(other);
        }

        public readonly bool Equals(RectF other)
        {
            return (Location == other.Location) && (Size == other.Size);
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Location, Size);
        }

        public override readonly string ToString()
        {
            return $"{{location={Location} size={Size}}}";
        }

        public static bool operator ==(RectF left, RectF right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(RectF left, RectF right)
        {
            return !left.Equals(right);
        }

        public static explicit operator RectU(RectF rect)
            => new((uint)rect.X, (uint)rect.Y, (uint)rect.Width, (uint)rect.Height);

        public static implicit operator Vector4(RectF rect)
            => new(rect.X, rect.Y, rect.Width, rect.Height);

        public static implicit operator RectF(Vector4 vector)
            => new(vector.X, vector.Y, vector.Z, vector.W);

        public static RectF FromMinMax(Vector2 min, Vector2 max)
            => new(min, max - min);

        public static RectF FromLTRB(float left, float top, float right, float bottom)
            => FromMinMax(new Vector2(left, top), new Vector2(right, bottom));

    }
}
