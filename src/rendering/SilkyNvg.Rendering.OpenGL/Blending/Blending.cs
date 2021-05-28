using Silk.NET.OpenGL;
using SilkyNvg.Blending;

namespace SilkyNvg.Rendering.OpenGL.Blending
{
    internal static class Blending
    {

        public static void Blend(CompositeOperationState compositeOperation, GL gl)
        {
            GLEnum srcRgb = ConvertBlend(compositeOperation.SrcRgb);
            GLEnum dstRgb = ConvertBlend(compositeOperation.SrcRgb);
            GLEnum srcAlpha = ConvertBlend(compositeOperation.SrcRgb);
            GLEnum dstAlpha = ConvertBlend(compositeOperation.SrcRgb);
            if (srcRgb == GLEnum.InvalidEnum | dstRgb == GLEnum.InvalidEnum |
                srcAlpha == GLEnum.InvalidEnum | dstAlpha == GLEnum.InvalidEnum)
            {
                srcRgb = GLEnum.One;
                dstRgb = GLEnum.OneMinusSrcAlpha;
                srcAlpha = GLEnum.One;
                dstAlpha = GLEnum.OneMinusDstAlpha;
            }
            gl.BlendFuncSeparate(srcRgb, dstRgb, srcAlpha, dstAlpha);
        }

        private static GLEnum ConvertBlend(BlendFactor op)
        {
            return op switch
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
                _ => GLEnum.InvalidEnum
            };
        }

    }
}
