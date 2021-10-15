using System;

namespace SilkyNvg.Rendering.OpenGL.Textures
{
    internal sealed class TextureManager : IDisposable
    {

        private readonly OpenGLRenderer _renderer;

        private Texture[] _textures;
        private int _count;

        private Texture _default = default;

        public TextureManager(OpenGLRenderer renderer)
        {
            _renderer = renderer;

            _textures = Array.Empty<Texture>();
            _count = 0;
        }

        public ref Texture AllocTexture()
        {
            int tex = -1;

            for (int i = 0; i < _count; i++)
            {
                if (_textures[i].Id == 0)
                {
                    tex = i;
                }
            }

            if (tex == -1)
            {
                if (_count + 1 > _textures.Length)
                {
                    int ctextures = Math.Max(_count + 1, 4) + _textures.Length / 2;
                    Array.Resize(ref _textures, ctextures);
                }
                tex = _count++;
            }

            _textures[tex] = new Texture(_renderer);

            return ref _textures[tex];
        }

        public ref Texture FindTexture(int id)
        {
            for (int i = 0; i < _count; i++)
            {
                if (_textures[i].Id == id)
                {
                    return ref _textures[i];
                }
            }
            return ref _default;
        }

        public bool DeleteTexture(int id)
        {
            for (int i = 0; i < _count; i++)
            {
                if (_textures[i].Id == id)
                {
                    _textures[i].Dispose();
                    _textures[i] = default;
                    return true;
                }
            }
            return false;
        }

        public void Dispose()
        {
            for (int i = 0; i < _count; i++)
            {
                _textures[i].Dispose();
            }
        }

    }
}
