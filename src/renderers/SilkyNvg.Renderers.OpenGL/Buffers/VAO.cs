using System;
using Silk.NET.OpenGL;

namespace SilkyNvg.Renderers.OpenGL.Buffers
{
    internal class VAO : IDisposable
    {

        private readonly uint _handle;
        
        private readonly GL _gl;

        internal VAO(GL gl)
        {
            _gl = gl;
            
            _handle = _gl.GenVertexArray();
            Bind();
        }

        internal void Bind()
        {
            _gl.BindVertexArray(_handle);
        }

        internal void Unbind()
        {
            _gl.BindVertexArray(0);
        }

        internal void AttribPointer(uint index, int count, VertexAttribPointerType type, uint vertexSize, int offset = 0)
        {
            Bind();
            _gl.VertexAttribPointer(index, count, type, false, vertexSize, offset);
        }
        
        public void Dispose()
        {
            Unbind();
            _gl.DeleteVertexArray(_handle);
        }
        
    }
}