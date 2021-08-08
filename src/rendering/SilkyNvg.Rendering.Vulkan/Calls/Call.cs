using SilkyNvg.Blending;

namespace SilkyNvg.Rendering.Vulkan.Calls
{
    internal abstract class Call
    {

        protected readonly int image;
        protected readonly Path[] paths;
        protected readonly int triangleOffset;
        protected readonly uint triangleCount;
        protected readonly int uniformOffset;
        protected readonly CompositeOperationState compositeOperation;

        protected readonly VulkanRenderer renderer;

        protected Call(int image, Path[] paths, int triangleOffset, uint triangleCount, int uniformOffset, CompositeOperationState compositeOperation, VulkanRenderer renderer)
        {
            this.image = image;
            this.paths = paths;
            this.triangleOffset = triangleOffset;
            this.triangleCount = triangleCount;
            this.uniformOffset = uniformOffset;
            this.compositeOperation = compositeOperation;
            this.renderer = renderer;
        }

        public abstract void Run();

    }
}