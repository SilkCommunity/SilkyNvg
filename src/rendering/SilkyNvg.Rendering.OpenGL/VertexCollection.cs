using System.Collections.Generic;

namespace SilkyNvg.Rendering.OpenGL
{
    internal class VertexCollection
    {

        private readonly List<Vertex> _vertices = new();

        public int CurrentsOffset => _vertices.Count;

        public Vertex[] Vertices => _vertices.ToArray();

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
