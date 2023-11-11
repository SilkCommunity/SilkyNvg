using System;
using Silk.NET.Maths;
using Veldrid;

namespace SilkyNvg.Rendering.Vulkan.Utils
{
    //TODO: Expose options in constructor for materials (eg, flip-mode, texture filtering, clamp mode, etc)
    public class Texture : IDisposable
    {

        public global::Veldrid.Texture _Texture;


        private Texture()
        {
            
        }

        public Texture(uint width, uint height, uint mips, GraphicsDevice device, PixelFormat format = PixelFormat.R8_G8_B8_A8_UNorm) : this()
        {
            TextureDescription textureDescription = TextureDescription.Texture2D(width, height, mips, 1,
                format, TextureUsage.Sampled);
            _Texture = device.ResourceFactory.CreateTexture(textureDescription);
        }

        public static Texture CreateFromBytes(GraphicsDevice device, uint width, uint height, ReadOnlySpan<byte> data, PixelFormat format = PixelFormat.R8_G8_B8_A8_UNorm)
        {
            Texture tex = new Texture();
            TextureDescription textureDescription = TextureDescription.Texture2D(width, height, mipLevels: 1, 1,
                format, TextureUsage.Sampled);
            tex._Texture = device.ResourceFactory.CreateTexture(textureDescription);
            device.UpdateTexture(tex._Texture, data, 0, 0, 0, width, height, 1, 0, 0);
            return tex;
        }


        


        public void UpdateTextureBytes(GraphicsDevice device, ReadOnlySpan<byte> bytes, Vector2D<int> offset, Vector2D<int> bounds)
        {
            // if the addition totally fits inside the current texture, update the current texture and return.
            if (bounds.X + offset.X <= _Texture.Width && bounds.Y + offset.Y <= _Texture.Height)
            {
                device.UpdateTexture(_Texture, bytes, (uint)offset.X, (uint)offset.Y, 0, (uint)bounds.X, (uint)bounds.Y, 1, 0, 0);

                return;
            }

            // Store the current texture for proper disposal at a later time and for reference when setting the new texture up.
            var oldTexture = _Texture;

            // Create a new texture fit to the correct size.
            TextureDescription textureDescription = TextureDescription.Texture2D(
                (uint)offset.X + (uint)bounds.X, (uint)offset.Y + (uint)bounds.Y, mipLevels: 1, 1, oldTexture.Format, oldTexture.Usage);
            _Texture = device.ResourceFactory.CreateTexture(textureDescription);

            // Copy the old texture's contents into the new one. We need to use a command list for this unfortunately...
            using (CommandList commandBuffer = device.ResourceFactory.CreateCommandList())
            {
                commandBuffer.CopyTexture(oldTexture, 0, 0, 0, 0, 0, _Texture, 0, 0, 0, 0, 0, oldTexture.Width, oldTexture.Height, oldTexture.Depth, oldTexture.ArrayLayers);
                device.SubmitCommands(commandBuffer);
            }

            // Update the texture we just created with the new information
            device.UpdateTexture(_Texture, bytes, 0, 0, 0, (uint)offset.X, (uint)offset.Y, 1, 0, 0);

            //dispose of the old texture and command list used to copy the texture
            device.DisposeWhenIdle(oldTexture);
        }
        
        
        void Load(GraphicsDevice graphicsDevice, ReadOnlySpan<byte> data, uint width, uint height, PixelFormat format)
        {
            if (graphicsDevice.GetPixelFormatSupport(format, TextureType.Texture2D, TextureUsage.Sampled))
            {
                TextureDescription textureDescription = TextureDescription.Texture2D(width, height, mipLevels: 1, 1,
                    format, TextureUsage.Sampled);
                _Texture = graphicsDevice.ResourceFactory.CreateTexture(textureDescription);

                graphicsDevice.UpdateTexture(_Texture, data, 0, 0, 0, width, height, 1, 0, 0);
            }
            else
            {
                throw new NotImplementedException("This texture format is not implemented in your Graphics Device!");
            }

        }
        

        public void Dispose()
        {
            _Texture.Dispose();
        }
        
        ~Texture()
        {
           Dispose();
        }
    }
    
}
