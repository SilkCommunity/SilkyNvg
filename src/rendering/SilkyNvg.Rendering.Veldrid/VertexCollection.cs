using System;
using System.Collections.Generic;

namespace SilkyNvg.Rendering.Vulkan
{
    internal class VertexCollection
    {

        private Vertex[] _vertices;
        private int _count;

        public int CurrentsOffset => _count;

        public ReadOnlySpan<Vertex> Vertices => _vertices;

        public VertexCollection()
        {
            _vertices = Array.Empty<Vertex>();
            _count = 0;
        }

        private void AllocVerts(int n)
        {
            if (_count + n > _vertices.Length)
            {
                int cverts = Math.Max(_count + n, 4096) + _vertices.Length / 2;
                Array.Resize(ref _vertices, cverts);
            }
        }

        public void AddVertex(Vertex vertex)
        {
            AllocVerts(1);
            _vertices[_count++] = vertex;
        }

        public void AddVertices(ICollection<Vertex> vertices)
        {
            AllocVerts(vertices.Count);
            vertices.CopyTo(_vertices, _count);
            _count += vertices.Count;
        }

        public void Clear()
        {
            _count = 0;
        }

    }
}