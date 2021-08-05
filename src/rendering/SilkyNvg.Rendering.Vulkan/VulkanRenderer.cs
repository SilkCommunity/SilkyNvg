using Silk.NET.Maths;
using SilkyNvg.Blending;
using SilkyNvg.Images;
using System;
using System.Collections.Generic;

namespace SilkyNvg.Rendering.Vulkan
{
    public sealed class VulkanRenderer : INvgRenderer
    {

        public bool EdgeAntiAlias => throw new NotImplementedException();

        public bool Create()
        {
            throw new NotImplementedException();
        }

        public int CreateTexture(Texture type, Vector2D<uint> size, ImageFlags imageFlags, byte[] data)
        {
            throw new NotImplementedException();
        }

        public bool UpdateTexture(int image, Vector4D<uint> bounds, byte[] data)
        {
            throw new NotImplementedException();
        }

        public bool GetTextureSize(int image, out Vector2D<uint> size)
        {
            throw new NotImplementedException();
        }

        public bool DeleteTexture(int image)
        {
            throw new NotImplementedException();
        }

        public void Viewport(Vector2D<float> size, float devicePixelRatio)
        {
            throw new NotImplementedException();
        }

        public void Cancel()
        {
            throw new NotImplementedException();
        }

        public void Flush()
        {
            throw new NotImplementedException();
        }

        public void Fill(Paint paint, CompositeOperationState compositeOperation, Scissor scissor, float fringe, Vector4D<float> bounds, IReadOnlyList<Path> paths)
        {
            throw new NotImplementedException();
        }

        public void Stroke(Paint strokePaint, CompositeOperationState compositeOperation, Scissor scissor, float fringeWidth, float strokeWidth, IReadOnlyList<Path> paths)
        {
            throw new NotImplementedException();
        }

        public void Triangles(Paint paint, CompositeOperationState compositeOperation, Scissor scissor, ICollection<Vertex> vertices, float fringeWidth)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

    }
}
