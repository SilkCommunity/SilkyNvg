using Silk.NET.Vulkan;
using System;

namespace SilkyNvg.Rendering.Vulkan.Utils
{
    internal struct Buffer<T> : IDisposable
        where T : unmanaged
    {
        private readonly DeviceMemory _mem;
        private readonly ulong _size;

        private readonly VulkanRenderer _renderer;

        public Silk.NET.Vulkan.Buffer Handle { get; }

        public unsafe Buffer(BufferUsageFlags usage,
            MemoryPropertyFlags memoryType, ReadOnlySpan<T> data, VulkanRenderer renderer)
        {
            Device device = renderer.Params.device;
            AllocationCallbacks* allocator = renderer.Params.allocator;

            ulong size = (ulong)(data.Length * sizeof(T));
            BufferCreateInfo bufCreateInfo = new(StructureType.BufferCreateInfo, null, 0, size, usage);

            renderer.Assert(renderer.Vk.CreateBuffer(device, bufCreateInfo, allocator, out Silk.NET.Vulkan.Buffer buffer));
            renderer.Vk.GetBufferMemoryRequirements(device, buffer, out MemoryRequirements memReqs);

            renderer.Assert(renderer.MemoryTypeFromProperties(memReqs.MemoryTypeBits, memoryType, out uint memoryTypeIndex));
            MemoryAllocateInfo memAlloc = new(StructureType.MemoryAllocateInfo, null, memReqs.Size, memoryTypeIndex);

            renderer.Assert(renderer.Vk.AllocateMemory(device, memAlloc, null, out DeviceMemory mem));

            void* mapped;
            renderer.Assert(renderer.Vk.MapMemory(device, mem, 0, memAlloc.AllocationSize, 0, &mapped));
            Span<T> dest = new(mapped, data.Length);
            data.CopyTo(dest);
            renderer.Vk.UnmapMemory(device, mem);
            renderer.Assert(renderer.Vk.BindBufferMemory(device, buffer, mem, 0));

            Handle = buffer;
            _mem = mem;
            _size = memAlloc.AllocationSize;

            _renderer = renderer;
        }

        private unsafe void Update(ReadOnlySpan<T> data)
        {
            ulong size = (ulong)(sizeof(T) * data.Length);

            void* mapped;
            _renderer.Assert(_renderer.Vk.MapMemory(_renderer.Params.device, _mem, 0, size, 0, &mapped));
            Span<T> dest = new(mapped, data.Length);
            data.CopyTo(dest);
            _renderer.Vk.UnmapMemory(_renderer.Params.device, _mem);
        }

        public unsafe void Dispose()
        {
            if (Handle.Handle != 0)
            {
                _renderer.Vk.DestroyBuffer(_renderer.Params.device, Handle, _renderer.Params.allocator);
            }
            if (_mem.Handle != 0)
            {
                _renderer.Vk.FreeMemory(_renderer.Params.device, _mem, _renderer.Params.allocator);
            }
        }

        public static unsafe void UpdateBuffer(ref Buffer<T> buffer, BufferUsageFlags usage, MemoryPropertyFlags memoryType, ReadOnlySpan<T> data, VulkanRenderer renderer)
        {
            ulong size = (ulong)(data.Length * sizeof(T));
            if (buffer._size < size)
            {
                buffer.Dispose();
                buffer = new Buffer<T>(usage, memoryType, data, renderer);
            }
            else
            {
                buffer.Update(data);
            }
        }

        public static unsafe void UpdateBuffer(ref Buffer<T> buffer, BufferUsageFlags usage, MemoryPropertyFlags memoryType, Span<T> data, VulkanRenderer renderer)
        {
            ulong size = (ulong)(data.Length * sizeof(T));
            if (buffer._size < size)
            {
                buffer.Dispose();
                buffer = new Buffer<T>(usage, memoryType, data, renderer);
            }
            else
            {
                buffer.Update(data);
            }
        }

    }
}
