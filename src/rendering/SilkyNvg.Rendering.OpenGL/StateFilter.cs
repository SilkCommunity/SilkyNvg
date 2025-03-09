using Silk.NET.OpenGL;
using SilkyNvg.Rendering.OpenGL.Blending;

namespace SilkyNvg.Rendering.OpenGL
{
    internal class StateFilter
    {

        public uint BoundTexture { get; set; }

        public TextureUnit ActiveTextureUnit { get; set; }

        public uint StencilMask { get; set; }

        public StencilFunction StencilFunc { get; set; }

        public int StencilFuncRef { get; set; }

        public uint StencilFuncMask { get; set; }

        public Blend BlendFunc { get; set; }

    }
}
