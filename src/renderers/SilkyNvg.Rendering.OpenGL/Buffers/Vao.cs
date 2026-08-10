using System;
using Silk.NET.OpenGL;

namespace SilkyNvg.Rendering.OpenGL.Buffers
{
    internal class Vao : IDisposable
    {

        private readonly uint _vaoId;
        private readonly GL _gl;

        internal Vao(GL gl)
        {
            _gl = gl;
            _vaoId = _gl.GenVertexArray();
        }
        
        internal unsafe void VertexAttributePointer<T>(uint location, int count, VertexAttribPointerType type, uint elementSize, uint offset)
            where T : unmanaged
        {
            _gl.VertexAttribPointer(location, count, type, false, elementSize * (uint)sizeof(T), (void*)(offset * sizeof(T)));
        }
        
        internal unsafe void VertexAttributeIPointer<T>(uint location, int count, VertexAttribIType type, uint elementSize, uint offset)
            where T : unmanaged
        {
            _gl.VertexAttribIPointer(location, count, type, elementSize * (uint)sizeof(T), (void*)(offset * sizeof(T)));
        }

        internal void Bind()
        {
            _gl.BindVertexArray(_vaoId);
        }
        
        public void Dispose()
        {
            _gl.DeleteVertexArray(_vaoId);
        }
        
    }
}