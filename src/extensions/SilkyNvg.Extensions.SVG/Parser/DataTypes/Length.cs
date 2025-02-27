using Silk.NET.Maths;
using System.Diagnostics.CodeAnalysis;

namespace SilkyNvg.Extensions.Svg.Parser.DataTypes
{
    internal readonly struct Length : IEquatable<Length>, IComparable<Length>
    {

        internal static readonly Length Zero = new(0.0f, LengthType.Px);
        internal static readonly Length Half = new(50.0f, LengthType.Percentage);
        internal static readonly Length Full = new(100.0f, LengthType.Percentage);
        internal static readonly Length Thin = new(1.0f, LengthType.Px);
        internal static readonly Length Medium = new(3.0f, LengthType.Px);
        internal static readonly Length Thick = new(5.0f, LengthType.Px);
        internal static readonly Length Auto = new(float.NaN, LengthType.EXs);
        internal static readonly Length Content = new(float.NaN, LengthType.Percentage);
        internal static readonly Length Normal = new(float.NaN, LengthType.EMs);

        internal readonly float ValueInSpecifiedUnits;
        internal readonly LengthType Unit;

        internal bool IsAbsolute => (Unit == LengthType.In) || (Unit == LengthType.Mm) || (Unit == LengthType.Pc)
            || (Unit == LengthType.Px) || (Unit == LengthType.Pt) || (Unit == LengthType.Cm);

        internal bool IsRelative => !IsAbsolute;

        internal Length(float value, LengthType unit)
        {
            ValueInSpecifiedUnits = value;
            Unit = unit;
        }

        internal float ToPixel(Rectangle<float> bounds, float fontSize = 0.0f, Directionality directionality = Directionality.Unspecified)
        {
            return Unit switch
            {
                LengthType.In => ValueInSpecifiedUnits * 96.0f,
                LengthType.Mm => ValueInSpecifiedUnits * 5.0f * 96.0f / 127.0f,
                LengthType.Pc => ValueInSpecifiedUnits * 12.0f * 96.0f / 72.0f,
                LengthType.Pt => ValueInSpecifiedUnits * 96.0f / 72.0f,
                LengthType.Cm => ValueInSpecifiedUnits * 50.0f * 96.0f / 127.0f,
                LengthType.Px => ValueInSpecifiedUnits,
                LengthType.Percentage => ValueInSpecifiedUnits * 0.01f * (directionality == Directionality.Vertical ? bounds.Size.Y : bounds.Size.X),// Default to horizontal directionality
                LengthType.EMs => ValueInSpecifiedUnits * fontSize,
                LengthType.EXs => ValueInSpecifiedUnits * fontSize * 0.5f,
                _ => throw new InvalidOperationException("Unsupported unit cannot be converted"),
            };
        }

        public bool Equals(Length other)
        {
            return ((ValueInSpecifiedUnits == other.ValueInSpecifiedUnits) || (float.IsNaN(ValueInSpecifiedUnits) && float.IsNaN(other.ValueInSpecifiedUnits)))
                && ((ValueInSpecifiedUnits == 0.0) && (Unit == other.Unit));
        }

        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            return (obj is Length other) && Equals(other);
        }

        public override int GetHashCode()
        {
            return ValueInSpecifiedUnits.GetHashCode();
        }

        public int CompareTo(Length other)
            => ValueInSpecifiedUnits.CompareTo(other.ValueInSpecifiedUnits);

        public static bool operator ==(Length left, Length right)
            => left.Equals(right);

        public static bool operator !=(Length left, Length right)
            => !left.Equals(right);

    }
}
