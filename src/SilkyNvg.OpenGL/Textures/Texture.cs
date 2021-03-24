using Silk.NET.OpenGL;

namespace SilkyNvg.OpenGL.Textures
{
    internal struct Texture
    {

        private readonly int _id;
        private readonly uint _textureId;
        private readonly int _width, _height;
        private readonly TextureType _type;
        private readonly TextureFlags _imageFlags;

        private readonly GL _gl;

        public int Id => _id;
        public uint TextureId => _textureId;
        public int Width => _width;
        public int Height => _height;
        public TextureType Type => _type;
        internal TextureFlags ImageFlags => _imageFlags;

        public Texture(int id, uint textureId, int width, int height, TextureType type, TextureFlags imageFlags, GL gl)
        {
            _id = id;
            _textureId = textureId;
            _width = width;
            _height = height;
            _type = type;
            _imageFlags = imageFlags;
            _gl = gl;
        }

        public void Bind()
        {
            _gl.BindTexture(TextureTarget.Texture2D, _textureId);
        }

    }
}
