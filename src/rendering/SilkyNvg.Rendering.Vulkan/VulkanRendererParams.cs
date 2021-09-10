using Silk.NET.Vulkan;
using System;

namespace SilkyNvg.Rendering.Vulkan
{
    public struct VulkanRendererParams
    {

        public PhysicalDevice PhysicalDevice;
        public Device Device;
        public IntPtr AllocationCallbacks;

        public CommandBuffer InitialCommandBuffer;

        public uint FrameCount;
        public bool AdvanceFrameIndexAutomatically;

        public RenderPass RenderPass;
        public uint SubpassIndex;

        public uint ImageTransitionQueueFamily;
        public uint ImageTransitionQueueFamilyIndex;

    }
}
