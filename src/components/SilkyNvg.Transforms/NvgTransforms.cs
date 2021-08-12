using Silk.NET.Maths;
using System;
using SilkyNvg.Common;

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
        public static Matrix3X2<float> Identity => Matrix3X2<float>.Identity;

        /// <inheritdoc cref="Identity"/>
        public static Matrix3X2<float> TransformIdentity(this Nvg _)
            => Identity;

        /// <returns>Translation matrix.</returns>
        public static Matrix3X2<float> Translate(Vector2D<float> t)
        {
            Matrix3X2<float> matrix = Matrix3X2<float>.Identity;
            matrix.M31 = t.X;
            matrix.M32 = t.Y;
            return matrix;
        }

        /// <inheritdoc cref="Translate(Vector2D{float})"/>
        public static Matrix3X2<float> Translate(float x, float y)
            => Translate(new Vector2D<float>(x, y));

        /// <inheritdoc cref="Translate(Vector2D{float})"/>
        public static Matrix3X2<float> TransformTranslate(this Nvg _, Vector2D<float> t)
            => Translate(t);

        /// <inheritdoc cref="Translate(Vector2D{float})"/>
        public static Matrix3X2<float> TransformTranslate(this Nvg _, float x, float y)
            => Translate(new Vector2D<float>(x, y));

        /// <returns>Scale matrix.</returns>
        public static Matrix3X2<float> Scale(Vector2D<float> s)
        {
            Matrix3X2<float> matrix = Matrix3X2<float>.Identity;
            matrix.M11 = s.X;
            matrix.M22 = s.Y;
            return matrix;
        }

        /// <inheritdoc cref="Scale(Vector2D{float})"/>
        public static Matrix3X2<float> Scale(float x, float y)
            => Scale(new Vector2D<float>(x, y));

        /// <inheritdoc cref="Scale(Vector2D{float})"/>
        public static Matrix3X2<float> TransformScale(this Nvg _, Vector2D<float> s)
            => Scale(s);

        /// <inheritdoc cref="Scale(Vector2D{float})"/>
        public static Matrix3X2<float> TransformScale(this Nvg _, float x, float y)
            => Scale(new Vector2D<float>(x, y));

        /// <param name="a">Is specified in radians.</param>
        /// <returns>Rotation matrix.</returns>
        public static Matrix3X2<float> Rotate(float a)
        {
            float cs = MathF.Cos(a);
            float sn = MathF.Sin(a);
            Matrix3X2<float> matrix = Matrix3X2<float>.Identity;
            matrix.M11 = cs;
            matrix.M12 = sn;
            matrix.M21 = -sn;
            matrix.M22 = cs;
            return matrix;
        }

        /// <inheritdoc cref="Rotate(float)"/>
        public static Matrix3X2<float> TransformRotate(this Nvg _, float a)
            => Rotate(a);

        /// <returns>Skew-X matrix.</returns>
        public static Matrix3X2<float> SkewX(float a)
        {
            Matrix3X2<float> matrix = Matrix3X2<float>.Identity;
            matrix.M21 = MathF.Tan(a);
            return matrix;
        }

        /// <inheritdoc cref="SkewX(float)"/>
        public static Matrix3X2<float> TransformSkewX(this Nvg _, float a)
            => SkewX(a);

        /// <returns>Skew-Y matrix.</returns>
        public static Matrix3X2<float> SkewY(float a)
        {
            Matrix3X2<float> matrix = Matrix3X2<float>.Identity;
            matrix.M12 = MathF.Tan(a);
            return matrix;
        }

        /// <inheritdoc cref="SkewY(float)"/>
        public static Matrix3X2<float> TransformSkewY(this Nvg _, float a)
            => SkewY(a);

        /// <returns>The multiplication of two transforms, of A = A * B</returns>
        public static Matrix3X2<float> Multiply(Matrix3X2<float> t, Matrix3X2<float> s)
            => Maths.Multiply(t, s);

        /// <inheritdoc cref="Multiply(Matrix3X2{float}, Matrix3X2{float})"/>
        public static Matrix3X2<float> TransformMultiply(this Nvg _, Matrix3X2<float> t, Matrix3X2<float> s)
            => Multiply(t, s);

        /// <returns>The result of multiplication of two transforms, of A = B * A</returns>
        public static Matrix3X2<float> Premultiply(Matrix3X2<float> t, Matrix3X2<float> s)
        {
            Matrix3X2<float> s2 = s;
            s2 = Multiply(s2, t);
            t = s2;
            return t;
        }

        /// <inheritdoc cref="Premultiply(Matrix3X2{float}, Matrix3X2{float})"/>
        public static Matrix3X2<float> TransformPremultiply(this Nvg _, Matrix3X2<float> t, Matrix3X2<float> s)
            => Premultiply(t, s);

        /// <summary>Sets the destination to inverse of specified transform.</summary>
        /// <returns><value>true</value> if the inverse could be calculated, else <value>false</value></returns>
        public static bool Inverse(out Matrix3X2<float> inv, Matrix3X2<float> t)
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

        /// <inheritdoc cref="Inverse(out Matrix3X2{float}, Matrix3X2{float})"/>
        public static bool TransformInverse(this Nvg _, out Matrix3X2<float> inv, Matrix3X2<float> t)
            => Inverse(out inv, t);

        /// <summary>Transforms a point by given transform.</summary>
        public static Vector2D<float> Point(Matrix3X2<float> t, Vector2D<float> s)
        {
            Vector2D<float> d = default;
            d.X = s.X * t.M11 + s.Y * t.M21 + t.M31;
            d.Y = s.X * t.M12 + s.Y * t.M22 + t.M32;
            return d;
        }

        /// <inheritdoc cref="Point(Matrix3X2{float}, Vector2D{float})"/>
        public static Vector2D<float> Point(Matrix3X2<float> t, float x, float y)
            => Point(t, new Vector2D<float>(x, y));

        /// <inheritdoc cref="Point(Matrix3X2{float}, Vector2D{float})"/>
        public static Vector2D<float> TransformPoint(this Nvg _, Matrix3X2<float> t, Vector2D<float> p)
            => Point(t, p);

        /// <inheritdoc cref="Point(Matrix3X2{float}, Vector2D{float})"/>
        public static Vector2D<float> TransformPoint(this Nvg _, Matrix3X2<float> t, float x, float y)
            => Point(t, new Vector2D<float>(x, y));

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
        public static void Transform(this Nvg nvg, Matrix3X2<float> transform)
        {
            nvg.stateStack.CurrentState.Transform = nvg.TransformPremultiply(nvg.stateStack.CurrentState.Transform, transform);
        }

        /// <summary>
        /// Translates current coordinate system.
        /// </summary>
        public static void Translate(this Nvg nvg, Vector2D<float> position)
        {
            Matrix3X2<float> t = nvg.TransformTranslate(position);
            nvg.stateStack.CurrentState.Transform = nvg.TransformPremultiply(nvg.stateStack.CurrentState.Transform, t);
        }

        /// <inheritdoc cref="Translate(Nvg, Vector2D{float})"/>
        public static void Translate(this Nvg nvg, float x, float y)
            => Translate(nvg, new Vector2D<float>(x, y));

        /// <summary>
        /// Rotates current coordinate system.
        /// </summary>
        /// <param name="angle">Is specified in radians.</param>
        public static void Rotate(this Nvg nvg, float angle)
        {
            Matrix3X2<float> t = nvg.TransformRotate(angle);
            nvg.stateStack.CurrentState.Transform = nvg.TransformPremultiply(nvg.stateStack.CurrentState.Transform, t);
        }

        /// <summary>
        /// Skews the current coordinate system along X axis.
        /// </summary>
        /// <param name="angle">Is specified in radians.</param>
        public static void SkewX(this Nvg nvg, float angle)
        {
            Matrix3X2<float> t = nvg.TransformSkewX(angle);
            nvg.stateStack.CurrentState.Transform = nvg.TransformPremultiply(nvg.stateStack.CurrentState.Transform, t);
        }

        /// <summary>
        /// Skews the current coordinate system along Y axis.
        /// </summary>
        /// <param name="angle">Is specified in radians.</param>
        public static void SkewY(this Nvg nvg, float angle)
        {
            Matrix3X2<float> t = nvg.TransformSkewY(angle);
            nvg.stateStack.CurrentState.Transform = nvg.TransformPremultiply(nvg.stateStack.CurrentState.Transform, t);
        }

        /// <summary>
        /// Scales current coordinate system.
        /// </summary>
        public static void Scale(this Nvg nvg, Vector2D<float> scale)
        {
            Matrix3X2<float> t = nvg.TransformScale(scale);
            nvg.stateStack.CurrentState.Transform = nvg.TransformPremultiply(nvg.stateStack.CurrentState.Transform, t);
        }

        /// <inheritdoc cref="Scale(Nvg, Vector2D{float})"/>
        public static void Scale(this Nvg nvg, float x, float y)
            => Scale(nvg, new Vector2D<float>(x, y));

        /// <returns>The current transform.</returns>
        public static Matrix3X2<float> CurrentTransform(this Nvg nvg)
        {
            return nvg.stateStack.CurrentState.Transform;
        }

        /// <summary>
        /// Converts degrees to radians.
        /// </summary>
        public static float DegToRad(float deg)
        {
            return deg / 180.0f * MathF.PI;
        }

        /// <inheritdoc cref="DegToRad(float)"/>
        public static float DegToRad(this Nvg _, float deg)
            => DegToRad(deg);

        /// <summary>
        /// Converts radians to degrees.
        /// </summary>
        public static float RadToDeg(float rad)
        {
            return rad / MathF.PI * 180.0f;
        }

        /// <inheritdoc cref="RadToDeg(float)"/>
        public static float RadToDeg(this Nvg _, float rad)
            => RadToDeg(rad);

    }
}
