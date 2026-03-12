using System;
using Silk.NET.OpenGL;

namespace SilkyNvg.Rendering.OpenGL.Buffers
{
    internal class Buffer<T> : IDisposable
        where T : unmanaged
    {

        private readonly uint _capacity;

        private readonly uint _bufferId;
        private readonly GL _gl;

        internal unsafe Buffer(uint binding, uint capacity, BufferUsageARB usage, GL gl)
        {
            _gl = gl;

            _capacity = capacity;
            
            _bufferId = gl.GenBuffer();
            Bind();
            _gl.BufferData(BufferTargetARB.ShaderStorageBuffer, _capacity * (uint)sizeof(T), null, usage);
            _gl.BindBufferBase(BufferTargetARB.ShaderStorageBuffer, binding, _bufferId);
            Unbind();
        }

        internal void Update(ReadOnlySpan<T> data)
        {
            Bind();
            _gl.BufferSubData(BufferTargetARB.ShaderStorageBuffer, 0, data);
            Unbind();
        }

        internal unsafe void Read(T[] output)
        {
            if (output.Length < _capacity)
            {
                throw new Exception("Output buffer is too small!");
            }

            Bind();
            void* dataPtr = _gl.MapBuffer(BufferTargetARB.ShaderStorageBuffer, BufferAccessARB.ReadOnly);
            if (dataPtr == null)
            {
                throw new Exception("Failed to map buffer!");
            }

            var data = new Span<T>(dataPtr, (int)_capacity);
            data.CopyTo(output);
            
            _gl.UnmapBuffer(BufferTargetARB.ShaderStorageBuffer);
            Unbind();
        }

        internal void Bind()
        {
            _gl.BindBuffer(BufferTargetARB.ShaderStorageBuffer, _bufferId);
        }

        internal void Unbind()
        {
            _gl.BindBuffer(BufferTargetARB.ShaderStorageBuffer, 0);
        }
        
        public void Dispose()
        {
            Unbind();
            _gl.DeleteBuffer(_bufferId);
        }
        
    }
}