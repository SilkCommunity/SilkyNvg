using Silk.NET.OpenGL;
using SilkyNvg.Core;
using SilkyNvg.Core.Composite;

namespace SilkyNvg.OpenGL
{
    public struct Blend
    {

        public GLEnum SrcRGB { get; private set; }
        public GLEnum DstRGB { get; private set; }
        public GLEnum SrcAlpha { get; private set; }
        public GLEnum DstAlpha { get; private set; }

        public Blend(BlendingFactor srcRGB, BlendingFactor dstRGB, BlendingFactor srcAlpha, BlendingFactor dstAlpha)
        {
            SrcRGB = (GLEnum)srcRGB;
            DstRGB = (GLEnum)dstRGB;
            SrcAlpha = (GLEnum)srcAlpha;
            DstAlpha = (GLEnum)dstAlpha;
        }

        public Blend(ICompositeOperation op)
        {
            SrcRGB = ConvertToBlendFuncFactor(op.SrcRGB);
            DstRGB = ConvertToBlendFuncFactor(op.SrcRGB);
            SrcAlpha = ConvertToBlendFuncFactor(op.SrcRGB);
            DstAlpha = ConvertToBlendFuncFactor(op.SrcRGB);
            if (SrcRGB == GLEnum.InvalidEnum || DstRGB == GLEnum.InvalidEnum ||
                SrcAlpha == GLEnum.InvalidEnum || DstAlpha == GLEnum.InvalidEnum)
            {
                SrcRGB = GLEnum.One;
                DstRGB = GLEnum.OneMinusSrcAlpha;
                SrcAlpha = GLEnum.One;
                DstRGB = GLEnum.OneMinusSrcAlpha;
            }
        }

        private static GLEnum ConvertToBlendFuncFactor(BlendFactor value)
        {
            switch (value)
            {
                case BlendFactor.One:
                    return GLEnum.One;
                case BlendFactor.OneMinusSrcAlpha:
                    return GLEnum.OneMinusSrcAlpha;
                default:
                    return GLEnum.InvalidEnum;
            }
        }

    }
}
