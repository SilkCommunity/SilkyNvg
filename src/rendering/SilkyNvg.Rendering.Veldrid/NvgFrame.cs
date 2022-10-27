using System;
using System.Runtime.CompilerServices;
using Silk.NET.Maths;
using SilkyNvg.Rendering.Vulkan.Calls;
using SilkyNvg.Rendering.Vulkan.Pipelines;
using SilkyNvg.Rendering.Vulkan.Shaders;
using SilkyNvg.Rendering.Vulkan.Utils;
using Veldrid;

namespace SilkyNvg.Rendering.Vulkan
{
    public class NvgFrame : IDisposable
    {
        public VertexBuffer<Vertex> VertexBuffer { get; }

        public UniformBuffer<VertUniforms> VertexUniformBuffer { get; }

        public readonly ResourceSetCache ResourceSetCache;

        public readonly PipelineCache PipelineCache;

        public readonly UniformManager UniformAllocator;

        public Framebuffer Framebuffer;

        public VertexBuffer<byte> FragmentUniformBuffer { get; }
        public readonly CallQueue Queue;

        public Vector2D<uint> Size;
        
        public NvgFrame(NvgFrameBufferParams parameters, GraphicsDevice device)
        {
        
            ResourceSetCache = new ResourceSetCache(this);
        
            uint fragSize = (uint) (Unsafe.SizeOf<FragUniforms>());
            UniformAllocator = new UniformManager(fragSize);

            Queue = new CallQueue(this);
        
            PipelineCache = new PipelineCache(this);
        

            VertexBuffer = new VertexBuffer<Vertex>(device, new Vertex[1]);

            VertexUniformBuffer = new UniformBuffer<VertUniforms>(device, 1u);

            // This is an ugly hack, I will need to fix it later
            FragmentUniformBuffer = new VertexBuffer<byte>(device, new byte[32]);

            Framebuffer = parameters.Framebuffer;

        }

        public void Dispose()
        {
        
            VertexBuffer.Dispose();
            VertexUniformBuffer.Dispose();
            FragmentUniformBuffer.Dispose();
            PipelineCache.Clear();
            ResourceSetCache.Clear();
        }

        public void Clear()
        {
            ResourceSetCache.Clear();
            UniformAllocator.Clear();
            Queue.Clear();
        }
    }
}
