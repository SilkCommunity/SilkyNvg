using Silk.NET.Vulkan;
using System;

namespace Vulkan_Example
{
    public class Frame : IDisposable
    {

        public const uint COMMAND_BUFFER_RESET_THRESHOLD = 10;

        private readonly CommandBuffer[] _commandBuffers = new CommandBuffer[COMMAND_BUFFER_RESET_THRESHOLD];
        private readonly CommandPool _commandPool;

        private readonly Device _device;
        private readonly Vk _vk;

        private uint _commandBufferIndex;

        public Semaphore RenderSemaphore { get; }

        public Semaphore PresentSemaphore { get; }

        public Fence RenderFence { get; }

        public unsafe Frame(uint graphicsQueueFamily, Device device, Vk vk)
        {
            _device = device;
            _vk = vk;

            FenceCreateInfo fenceCreateInfo = VkInit.FenceCreateInfo(true);
            SemaphoreCreateInfo semaphoreCreateInfo = VkInit.SemaphoreCreateInfo();
            VkUtil.AssertVulkan(vk.CreateFence(_device, fenceCreateInfo, null, out Fence renderFence));
            VkUtil.AssertVulkan(vk.CreateSemaphore(_device, semaphoreCreateInfo, null, out Semaphore renderSemaphore));
            VkUtil.AssertVulkan(vk.CreateSemaphore(_device, semaphoreCreateInfo, null, out Semaphore presentSemaphore));
            RenderSemaphore = renderSemaphore;
            PresentSemaphore = presentSemaphore;
            RenderFence = renderFence;

            CommandPoolCreateInfo commandPoolCreateInfo = VkInit.CommandPoolCreateInfo(graphicsQueueFamily, 0);
            VkUtil.AssertVulkan(_vk.CreateCommandPool(_device, commandPoolCreateInfo, null, out _commandPool));

            CreateCommandBuffers();

            _commandBufferIndex = 0;
        }

        public CommandBuffer GetCommandBuffer()
        {
            if (_commandBufferIndex >= COMMAND_BUFFER_RESET_THRESHOLD)
            {
                VkUtil.AssertVulkan(_vk.ResetCommandPool(_device, _commandPool, 0));
                _commandBufferIndex = 0;
            }
            return _commandBuffers[_commandBufferIndex++];
        }

        public void FreeCommandBuffers()
        {
            _vk.FreeCommandBuffers(_device, _commandPool, _commandBuffers);
        }

        public void CreateCommandBuffers()
        {
            CommandBufferAllocateInfo commandBufferAllocateInfo = VkInit.CommandBufferAllocateInfo(_commandPool, COMMAND_BUFFER_RESET_THRESHOLD);
            for (int i = 0; i < COMMAND_BUFFER_RESET_THRESHOLD; i++)
            {
                VkUtil.AssertVulkan(_vk.AllocateCommandBuffers(_device, commandBufferAllocateInfo, out _commandBuffers[0]));
            }
        }

        public unsafe void Dispose()
        {
            _vk.DestroyCommandPool(_device, _commandPool, null);
            _vk.DestroyFence(_device, RenderFence, null);
            _vk.DestroySemaphore(_device, RenderSemaphore, null);
            _vk.DestroySemaphore(_device, PresentSemaphore, null);
        }

    }
}
