using Silk.NET.Maths;
using SilkyNvg.Rendering;
using StbImageSharp;
using System.IO;

namespace SilkyNvg.Images
{
    public static class NvgImages
    {

        public static int CreateImage(this Nvg nvg, string fileName, ImageFlags imageFlags)
        {
            Stream stream = null;
            try
            {
                stream = File.OpenRead(fileName);
            }
            catch
            {
                return 0;
            }
            StbImage.stbi_set_unpremultiply_on_load(1);
            StbImage.stbi_convert_iphone_png_to_rgb(1);
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

        public static int CreateImageRgba(this Nvg nvg, Vector2D<uint> size, ImageFlags imageFlags, byte[] data)
        {
            return nvg.renderer.CreateTexture(Texture.Rgba, size, imageFlags, data);
        }

        public static int CreateImageRgba(this Nvg nvg, uint width, uint height, ImageFlags imageFlags, byte[] data)
            => CreateImageRgba(nvg, new Vector2D<uint>(width, height), imageFlags, data);

        public static void UpdateImage(this Nvg nvg, int image, byte[] data)
        {
            _ = nvg.renderer.GetTextureSize(image, out Vector2D<uint> size);
            _ = nvg.renderer.UpdateTexture(image, new Vector4D<uint>(0, 0, size.X, size.Y), data);
        }

        public static void ImageSize(this Nvg nvg, int image, out Vector2D<uint> size)
        {
            _ = nvg.renderer.GetTextureSize(image, out size);
        }

        public static void ImageSize(this Nvg nvg, int image, out uint width, out uint height)
        {
            ImageSize(nvg, image, out Vector2D<uint> size);
            width = size.X;
            height = size.Y;
        }

        public static void DeleteImage(this Nvg nvg, int image)
        {
            _ = nvg.renderer.DeleteTexture(image);
        }

    }
}
