using System;
using System.Runtime.InteropServices;
using Silk.NET.OpenGL;

namespace SilkyNvg.Rendering.OpenGL.Buffers
{
    internal class Vbo : IDisposable
    {

        private readonly uint _bufferId;
        private readonly BufferTargetARB _bufferType;
        private readonly GL _gl;

        internal Vbo(BufferTargetARB bufferType, GL gl)
        {
            _bufferType = bufferType;
            _gl = gl;
            
            _bufferId = gl.GenBuffer();
        }

        internal void Store<T>(ReadOnlySpan<T> data, uint dataCount, BufferUsageARB usage)
            where T : unmanaged
        {
            Bind();
            _gl.BufferData(_bufferType, dataCount * (uint)Marshal.SizeOf<T>(), data, usage);
        }

        internal void Bind()
        {
            _gl.BindBuffer(_bufferType, _bufferId);
        }

        internal void Unbind()
        {
            _gl.BindBuffer(_bufferType, 0);
        }

        public void Dispose()
        {
            Unbind();
            _gl.DeleteBuffer(_bufferId);
        }

    }
}