using System.Numerics;

namespace SilkyNvg.Transforms
{
    /// <summary>
    /// <para>The paths, gradients, patterns and scissor region are transformed by an transformation
    /// matrix at the time when they are passed to the API.</para>
    /// <para>The current transformation matrix is a affine matrix:<br/>
    /// [sx  kx  tx]<br/>
    /// [ky  sy  ty]<br/>
    /// [ 0   0   1]<br/>
    /// Where: sx, sy define scaling, kx, ky skewing, tx, ty translation.
    /// The last row is assumed to be 0, 0, 1 and is not stored.</para>
    /// <para>Apart from <see cref="ResetTransform(Nvg)"/>, each transformation function first creates
    /// specific transformation matrix and pre-multiplies the current transformation by it.</para>
    /// <para>Current coordinate system (transformation) can be saved and restored using <see cref="Nvg.Save()"/> and <see cref="Nvg.Restore()"/>.</para>
    /// </summary>
    public static class NvgTransforms
    {

        /// <summary>
        /// Identity matrix.
        /// </summary>
        public static Matrix3x2 Identity => Matrix3x2.Identity;

        /// <inheritdoc cref="Identity"/>
        public static Matrix3x2 TransformIdentity(this Nvg _)
            => Identity;

        /// <returns>Translation matrix.</returns>
        public static Matrix3x2 Translate(Vector2 t)
            => Matrix3x2.CreateTranslation(t);

        /// <inheritdoc cref="Translate(Vector2)"/>
        public static Matrix3x2 Translate(float x, float y)
            => Translate(new Vector2(x, y));

        /// <inheritdoc cref="Translate(Vector2)"/>
        public static Matrix3x2 TransformTranslate(this Nvg _, Vector2 t)
            => Translate(t);

        /// <inheritdoc cref="Translate(Vector2)"/>
        public static Matrix3x2 TransformTranslate(this Nvg _, float x, float y)
            => Translate(x, y);

        /// <returns>Scale matrix.</returns>
        public static Matrix3x2 Scale(Vector2 s)
            => Matrix3x2.CreateScale(s);

        /// <inheritdoc cref="Scale(Vector2)"/>
        public static Matrix3x2 Scale(float x, float y)
            => Scale(new Vector2(x, y));

        /// <inheritdoc cref="Scale(Vector2)"/>
        public static Matrix3x2 TransformScale(this Nvg _, Vector2 s)
            => Scale(s);

        /// <inheritdoc cref="Scale(Vector2)"/>
        public static Matrix3x2 TransformScale(this Nvg _, float x, float y)
            => Scale(x, y);

        /// <param name="a">Is specified in radians.</param>
        /// <returns>Rotation matrix.</returns>
        public static Matrix3x2 Rotate(float a)
            => Matrix3x2.CreateRotation(a);

        /// <inheritdoc cref="Rotate(float)"/>
        public static Matrix3x2 TransformRotate(this Nvg _, float a)
            => Rotate(a);

        /// <returns>Skew-X matrix.</returns>
        public static Matrix3x2 SkewX(float a)
            => Matrix3x2.CreateSkew(a, 0.0f);

        /// <inheritdoc cref="SkewX(float)"/>
        public static Matrix3x2 TransformSkewX(this Nvg _, float a)
            => SkewX(a);

        /// <returns>Skew-Y matrix.</returns>
        public static Matrix3x2 SkewY(float a)
            => Matrix3x2.CreateSkew(0.0f, a);

        /// <inheritdoc cref="SkewY(float)"/>
        public static Matrix3x2 TransformSkewY(this Nvg _, float a)
            => SkewY(a);

        /// <returns>The multiplication of two transforms, of A = A * B</returns>
        public static Matrix3x2 Multiply(Matrix3x2 t, Matrix3x2 s)
            => t * s;

        /// <inheritdoc cref="Multiply(Matrix3x2, Matrix3x2)"/>
        public static Matrix3x2 TransformMultiply(this Nvg _, Matrix3x2 t, Matrix3x2 s)
            => Multiply(t, s);

        /// <returns>The result of multiplication of two transforms, of A = B * A</returns>
        public static Matrix3x2 Premultiply(Matrix3x2 t, Matrix3x2 s)
            => s * t;

        /// <inheritdoc cref="Premultiply(Matrix3x2, Matrix3x2)"/>
        public static Matrix3x2 TransformPremultiply(this Nvg _, Matrix3x2 t, Matrix3x2 s)
            => Premultiply(t, s);

        /// <summary>Sets the destination to inverse of specified transform.</summary>
        /// <returns><value>true</value> if the inverse could be calculated, else <value>false</value></returns>
        public static bool Inverse(out Matrix3x2 inv, Matrix3x2 t)
        {
            double det = (double)t.M11 * t.M22 - (double)t.M21 * t.M12;
            if (det > -1e-6 && det < 1e-6)
            {
                inv = Identity;
                return false;
            }
            inv = default;
            double invdet = 1.0 / det;
            inv.M11 = (float)(t.M22 * invdet);
            inv.M21 = (float)(-t.M21 * invdet);
            inv.M31 = (float)(((double)t.M21 * t.M32 - (double)t.M22 * t.M31) * invdet);
            inv.M12 = (float)(-t.M12 * invdet);
            inv.M22 = (float)(t.M11 * invdet);
            inv.M32 = (float)(((double)t.M12 * t.M31 - (double)t.M11 * t.M32) * invdet);
            return true;
        }

