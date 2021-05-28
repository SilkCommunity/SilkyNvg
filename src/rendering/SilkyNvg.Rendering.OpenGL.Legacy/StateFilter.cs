using Silk.NET.OpenGL.Legacy;
using SilkyNvg.Rendering.OpenGL.Legacy.Blending;

namespace SilkyNvg.Rendering.OpenGL.Legacy
{
    internal struct StateFilter
    {

        public uint BoundsTexture { get; set; }

        public uint StencilMask { get; set; }

        public StencilFunction StencilFunk { get; set; }

        public int StencilFuncRef { get; set; }

        public uint StencilFuncMask { get; set; }

        public Blend BlendFunc { get; set; }

    }
}
