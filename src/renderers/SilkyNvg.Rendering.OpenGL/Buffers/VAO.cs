using System;
using Silk.NET.OpenGL;

namespace SilkyNvg.Rendering.OpenGL.Buffers
{
    internal class VAO : IDisposable
    {

        private readonly uint _vaoId;

        private readonly GL _gl;

        internal VAO(GL gl)
        {
            _gl = gl;
            
            _vaoId = gl.GenVertexArray();
        }

        internal unsafe void VertexAttributePointer<T>(uint location, int count, VertexAttribPointerType type, uint elementSize, uint offset)
            where T : unmanaged
        {
            _gl.VertexAttribPointer(location, count, type, false, elementSize * (uint)sizeof(T), (void*)(offset * sizeof(T)));
        }

        internal void Bind()
        {
            _gl.BindVertexArray(_vaoId);
        }

        internal void Unbind()
        {
            _gl.BindVertexArray(0);
        }
        
        public void Dispose()
        {
            _gl.DeleteVertexArray(_vaoId);
        }
        
    }
}