using SilkyNvg.Rendering;
using StbImageSharp;
using System;
using System.Drawing;
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
            int image = CreateImageRgba(nvg, result.Width, result.Height, imageFlags, result.Data);
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
            int image = CreateImageRgba(nvg, result.Width, result.Height, imageFlags, data);
            return image;
        }


        /// <summary>
        /// Creates image from the specified image data.
        /// </summary>
        /// <returns>Handle to the image.</returns>
        public static int CreateImageRgba(this Nvg nvg, Size size, ImageFlags imageFlags, ReadOnlySpan<byte> data)
        {
            return nvg.renderer.CreateTexture(Texture.Rgba, size, imageFlags, data);
        }

        /// <inheritdoc cref="CreateImageRgba(Nvg, Size, ImageFlags, ReadOnlySpan{byte})"/>
        public static int CreateImageRgba(this Nvg nvg, int width, int height, ImageFlags imageFlags, ReadOnlySpan<byte> data)
            => CreateImageRgba(nvg, new Size(width, height), imageFlags, data);

        /// <summary>
        /// Updates image data specified by image handle.
        /// </summary>
        public static void UpdateImage(this Nvg nvg, int image, ReadOnlySpan<byte> data)
        {
            _ = nvg.renderer.GetTextureSize(image, out Size size);
            _ = nvg.renderer.UpdateTexture(image, Rectangle.FromLTRB(0, 0, size.Width, size.Height), data);
        }

        /// <summary>
        /// Returns the dimensions of a created image.
        /// </summary>
        public static void ImageSize(this Nvg nvg, int image, out Size size)
        {
            _ = nvg.renderer.GetTextureSize(image, out size);
        }

        /// <inheritdoc cref="ImageSize(Nvg, int, out Size)"/>
        public static void ImageSize(this Nvg nvg, int image, out int width, out int height)
        {
            ImageSize(nvg, image, out Size size);
            width = size.Width;
            height = size.Height;
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
