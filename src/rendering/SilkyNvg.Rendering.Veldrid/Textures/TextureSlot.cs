using System;
using SilkyNvg.Images;
using Veldrid;

namespace SilkyNvg.Rendering.Vulkan.Textures
{
    public struct TextureSlot : IDisposable, IEquatable<TextureSlot>
    {
    
        public ImageFlags Flags;
        public Sampler TextureSampler;
        public Utils.Texture Texture;
        public int Id { get; internal set; }

        public void Dispose()
        {
            Texture?.Dispose();
        }

        public bool HasFlag(ImageFlags flag)
        {
            return Flags.HasFlag(flag);
        }

        public bool Equals(TextureSlot other)
        {
            return Flags == other.Flags && Texture.Equals(other.Texture) && Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            return obj is TextureSlot other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine((int) Flags, Texture, Id);
        }
    }    
}
