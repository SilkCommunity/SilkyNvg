using System;
using System.Collections.Generic;

namespace SilkyNvg.Rendering.WebGPU
{
    public class VertexCollection
    {
        private Vertex[] _vertices;
        private int _count;

        public ReadOnlySpan<Vertex> Vertices => _vertices;
        public int CurrentOffset => _count;

        public VertexCollection()
        {
            _vertices = Array.Empty<Vertex>();
            _count = 0;
        }

        private void AllocateVertices(int numberOfVertices)
        {
            if (_count + numberOfVertices > _vertices.Length)
            {
                int verticesCount = Math.Max(_count + numberOfVertices, 4096) + _vertices.Length / 2;
                Array.Resize(ref _vertices, verticesCount);
            }
        }

        public void AddVertex(Vertex vertex)
        {
            AllocateVertices(1);
            _vertices[_count++] = vertex;
        }
        
        public void AddVertices(ICollection<Vertex> vertices)
        {
            AllocateVertices(vertices.Count);
            vertices.CopyTo(_vertices, _count);
            _count += vertices.Count;
        }
        
        public void Clear()
        {
            _count = 0;
        }
    }
}