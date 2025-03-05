using SilkyNvg.Blending;
using SilkyNvg.Common.Geometry;
using SilkyNvg.Images;
using System;
using System.Collections.Generic;

namespace SilkyNvg.Rendering
{
    public interface INvgRenderer : IDisposable
    {

        bool EdgeAntiAlias { get; }

        bool Create();

        int CreateTexture(Texture type, SizeU size, ImageFlags imageFlags, ReadOnlySpan<byte> data);

        bool DeleteTexture(int image);

        bool UpdateTexture(int image, RectU bounds, ReadOnlySpan<byte> data);

        bool GetTextureSize(int image, out SizeU size);

        void Viewport(SizeF size, float devicePixelRatio);

        void Cancel();

        void Flush();

        void Fill(Paint paint, CompositeOperationState compositeOperation, Scissor scissor, float fringe, RectF bounds, IReadOnlyList<Path> paths);

        void Stroke(Paint paint, CompositeOperationState compositeOperation, Scissor scissor, float fringe, float strokeWidth, IReadOnlyList<Path> paths);

        void Triangles(Paint paint, CompositeOperationState compositeOperation, Scissor scissor, ICollection<Vertex> vertices, float fringeWidth);

    }
}
