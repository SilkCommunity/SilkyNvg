using Silk.NET.Maths;
using SilkyNvg.Rendering;
using StbImageSharp;
using System;
using System.IO;

namespace SilkyNvg.Images
{
    /// <summary>
    /// <para>NanoVG allows you to load jpg, png, psd, tga, pic and gif files to be used for rendering.
    /// In addition you can upload your own image. The image loading is provided by StbImageSharp.
    /// The parameter imageFlags is a combination of flags defined in <see cref="ImageFlags"/>.</para>
    /// </summary>
    public static class NvgImages
    {

        /// <summary>
        /// Creates image by loading it from the disk from specified file name.
        /// </summary>
        /// <returns>Handle to the image.</returns>
        public static int CreateImage(this Nvg nvg, string fileName, ImageFlags imageFlags)
        {
            Stream stream;
            try
            {
                stream = File.OpenRead(fileName);
            }
            catch
            {
                return 0;
            }
            ImageResult result = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
            if (result == null)
            {
                return 0;
            }
            int image = CreateImageRgba(nvg, (uint)result.Width, (uint)result.Height, imageFlags, result.Data);
            stream.Close();
            stream.Dispose();
            return image;
        }

        /// <summary>
        /// Creates image by loading it from the specified chunk of memory.
        /// </summary>
        /// <returns>Handle to the image.</returns>
        public static int CreateImageMem(this Nvg nvg, ImageFlags imageFlags, byte[] data)
        {
            ImageResult result = ImageResult.FromMemory(data, ColorComponents.RedGreenBlueAlpha);
            if (result == null)
            {
                return 0;
            }
            int image = CreateImageRgba(nvg, (uint)result.Width, (uint)result.Height, imageFlags, data);
            return image;
        }


        /// <summary>
        /// Creates image from the specified image data.
        /// </summary>
        /// <returns>Handle to the image.</returns>
        public static int CreateImageRgba(this Nvg nvg, Vector2D<uint> size, ImageFlags imageFlags, ReadOnlySpan<byte> data)
        {
            return nvg.renderer.CreateTexture(Texture.Rgba, size, imageFlags, data);
        }

        /// <inheritdoc cref="CreateImageRgba(Nvg, Vector2D{uint}, ImageFlags, ReadOnlySpan{byte})"/>
        public static int CreateImageRgba(this Nvg nvg, uint width, uint height, ImageFlags imageFlags, ReadOnlySpan<byte> data)
            => CreateImageRgba(nvg, new Vector2D<uint>(width, height), imageFlags, data);

        /// <summary>
        /// Updates image data specified by image handle.
        /// </summary>
        public static void UpdateImage(this Nvg nvg, int image, ReadOnlySpan<byte> data)
        {
            _ = nvg.renderer.GetTextureSize(image, out Vector2D<uint> size);
            _ = nvg.renderer.UpdateTexture(image, Rectangle.FromLTRB((uint)0, (uint)0, size.X, size.Y), data);
        }

        /// <summary>
        /// Returns the dimensions of a created image.
        /// </summary>
        public static void ImageSize(this Nvg nvg, int image, out Vector2D<uint> size)
        {
            _ = nvg.renderer.GetTextureSize(image, out size);
        }

        /// <inheritdoc cref="ImageSize(Nvg, int, out Vector2D{uint})"/>
        public static void ImageSize(this Nvg nvg, int image, out uint width, out uint height)
        {
            ImageSize(nvg, image, out Vector2D<uint> size);
            width = size.X;
            height = size.Y;
        }


        /// <summary>
        /// Deletes created image.
        /// </summary>
        public static void DeleteImage(this Nvg nvg, int image)
        {
            _ = nvg.renderer.DeleteTexture(image);
        }

    }
}
