using Silk.NET.Vulkan;
using SilkyNvg.Rendering.Vulkan.Pipelines;

namespace SilkyNvg.Rendering.Vulkan.Calls
{
    internal abstract class Call
    {

        protected readonly int image;
        protected readonly Path[] paths;
        protected readonly int triangleOffset;
        protected readonly uint triangleCount;
        protected readonly int uniformOffset;

        protected readonly Pipelines.Pipeline renderPipeline;
        protected readonly Pipelines.Pipeline antiAliasPipeline;

        protected readonly VulkanRenderer renderer;

        protected Call(int image, Path[] paths, int triangleOffset, uint triangleCount, int uniformOffset, Pipelines.Pipeline renderPipeline, Pipelines.Pipeline antiAliasPipeline)
        {
            this.image = image;
            this.paths = paths;
            this.triangleOffset = triangleOffset;
            this.triangleCount = triangleCount;
            this.uniformOffset = uniformOffset;
            this.renderPipeline = renderPipeline;
            this.antiAliasPipeline = antiAliasPipeline;
            renderer = VulkanRenderer.Instance;
        }

        public abstract void Run(CommandBuffer cmd);

    }
}