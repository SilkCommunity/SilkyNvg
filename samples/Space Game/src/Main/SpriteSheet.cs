using SilkyNvg;
using SilkyNvg.Image;
using StbImageSharp;

namespace SpaceGame.Main
{
    public class SpriteSheet
    {

        private readonly Nvg _nvg;

        private readonly ImageResult _image;

        public SpriteSheet(ImageResult image, Nvg nvg)
        {
            _image = image;
            _nvg = nvg;
        }

        public int GrabImage(int col, int row, int width, int height)
        {
            byte[ , ] bm = new byte[_image.Width * 4, _image.Height];
            for (int i = 0; i < _image.Height; i++)
            {
                for (int j = 0; j < _image.Width * 4; j++)
                {
                    bm[j, i] = _image.Data[i * _image.Width * 4 + j];
                }
            }

            int minY = row * 32 - 32;
            int minX = (col - 1) * 32 * 4;
            int maxY = minY + height;
            int maxX = minX + width * 4;
            int index = 0;

            byte[] img = new byte[width * height * 4];
            for (int i = minY; i < maxY; i++)
            {
                for (int j = minX; j < maxX; j++)
                {
                    img[index++] = bm[j, i];
                }
            }

            return _nvg.CreateImageRgba(width, height, (uint)ImageFlags.Premultiplied, img);
        }

    }
}
