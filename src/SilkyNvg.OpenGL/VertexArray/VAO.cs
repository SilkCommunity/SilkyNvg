using Silk.NET.OpenGL;
using SilkyNvg.OpenGL.VertexBuffer;
using System;
using System.Collections.Generic;

namespace SilkyNvg.OpenGL.VertexArray
{
    internal sealed class VAO : IDisposable
    {

        private readonly IList<VBO> _vbos = new List<VBO>();
        private readonly IList<uint> _slots = new List<uint>();
        private readonly uint _vaoID;
        private readonly GL _gl;

        public VAO(GL gl)
        {
            _gl = gl;
            _vaoID = _gl.GenVertexArray();
        }

        public void Bind()
        {
            _gl.BindVertexArray(_vaoID);
            EnableAttributeLists();
        }

        public void Unbind()
        {
            DisableAttributeLists();
            _gl.BindVertexArray(0);
        }

        public unsafe void StoreVBO(uint slot, int dimensions, VertexAttribPointerType type, VBO vbo)
        {
            Bind();
            _gl.VertexAttribPointer(slot, dimensions, type, false, 0, null);
            Unbind();
            _vbos.Add(vbo);
            _slots.Add(slot);
        }

        public void EnableAttributeLists()
        {
            foreach (uint slot in _slots)
            {
                _gl.EnableVertexAttribArray(slot);
            }
        }

        public void DisableAttributeLists()
        {
            foreach (uint slot in _slots)
            {
                _gl.DisableVertexAttribArray(slot);
            }
        }

        public void Dispose()
        {
            Unbind();
            foreach (VBO vbo in _vbos)
            {
                vbo.Dispose();
            }
            _gl.DeleteVertexArray(_vaoID);
        }

    }
}
