using System;
using Silk.NET.OpenGL;

namespace SilkyNvg.Renderers.OpenGL.Buffers
{
    internal class VBO<T> : IDisposable
        where T : unmanaged
    {
        
        private readonly GL _gl;
        private readonly uint _handle;

        internal VBO(GL gl)
        {
            _gl = gl;

            _handle = _gl.GenBuffer();
        }

        internal void Bind()
        {
            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _handle);
        }

        internal void Unbind()
        {
            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        }

        internal void Store(ReadOnlySpan<T> data, BufferUsageARB usage)
        {
            _gl.BufferData<T>(BufferTargetARB.ArrayBuffer, data, usage);
        }
        
        public void Dispose()
        {
            Unbind();
            _gl.DeleteBuffer(_handle);
        }
        
    }
}