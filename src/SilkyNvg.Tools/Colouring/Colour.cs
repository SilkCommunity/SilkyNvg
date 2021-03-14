using Silk.NET.Maths;

namespace SilkyNvg.Colouring
{
    public struct Colour
    {

        private Vector4D<float> _rgba;

        /// <summary>
        /// X  is R
        /// Y is G
        /// Z is B
        /// W is A
        /// </summary>
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
        /// <inheritdoc cref="Common.Docs.Colours"/>
        /// 
        /// Create a new Colour from the rgba components
        /// between 0 and 1.
        /// </summary>
        /// <param name="rgba">The Rgba components</param>
        public Colour(Vector4D<float> rgba)
        {
            _rgba = rgba;
        }

        /// <summary>
        /// <inheritdoc cref="Common.Docs.Colours"/>
        /// </summary>
        /// <param name="r">The red component.</param>
        /// <param name="g">The green component.</param>
        /// <param name="b">The blue component.</param>
        /// <param name="a">The alpha component.</param>
        /// <returns>A colour value from red, green, blue and alpha values.</returns>
        public Colour(float r, float g, float b, float a) : this(new Vector4D<float>(r, g, b, a)) { }

        /// <summary>
        /// Premultiplies the colour by multiplying each value with the alpha value.
        /// </summary>
        /// <param name="colour">The colour to be premultiplied.</param>
        /// <returns>The premultiplied colour.</returns>
        public static Colour Premult(Colour colour)
        {
            return new Colour(colour.R * colour.A, colour.G * colour.A, colour.B * colour.A, colour.A);
        }

    }
}