using SilkyNvg.Images;
using System;
using System.Collections.Generic;
using System.Drawing;
using Veldrid;

namespace SilkyNvg.Rendering.Veldrid
{
    /// <summary>
    /// Manages NVG textures (font atlas, images) and their Veldrid GPU resources.
    /// Handles creation, updates, deletion, and ResourceSet caching for textured draw calls.
    /// </summary>
    internal sealed class TextureRegistry : IDisposable
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly ResourceLayout _texturedResourceLayout;
        private readonly DeviceBuffer _viewSizeUniformBuffer;
        private readonly Sampler _textureSampler;

        private readonly Dictionary<int, ManagedTexture> _textures = new();
        private readonly Dictionary<int, ResourceSet> _resourceSetCache = new();
        private int _nextTextureId = 1;

        internal struct ManagedTexture
        {
            public global::Veldrid.Texture GpuTexture;
            public TextureView TextureView;
            public Size TextureSize;
            public ImageFlags CreationFlags;
            public bool IsAlphaOnly; // Font atlas uses R8_UNorm (alpha only)
        }

        public TextureRegistry(
            GraphicsDevice graphicsDevice,
            ResourceLayout texturedResourceLayout,
            DeviceBuffer viewSizeUniformBuffer,
            Sampler textureSampler)
        {
            _graphicsDevice = graphicsDevice;
            _texturedResourceLayout = texturedResourceLayout;
            _viewSizeUniformBuffer = viewSizeUniformBuffer;
            _textureSampler = textureSampler;
        }

        public int CreateTexture(Texture type, Size size, ImageFlags imageFlags, ReadOnlySpan<byte> data)
        {
            var factory = _graphicsDevice.ResourceFactory;

            bool isAlphaOnly = (type == Texture.Alpha);
            var pixelFormat = isAlphaOnly ? VeldridCompat.PixelFormatR8UNorm : VeldridCompat.PixelFormatR8G8B8A8UNorm;

            var textureDescription = TextureDescription.Texture2D(
                (uint)size.Width,
                (uint)size.Height,
                mipLevels: 1,
                arrayLayers: 1,
                pixelFormat,
                TextureUsage.Sampled);

            var gpuTexture = factory.CreateTexture(textureDescription);
            var textureView = factory.CreateTextureView(gpuTexture);

            if (!data.IsEmpty)
            {
                _graphicsDevice.UpdateTexture(
                    gpuTexture,
                    data.ToArray(),
                    0, 0, 0,
                    (uint)size.Width, (uint)size.Height, 1,
                    0, 0);
            }

            int textureId = _nextTextureId++;
            _textures[textureId] = new ManagedTexture
            {
                GpuTexture = gpuTexture,
                TextureView = textureView,
                TextureSize = size,
                CreationFlags = imageFlags,
                IsAlphaOnly = isAlphaOnly
            };

            return textureId;
        }

        public bool DeleteTexture(int textureId)
        {
            if (!_textures.TryGetValue(textureId, out var managedTexture))
            {
                return false;
            }

            // Invalidate cached ResourceSet for this texture
            if (_resourceSetCache.TryGetValue(textureId, out var cachedResourceSet))
            {
                cachedResourceSet.Dispose();
                _resourceSetCache.Remove(textureId);
            }

            managedTexture.TextureView.Dispose();
            managedTexture.GpuTexture.Dispose();
            _textures.Remove(textureId);

            return true;
        }

        public bool UpdateTexture(int textureId, Rectangle bounds, ReadOnlySpan<byte> data)
        {
            if (!_textures.TryGetValue(textureId, out var managedTexture))
            {
                Console.WriteLine($"[TextureRegistry] UpdateTexture: texture {textureId} not found!");
                return false;
            }

            if (data.IsEmpty)
            {
                return true;
            }

            uint bytesPerPixel = managedTexture.IsAlphaOnly ? 1u : 4u;
            int atlasWidth = managedTexture.TextureSize.Width;
            int regionWidth = bounds.Width;
            int regionHeight = bounds.Height;
            uint regionRowBytes = (uint)(regionWidth * bytesPerPixel);
            uint atlasRowBytes = (uint)(atlasWidth * bytesPerPixel);
            uint expectedRegionSize = regionRowBytes * (uint)regionHeight;

            // FontStash passes the ENTIRE atlas data but with dirty region bounds.
            // We must extract just the dirty sub-rectangle for Veldrid's tightly-packed upload.
            byte[] regionPixelData;
            if ((uint)data.Length == expectedRegionSize)
            {
                regionPixelData = data.ToArray();
            }
            else
            {
                regionPixelData = new byte[expectedRegionSize];
                for (int row = 0; row < regionHeight; row++)
                {
                    int sourceOffset = (int)(((bounds.Y + row) * atlasRowBytes) + (bounds.X * bytesPerPixel));
                    int destOffset = (int)(row * regionRowBytes);
                    data.Slice(sourceOffset, (int)regionRowBytes).CopyTo(regionPixelData.AsSpan(destOffset));
                }
            }

            _graphicsDevice.UpdateTexture(
                managedTexture.GpuTexture,
                regionPixelData,
                (uint)bounds.X, (uint)bounds.Y, 0,
                (uint)regionWidth, (uint)regionHeight, 1,
                0, 0);

            return true;
        }

        public bool GetTextureSize(int textureId, out Size size)
        {
            if (!_textures.TryGetValue(textureId, out var managedTexture))
            {
                size = new Size(1, 1);
                return false;
            }

            size = managedTexture.TextureSize;
            return true;
        }

        /// <summary>
        /// Gets the TextureView for a given texture ID.
        /// Used by VeldridRenderer to create image pattern resource sets.
        /// </summary>
        public TextureView GetTextureView(int textureId)
        {
            if (!_textures.TryGetValue(textureId, out var managedTexture))
            {
                throw new InvalidOperationException(
                    $"TextureRegistry.GetTextureView: Texture ID {textureId} not found. " +
                    "A draw call references a texture that was never created or was already deleted.");
            }
            return managedTexture.TextureView;
        }

        /// <summary>
        /// Gets or creates a ResourceSet for the textured pipeline bound to the given texture.
        /// Caches ResourceSets so they aren't recreated every frame.
        /// </summary>
        public ResourceSet GetOrCreateTexturedResourceSet(int textureId)
        {
            if (_resourceSetCache.TryGetValue(textureId, out var existingResourceSet))
            {
                return existingResourceSet;
            }

            if (!_textures.TryGetValue(textureId, out var managedTexture))
            {
                throw new InvalidOperationException(
                    $"TextureRegistry: Texture ID {textureId} not found. " +
                    "A draw call references a texture that was never created or was already deleted.");
            }

            var newTexturedResourceSet = _graphicsDevice.ResourceFactory.CreateResourceSet(new ResourceSetDescription(
                _texturedResourceLayout,
                _viewSizeUniformBuffer,
                managedTexture.TextureView,
                _textureSampler));

            _resourceSetCache[textureId] = newTexturedResourceSet;
            return newTexturedResourceSet;
        }

        public void Dispose()
        {
            foreach (var kvp in _textures)
            {
                kvp.Value.TextureView.Dispose();
                kvp.Value.GpuTexture.Dispose();
            }
            _textures.Clear();

            foreach (var kvp in _resourceSetCache)
            {
                kvp.Value.Dispose();
            }
            _resourceSetCache.Clear();
        }
    }
}