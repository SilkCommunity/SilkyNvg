using Silk.NET.Maths;

namespace SilkyNvg
{
    public struct Colour
    {

        private Vector4D<float> _rgba;

        public Vector4D<float> Rgba => _rgba;
        public float R => _rgba.X;
        public float G => _rgba.Y;
        public float B => _rgba.Z;
        public float A => _rgba.W;

        /// <summary>
        /// Create a new Colour from the rgba components
        /// between 0 and 1.
        /// </summary>
        /// <param name="rgba">The Rgba components</param>
        public Colour(Vector4D<float> rgba)
        {
            _rgba = rgba;
        }

        /// <summary>
        /// Create a <see cref="Colour"/> from the rgb components
        /// between 0 and 255. Alpha will be 255.
        /// </summary>
        /// <param name="r">The red component (0 - 255)</param>
        /// <param name="g">The green component (0 - 255)</param>
        /// <param name="b">The blue component (0 - 255)</param>
        public Colour(byte r, byte g, byte b) : this(new Vector4D<float>((float)r / 255.0f, (float)g / 255.0f, (float)b / 255.0f, 1.0f)) { }

        /// <summary>
        /// Create a <see cref="Colour"/> from the rgb components
        /// between 0 and 1. Alpha will be 1.
        /// </summary>
        /// <param name="r">The red component (0 - 1)</param>
        /// <param name="g">The green component (0 - 1)</param>
        /// <param name="b">The blue component (0 - 1)</param>
        public Colour(float r, float g, float b) : this(new Vector4D<float>(r, g, b, 1.0f)) { }

        /// <summary>
        /// Create a new <see cref="Colour"/> from the rgba components
        /// between 0 and 255.
        /// </summary>
        /// <param name="r">The red component (0 - 255)</param>
        /// <param name="g">The green component (0 - 255)</param>
        /// <param name="b">The blue component (0 - 255)</param>
        /// <param name="a">The alpha component (0 - 255)</param>
        public Colour(byte r, byte g, byte b, byte a) : this(new Vector4D<float>((float)r / 255.0f, (float)g / 255.0f, (float)b / 255.0f, (float)a / 255.0f))  { }

        /// <summary>
        /// Create a new <see cref="Colour"/> from the rgba components
        /// between 0 and 1.
        /// </summary>
        /// <param name="r">The red component (0 - 1)</param>
        /// <param name="g">The green component (0 - 1)</param>
        /// <param name="b">The blue component (0 - 1)</param>
        /// <param name="a">The alpha component (0 - 1)</param>
        public Colour(float r, float g, float b, float a) : this(new Vector4D<float>(r, g, b, a)) { }

    }
}
