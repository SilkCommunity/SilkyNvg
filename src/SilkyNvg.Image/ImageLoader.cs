using SilkyNvg.OpenGL;
using SilkyNvg.OpenGL.Textures;
using StbImageSharp;
using System.IO;

namespace SilkyNvg.Image
{
    internal static class ImageLoader
    {

        public static int LoadImage(string filename, uint imageFlags, GraphicsManager graphicsManager)
        {
            StbImage.stbi_set_unpremultiply_on_load(1);
            StbImage.stbi_convert_iphone_png_to_rgb(1);
            var stream = File.OpenRead(filename);
            var img = ImageResultFloat.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
            int image = CreateImageRgba(img.Width, img.Height, imageFlags, img.Data, graphicsManager);
            stream.Close();
            return image;
        }

        public static int CreateImageRgba(int w, int h, uint imageFlags, float[] data, GraphicsManager graphicsManager)
        {
            return graphicsManager.CreateTexture(TextureType.Rgba, w, h, imageFlags, data);
        }

    }
}