        /// <inheritdoc cref="Inverse(out Matrix3x2, Matrix3x2)"/>
        public static bool TransformInverse(this Nvg _, out Matrix3x2 inv, Matrix3x2 t)
            => Inverse(out inv, t);

        /// <summary>Transforms a point by given transform.</summary>
        public static Vector2 Point(Matrix3x2 t, Vector2 s)
            => Vector2.Transform(s, t);

        /// <inheritdoc cref="Point(Matrix3x2, Vector2)"/>
        public static Vector2 Point(Matrix3x2 t, float x, float y)
            => Point(t, new Vector2(x, y));

        /// <inheritdoc cref="Point(Matrix3x2, Vector2)"/>
        public static Vector2 TransformPoint(this Nvg _, Matrix3x2 t, Vector2 p)
            => Point(t, p);

        /// <inheritdoc cref="Point(Matrix3x2, Vector2)"/>
        public static Vector2 TransformPoint(this Nvg _, Matrix3x2 t, float x, float y)
            => Point(t, new Vector2(x, y));

        /// <summary>
        /// Resets current transform to a identity matrix.
        /// </summary>
        public static void ResetTransform(this Nvg nvg)
        {
            nvg.stateStack.CurrentState.Transform = nvg.TransformIdentity();
        }

        /// <summary>
        /// Premultiplies currentn coordinate system by specified matrix.
        /// For matrix layout <see cref="NvgTransforms"/>
        /// </summary>
        public static void Transform(this Nvg nvg, Matrix3x2 transform)
        {
            nvg.stateStack.CurrentState.Transform = nvg.TransformPremultiply(nvg.stateStack.CurrentState.Transform, transform);
        }

        /// <summary>
        /// Translates current coordinate system.
        /// </summary>
        public static void Translate(this Nvg nvg, Vector2 position)
        {
            Matrix3x2 t = nvg.TransformTranslate(position);
            nvg.stateStack.CurrentState.Transform = nvg.TransformPremultiply(nvg.stateStack.CurrentState.Transform, t);
        }

        /// <inheritdoc cref="Translate(Nvg, Vector2)"/>
        public static void Translate(this Nvg nvg, float x, float y)
            => Translate(nvg, new Vector2(x, y));

        /// <summary>
        /// Rotates current coordinate system.
        /// </summary>
        /// <param name="angle">Is specified in radians.</param>
        public static void Rotate(this Nvg nvg, float angle)
        {
            Matrix3x2 t = nvg.TransformRotate(angle);
            nvg.stateStack.CurrentState.Transform = nvg.TransformPremultiply(nvg.stateStack.CurrentState.Transform, t);
        }

        /// <summary>
        /// Skews the current coordinate system along X axis.
        /// </summary>
        /// <param name="angle">Is specified in radians.</param>
        public static void SkewX(this Nvg nvg, float angle)
        {
            Matrix3x2 t = nvg.TransformSkewX(angle);
            nvg.stateStack.CurrentState.Transform = nvg.TransformPremultiply(nvg.stateStack.CurrentState.Transform, t);
        }

        /// <summary>
        /// Skews the current coordinate system along Y axis.
        /// </summary>
        /// <param name="angle">Is specified in radians.</param>
        public static void SkewY(this Nvg nvg, float angle)
        {
            Matrix3x2 t = nvg.TransformSkewY(angle);
            nvg.stateStack.CurrentState.Transform = nvg.TransformPremultiply(nvg.stateStack.CurrentState.Transform, t);
        }

        /// <summary>
        /// Scales current coordinate system.
        /// </summary>
        public static void Scale(this Nvg nvg, Vector2 scales)
        {
            Matrix3x2 t = nvg.TransformScale(scales);
            nvg.stateStack.CurrentState.Transform = nvg.TransformPremultiply(nvg.stateStack.CurrentState.Transform, t);
        }

        /// <inheritdoc cref="Scale(Nvg, Vector2)"/>
        public static void Scale(this Nvg nvg, float sx, float sy)
            => Scale(nvg, new Vector2(sx, sy));

        /// <returns>The current transform.</returns>
        public static Matrix3x2 CurrentTransform(this Nvg nvg)
        {
            return nvg.stateStack.CurrentState.Transform;
        }

        /// <summary>
        /// Converts degrees to radians.
        /// </summary>
        public static float DegToRad(float deg)
            => float.DegreesToRadians(deg);

        /// <inheritdoc cref="DegToRad(float)"/>
        public static float DegToRad(this Nvg _, float deg)
            => DegToRad(deg);

        /// <summary>
        /// Converts radians to degrees.
        /// </summary>
        public static float RadToDeg(float rad)
            => float.RadiansToDegrees(rad);

        /// <inheritdoc cref="RadToDeg(float)"/>
        public static float RadToDeg(this Nvg _, float rad)
            => RadToDeg(rad);

    }
}
