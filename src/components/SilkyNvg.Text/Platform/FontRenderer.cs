using FontStashSharp.Interfaces;
using SilkyNvg.Core.States;
using SilkyNvg.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SilkyNvg.Text.Platform
{
    internal class FontRenderer : IFontStashRenderer2, IDisposable
    {

        private const int VERTEX_CAPACITY = 8192;

        private readonly Vertex[] _vertexBuffer = new Vertex[VERTEX_CAPACITY];

        private readonly AtlasManager _atlasManager;
        private readonly Nvg _nvg;

        private int _vertexIndex = 0;
        private int _previousTexture = -1;

        public ITexture2DManager TextureManager => _atlasManager;

        internal FontRenderer(Nvg nvg)
        {
            _nvg = nvg;

            _atlasManager = new AtlasManager(_nvg.renderer);
        }

        private void FlushBuffer()
        {
            if ((_vertexIndex == 0) || (_previousTexture == -1))
            {
                return;
            }

            var verts = new ReadOnlySpan<Vertex>(_vertexBuffer, 0, _vertexIndex);

            var state = _nvg.stateStack.CurrentState;
            Paint paint = Paint.ForText(_previousTexture, state.Fill);

            _nvg.renderer.Triangles(paint, state.CompositeOperation, state.Scissor, verts, _nvg.pixelRatio.FringeWidth);

            _vertexIndex = 0;
            _previousTexture = -1;
        }

        internal void PrepareDraw()
        {
            _vertexIndex = 0;
            _previousTexture = -1;
        }

        public void DrawQuad(object texture,
            ref VertexPositionColorTexture topLeft, ref VertexPositionColorTexture topRight,
            ref VertexPositionColorTexture bottomLeft, ref VertexPositionColorTexture bottomRight)
        {
            if (_vertexIndex + 6 > VERTEX_CAPACITY)
            {
                return;
            }

            var tl = new Vertex(topLeft.Position.X, topLeft.Position.Y, topLeft.TextureCoordinate.X, topLeft.TextureCoordinate.Y);
            var tr = new Vertex(topRight.Position.X, topRight.Position.Y, topRight.TextureCoordinate.X, topRight.TextureCoordinate.Y);
            var bl = new Vertex(bottomLeft.Position.X, bottomLeft.Position.Y, bottomLeft.TextureCoordinate.X, bottomLeft.TextureCoordinate.Y);
            var br = new Vertex(bottomRight.Position.X, bottomRight.Position.Y, bottomRight.TextureCoordinate.X, bottomRight.TextureCoordinate.Y);

            int textureID = (int)texture;

            if (textureID != _previousTexture)
            {
                FlushBuffer();
                _previousTexture = textureID;
            }

            _vertexBuffer[_vertexIndex++] = tl;
            _vertexBuffer[_vertexIndex++] = bl;
            _vertexBuffer[_vertexIndex++] = tr;

            _vertexBuffer[_vertexIndex++] = bl;
            _vertexBuffer[_vertexIndex++] = br;
            _vertexBuffer[_vertexIndex++] = tr;
        }

        internal void FinishDraw()
        {
            FlushBuffer();
        }

        public void Dispose()
        {
            _atlasManager.Dispose();
        }

    }
}
