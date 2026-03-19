using System.Numerics;

namespace SilkyNvg.Utils
{
    internal static class Maths
    {

        internal static bool IsNumeric(this float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }

        internal static bool IsNumeric(this Vector2 value)
        {
            return value.X.IsNumeric() && value.Y.IsNumeric();
        }
        
    }
}