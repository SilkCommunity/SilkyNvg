using System;
using Silk.NET.OpenGL;

namespace SilkyNvg.Rendering.OpenGL.Buffers
{
    internal class Buffer : IDisposable
    {

        private readonly BufferTargetARB _target;
        private readonly BufferUsageARB _usage;
        private readonly uint _bufferId;
        
        private readonly GL _gl;
        
        internal uint Capacity { get; private set; }
        
        internal uint Count { get; private set; }

        internal Buffer(BufferTargetARB target, BufferUsageARB usage, GL gl)
        {
            _target = target;
            _usage = usage;
            _gl = gl;

            _bufferId = _gl.GenBuffer();
        }

        internal unsafe void EnsureCapacity(uint capacity)
        {
            if (Capacity < capacity)
            {
                Bind();
                _gl.BufferData(_target, capacity, null, _usage);
                Unbind();
                Capacity = capacity;
            }
        }

        internal unsafe void Update<T>(uint offset, uint dataCount, ReadOnlySpan<T> data)
            where T : unmanaged
        {
            uint size = dataCount * (uint)sizeof(T);
            uint necessaryCapacity = offset * (uint)sizeof(T) + size;
            EnsureCapacity(necessaryCapacity);
            
            Bind();
            fixed (void* ptr = data)
            {
                _gl.BufferSubData(_target, (int)offset, size, ptr);
            }

            Count = Math.Max(offset + dataCount, Count);
            Unbind();
        }

        internal void UpdateDynamicRange<T>(uint offset, params T[] data)
            where T : unmanaged
            => Update<T>(offset, (uint)data.Length, data);

        internal void Bind()
        {
            _gl.BindBuffer(_target, _bufferId);
        }

        internal void Unbind()
        {
            _gl.BindBuffer(_target, 0);
        }
        
        public void Dispose()
        {
            _gl.DeleteBuffer(_bufferId);
        }
        
    }
}