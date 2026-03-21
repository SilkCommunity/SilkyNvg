using System;
using Silk.NET.OpenGL;

namespace SilkyNvg.Rendering.OpenGL.Buffers
{
    internal class Ssbo : IDisposable
    {

        private readonly uint _binding;
        private readonly GL _gl;
        
        private uint _bufferId = 0;
        
        internal uint Capacity { get; private set; }

        internal Ssbo(uint binding, GL gl)
        {
            _gl = gl;
            _binding = binding;

            Capacity = 0;
        }

        internal unsafe void EnsureCapacity(uint capacity, BufferStorageMask mask)
        {
            if (Capacity < capacity)
            {
                if (_bufferId != 0)
                {
                    _gl.BindBuffer(BufferTargetARB.ShaderStorageBuffer, 0);
                    _gl.DeleteBuffer(_bufferId);
                }

                _bufferId = _gl.GenBuffer();
                Bind();
                _gl.NamedBufferStorage(_bufferId, capacity, null, mask);
                _gl.BindBufferBase(BufferTargetARB.ShaderStorageBuffer, _binding, _bufferId);
                
                Capacity = capacity;
            }
        }

        internal unsafe void Store<T>(BufferStorageMask mask, uint count, ReadOnlySpan<T> data)
            where T : unmanaged
        {
            uint necessaryCapacity = (uint)(count * sizeof(T));
            Bind();
            EnsureCapacity(necessaryCapacity, mask);
            _gl.BufferSubData(BufferTargetARB.ShaderStorageBuffer, 0, necessaryCapacity, data);
        }

        internal void Bind()
        {
            if (_bufferId != 0)
            {
                _gl.BindBuffer(BufferTargetARB.ShaderStorageBuffer, _bufferId);
            }
        }
        
        public void Dispose()
        {
            if (_bufferId != 0)
            {
                _gl.BindBuffer(BufferTargetARB.ShaderStorageBuffer, 0);
                _gl.DeleteBuffer(_bufferId);
            }
        }
        
    }
}