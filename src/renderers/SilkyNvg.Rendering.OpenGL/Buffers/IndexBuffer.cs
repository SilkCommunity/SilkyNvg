using System;
using Silk.NET.OpenGL;

namespace SilkyNvg.Rendering.OpenGL.Buffers
{
    internal class IndexBuffer : IDisposable
    {

        private readonly BufferUsageARB _usage;
        private readonly uint _bufferId;
        
        private readonly GL _gl;

        private uint _capacity;

        internal IndexBuffer(BufferUsageARB usage, GL gl)
        {
            _gl = gl;
            _usage = usage;

            _capacity = 0;
            
            _bufferId = gl.GenBuffer();
        }

        internal void Bind()
        {
            _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _bufferId);
        }

        internal void Load(uint count, ReadOnlySpan<uint> indices)
        {
            Bind();
            if (_capacity < count)
            {
                _gl.BufferData(BufferTargetARB.ElementArrayBuffer, count * sizeof(uint), indices, _usage);
                _capacity = (uint)indices.Length;
            }
            else
            {
                _gl.BufferSubData(BufferTargetARB.ElementArrayBuffer, 0, count * sizeof(uint), indices);
            }
        }

        public void Dispose()
        {
            _gl.DeleteBuffer(_bufferId);
        }
        
    }
}