using Silk.NET.Maths;
using Silk.NET.OpenGL;
using SilkyNvg.Images;
using System;
using System.Collections.Generic;

namespace SilkyNvg.Rendering.OpenGL.Textures
{
    internal class Texture
    {

        private static readonly IList<Texture> textures = new List<Texture>();

        public static Texture DummyTex => textures[0];

        private readonly uint _textureID;
        private readonly ImageFlags _flags;

        private readonly OpenGLRenderer _renderer;
        private readonly GL _gl;

        public int Id { get; }

        public Vector2D<uint> Size { get; }

        public Rendering.Texture TextureType { get; }

        public unsafe Texture(Vector2D<uint> size, ImageFlags flags, Rendering.Texture type, byte[] data, OpenGLRenderer renderer)
        {
            _renderer = renderer;
            _gl = _renderer.Gl;

            Id = textures.Count;
            _textureID = _gl.GenTexture();
            Size = size;
            TextureType = type;
            _flags = flags;

            Bind();
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

            textures.Add(this);
        }

        private void SetPixelStore()
        {
            _gl.PixelStore(PixelStoreParameter.UnpackAlignment, 1);

            _gl.PixelStore(PixelStoreParameter.UnpackRowLength, Size.X);
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

        private unsafe void Load(byte[] data)
        {
            fixed (byte* d = data)
            {
                if (TextureType == Rendering.Texture.Rgba)
                {
                    _gl.TexImage2D(TextureTarget.Texture2D, 0, (int)InternalFormat.Rgba, Size.X, Size.Y, 0, GLEnum.Rgba, GLEnum.UnsignedByte, d);
                }
                else
                {
                    _gl.TexImage2D(TextureTarget.Texture2D, 0, (int)InternalFormat.Red, Size.X, Size.Y, 0, GLEnum.Red, GLEnum.UnsignedByte, d);
                }
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

        public unsafe void Update(Vector4D<uint> bounds, byte[] data)
        {
            Bind();

            _gl.PixelStore(PixelStoreParameter.UnpackAlignment, 1);

            _gl.PixelStore(PixelStoreParameter.UnpackRowLength, Size.X);
            _gl.PixelStore(PixelStoreParameter.UnpackSkipPixels, bounds.X);
            _gl.PixelStore(PixelStoreParameter.UnpackSkipRows, bounds.Y);

            _gl.GetInteger(GetPName.MaxTextureSize, out int dat);

            fixed (byte* d = data)
            {
                if (TextureType == Rendering.Texture.Rgba)
                {
                    _gl.TexSubImage2D(TextureTarget.Texture2D, 0, (int)bounds.X, (int)bounds.Y, bounds.Z, bounds.W, GLEnum.Rgba, GLEnum.UnsignedByte, d);
                }
                else
                {
                    _gl.TexSubImage2D(TextureTarget.Texture2D, 0, (int)bounds.X, (int)bounds.Y, bounds.Z, bounds.W, GLEnum.Red, GLEnum.UnsignedByte, d);
                }
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

        public static Texture FindTexture(int image)
        {
            return (image >= textures.Count) || (image < 0) ? null : textures[image];
        }

        public static void DeleteAll()
        {
            foreach (Texture tex in textures)
            {
                tex.Dispose();
            }
        }

    }
}
