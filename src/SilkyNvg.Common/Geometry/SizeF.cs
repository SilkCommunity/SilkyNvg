using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.InteropServices;

namespace SilkyNvg.Common.Geometry
{
    [StructLayout(LayoutKind.Sequential)]
    public struct SizeF(float width, float height) : IEquatable<SizeF>
    {

        public static readonly SizeF Zero = new(0, 0);
        public static readonly SizeF One = new(1, 1);

        public float Width = width;
        public float Height = height;

        public SizeF(float value)
            : this(value, value) { }

        public override bool Equals([NotNullWhen(true)] object obj)
        {
            return (obj is SizeU s) && Equals(s);
        }

        public readonly bool Equals(SizeF other)
        {
            return (Width == other.Width) && (Height == other.Height);
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Width, Height);
        }

        public override readonly string ToString()
        {
            return $"{{Width={Width} Height={Height}}}";
        }

        public static SizeF operator +(SizeF left, SizeF right)
            => Add(left, right);

        public static SizeF operator -(SizeF left, SizeF right)
            => Subtract(left, right);

        public static SizeF operator *(float factor, SizeF s)
            => Scale(factor, s);

        public static SizeF operator *(SizeF s, float factor)
            => Scale(factor, s);

        public static SizeF operator *(SizeF left, SizeF right)
            => Multiply(left, right);

        public static SizeF operator /(SizeF s, float value)
            => Scale(value, s);

        public static SizeF operator /(SizeF left, SizeF right)
            => Divide(left, right);

        public static bool operator ==(SizeF left, SizeF right)
            => left.Equals(right);

        public static bool operator !=(SizeF left, SizeF right)
            => !left.Equals(right);

        public static explicit operator SizeU(SizeF size) => new((uint)size.Width, (uint)size.Height);

        public static implicit operator Vector2(SizeF size) => new(size.Width, size.Height);

        public static implicit operator SizeF(Vector2 vector) => new(vector.X, vector.Y);

        public static SizeF Add(SizeF left, SizeF right)
        {
            return new SizeF(left.Width + right.Width, left.Height + right.Height);
        }

        public static SizeF Subtract(SizeF left, SizeF right)
        {
            return new SizeF(left.Width - right.Width, left.Height - right.Height);
        }

        public static SizeF Scale(float factor, SizeF s)
        {
            return new SizeF(factor * s.Width, factor * s.Height);
        }

        public static SizeF Multiply(SizeF left, SizeF right)
        {
            return new SizeF(left.Width * right.Width, left.Height * right.Height);
        }

        public static SizeF Divide(SizeF left, SizeF right)
        {
            return new SizeF(unchecked(left.Width / right.Width), unchecked(left.Height / right.Height));
        }

    }
}
