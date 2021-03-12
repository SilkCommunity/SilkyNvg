using Silk.NET.OpenGL;

namespace SilkyNvg.OpenGL
{
    internal struct RenderMeta
    {

        private int _bondTexture;
        private uint _stencilMask;
        private GLEnum _stencilFunk;
        private uint _stencilFunkRef;
        private uint _stencilFunkMask;
        private GLEnum _srcRgb;
        private GLEnum _srcAlpha;
        private GLEnum _dstRgb;
        private GLEnum _dstAlpha;

        public int BondTexture
        {
            get => _bondTexture;
            set => _bondTexture = value;
        }

        public uint StencilMask
        {
            get => _stencilMask;
            set => _stencilMask = value;
        }

        public uint StencilFunkRef
        {
            get => _stencilFunkRef;
            set => _stencilFunkRef = value;
        }

        public uint StencilFunkMask
        {
            get => _stencilFunkMask;
            set => _stencilFunkMask = value;
        }

        public GLEnum StencilFunk
        {
            get => _stencilFunk;
            set => _stencilFunk = value;
        }

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

    }
}