using StbImageSharp;
using System.IO;

namespace SpaceGame.Main
{
    public class ImageLoader
    {

        private ImageResult image;

        public ImageResult LoadImage(string path)
        {
            StbImage.stbi_set_unpremultiply_on_load(1);
            StbImage.stbi_convert_iphone_png_to_rgb(1);
            image = ImageResult.FromStream(File.OpenRead(path), ColorComponents.RedGreenBlueAlpha);
            return image;
        }

    }
}
