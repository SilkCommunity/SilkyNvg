using FontStashSharp.Interfaces;
using SilkyNvg.Images;
using SilkyNvg.Rendering;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SilkyNvg.Text.Platform
{
    internal class AtlasManager(INvgRenderer renderer) : ITexture2DManager, IDisposable
    {

        private readonly List<int> _atlasTextures = [];

        public object CreateTexture(int width, int height)
        {
            int textureID = renderer.CreateTexture(Texture.FontAtlas, new Size(width, height), ImageFlags.GenerateMimpas, null);
            _atlasTextures.Add(textureID);
            return textureID;
        }

        public System.Drawing.Point GetTextureSize(object texture)
        {
            int textureID = (int)texture;
            if (!renderer.GetTextureSize(textureID, out Size size))
            {
                return System.Drawing.Point.Empty;
            }
            return (System.Drawing.Point)size;
        }

        public void SetTextureData(object texture, Rectangle bounds, byte[] data)
        {
            int textureID = (int)texture;
            if (!renderer.UpdateTexture(textureID, bounds, data))
            {
                throw new InvalidOperationException("Failed to updated texture atlas");
            }
        }

        public void Dispose()
        {
            foreach (int textureID in _atlasTextures)
            {
                _ = renderer.DeleteTexture(textureID);
            }
        }

    }
}
