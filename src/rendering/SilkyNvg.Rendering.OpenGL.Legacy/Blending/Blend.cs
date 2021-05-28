using Silk.NET.OpenGL.Legacy;
using SilkyNvg.Blending;

namespace SilkyNvg.Rendering.OpenGL.Legacy.Blending
{
    internal struct Blend
    {

        public GLEnum SrcRgb { get; }

        public GLEnum DstRgb { get; }

        public GLEnum SrcAlpha { get; }

        public GLEnum DstAlpha { get; }

        private readonly GL _gl;

        public Blend(GLEnum value, GL gl)
        {
            _gl = gl;
            SrcRgb = DstRgb = SrcAlpha = DstAlpha = value;
        }

        public Blend(CompositeOperationState op, GL gl)
        {
            _gl = gl;
            SrcRgb = ConvertBlendFuncFactor(op.SrcRgb);
            DstRgb = ConvertBlendFuncFactor(op.DstRgb);
            SrcAlpha = ConvertBlendFuncFactor(op.SrcAlpha);
            DstAlpha = ConvertBlendFuncFactor(op.DstAlpha);
            if (SrcRgb == GLEnum.InvalidEnum || DstRgb == GLEnum.InvalidEnum || SrcAlpha == GLEnum.InvalidEnum || DstAlpha == GLEnum.InvalidEnum)
            {
                SrcRgb = GLEnum.One;
                DstRgb = GLEnum.OneMinusSrcAlpha;
                SrcAlpha = GLEnum.One;
                DstAlpha = GLEnum.OneMinusSrcAlpha;
            }
        }

        public void BlendFuncSeperate(StateFilter filter)
        {
            if (SrcRgb != filter.BlendFunc.SrcRgb ||
                DstRgb != filter.BlendFunc.DstRgb ||
                SrcAlpha != filter.BlendFunc.SrcAlpha ||
                DstAlpha != filter.BlendFunc.DstAlpha)
            {
                filter.BlendFunc = this;
                _gl.BlendFuncSeparate(SrcRgb, DstRgb, SrcAlpha, DstAlpha);
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
