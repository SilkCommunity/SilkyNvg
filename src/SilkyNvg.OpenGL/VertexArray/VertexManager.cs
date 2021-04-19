using SilkyNvg.Common;
using System;
using System.Collections.Generic;

namespace SilkyNvg.OpenGL.VertexArray
{
    internal class VertexManager : IDisposable
    {

        private readonly List<Vertex> _vertices = new List<Vertex>();

        public int Offset { get => _vertices.Count; }

        public Vertex[] Vertices { get => _vertices.ToArray(); }

        public void AddVertices(IEnumerable<Vertex> vertices)
        {
            _vertices.AddRange(vertices);
        }

        public void AddVertex(Vertex vertex)
        {
            _vertices.Add(vertex);
        }

        public void Clear()
        {
            _vertices.Clear();
        }

        public void Dispose() => Clear();

    }
}
