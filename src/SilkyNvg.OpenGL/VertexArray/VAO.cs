using Silk.NET.OpenGL;
using SilkyNvg.Common;
using System;
using System.Collections.Generic;

namespace SilkyNvg.OpenGL.VertexArray
{
    internal class VAO : IDisposable
    {

        private readonly uint _vaoID;
        private readonly GL _gl;

        private readonly VBO _dataBuffer;

        private readonly IList<uint> _indices = new List<uint>();

        public VBO Buffer => _dataBuffer;

        public uint ID => _vaoID;

        public VAO(GL gl)
        {
            _gl = gl;

            _vaoID = _gl.GenVertexArray();
            _dataBuffer = new VBO(_gl);
        }

        public void Bind()
        {
            _gl.BindVertexArray(_vaoID);
        }

        public unsafe void VertexAttribPointer(uint index, int dimensions)
        {
            _indices.Add(index);
            _gl.VertexAttribPointer(index, dimensions, VertexAttribPointerType.Float, false, (uint)sizeof(Vertex), (float*)0);
            _gl.EnableVertexAttribArray(index);
        }

        public void Unbind()
        {
            foreach (uint index in _indices)
            {
                _gl.DisableVertexAttribArray(index);
            }
            _indices.Clear();
            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
            _gl.BindVertexArray(0);
        }

        public void Dispose()
        {
            _indices.Clear();
            Unbind();
            _gl.DeleteVertexArray(_vaoID);
            _dataBuffer.Dispose();
        }

    }
}
