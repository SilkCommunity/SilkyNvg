using System;
using System.Collections.Generic;

namespace SilkyNvg.Rendering.OpenGL
{
    internal class VertexCollection
    {

        private const int MAX_VERTICES = 10_000_000;

        private readonly List<Vertex> _vertices = new(MAX_VERTICES);

        public int CurrentsOffset => _vertices.Count;

        public ReadOnlySpan<Vertex> Vertices => _vertices.ToArray();

        public VertexCollection()
        {

        }

        public void AddVertex(Vertex vertex)
        {
            _vertices.Add(vertex);
        }

        public void AddVertices(IEnumerable<Vertex> vertices)
        {
            _vertices.AddRange(vertices);
        }

        public void Clear()
        {
            _vertices.Clear();
        }

    }
}