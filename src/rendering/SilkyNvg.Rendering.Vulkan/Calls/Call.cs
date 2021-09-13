using Silk.NET.Vulkan;
using SilkyNvg.Rendering.Vulkan.Pipelines;

namespace SilkyNvg.Rendering.Vulkan.Calls
{
    internal abstract class Call
    {

        protected readonly int image;
        protected readonly Path[] paths;
        protected readonly uint triangleOffset;
        protected readonly uint triangleCount;
        protected readonly ulong uniformOffset;

        protected readonly PipelineSettings renderPipeline;
        protected readonly PipelineSettings stencilPipeline;
        protected readonly PipelineSettings antiAliasPipeline;

        protected readonly VulkanRenderer renderer;

        protected Call(int image, Path[] paths, uint triangleOffset, uint triangleCount, ulong uniformOffset,
            PipelineSettings renderPipeline, PipelineSettings stencilPipeline, PipelineSettings antiAliasPipeline, VulkanRenderer renderer)
        {
            this.image = image;
            this.paths = paths;
            this.triangleOffset = triangleOffset;
            this.triangleCount = triangleCount;
            this.uniformOffset = uniformOffset;
            this.renderPipeline = renderPipeline;
            this.stencilPipeline = stencilPipeline;
            this.antiAliasPipeline = antiAliasPipeline;
            this.renderer = renderer;
        }

        public abstract unsafe void Run(Frame frame, CommandBuffer cmd);

    }
}