using Silk.NET.Maths;

namespace SilkyNvg.Core
{
    public sealed class Maths
    {

        public static Vector2D<float> TransformPoint(float sx, float sy, params float[] t)
        {
            float dx = sx * t[0] + sy * t[2] + t[4];
            float dy = sx * t[1] + sy * t[3] + t[5];
            return new Vector2D<float>(dx, dy);
        }

    }
}
