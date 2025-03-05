using Silk.NET.OpenGL;
using SilkyNvg.Common.Geometry;
using SilkyNvg.Images;
using System;

namespace SilkyNvg.Rendering.OpenGL.Textures
{
    internal struct Texture : IDisposable
    {

        private static int _idCounter = 0;

        private readonly OpenGLRenderer _renderer;
        private readonly GL _gl;

        private uint _textureID;
        private ImageFlags _flags;

        public int Id { get; private set; }

        public SizeU Size { get; private set; }

        public Rendering.Texture TextureType { get; private set; }

        public unsafe Texture(OpenGLRenderer renderer)
            : this()
        {
            _renderer = renderer;
            _gl = _renderer.Gl;
        }

        public void Load(SizeU size, ImageFlags flags, Rendering.Texture type, ReadOnlySpan<byte> data)
        {
            Id = ++_idCounter;
            _textureID = _gl.GenTexture();
            Size = size;
            TextureType = type;
            _flags = flags;

            Bind();
            _renderer.CheckError("tex paint tex");
            SetPixelStore();
            Load(data);
            MinFilter();
            MagFilter();
            RepeatX();
            RepeatY();
            ResetPixelStore();
            Mipmaps();

            _renderer.CheckError("create tex");
            Unbind();
        }

        private void SetPixelStore()
        {
            _gl.PixelStore(PixelStoreParameter.UnpackAlignment, 1);

            _gl.PixelStore(PixelStoreParameter.UnpackRowLength, Size.Width);
            _gl.PixelStore(PixelStoreParameter.UnpackSkipPixels, 0);
            _gl.PixelStore(PixelStoreParameter.UnpackSkipRows, 0);
        }

        private void ResetPixelStore()
        {
            _gl.PixelStore(PixelStoreParameter.UnpackAlignment, 4);

            _gl.PixelStore(PixelStoreParameter.UnpackRowLength, 0);
            _gl.PixelStore(PixelStoreParameter.UnpackSkipPixels, 0);
            _gl.PixelStore(PixelStoreParameter.UnpackSkipRows, 0);
        }

        private unsafe void Load(ReadOnlySpan<byte> data)
        {
            if (TextureType == Rendering.Texture.Rgba)
            {
                _gl.TexImage2D(TextureTarget.Texture2D, 0, (int)InternalFormat.Rgba, Size.Width, Size.Height, 0, GLEnum.Rgba, GLEnum.UnsignedByte, data);
            }
            else
            {
                _gl.TexImage2D(TextureTarget.Texture2D, 0, (int)InternalFormat.Red, Size.Width, Size.Height, 0, GLEnum.Red, GLEnum.UnsignedByte, data);
            }
        }

        private void MinFilter()
        {
            if (HasFlag(ImageFlags.GenerateMimpas))
            {
                if (HasFlag(ImageFlags.Nearest))
                {
                    _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.NearestMipmapNearest);
                }
                else
                {
                    _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
                }
            }
            else
            {
                if (HasFlag(ImageFlags.Nearest))
                {
                    _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                }
                else
                {
                    _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                }
            }
        }

        private void MagFilter()
        {
            if (HasFlag(ImageFlags.Nearest))
            {
                _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            }
            else
            {
                _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            }
        }

        private void RepeatX()
        {
            if (HasFlag(ImageFlags.RepeatX))
            {
                _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            }
            else
            {
                _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            }
        }

        private void RepeatY()
        {
            if (HasFlag(ImageFlags.RepeatY))
            {
                _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            }
            else
            {
                _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            }
        }

        private void Mipmaps()
        {
            if (HasFlag(ImageFlags.GenerateMimpas))
            {
                _gl.GenerateMipmap(TextureTarget.Texture2D);
            }
        }

        public void Bind()
        {
            if (_renderer.Filter.BoundTexture != _textureID)
            {
                _renderer.Filter.BoundTexture = _textureID;
                _gl.BindTexture(TextureTarget.Texture2D, _textureID);
            }
        }

        public void Unbind()
        {
            if (_renderer.Filter.BoundTexture != 0)
            {
                _renderer.Filter.BoundTexture = 0;
                _gl.BindTexture(TextureTarget.Texture2D, 0);
            }
        }

        public unsafe void Update(RectU bounds, ReadOnlySpan<byte> data)
        {
            Bind();

            _gl.PixelStore(PixelStoreParameter.UnpackAlignment, 1);

            _gl.PixelStore(PixelStoreParameter.UnpackRowLength, Size.Width);
            _gl.PixelStore(PixelStoreParameter.UnpackSkipPixels, bounds.X);
            _gl.PixelStore(PixelStoreParameter.UnpackSkipRows, bounds.Y);

            if (TextureType == Rendering.Texture.Rgba)
            {
                _gl.TexSubImage2D(TextureTarget.Texture2D, 0, (int)bounds.X, (int)bounds.Y, bounds.Width, bounds.Height, GLEnum.Rgba, GLEnum.UnsignedByte, data);
            }
            else
            {
                _gl.TexSubImage2D(TextureTarget.Texture2D, 0, (int)bounds.X, (int)bounds.Y, bounds.Width, bounds.Height, GLEnum.Red, GLEnum.UnsignedByte, data);
            }

            ResetPixelStore();
            Unbind();
        }

        public bool HasFlag(ImageFlags flag)
        {
            return _flags.HasFlag(flag);
        }

        public void Dispose()
        {
            if (_textureID != 0)
            {
                _gl.DeleteTexture(_textureID);
            }
        }

    }
}