using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace SilkyNvg.Common.Geometry
{
    [StructLayout(LayoutKind.Sequential)]
    public struct SizeU(uint width, uint height) : IEquatable<SizeU>
    {

        public static readonly SizeU Zero = new(0, 0);
        public static readonly SizeU One = new(1, 1);

        public uint Width = width;
        public uint Height = height;

        public SizeU(uint value)
            : this(value, value) { }

        public override readonly bool Equals([NotNullWhen(true)] object obj)
        {
            return (obj is SizeU s) && Equals(s);
        }

        public readonly bool Equals(SizeU other)
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

        public static SizeU operator +(SizeU left, SizeU right)
            => Add(left, right);

        public static SizeU operator -(SizeU left, SizeU right)
            => Subtract(left, right);

        public static SizeU operator *(float factor, SizeU s)
            => Scale(factor, s);

        public static SizeU operator *(SizeU s, float factor)
            => Scale(factor, s);

        public static SizeU operator *(SizeU left, SizeU right)
            => Multiply(left, right);

        public static SizeU operator /(SizeU s, float value)
            => Scale(value, s);

        public static SizeU operator /(SizeU left, SizeU right)
            => Divide(left, right);

        public static bool operator ==(SizeU left, SizeU right)
            => Equals(left, right);

        public static bool operator !=(SizeU left, SizeU right)
            => !Equals(left, right);

        public static implicit operator SizeF(SizeU size) => new(size.Width, size.Height);

        public static SizeU Add(SizeU left, SizeU right)
        {
            return new SizeU(left.Width + right.Width, left.Height + right.Height);
        }

        public static SizeU Subtract(SizeU left, SizeU right)
        {
            return new SizeU(left.Width - right.Width, left.Height - right.Height);
        }

        public static SizeU Scale(float factor, SizeU s)
        {
            return new SizeU((uint)(factor * s.Width), (uint)(factor * s.Height));
        }

        public static SizeU Multiply(SizeU left, SizeU right)
        {
            return new SizeU(left.Width * right.Width, left.Height * right.Height);
        }

        public static SizeU Divide(SizeU left, SizeU right)
        {
            return new SizeU(unchecked(left.Width / right.Width), unchecked(left.Height / right.Height));
        }

    }
}
