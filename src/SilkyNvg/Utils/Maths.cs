using System.Numerics;

namespace SilkyNvg.Utils
{
    internal static class Maths
    {

        internal static bool IsAlgebraic(this float x)
        {
            return float.IsNaN(x) || float.IsInfinity(x);
        }

        internal static bool IsAlgebraic(this Vector2 v)
        {
            return v.X.IsAlgebraic() && v.Y.IsAlgebraic();
        }
        
    }
}