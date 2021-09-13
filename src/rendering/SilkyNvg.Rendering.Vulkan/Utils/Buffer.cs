using Silk.NET.Vulkan;
using System;
using System.Runtime.InteropServices;

namespace SilkyNvg.Rendering.Vulkan.Utils
{
    internal class Buffer<T> : IDisposable
        where T : unmanaged
    {

        private readonly VulkanRenderer _renderer;
        private readonly BufferUsageFlags _usage;
        private readonly MemoryPropertyFlags _memoryPropertyFlags;

        private Silk.NET.Vulkan.Buffer _buffer;
        private DeviceMemory _memory;

        private ulong _size;

        public Silk.NET.Vulkan.Buffer Handle => _buffer;

        public DescriptorBufferInfo BufferInfo => new()
        {
            Buffer = _buffer,
            Offset = 0,
            Range = (uint)Marshal.SizeOf<T>()
        };

        public Buffer(BufferUsageFlags usage, MemoryPropertyFlags memoryPropertyFlags, VulkanRenderer renderer)
        {
            _renderer = renderer;
            _usage = usage;
            _memoryPropertyFlags = memoryPropertyFlags;

            _size = 0;
            Recreate();
        }

        private unsafe void Recreate()
        {
            Device device = _renderer.Params.Device;
            AllocationCallbacks* allocator = (AllocationCallbacks*)_renderer.Params.AllocationCallbacks.ToPointer();
            Vk vk = _renderer.Vk;

            if (_buffer.Handle == 0)
            {
                Dispose();
            }

            if (!(_size > 0))
            {
                return;
            }

            BufferCreateInfo bufferCreateInfo = new()
            {
                SType = StructureType.BufferCreateInfo,

                Size = _size,
                Usage = _usage,
                SharingMode = SharingMode.Exclusive
            };

            _renderer.AssertVulkan(vk.CreateBuffer(device, bufferCreateInfo, allocator, out _buffer));

            vk.GetBufferMemoryRequirements(device, _buffer, out MemoryRequirements memoryRequirements);
            uint memoryTypeIndex = _renderer.FindMemoryTypeIndex(memoryRequirements.MemoryTypeBits, _memoryPropertyFlags);

            MemoryAllocateInfo memoryAllocateInfo = new()
            {
                SType = StructureType.MemoryAllocateInfo,
                AllocationSize = memoryRequirements.Size,
                MemoryTypeIndex = memoryTypeIndex
            };

            _renderer.AssertVulkan(vk.AllocateMemory(device, memoryAllocateInfo, allocator, out _memory));
            _renderer.AssertVulkan(vk.BindBufferMemory(device, _buffer, _memory, 0));
        }

        private unsafe void Upload(ReadOnlySpan<T> data)
        {
            Device device = _renderer.Params.Device;
            Vk vk = _renderer.Vk;

            void* ptr;
            vk.MapMemory(device, _memory, 0, _size, 0, &ptr);
            Span<T> dst = new(ptr, data.Length);
            data.CopyTo(dst);
            vk.UnmapMemory(device, _memory);
        }

        public void Update(ReadOnlySpan<T> data)
        {
            ulong newSize = (ulong)(Marshal.SizeOf<T>() * data.Length);
            if (newSize > _size)
            {
                _size = newSize;
                Recreate();
                Upload(data);
            }
            else
            {
                Upload(data);
            }
        }

        public void Update(params T[] data)
        {
            Update((ReadOnlySpan<T>)data);
        }

        public unsafe void Dispose()
        {
            Device device = _renderer.Params.Device;
            AllocationCallbacks* allocator = (AllocationCallbacks*)_renderer.Params.AllocationCallbacks.ToPointer();
            Vk vk = _renderer.Vk;

            vk.FreeMemory(device, _memory, allocator);
            vk.DestroyBuffer(device, _buffer, allocator);
        }

    }
}
