using SilkyNvg.Image;

namespace SilkyNvg.OpenGL.Textures
{
    internal struct TextureFlags
    {

        private readonly bool _mipmaps;
        private readonly bool _repeatX;
        private readonly bool _repeatY;
        private readonly bool _flipY;
        private readonly bool _premult;
        private readonly bool _nearest;

        public bool Mipmaps => _mipmaps;
        public bool RepeatX => _repeatX;
        public bool RepeatY => _repeatY;
        public bool FlipY => _flipY;
        public bool Premult => _premult;
        public bool Nearest => _nearest;

        public TextureFlags(uint imageFlags)
        {
            _mipmaps = (imageFlags & (uint)ImageFlags.GenerateMipmaps) != 0;
            _repeatX = (imageFlags & (uint)ImageFlags.RepeatX) != 0;
            _repeatY = (imageFlags & (uint)ImageFlags.RepeatY) != 0;
            _flipY = (imageFlags & (uint)ImageFlags.FlipY) != 0;
            _premult = (imageFlags & (uint)ImageFlags.Premultiplied) != 0;
            _nearest = (imageFlags & (uint)ImageFlags.Nearest) != 0;
        }

    }
}
