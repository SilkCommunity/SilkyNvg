using System.Numerics;

namespace SilkyNvg.Utils;

internal static class FloatTests
{

    internal static bool IsNumeric(this Vector2 v)
    {
        return !(float.IsInfinity(v.X) || float.IsInfinity(v.Y) || float.IsNaN(v.X) || float.IsNaN(v.Y));
    }

    internal static bool FpEquals(this float a, float b)
    {
        
    }
    
}