using Silk.NET.OpenGL;
using SilkyNvg.Blending;

namespace SilkyNvg.OpenGL
{

    internal struct Blend
    {

        private GLEnum _srcRgb;
        private GLEnum _dstRgb;
        private GLEnum _srcAlpha;
        private GLEnum _dstAlpha;

        public GLEnum SrcRgb
        {
            get => _srcRgb;
            set => _srcRgb = value;
        }

        public GLEnum DstRgb
        {
            get => _dstRgb;
            set => _dstRgb = value;
        }

        public GLEnum SrcAlpha
        {
            get => _srcAlpha;
            set => _srcAlpha = value;
        }

        public GLEnum DstAlpha
        {
            get => _dstAlpha;
            set => _dstAlpha = value;
        }

        public Blend(GLEnum srgb, GLEnum drgb, GLEnum salpha, GLEnum dalpha)
        {
            _srcRgb = srgb;
            _dstRgb = drgb;
            _srcAlpha = salpha;
            _dstAlpha = dalpha;
        }

        public Blend(CompositeOperationState op)
        {
            _srcRgb = ConvertBlendFuncFactor(op.SrcRgb);
            _dstRgb = ConvertBlendFuncFactor(op.DstRgb);
            _srcAlpha = ConvertBlendFuncFactor(op.SrcAlpha);
            _dstAlpha = ConvertBlendFuncFactor(op.DstAlpha);
            if (_srcRgb == GLEnum.InvalidEnum || _dstRgb == GLEnum.InvalidEnum || _srcAlpha == GLEnum.InvalidEnum || _dstRgb == GLEnum.InvalidEnum)
            {
                _srcRgb = GLEnum.One;
                _dstRgb = GLEnum.OneMinusSrcAlpha;
                _srcAlpha = GLEnum.One;
                _dstAlpha = GLEnum.OneMinusSrcAlpha;
            }
        }

        private static GLEnum ConvertBlendFuncFactor(BlendFactor factor)
        {
            return factor switch
            {
                BlendFactor.Zero => GLEnum.Zero,
                BlendFactor.One => GLEnum.One,
                BlendFactor.SrcColour => GLEnum.SrcColor,
                BlendFactor.OneMinusSrcColour => GLEnum.OneMinusSrcColor,
                BlendFactor.DstColour => GLEnum.DstColor,
                BlendFactor.OneMinusDstColour => GLEnum.OneMinusDstColor,
                BlendFactor.SrcAlpha => GLEnum.SrcAlpha,
                BlendFactor.OneMinusSrcAlpha => GLEnum.OneMinusSrcAlpha,
                BlendFactor.DstAlpha => GLEnum.DstAlpha,
                BlendFactor.OneMinusDstAlpha => GLEnum.OneMinusDstAlpha,
                BlendFactor.SrcAlphaSaturate => GLEnum.SrcAlphaSaturate,
                _ => GLEnum.InvalidEnum,
            };
        }

    }
}