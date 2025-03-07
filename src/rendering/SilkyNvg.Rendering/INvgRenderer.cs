using SilkyNvg.Blending;
using SilkyNvg.Images;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace SilkyNvg.Rendering
{
    public interface INvgRenderer : IDisposable
    {

        bool EdgeAntiAlias { get; }

        bool Create();

        int CreateTexture(Texture type, Size size, ImageFlags imageFlags, ReadOnlySpan<byte> data);

        bool DeleteTexture(int image);

        bool UpdateTexture(int image, Rectangle bounds, ReadOnlySpan<byte> data);

        bool GetTextureSize(int image, out Size size);

        void Viewport(SizeF size, float devicePixelRatio);

        void Cancel();

        void Flush();

        void Fill(Paint paint, CompositeOperationState compositeOperation, Scissor scissor, float fringe, RectangleF bounds, IReadOnlyList<Path> paths);

        void Stroke(Paint paint, CompositeOperationState compositeOperation, Scissor scissor, float fringe, float strokeWidth, IReadOnlyList<Path> paths);

        void Triangles(Paint paint, CompositeOperationState compositeOperation, Scissor scissor, ICollection<Vertex> vertices, float fringeWidth);

    }
}
