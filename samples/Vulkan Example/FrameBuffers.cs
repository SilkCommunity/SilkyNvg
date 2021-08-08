using Silk.NET.Core;
using Silk.NET.Vulkan;
using System;
using System.Diagnostics;

namespace Vulkan_Example
{
    public struct FrameBuffers : IDisposable
    {

        private readonly Semaphore _presentCompleteSemaphore;
        private readonly Semaphore _renderCompleteSemaphore;

        private readonly VulkanDevice _device;
        private readonly Vk _vk;

        public SwapchainKHR SwapChain { get; }

        public SwapchainBuffers[] SwapChainBuffers { get; }

        public uint SwapchainImageCount { get; }

        public Framebuffer[] Framebuffers { get; }

        public uint CurrentBuffer;

        public Extent2D BufferSize { get; }

        public RenderPass RenderPass { get; }

        public Format Format { get; }

        public DepthBuffer Depth { get; }

        public Semaphore PresentCompleteSemaphore => _presentCompleteSemaphore;

        public Semaphore RenderCompleteSemaphore => _renderCompleteSemaphore;

        public unsafe FrameBuffers(VulkanDevice device, SurfaceKHR surface, Queue queue, int winWidth, int winHeight, SwapchainKHR oldSwapchain, Vk vk)
        {
            _vk = vk;
            _device = device;

            Result res;

            VkUtil.KhrSurface.GetPhysicalDeviceSurfaceSupport(device.Gpu, device.GraphicsQueueFamilyIndex, surface, out Bool32 supportsPresent);
            if (!supportsPresent)
            {
                Environment.Exit(-1);
            }
            CommandBuffer setupCmdBuffer = VkUtil.CreateCmdBuffer(device.Device, device.CommandPool);

            CommandBufferBeginInfo cmdBufInfo = new(StructureType.CommandBufferBeginInfo);
            _vk.BeginCommandBuffer(setupCmdBuffer, cmdBufInfo);

            Format colourFormat = Format.B8G8R8A8Unorm;
            ColorSpaceKHR colourSpace;
            {
                uint formatCount = 0;
                res = VkUtil.KhrSurface.GetPhysicalDeviceSurfaceFormats(device.Gpu, surface, ref formatCount, null);
                Debug.Assert(res == Result.Success);
                SurfaceFormatKHR* surfaceFormats = stackalloc SurfaceFormatKHR[(int)formatCount];
                res = VkUtil.KhrSurface.GetPhysicalDeviceSurfaceFormats(device.Gpu, surface, ref formatCount, surfaceFormats);
                Debug.Assert(res == Result.Success);

                if (formatCount == 1 && surfaceFormats[0].Format == Format.Undefined)
                {
                    colourFormat = Format.B8G8R8A8Unorm;
                }
                else
                {
                    Debug.Assert(formatCount >= 1);
                    colourFormat = surfaceFormats[0].Format;
                }
                colourSpace = surfaceFormats[0].ColorSpace;
            }
            colourFormat = Format.B8G8R8A8Unorm;

            res = VkUtil.KhrSurface.GetPhysicalDeviceSurfaceCapabilities(device.Gpu, surface, out SurfaceCapabilitiesKHR surfCapabillities);
            Debug.Assert(res == Result.Success);

            Extent2D bufferSize = default;
            if (surfCapabillities.CurrentExtent.Width == uint.MaxValue)
            {
                bufferSize.Width = (uint)winWidth;
                bufferSize.Height = (uint)winHeight;
            }
            else
            {
                bufferSize = surfCapabillities.CurrentExtent;
            }

            DepthBuffer depth = new(device, (int)bufferSize.Width, (int)bufferSize.Height, vk);

            RenderPass renderPass = VkUtil.CreateRenderPass(device.Device, colourFormat, depth.Format);

            PresentModeKHR swapchainPresentMode = PresentModeKHR.PresentModeFifoKhr;

            uint presentModeCount = 0;
            VkUtil.KhrSurface.GetPhysicalDeviceSurfacePresentModes(device.Gpu, surface, ref presentModeCount, null);
            Debug.Assert(presentModeCount > 0);

            PresentModeKHR* presentModes = stackalloc PresentModeKHR[(int)presentModeCount];
            VkUtil.KhrSurface.GetPhysicalDeviceSurfacePresentModes(device.Gpu, surface, ref presentModeCount, presentModes);

            for (uint i = 0; i < presentModeCount; i++)
            {
                if (presentModes[i] == PresentModeKHR.PresentModeMailboxKhr)
                {
                    swapchainPresentMode = PresentModeKHR.PresentModeMailboxKhr;
                    break;
                }
                if ((swapchainPresentMode != PresentModeKHR.PresentModeMailboxKhr) && (presentModes[i] == PresentModeKHR.PresentModeImmediateKhr))
                {
                    swapchainPresentMode = PresentModeKHR.PresentModeImmediateKhr;
                }
            }

            SurfaceTransformFlagsKHR preTransform;
            if (surfCapabillities.SupportedTransforms.HasFlag(SurfaceTransformFlagsKHR.SurfaceTransformIdentityBitKhr))
            {
                preTransform = SurfaceTransformFlagsKHR.SurfaceTransformIdentityBitKhr;
            }
            else
            {
                preTransform = surfCapabillities.CurrentTransform;
            }

            uint desiredNumberOfSwapchainImages = surfCapabillities.MinImageCount + 1;
            if ((surfCapabillities.MaxImageCount > 0) && (desiredNumberOfSwapchainImages > surfCapabillities.MaxImageCount))
            {
                desiredNumberOfSwapchainImages = surfCapabillities.MaxImageCount;
            }

            SwapchainCreateInfoKHR swapchainInfo = new()
            {
                SType = StructureType.SwapchainCreateInfoKhr,

                Surface = surface,
                MinImageCount = desiredNumberOfSwapchainImages,
                ImageFormat = colourFormat,
                ImageColorSpace = colourSpace,
                ImageExtent = bufferSize,
                ImageUsage = ImageUsageFlags.ImageUsageColorAttachmentBit,
                PreTransform = preTransform,
                CompositeAlpha = CompositeAlphaFlagsKHR.CompositeAlphaOpaqueBitKhr,
                ImageArrayLayers = 1,
                ImageSharingMode = SharingMode.Exclusive,
                PresentMode = swapchainPresentMode,
                OldSwapchain = oldSwapchain,
                Clipped = true
            };

            res = VkUtil.KhrSwapchain.CreateSwapchain(device.Device, swapchainInfo, null, out SwapchainKHR swapchain);
            Debug.Assert(res == Result.Success);

            if (oldSwapchain.Handle != 0)
            {
                VkUtil.KhrSwapchain.DestroySwapchain(device.Device, swapchain, null);
            }

            uint swapchainImageCount = 0;
            res = VkUtil.KhrSwapchain.GetSwapchainImages(device.Device, swapchain, ref swapchainImageCount, null);
            Debug.Assert(res == Result.Success);

            Image* swapchainImages = stackalloc Image[(int)swapchainImageCount];
            Debug.Assert(swapchainImages != null);
            res = VkUtil.KhrSwapchain.GetSwapchainImages(device.Device, swapchain, ref swapchainImageCount, swapchainImages);
            Debug.Assert(res == Result.Success);

            SwapchainBuffers[] swapChainBuffers = new SwapchainBuffers[(int)swapchainImageCount];
            for (uint i = 0; i < swapchainImageCount; i++)
            {
                swapChainBuffers[(int)i] = new SwapchainBuffers(device, colourFormat, setupCmdBuffer, swapchainImages[i], _vk);
            }

            ImageView* attachments = stackalloc ImageView[2];
            attachments[1] = depth.View;

            FramebufferCreateInfo fbInfo = new()
            {
                SType = StructureType.FramebufferCreateInfo,

                RenderPass = renderPass,
                AttachmentCount = 2,
                PAttachments = attachments,
                Width = bufferSize.Width,
                Height = bufferSize.Height,
                Layers = 1
            };

            Framebuffer[] framebuffers = new Framebuffer[(int)swapchainImageCount];
            Debug.Assert(framebuffers != null);

            for (int i = 0; i < swapchainImageCount; i++)
            {
                attachments[0] = swapChainBuffers[i].View;
                res = _vk.CreateFramebuffer(device.Device, fbInfo, null, out framebuffers[i]);
                Debug.Assert(res == Result.Success);
            }

            _vk.EndCommandBuffer(setupCmdBuffer);
            SubmitInfo submitInfo = new()
            {
                SType = StructureType.SubmitInfo,
                CommandBufferCount = 1,
                PCommandBuffers = &setupCmdBuffer
            };

            _vk.QueueSubmit(queue, 1, &submitInfo, default);
            _vk.QueueWaitIdle(queue);

            _vk.FreeCommandBuffers(device.Device, device.CommandPool, 1, setupCmdBuffer);

            SwapChain = swapchain;
            SwapChainBuffers = swapChainBuffers;
            SwapchainImageCount = swapchainImageCount;
            Framebuffers = framebuffers;
            CurrentBuffer = 0;
            Format = colourFormat;
            BufferSize = bufferSize;
            RenderPass = renderPass;
            Depth = depth;

            SemaphoreCreateInfo presentSemaphoreCreateInfo = new(StructureType.SemaphoreCreateInfo);
            res = _vk.CreateSemaphore(device.Device, presentSemaphoreCreateInfo, null, out _presentCompleteSemaphore);
            Debug.Assert(res == Result.Success);

            res = _vk.CreateSemaphore(device.Device, presentSemaphoreCreateInfo, null, out _renderCompleteSemaphore);
        }

        public unsafe void Dispose()
        {
            if (_presentCompleteSemaphore.Handle != 0)
            {
                _vk.DestroySemaphore(_device.Device, _presentCompleteSemaphore, null);
            }

            if (_renderCompleteSemaphore.Handle != 0)
            {
                _vk.DestroySemaphore(_device.Device, _renderCompleteSemaphore, null);
            }

            for (uint i = 0; i < SwapchainImageCount; i++)
            {
                _vk.DestroyImageView(_device.Device, SwapChainBuffers[i].View, null);
                _vk.DestroyFramebuffer(_device.Device, Framebuffers[i], null);
            }

            _vk.DestroyImageView(_device.Device, Depth.View, null);
            _vk.DestroyImage(_device.Device, Depth.Image, null);
            _vk.FreeMemory(_device.Device, Depth.Mem, null);

            _vk.DestroyRenderPass(_device.Device, RenderPass, null);
            VkUtil.KhrSwapchain.DestroySwapchain(_device.Device, SwapChain, null);
        }

    }
}
