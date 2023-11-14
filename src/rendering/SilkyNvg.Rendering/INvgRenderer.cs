﻿using Silk.NET.Maths;
using SilkyNvg.Blending;
using SilkyNvg.Images;
using System;
using System.Collections.Generic;
using SilkyNvg.Graphics;

namespace SilkyNvg.Rendering
{
    public interface INvgRenderer : IDisposable
    {

        bool EdgeAntiAlias { get; }

        bool Create();

        int CreateTexture(Texture type, Vector2D<uint> size, ImageFlags imageFlags, ReadOnlySpan<byte> data);

        bool DeleteTexture(int image);

        bool UpdateTexture(int image, Rectangle<uint> bounds, ReadOnlySpan<byte> data);

        bool GetTextureSize(int image, out Vector2D<uint> size);

        void Viewport(Vector2D<float> size, float devicePixelRatio);

        void Cancel();

        void Flush();

        void Fill(Paint paint, CompositeOperationState compositeOperation, Scissor scissor, LineStyle lineStyle, float fringe, Box2D<float> bounds, IReadOnlyList<Path> paths);

        void Stroke(Paint paint, CompositeOperationState compositeOperation, Scissor scissor, LineStyle lineStyle, float fringe, float strokeWidth, IReadOnlyList<Path> paths);

        void Triangles(Paint paint, CompositeOperationState compositeOperation, Scissor scissor, LineStyle lineStyle, ICollection<Vertex> vertices, float fringeWidth);

    }
}
