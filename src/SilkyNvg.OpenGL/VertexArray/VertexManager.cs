using SilkyNvg.Common;
using System;
using System.Collections.Generic;

namespace SilkyNvg.OpenGL.VertexArray
{
    internal class VertexManager : IDisposable
    {

        private readonly IList<Vertex> _vertices = new List<Vertex>();

        private readonly List<float> _positions = new List<float>();
        private readonly List<float> _textureCoords = new List<float>();

        public IList<Vertex> Vertices => _vertices;

        public int Offset { get => _vertices.Count; }

        public float[] Positions { get => _positions.ToArray(); }
        public float[] TextureCoords { get => _textureCoords.ToArray(); }

        public void AddVertex(Vertex vertex)
        {
            _vertices.Add(vertex);
            _positions.Add(vertex.X);
            _positions.Add(vertex.Y);
            _textureCoords.Add(vertex.U);
            _textureCoords.Add(vertex.V);
        }

        public void AddVertices(List<Vertex> vertices)
        {
            foreach (Vertex vert in vertices)
            {
                AddVertex(vert);
            }
        }

        public void Clear()
        {
            _vertices.Clear();
            _positions.Clear();
            _textureCoords.Clear();
        }

        public void Dispose() => Clear();

    }
}
