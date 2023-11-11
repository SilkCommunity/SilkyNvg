using System;
using System.Runtime.InteropServices;
using Silk.NET.Maths;
using SilkyNvg.Images;
using SilkyNvg.Rendering.Vulkan.Textures;
using SilkyNvg.Transforms;
using Veldrid;

namespace SilkyNvg.Rendering.Vulkan.Shaders
{
    [StructLayout(LayoutKind.Sequential)]
    public struct FragUniforms
    {

        // ReSharper disable PrivateFieldCanBeConvertedToLocalVariable
        private readonly Matrix3X4<float> _scissorMat;
        private readonly Matrix3X4<float> _paintMat;
        private readonly Colour _innerCol;
        private readonly Colour _outerCol;
        private readonly Vector2D<float> _scissorExt;
        private readonly Vector2D<float> _scissorScale;
        private readonly Vector2D<float> _extent;
        private readonly float _radius;
        private readonly float _feather;
        private readonly float _strokeMult;
        private readonly float _strokeThr;
        private readonly int _texType;
        private readonly int _type;
        // ReSharper enable PrivateFieldCanBeConvertedToLocalVariable
        public FragUniforms(float strokeThr, ShaderType type) : this()
        {
            _strokeThr = strokeThr;
            _type = (int)type;
        }

        public FragUniforms(Paint paint, Scissor scissor, float width, float fringe, float strokeThr, VeldridRenderer renderer)
        {
            Matrix3X2<float> invtransform;

            _innerCol = paint.InnerColour.Premult();
            _outerCol = paint.OuterColour.Premult();

            if (scissor.Extent.X < -0.5f || scissor.Extent.Y < -0.5f)
            {
                _scissorMat = new Matrix3X4<float>();
                _scissorExt = new Vector2D<float>(1.0f);
                _scissorScale = new Vector2D<float>(1.0f);
            }
            else
            {
                _ = Matrix3X2.Invert(scissor.Transform, out invtransform);
                _scissorMat = new Matrix3X4<float>(invtransform);
                _scissorExt = scissor.Extent;
                _scissorScale = new Vector2D<float>(
                    MathF.Sqrt(scissor.Transform.M11 * scissor.Transform.M11 + scissor.Transform.M21 * scissor.Transform.M21) / fringe,
                    MathF.Sqrt(scissor.Transform.M21 * scissor.Transform.M21 + scissor.Transform.M22 * scissor.Transform.M22) / fringe
                );
            }

            _extent = paint.Extent;
            _strokeMult = (width * 0.5f + fringe * 0.5f) / fringe;
            _strokeThr = strokeThr;

            if (paint.Image != 0)
            {
                _ = renderer.TextureManager.FindTexture(paint.Image, out TextureSlot tex);
                if (tex.Id == 0)
                {
                    _type = (int)ShaderType.FillGrad;
                    _radius = paint.Radius;
                    _feather = paint.Feather;
                    _texType = 0;

                    _ = Matrix3X2.Invert(paint.Transform, out invtransform);
                }
                else
                {
                    if (tex.HasFlag(ImageFlags.FlipY))
                    {
                        Matrix3X2<float> m1, m2;
                        m1 = Matrix3X2.CreateTranslation(new Vector2D<float>(0.0f, _extent.Y * 0.5f));
                        m1 = NvgTransforms.Multiply(m1, paint.Transform);
                        m2 = Matrix3X2.CreateScale(new Vector2D<float>(1.0f, -1.0f));
                        m2 = NvgTransforms.Multiply(m2, m1);
                        m1 = Matrix3X2.CreateTranslation(new Vector2D<float>(0.0f, -_extent.Y * 0.5f));
                        m1 = NvgTransforms.Multiply(m1, m2);
                        _ = Matrix3X2.Invert(m1, out invtransform);
                    }
                    else
                    {
                        _ = Matrix3X2.Invert(paint.Transform, out invtransform);
                    }
                    _type = (int)ShaderType.FillImg;

                    if (tex.Texture._Texture.Format == PixelFormat.R8_G8_B8_A8_UNorm)
                    {
                        _texType = tex.HasFlag(ImageFlags.Premultiplied) ? 0 : 1;
                    }
                    else
                    {
                        _texType = 2;
                    }

                    _radius = _feather = 0.0f;
                }
            }
            else
            {
                _type = (int)ShaderType.FillGrad;
                _radius = paint.Radius;
                _feather = paint.Feather;
                _texType = 0;

                _ = Matrix3X2.Invert(paint.Transform, out invtransform);
            }

            _paintMat = new Matrix3X4<float>(invtransform);
        }

        public FragUniforms(Paint paint, Scissor scissor, float fringe, VeldridRenderer renderer)
            : this(paint, scissor, 1.0f, fringe, -1.0f, renderer)
        {
            _type = (int)ShaderType.Img;
        }

    }
}