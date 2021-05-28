using Silk.NET.Maths;
using SilkyNvg.Renderer;
using System;

namespace SilkyNvg.Rendering.OpenGL.Legacy.Shaders
{
    internal struct Uniforms
    {

        private readonly Matrix3X4<float> _scissorMat;
        private readonly Matrix3X4<float> _paintMat;
        private readonly Colour _innerCol;
        private readonly Colour _outerCol;
        private readonly Vector2D<float> _scissorExt;
        private readonly Vector2D<float> _scissorScale;
        private readonly Vector2D<float> _extent;
        private readonly float _radius;
        private readonly float _strokeMult;
        private readonly float _strokeThr;
        private readonly float _feather;
        private readonly int _texType;
        private readonly int _type;

        public Uniforms(Paint paint, Scissor scissor, float width, float fringe, float strokeThr)
        {
            _innerCol = paint.InnerColour.Premult();
            _outerCol = paint.OuterColour.Premult();

            if ((scissor.Extent.X < -0.5f) && (scissor.Extent.Y < -0.5f))
            {
                _scissorMat = default;
                _scissorExt = new Vector2D<float>(1.0f);
                _scissorScale = new Vector2D<float>(1.0f);
            }
            else
            {
                Matrix3X2.Invert(scissor.Transform, out Matrix3X2<float> invtransform);
                _scissorMat = TransformToMatrix3x4(invtransform);
                _scissorExt = scissor.Extent;
                _scissorScale.X = MathF.Sqrt((scissor.Transform.M11 * scissor.Transform.M11) + (scissor.Transform.M21 * scissor.Transform.M21)) / fringe;
                _scissorScale.Y = MathF.Sqrt((scissor.Transform.M12 * scissor.Transform.M12) + (scissor.Transform.M22 * scissor.Transform.M22)) / fringe;
            }

            _extent = paint.Extent;
            _strokeMult = (width * 0.5f + fringe * 0.5f) / fringe;
            _strokeThr = strokeThr;

            Matrix3X2<float> paintTransform = default;

            if (paint.Image != 0)
            {
                // TODO: Image
                throw new Exception();
            }
            else
            {
                _type = (int)ShaderType.FillGrad;
                _radius = paint.Radius;
                _feather = paint.Feather;
                Matrix3X2.Invert(paint.Transform, out paintTransform);
                _texType = -1;
            }

            _paintMat = TransformToMatrix3x4(paintTransform);

        }

        public void LoadToShader(Shader shader)
        {
            shader.LoadMatrix(UniformLoc.ScissorMat, _scissorMat);
            shader.LoadMatrix(UniformLoc.PaintMat, _paintMat);
            shader.LoadColour(UniformLoc.InnerCol, _innerCol);
            shader.LoadColour(UniformLoc.OuterCol, _outerCol);
            shader.LoadVector(UniformLoc.ScissorExt, _scissorExt);
            shader.LoadVector(UniformLoc.ScissorScale, _scissorScale);
            shader.LoadVector(UniformLoc.Extent, _extent);
            shader.LoadFloat(UniformLoc.Radius, _radius);
            shader.LoadFloat(UniformLoc.StrokeMult, _strokeMult);
            shader.LoadFloat(UniformLoc.StrokeThr, _strokeThr);
            shader.LoadFloat(UniformLoc.Feather, _feather);
            shader.LoadInt(UniformLoc.TexType, _texType);
            shader.LoadInt(UniformLoc.Type, _type);
        }

        private static Matrix3X4<float> TransformToMatrix3x4(Matrix3X2<float> t)
        {
            return new Matrix3X4<float>(t);
        }

    }
}
