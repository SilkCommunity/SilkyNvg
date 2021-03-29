using Silk.NET.Maths;
using System;

namespace SilkyNvg.Colouring
{

    /// <summary>
    /// <inheritdoc cref="Common.Docs.Paints"/>
    /// </summary>
    public class Paint : ICloneable
    {

        private Matrix3X2<float> _xform;
        private Vector2D<float> _extent;
        private float _radius;
        private float _feather;

        private Colour _innerColour;
        private Colour _outerColour;

        private int _image;

        internal Matrix3X2<float> XForm
        {
            get => _xform;
            set => _xform = value;
        }

        internal Vector2D<float> Extent
        {
            get => _extent;
            set => _extent = value;
        }

        internal float Radius
        {
            get => _radius;
            set => _radius = value;
        }

        internal float Feather
        {
            get => _feather;
            set => _feather = value;
        }

        internal Colour InnerColour
        {
            get => _innerColour;
            set => _innerColour = value;
        }

        internal Colour OuterColour
        {
            get => _outerColour;
            set => _outerColour = value;
        }

        internal int Image
        {
            get => _image;
            set => _image = value;
        }

        private Paint() { }

        /// <summary>
        /// <inheritdoc cref="Paint"/>
        /// 
        /// Create a paint from a <see cref="Colour"/>.
        /// </summary>
        /// <param name="colour">The colour the paint will have.</param>
        public Paint(Colour colour)
        {
            _xform = Matrix3X2<float>.Identity;
            _radius = 0.0f;
            _feather = 1.0f;
            _innerColour = colour;
            _outerColour = colour;
        }

        /// <summary>
        /// <inheritdoc cref="Paint"/>
        /// 
        /// Creates and returns a linear gradient.
        /// 
        /// The gradient is transformed by the current transform when it is passed to FillPaint or StrokePaint.
        /// </summary>
        /// <param name="startX">The start X coordinate</param>
        /// <param name="startY">The start Y coordinate</param>
        /// <param name="endX">The end X coordinate</param>
        /// <param name="endY">The end Y coordinate</param>
        /// <param name="innerColour">The start colour</param>
        /// <param name="outerColour">The end colour</param>
        /// <returns>A new linear gradient paint.</returns>
        public static Paint LinearGradient(float startX, float startY, float endX, float endY, Colour innerColour, Colour outerColour)
        {
            const float large = 1e5f;

            var paint = new Paint();

            float dx = endX - startX;
            float dy = endY - startY;
            float d = MathF.Sqrt(dx * dx + dy * dy);
            if (d > 0.0001f)
            {
                dx /= d;
                dy /= d;
            }
            else
            {
                dx = 0;
                dy = 1;
            }

            paint.XForm = new Matrix3X2<float>
            {
                M11 = dy,
                M12 = -dx,
                M21 = dx,
                M22 = dy,
                M31 = startX - dx * large,
                M32 = startY - dy * large
            };

            paint.Extent = new Vector2D<float>(large, large + d * 0.5f);

            paint.Radius = 0.0f;

            paint.Feather = MathF.Max(1.0f, d);

            paint.InnerColour = innerColour;
            paint.OuterColour = outerColour;

            return paint;
        }

        /// <summary>
        /// <inheritdoc cref="Paint"/>
        /// 
        /// Creates and returns a box gradient. Box gradient is a feathered rounded rectangle. It is usefull for
        /// rendering drop shadows or hilights for boxes.
        /// 
        /// The gradient is transformed by the current transform when it is passed to FillPaint or StrokePaint.
        /// </summary>
        /// <param name="x">The top-left X Position of the rectangle</param>
        /// <param name="y">The top-left Y Position of the rectangle</param>
        /// <param name="width">The width of the rectangle</param>
        /// <param name="height">The height of the rectangle</param>
        /// <param name="radius">The corner radius</param>
        /// <param name="feather">Defines how blurry the border is</param>
        /// <param name="innerColour">The gradient's inner colour</param>
        /// <param name="outerColour">The gradient's outer colour</param>
        /// <returns>A new box gradient paint.</returns>
        public static Paint BoxGradient(float x, float y, float width, float height, float radius, float feather, Colour innerColour, Colour outerColour)
        {
            var paint = new Paint();

            var xform = Matrix3X2<float>.Identity;
            xform.M31 = x + width * 0.5f;
            xform.M32 = y + height * 0.5f;
            paint.XForm = xform;

            paint.Extent = new Vector2D<float>(width * 0.5f, height * 0.5f);

            paint.Radius = radius;

            paint.Feather = MathF.Max(feather, 1.0f);

            paint.InnerColour = innerColour;
            paint.OuterColour = outerColour;

            return paint;
        }

        /// <summary>
        /// <inheritdoc cref="Paint"/>
        /// 
        /// Creates and returns a radial gradient.
        /// 
        /// The gradient is transformed by the current transform when passed into FillPaint or StrokePaint.
        /// </summary>
        /// <param name="x">The centre X Position</param>
        /// <param name="y">The centre Y Position</param>
        /// <param name="innerRadius">The gradient's inner radius</param>
        /// <param name="outerRadius">The gradient's outer radius</param>
        /// <param name="innerColour">The start colour</param>
        /// <param name="outerColour">The end colour</param>
        /// <returns>A new radial gradient paint.</returns>
        public static Paint RadialGradient(float x, float y, float innerRadius, float outerRadius, Colour innerColour, Colour outerColour)
        {
            float r = (innerRadius + outerRadius) * 0.5f;
            float f = outerRadius - innerRadius;

            var paint = new Paint();

            var xform = Matrix3X2<float>.Identity;
            xform.M31 = x;
            xform.M32 = y;
            paint.XForm = xform;

            paint.Extent = new Vector2D<float>(r);

            paint.Radius = r;

            paint.Feather = MathF.Max(f, 1.0f);

            paint.InnerColour = innerColour;
            paint.OuterColour = outerColour;

            return paint;
        }

        /// <summary>
        /// <inheritdoc cref="Paint"/>
        /// 
        /// Creates and returns an image pattern.<br/>
        /// The gradient is transformed by the current transformation when it is passed to FillPaint() or StrokePaint().
        /// </summary>
        /// <param name="x">The left location of the image pattern.</param>
        /// <param name="y">The top location of the image pattern.</param>
        /// <param name="width">The width of one image.</param>
        /// <param name="height">The height of one image.</param>
        /// <param name="angle">The angle rotation arount the top-left corner (in degrees).</param>
        /// <param name="image">The handle to the image.</param>
        /// <param name="alpha">The alpha value of the image.</param>
        /// <returns></returns>
        public static Paint ImagePattern(float x, float y, float width, float height, float angle, int image, float alpha)
        {
            var paint = new Paint();

            float cs = MathF.Cos(angle * MathF.PI / 180);
            float sn = MathF.Sin(angle * MathF.PI / 180);
            paint.XForm = new Matrix3X2<float>();
            paint._xform.M11 = cs;
            paint._xform.M12 = sn;
            paint._xform.M21 = -sn;
            paint._xform.M22 = cs;
            paint._xform.M31 = x;
            paint._xform.M32 = y;

            paint.Extent = new Vector2D<float>(width, height);

            paint.Image = image;

            paint.InnerColour = paint.OuterColour = new Colour(1.0f, 1.0f, 1.0f, alpha);

            return paint;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

    }
}