using Silk.NET.Vulkan;
using SilkyNvg.Rendering.Vulkan.Shaders;
using SilkyNvg.Rendering.Vulkan.Utils;
using System;

namespace SilkyNvg.Rendering.Vulkan
{
    internal class Frame : IDisposable
    {

        private readonly VulkanRenderer _renderer;

        public Buffer<Vertex> VertexBuffer { get; }

        public Buffer<VertUniforms> VertexUniformBuffer { get; }

        public Buffer<byte> FragmentUniformBuffer { get; }

        public DescriptorSetManager DescriptorSetManager { get; }

        public unsafe Frame(VulkanRenderer renderer)
        {
            _renderer = renderer;

            VertexBuffer = new Buffer<Vertex>(BufferUsageFlags.BufferUsageVertexBufferBit,
                MemoryPropertyFlags.MemoryPropertyHostVisibleBit, _renderer);

            VertexUniformBuffer = new Buffer<VertUniforms>(BufferUsageFlags.BufferUsageUniformBufferBit,
                MemoryPropertyFlags.MemoryPropertyHostVisibleBit, _renderer);

            FragmentUniformBuffer = new Buffer<byte>(BufferUsageFlags.BufferUsageUniformBufferBit,
                MemoryPropertyFlags.MemoryPropertyHostVisibleBit, _renderer);

            DescriptorSetManager = new DescriptorSetManager(_renderer);
        }

        public unsafe void Dispose()
        {
            VertexBuffer.Dispose();
            VertexUniformBuffer.Dispose();
            FragmentUniformBuffer.Dispose();
            DescriptorSetManager.Dispose();
        }

    }
}
