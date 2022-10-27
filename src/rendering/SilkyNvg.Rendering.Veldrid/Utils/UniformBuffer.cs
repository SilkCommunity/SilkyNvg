using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Veldrid;

namespace SilkyNvg.Rendering.Vulkan.Utils
{

    public abstract class BaseUnifomrBuffer
    {
        public abstract DeviceBuffer GetBuffer();
    }
    public unsafe class UniformBuffer<TDataType> : BaseUnifomrBuffer where TDataType : unmanaged
    {
        private readonly uint _alignment;

        internal DeviceBuffer bufferObject;
        GraphicsDevice _device;

        public UniformBuffer(GraphicsDevice gDevice, uint length, uint alignment)
        {
            _device = gDevice;
            _alignment = unchecked((alignment + (16u - 1u)) & (uint)(-16));

            bufferObject = _device.ResourceFactory.CreateBuffer(new BufferDescription(
                length * _alignment,
                BufferUsage.UniformBuffer | BufferUsage.Dynamic));
        }

        public UniformBuffer(GraphicsDevice gDevice, uint length) : this(gDevice, length, (uint)Unsafe.SizeOf<TDataType>())
        {
        }

        public uint ByteLength => bufferObject?.SizeInBytes ?? 0;
        public uint Length => bufferObject?.SizeInBytes / _alignment ?? 0;

        public void Dispose()
        {
            _device.DisposeWhenIdle(bufferObject);
        }

        public void Resize(uint capacity)
        {
            _device.DisposeWhenIdle(bufferObject);

            bufferObject = _device.ResourceFactory.CreateBuffer(new BufferDescription(
                capacity * _alignment,
                BufferUsage.UniformBuffer | BufferUsage.Dynamic));
        }

        public void ModifyBuffer(ReadOnlySpan<byte> data, uint alignment, CommandList list = null)
        {
            uint count = (uint)data.Length / alignment;
            Debug.Assert(data.Length % alignment == 0);

            uint capacity = bufferObject.SizeInBytes / _alignment;

            if (count > capacity)
            {
                Resize(count);
            }

            if (alignment == _alignment)
            {
                if (list != null)
                    list.UpdateBuffer(bufferObject, 0, data);
                else
                    _device.UpdateBuffer(bufferObject, 0, data);
            }
            else
            {
                Span<byte> buffer = stackalloc byte[2048];
                uint maxLength = (uint)(buffer.Length + _alignment - 1) / _alignment;

                
                uint srcOffset = 0;
                uint dstOffset = 0;
                uint dstAlignment = _alignment;

                while (count > 0)
                {
                    uint toUpload = count;
                    if (toUpload > maxLength)
                        toUpload = maxLength;

                    uint srcLength = toUpload * alignment;
                    uint dstLength = toUpload * dstAlignment;

                    ReadOnlySpan<byte> src = data.Slice((int)srcOffset, (int)srcLength);
                    Span<byte> dst = buffer.Slice(0, (int)dstLength);

                    for (uint i = 0; i < toUpload; i++)
                    {
                        src.Slice((int)(i * alignment), (int)alignment).CopyTo(dst.Slice((int)(i * dstAlignment), (int)dstAlignment));
                    }

                    if (list != null)
                        list.UpdateBuffer(bufferObject, dstOffset, dst);
                    else
                        _device.UpdateBuffer(bufferObject, dstOffset, dst);

                    count -= toUpload;
                    srcOffset += srcLength;
                    dstOffset += dstLength;
                }
            }
        }

        public void ModifyBuffer(ReadOnlySpan<TDataType> data, uint alignment, CommandList list = null)
        {
            ModifyBuffer(MemoryMarshal.AsBytes(data), alignment, list);
        }

        public void ModifyBuffer(ReadOnlySpan<TDataType> data, CommandList list = null)
        {
            ModifyBuffer(data, (uint)Unsafe.SizeOf<TDataType>(), list);
        }

        public void ModifyBuffer(TDataType data, CommandList list = null)
        {
            ModifyBuffer(MemoryMarshal.CreateSpan(ref data, 1), list);
        }

        public override DeviceBuffer GetBuffer()
        {
            return bufferObject;
        }
    }
}