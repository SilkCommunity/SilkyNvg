using System;
using System.Runtime.CompilerServices;
using Silk.NET.OpenGL;
using SilkyNvg.Rendering.OpenGL.Fragments;

namespace SilkyNvg.Rendering.OpenGL.Buffers
{
    internal class FragmentBuffer : IDisposable
    {

        private readonly BufferUsageARB _usage;

        private readonly uint _bufferId;
        private readonly GL _gl;

        internal uint Capacity { get; private set; }

        internal unsafe FragmentBuffer(uint binding, uint capacity, GL gl)
        {
            _gl = gl;
            _usage = BufferUsageARB.StreamCopy;
            
            Capacity = capacity;
            
            _bufferId = gl.GenBuffer();
            Bind();
            _gl.BufferData(BufferTargetARB.ShaderStorageBuffer, Capacity * (uint)sizeof(FragmentData), null, _usage);
            _gl.BindBufferBase(BufferTargetARB.ShaderStorageBuffer, binding, _bufferId);
            Unbind();
        }

        internal unsafe void EnsureCapacity(uint necessaryCapacity)
        {
            if (Capacity < necessaryCapacity)
            {
                Bind();
                // 4 bytes for emptyFragmentCount uint
                _gl.BufferData(BufferTargetARB.ShaderStorageBuffer, necessaryCapacity * (uint)sizeof(FragmentData) + 4, null, _usage);
                Unbind();
                Capacity = necessaryCapacity;
            }
        }

        internal unsafe void Read(FragmentData[] output, out uint emptyFragmentCount)
        {
            if (output.Length < Capacity)
            {
                throw new Exception("Output buffer is too small!");
            }

            Bind();
            void* dataPtr = _gl.MapBuffer(BufferTargetARB.ShaderStorageBuffer, BufferAccessARB.ReadOnly);
            if (dataPtr == null)
            {
                throw new Exception("Failed to map buffer!");
            }

            emptyFragmentCount = *(uint*)dataPtr;

            var ptr = new IntPtr(dataPtr);
            var data = new Span<FragmentData>((ptr + 4).ToPointer(), (int)Capacity);
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