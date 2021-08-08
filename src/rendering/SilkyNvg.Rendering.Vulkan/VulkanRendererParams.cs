using Silk.NET.Vulkan;

namespace SilkyNvg.Rendering.Vulkan
{
    public unsafe struct VulkanRendererParams
    {

        public PhysicalDevice gpu;
        public Device device;
        public RenderPass renderpass;
        public CommandBuffer cmdBuffer;

        public AllocationCallbacks* allocator;

        public VulkanRendererParams(PhysicalDevice gpu, Device device, RenderPass renderpass, CommandBuffer cmdBuffer, AllocationCallbacks? allocator = null)
        {
            this.gpu = gpu;
            this.device = device;
            this.renderpass = renderpass;
            this.cmdBuffer = cmdBuffer;
            AllocationCallbacks allc = (AllocationCallbacks)allocator;
            this.allocator = &allc;
        }

    }
}
