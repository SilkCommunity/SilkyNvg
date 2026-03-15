using System;
using Silk.NET.OpenGL;

namespace SilkyNvg.Rendering.OpenGL.Buffers
{
    internal class Buffer<T> : IDisposable
        where T : unmanaged
    {

        private readonly BufferUsageARB _usage;

        private readonly uint _bufferId;
        private readonly GL _gl;
        
        private uint _capacity;
        
        internal unsafe Buffer(uint binding, uint capacity, BufferUsageARB usage, GL gl)
        {
            _gl = gl;
            _usage = usage;
            
            _capacity = capacity;
            
            _bufferId = gl.GenBuffer();
            Bind();
            _gl.BufferData(BufferTargetARB.ShaderStorageBuffer, _capacity * (uint)sizeof(T), null, _usage);
            _gl.BindBufferBase(BufferTargetARB.ShaderStorageBuffer, binding, _bufferId);
            Unbind();
        }

        internal unsafe void EnsureCapacity(uint necessaryCapacity)
        {
            if (_capacity < necessaryCapacity)
            {
                Bind();
                _gl.BufferData(BufferTargetARB.ShaderStorageBuffer, _capacity * (uint)sizeof(T), null, _usage);
                Unbind();
                _capacity = necessaryCapacity;
            }
        }

        internal unsafe void Update(ReadOnlySpan<T> data, uint offset, uint dataCount)
        {
            uint size = dataCount * (uint)sizeof(T);
            EnsureCapacity(offset + dataCount);
            Bind();
            fixed (void* ptr = data)
            {
                _gl.BufferSubData(BufferTargetARB.ShaderStorageBuffer, (int)offset, size, ptr);
            }
            Unbind();
        }
        
        internal void Update(ReadOnlySpan<T> data)
        {
            Update(data, 0, (uint)data.Length);
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