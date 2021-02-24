using Silk.NET.Maths;

namespace SilkyNvg.Core
{
    public struct Colour
    {

        public float R
        {
            get => _rgba.X;
            set { _rgba.X = value; }
        }
        public float G
        {
            get => _rgba.Y;
            set { _rgba.Y = value; }
        }
        public float B
        {
            get => _rgba.Z;
            set { _rgba.Z = value; }
        }
        public float A
        {
            get => _rgba.W;
            set { _rgba.W = value; }
        }

        public Vector4D<float> Rgba => _rgba;

        private Vector4D<float> _rgba;

        private Colour(float r, float g, float b, float a)
        {
            _rgba = new Vector4D<float>(r, g, b, a);
        }

        private Colour(Vector4D<float> rgba)
        {
            _rgba = rgba;
        }

        /// <summary>
        /// Create a new Colour with a vector.
        /// All colours are represented between 0 and 1
        /// where 0 is 0 and 1 is 255.
        /// </summary>
        /// <param name="rgba">The vector.</param>
        /// <returns>A new colour from the vector.</returns>
        public static Colour FromRGBAf(Vector4D<float> rgba)
        {
            return new Colour(rgba);
        }


        /// <summary>
        /// Create a new Colour with a vector.
        /// All colours are represented between 0 and 1
        /// where 0 is 0 and 1 is 255.
        /// </summary>
        /// <param name="r">Red component</param>
        /// <param name="g">Green component</param>
        /// <param name="b">Blue component</param>
        /// <param name="a">Alpha component</param>
        /// <returns>A new colour with the specified values.</returns>
        public static Colour FromRGBAf(float r, float g, float b, float a)
        {
            return new Colour(r, g, b, a);
        }

    }
}
