using Silk.NET.Maths;
using System;

namespace SilkyNvg.Rendering.OpenGL.Shaders
{
    internal struct FragUniforms
    {

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

        public FragUniforms(float strokeThr, ShaderType type) : this()
        {
            _strokeThr = strokeThr;
            _type = (int)type;
        }

        public FragUniforms(Paint paint, Scissor scissor, float width, float fringe, float strokeThr)
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
                // TODO: Image
                throw new NotImplementedException("Images not yet implemented!");
            }
            else
            {
                _type = (int)ShaderType.Fillgrad;
                _radius = paint.Radius;
                _feather = paint.Feather;
                _texType = 0;

                _ = Matrix3X2.Invert(paint.Transform, out invtransform);
            }

            _paintMat = new Matrix3X4<float>(invtransform);
        }

        public void LoadToShader(Shader shader)
        {
            shader.LoadMatrix(UniformLoc.ScissorMat, _scissorMat);
            shader.LoadVector(UniformLoc.ScissorExt, _scissorExt);
            shader.LoadVector(UniformLoc.ScissorScale, _scissorScale);
            shader.LoadMatrix(UniformLoc.PaintMat, _paintMat);
            shader.LoadVector(UniformLoc.Extent, _extent);
            shader.LoadFloat(UniformLoc.Radius, _radius);
            shader.LoadFloat(UniformLoc.Feather, _feather);
            shader.LoadColour(UniformLoc.InnerCol, _innerCol);
            shader.LoadColour(UniformLoc.OuterCol, _outerCol);
            shader.LoadFloat(UniformLoc.StrokeMult, _strokeMult);
            shader.LoadFloat(UniformLoc.StrokeThr, _strokeThr);
            shader.LoadInt(UniformLoc.TexType, _texType);
            shader.LoadInt(UniformLoc.Type, _type);
        }

    }
}
