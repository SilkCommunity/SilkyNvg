using Silk.NET.Maths;

namespace SilkyNvg.Colouring
{
    public struct Colour
    {

        private Vector4D<float> _rgba;

        public Vector4D<float> Rgba
        {
            get => _rgba;
            set => _rgba = value;
        }
        public float R
        {
            get => _rgba.X;
            set => _rgba.X = value;
        }
        public float G
        {
            get => _rgba.Y;
            set => _rgba.Y = value;
        }
        public float B
        {
            get => _rgba.Z;
            set => _rgba.Z = value;
        }
        public float A
        {
            get => _rgba.W;
            set => _rgba.W = value;
        }

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
        public Colour(byte r, byte g, byte b, byte a) : this(new Vector4D<float>((float)r / 255.0f, (float)g / 255.0f, (float)b / 255.0f, (float)a / 255.0f)) { }

        /// <summary>
        /// Create a new <see cref="Colour"/> from the rgba components
        /// between 0 and 1.
        /// </summary>
        /// <param name="r">The red component (0 - 1)</param>
        /// <param name="g">The green component (0 - 1)</param>
        /// <param name="b">The blue component (0 - 1)</param>
        /// <param name="a">The alpha component (0 - 1)</param>
        public Colour(float r, float g, float b, float a) : this(new Vector4D<float>(r, g, b, a)) { }


        public static Colour Premult(Colour colour)
        {
            return new Colour(colour.R * colour.A, colour.G * colour.A, colour.B * colour.A, colour.A);
        }

    }
}