using System.Drawing;
using System.Numerics;

namespace SilkyNvg.Utils
{
    internal static class Conversions
    {

        internal static Vector2 ToVector2(this PointF point)
        {
            return new Vector2(point.X, point.Y);
        }

        internal static PointF ToPointF(this Vector2 vector)
        {
            return new PointF(vector.X, vector.Y);
        }
        
    }
}