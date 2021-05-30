using Silk.NET.OpenGL;
using SilkyNvg.Blending;

namespace SilkyNvg.Rendering.OpenGL.Blending
{
    internal class Blend
    {

        private readonly GLEnum _srcRgb;
        private readonly GLEnum _dstRgb;
        private readonly GLEnum _srcAlpha;
        private readonly GLEnum _dstAlpha;

        private readonly OpenGLRenderer _renderer;

        public Blend(GLEnum srcRgb, GLEnum dstRgb, GLEnum srcAlpha, GLEnum dstAlpha)
        {
            _srcRgb = srcRgb;
            _dstRgb = dstRgb;
            _srcAlpha = srcAlpha;
            _dstAlpha = dstAlpha;
        }

        public Blend(CompositeOperationState compositeOperation, OpenGLRenderer renderer)
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
            _srcRgb = srcRgb;
            _dstRgb = dstRgb;
            _srcAlpha = srcAlpha;
            _dstAlpha = dstAlpha;

            _renderer = renderer;
        }

        public void BlendFuncSeperate()
        {
            StateFilter filter = _renderer.Filter;
            GL gl = _renderer.Gl;
            if (filter.BlendFunc != this)
            {
                filter.BlendFunc = this;
                gl.BlendFuncSeparate(_srcRgb, _dstRgb, _srcAlpha, _dstAlpha);
                _renderer.CheckError("blend func seperate");
            }
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

        public override bool Equals(object obj)
        {
            if (obj is Blend)
            {
                return ((Blend)obj) == this;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            int hash = 0;
            hash += (int)_srcRgb;
            hash += (int)_dstRgb << 4;
            hash += (int)_srcAlpha << 8;
            hash += (int)_dstAlpha << 16;
            return hash;
        }

        public static bool operator ==(Blend left, Blend right)
        {
            if (left._srcRgb == right._srcRgb &&
                left._dstRgb == right._dstRgb &&
                left._srcAlpha == right._srcAlpha &&
                left._dstAlpha == right._dstAlpha)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool operator !=(Blend left, Blend right)
        {
            if (left._srcRgb != right._srcRgb ||
                left._dstRgb != right._dstRgb ||
                left._srcAlpha != right._srcAlpha ||
                left._dstAlpha != right._dstAlpha)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

    }
}
